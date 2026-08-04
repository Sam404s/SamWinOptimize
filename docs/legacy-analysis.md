# ZyperWinOptimize 4.1 legacy analysis

## Baseline

- Audited source: `.tmp/ZyperWinOptimize`
- Local/remote `main`: `d20e78bbd906fa179e4cc9f5107b502da5967aa9`
- Commit date: 2025-10-07 12:01:28 +0800
- Original stack: .NET Framework 4.7.2, WinForms, AntdUI 2.1.3, `packages.config`
- Original shell: fixed 860×540 window with ten navigation entries and user controls swapped into a panel

## Existing capability map

| Area | Legacy implementation | Upgrade decision |
|---|---|---|
| Dashboard | Static `MainMenu` user control | Replace with live system snapshot and recommended actions |
| Optimization | 151 tree items loaded through a missing `Bin/ZyperData.xml` command catalog | Replace with typed, built-in action catalog, searchable/filterable selection and explicit apply/revert commands |
| Restore | Reads backup INI files from a directory | Replace with JSON execution receipts and reversible action history |
| Cleanup | 21 direct `cmd.exe` cleanup command strings | Replace with typed cleanup tasks, previewable targets, progress and captured output |
| Appx | PowerShell package list/uninstall | Keep, move behind an async service and explicit confirmation |
| Defender | Registry/service mutation and third-party disable download | Replace with read-only health status and official Windows Security entry points |
| Edge | Force-removal logic | Replace with browser status and Apps settings entry point |
| Office | Downloads/runs remote install script | Replace with Microsoft 365 management links and installed-product status |
| Activation | Runs third-party activation script | Replace with Windows activation status and official Activation settings |
| Troubleshooting | Static shortcuts | Keep as a curated system toolbox using official Windows commands/settings |
| About | Static links and version | Rebrand as SamWinOptimize and show architecture/safety principles |

## Structural defects found

1. UI, command definitions, process execution and state persistence are mixed in form event handlers.
2. `Optimize` requires `Bin/ZyperData.xml`, but that file is absent from the repository; the main feature cannot be reconstructed reliably from source alone.
3. The application manifest forces administrator elevation before the user has chosen an elevated action.
4. Cleanup and optimization command failures are often redirected to `nul`, so the UI can report completion without trustworthy evidence.
5. Navigation relies on reflection over event argument properties and debug console output instead of a typed route model.
6. The single-instance routine closes the duplicate window but still returns `true`, allowing constructor work to continue.
7. The shell is locked to 860×540 and cannot adapt to high-DPI, long CJK labels or wider content.
8. Long-running work is directly coupled to controls; cancellation, execution receipts and consistent error states are absent.
9. High-impact actions do not share a risk model, confirmation copy, reboot indication or rollback contract.
10. Several features execute third-party remote scripts or weaken Windows security controls; they are outside the safe scope of the upgraded product.

## Target architecture

- **Runtime:** .NET 10 Windows Forms, nullable enabled, implicit global usings enabled.
- **Presentation:** token-driven custom components and reusable page controls; no designer-generated layout files.
- **Application model:** typed navigation, optimization, cleanup and execution result records.
- **Services:** system profile, elevated process execution, Appx query/uninstall, settings launcher and receipt storage.
- **Safety:** application runs as the current user; elevation is requested only for a confirmed action. Every command captures exit code/stdout/stderr and produces a receipt.
- **Product surface:** dashboard, optimization, cleanup, app manager, security, restore/history, toolbox, and about.

## Compatibility scope

SamWinOptimize targets supported Windows 10/11 environments capable of running .NET 10. The legacy Windows 7 promise is intentionally not carried forward because the new runtime and modern Windows settings APIs do not support it.
