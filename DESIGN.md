# SamWinOptimize Design System

## 0. Design Decision Log

- Product surface: native .NET WinForms desktop utility for Windows 10/11.
- Redesign intent: modern, spacious, operationally trustworthy, and visually confident. Information density is reduced without hiding execution scope, risk, or rollback information.
- Current visual direction: atmospheric dark glass system with deep navy depth, electric cyan signal color, floating surfaces, and generous breathing room.
  - Hero architecture: Editorial Split.
  - Typography direction: Outfit, adapted to installed Windows fonts as `Segoe UI Variable Display` and `Segoe UI Variable Text`.
  - Component architectures: Ambient Status Canvas, Command Deck, Editorial Metric Rail.
  - Motion paradigms: Hover Physics and Horizontal Progress, translated to meaningful WinForms hover/pressed/focus/loading feedback.
- Platform adaptation: no web runtime, GSAP, acrylic imitation, remote assets, or emoji. Native controls, owner drawing, Fluent glyphs, tonal depth, and responsive WinForms layout are used instead.

## 1. Atmosphere and Identity

SamWinOptimize is an atmospheric Windows care workspace, not a dense administrator console. The shell uses a deep navy canvas, a layered sidebar, and floating glass surfaces with cyan hairlines and soft ambient glow. Cards feel like transparent panels suspended above the workspace instead of flat white boxes. Electric cyan identifies the primary path; mint, amber, rose, and violet communicate health, attention, destructive impact, and neutral system state. Copy is direct and reassuring, with enough whitespace to scan before acting.

The visual hierarchy follows four chapters:

1. Attention: a clear page statement with no more than two lines of title and description.
2. Interest: one dominant task surface or status canvas.
3. Desire: grouped details, metrics, or command choices with progressive disclosure.
4. Action: a single obvious primary action plus restrained secondary actions.

## 2. Color

| Role | Token | Value | Usage |
|---|---|---:|---|
| Canvas | `Canvas` | `#08111F` | Deep navy application background |
| Canvas soft | `CanvasSoft` | `#0D1B2B` | Workspace and status transitions |
| Sidebar | `Sidebar` | `#0A1727` | Layered navigation rail |
| Sidebar raised | `SidebarRaised` | `#10243A` | Brand, navigation, and permission surfaces |
| Surface | `Surface` | `#102236` | Primary glass cards and grouped content |
| Surface raised | `SurfaceRaised` | `#152C43` | Inputs, selected rows, and secondary actions |
| Surface hover | `SurfaceHover` | `#1A3851` | Hover state and tonal lift |
| Surface strong | `SurfaceStrong` | `#1E405A` | Pressed state and emphasized blocks |
| Glass top | `GlassTop` | `#2B5B78` | Transparent top highlight and depth cue |
| Glass bottom | `GlassBottom` | `#0A1728` | Lower glass gradient anchor |
| Text primary | `TextPrimary` | `#E9F6FF` | Titles and important values |
| Text secondary | `TextSecondary` | `#AFC6D7` | Body copy |
| Text muted | `TextMuted` | `#6F8DA3` | Metadata and disabled states |
| Border | `Border` | `#26455D` | Neutral surface outline |
| Border strong | `BorderStrong` | `#3A6682` | Focus-neutral elevated outline |
| Glass highlight | `GlassHighlight` | `#8EDCFF` | Hairline highlights and edge glints |
| Accent | `Accent` | `#25D4FF` | Primary action and active navigation |
| Accent strong | `AccentStrong` | `#0AB1DD` | Pressed primary action |
| Accent wash | `AccentWash` | `#123E52` | Selected background and cyan wash |
| Accent soft | `AccentSoft` | `#8EEFFF` | Focus and progress track |
| Accent deep | `AccentDeep` | `#084B75` | Deep gradient anchor for primary surfaces |
| Success | `Success` | `#57E2B2` | Healthy and completed |
| Warning | `Warning` | `#FFCC66` | Confirmation and restart |
| Danger | `Danger` | `#FF7095` | Destructive and failed |
| Danger surface | `DangerSurface` | `#442338` | Destructive context |
| Info | `Info` | `#A18FFF` | Neutral system state |

Rules:

- Accent is used for one primary path per surface, selection, focus, and verified active state.
- Danger never competes with the primary action unless the page is explicitly destructive.
- Depth comes from layered translucent gradients, soft cyan glow, hairline highlights, and restrained shadows; surfaces should feel like glass, not opaque rectangles.
- Use glow as an edge cue, never as a substitute for hierarchy or contrast.
- Status is always text plus color, never color alone.
## 3. Typography

### Font stack

- Display/headings: `Segoe UI Variable Display`, fallback `Segoe UI`.
- UI/body: `Segoe UI Variable Text`, fallback `Microsoft YaHei UI`, then `Segoe UI`.
- Technical output: `Cascadia Mono`, fallback `Consolas`.
- Iconography: `Segoe Fluent Icons`; emoji glyphs are prohibited.

### Scale

| Level | Point size | Weight | Typical height | Usage |
|---|---:|---:|---:|---|
| Display | 28–30 pt | 700 | 48–56 px | Device/product hero |
| Page title | 22 pt | 700 | 38 px | Page statement |
| Section title | 14–16 pt | 600–700 | 28–34 px | Dominant section/card |
| Card title | 11–12 pt | 600–700 | 24–28 px | Task/tool title |
| Body | 10 pt | 400 | 24 px | Primary explanatory copy |
| Body small | 9 pt | 400 | 20 px | Secondary detail |
| Caption | 8–8.5 pt | 600 | 18 px | Metadata and overlines |
| Metric | 18–22 pt | 700 | 34–40 px | System metrics |

Rules:

- Page titles and hero statements occupy no more than two lines.
- Chinese explanations wrap before they are truncated. Truncation is reserved for device identifiers and table cells.
- Technical metadata may use uppercase mono text; human-facing labels use normal sentence case.
- Body copy never drops below 9 pt.

## 4. Spacing and Layout

- Base unit: 4 px.
- Spacing scale: `4, 8, 12, 16, 20, 24, 28, 32, 40, 48, 56, 64`.
- Default window: 1520×960.
- Minimum window: 1240×800.
- Sidebar: 344 px, fixed for this release.
- Status bar: 56 px.
- Page padding: 40 px horizontal, 32 px top, 36 px bottom.
- Page header: 146 px including its bottom breathing room.
- Section gap: 20–28 px.
- Primary surface padding: 28–32 px.
- Card padding: 22–28 px.
- Navigation item: 64 px high with 14 px inter-item rhythm; compact mode reduces to 56 px with 6 px rhythm.
- Action button: 46 px default height, 40 px compact minimum.
- Task card: 116 px minimum height.
- Table header: 48–52 px; row: 54–58 px.
- Scrollable pages keep 12 px breathing room on the scroll edge.

Responsive rules:

- At the 1240 px minimum window width, content remains readable without horizontal scrolling.
- Editorial two-column groups collapse to one column before body copy becomes narrow.
- Tool and metric grids use two columns only when each item receives at least 360 px.
- Header actions remain on one line; less important actions may move into the page command deck at narrow widths.
- Resize handlers reposition anchors from the current client rectangle; no coordinates assume the default window size.

## 5. Shell and Navigation

### App shell

- The sidebar is a calm product rail with a spacious brand block, grouped navigation, and a clear permission card.
- The workspace has no heavy top chrome. Each page begins with its own editorial statement.
- A quiet 44 px status strip confirms local execution and the current route.
- The selected route uses an accent wash, left signal bar, stronger label, and bright icon.

### Page header

- 70 px glass icon tile, 26 pt title, technical eyebrow, and one-line supportive description.
- Icon tile is visually secondary to the title, not a decorative badge.
- Actions align right and use 46 px controls.
- Header remains visually open; no pills or metrics are placed inside it.

## 6. Component Anatomy

### SurfacePanel

Variants:

- `Default`: primary grouped surface.
- `Raised`: stronger contrast for nested command decks and inputs.
- `Quiet`: low-contrast supporting section.
- `Accent`: primary status canvas with a calm teal border and subtle top highlight.
- `Danger`: destructive context using a restrained coral tint.

All variants define rounded geometry, border color, fill color, focus-neutral top highlight, and optional hover treatment.

### Ambient Status Canvas

- Used on the dashboard and major state pages.
- Editorial split: device/status statement on the left, two clear actions on the right.
- Status indicators sit below the statement and never compete with the hero title.

### Editorial Metric Rail

- Four metrics may share a single grouped rail.
- Each metric has a Fluent icon, quiet label, large value, and one-line hint.
- Dividers or tonal separation replace four unrelated floating cards.

### Command Deck

- Search/filter/selection/action controls live in one spacious surface.
- The primary action sits at the right edge and remains visually dominant.
- Loading state changes the label and disables related actions; progress messaging stays visible.

### TaskCard

- 116 px minimum height.
- Selection control has a generous hit target.
- Title and description occupy the center column; semantic tags form a right metadata rail.
- Hover uses a tonal lift and border emphasis. Selected state uses an accent signal and remains distinguishable without relying only on color.

### Data surfaces

- Tables use 56 px rows, restrained alternating fills, clear selected state, and generous cell padding.
- Technical detail views use a raised mono surface with selectable text.
- Empty/error/loading states are full-width and explain the next available action.

### Buttons

- Primary: electric cyan-to-deep-blue glass gradient, light text, one per command surface.
- Secondary: dark raised glass fill, light text, visible hairline border.
- Danger: rose-tinted glass fill, light text.
- All variants define default, hover, pressed, focus-visible, disabled, and loading states.

## 7. Motion and Feedback

WinForms mapping of the selected motion paradigms:

- Hover Physics: clickable navigation, cards, and buttons receive immediate tonal lift, border emphasis, and pressed compression through owner-drawn state changes.
- Horizontal Progress: long-running tasks expose a stable textual progress/status line in the command deck. Indeterminate decorative loops are avoided.
- Timing targets: 120 ms hover/focus, 160 ms pressed recovery, 220 ms status/progress transitions where practical.
- Motion never changes layout or causes scroll jumps.
- Reduced-motion environments receive immediate state changes.

## 8. Accessibility and Cognitive Safety

- Keyboard order follows the visual reading order.
- All actions have visible focus cues and at least 40×40 px hit areas.
- Contrast targets WCAG AA; primary content exceeds 4.5:1.
- Destructive/system-changing actions state scope and require confirmation.
- High-risk actions are never preselected.
- Command output remains selectable and copyable.
- Labels and descriptions are concise, friendly, and do not use alarmist language.
- The interface never hides elevation, restart, failure, or rollback implications.

## 9. Page Composition

- Dashboard: Ambient Status Canvas, Editorial Metric Rail, and three-step recommended path.
- Optimize: page header, spacious Command Deck, large task cards.
- Cleanup: safety statement integrated into the Command Deck, then large task cards.
- Apps: Command Deck plus a single expansive application table.
- Security: one dominant Defender card followed by larger grouped license/control surfaces.
- History: command deck plus editorial split between receipt list and technical detail.
- Toolbox: responsive two-column tool grid with fewer, larger cards.
- About: broad product hero, three principles, and migration statement.
- Execution result: clear outcome header, large technical output surface, one primary completion action.

## 10. Accepted Platform Debt

- WinForms does not provide web-grade typography metrics or composition animation; the implementation prioritizes stable spacing and native responsiveness.
- Segoe Fluent Icons availability depends on supported Windows versions.
- Native TextBox and ComboBox rendering cannot fully match owner-drawn surfaces without replacing their accessibility behavior; they receive tokenized colors, larger bounds, and grouped surface treatment.
- Live Defender, licensing, Office, and Appx state varies by Windows edition and policy. Unavailable state must remain truthful and readable.
- Future UI additions must extend this document and `Theme` before introducing new visual constants.
