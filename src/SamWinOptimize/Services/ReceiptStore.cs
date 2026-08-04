using System.Text.Json;
using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public sealed class ReceiptStore : IDisposable
{
    private const int MaxReceiptCount = 100;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _directory;
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ReceiptStore()
    {
        _directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SamWinOptimize");
        Directory.CreateDirectory(_directory);
        _filePath = Path.Combine(_directory, "execution-history.json");
    }

    public async Task<IReadOnlyList<ExecutionReceipt>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadReceiptsAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AppendAsync(ExecutionReceipt receipt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receipt);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var receipts = (await ReadReceiptsAsync(cancellationToken)).ToList();
            receipts.Insert(0, receipt);
            if (receipts.Count > MaxReceiptCount)
            {
                receipts.RemoveRange(MaxReceiptCount, receipts.Count - MaxReceiptCount);
            }

            await WriteReceiptsAtomicallyAsync(receipts, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();

    private async Task<IReadOnlyList<ExecutionReceipt>> ReadReceiptsAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            await using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await JsonSerializer.DeserializeAsync<List<ExecutionReceipt>>(
                stream, JsonOptions, cancellationToken) ?? [];
        }
        catch (JsonException)
        {
            // A partial write from an interrupted previous version should not make history unusable.
            return [];
        }
    }

    private async Task WriteReceiptsAtomicallyAsync(
        IReadOnlyList<ExecutionReceipt> receipts,
        CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(_directory, $"execution-history.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, receipts, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(_filePath))
            {
                File.Replace(tempPath, _filePath, destinationBackupFileName: null);
            }
            else
            {
                File.Move(tempPath, _filePath);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
