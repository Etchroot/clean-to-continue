# Mouse Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a browser-playable vertical slice that starts at the main menu, plays the short opening, lets the player clean a mouse with three freely switchable tools, completes at 90%, and displays the first memory reward.

**Architecture:** Keep gameplay rules in small testable C# models, then connect them to Unity components in a shared `StageRoot`. Surface dust and polish use UV masks, gap dirt uses small world-space targets, and all three report progress to one completion model. All six numbered scene skeletons exist first; the initial playable slice fills `01.MainMenu`, `02.Opening`, and `03.Mouse`, while `04.Keyboard`, `05.Headset`, and `06.Ending` remain placeholders for later roadmap work.

**Tech Stack:** Unity 6000.3.22f1, Universal Render Pipeline 17.3.0, Input System 1.17.0, uGUI, Unity Test Framework 1.6.0, C#, Shader Graph/ShaderLab, Web platform.

**Spec:** `docs/GAME_DESIGN.md`

## Global Constraints

- Final output must run in a browser without installation or login.
- Controls are left-drag to clean, right-drag to rotate, `Space` to highlight, and UI buttons or `1`/`2`/`3` to select air gun, cotton swab, or cloth.
- Tools may be switched in any order; a tool changes only its matching contamination and wrong tools have no penalty.
- Do not render a hand or 3D cleaning tool.
- Dust and polish use UV masks; gap dirt uses small 3D targets.
- Cloth reveals the original material smoothness and lighting response; it does not erase a visible stain texture.
- `Space` works at every progress value and highlights only unfinished areas.
- Overall progress completes once at `0.90`; remaining traces are cleared during the completion presentation.
- Unity Asset Store originals stay under `Game/Assets/ThirdParty/` and are not committed.
- Never open the same Unity project in GUI and batch mode simultaneously; while the Editor is open use MCP tests, or close it with the user's knowledge before a batch run.
- Every task ends with tests, console inspection, documentation where applicable, and a focused commit.

## Planned File Map

| Path | Responsibility |
|---|---|
| `Game/Assets/CleanToContinue/Runtime/CleanToContinue.Runtime.asmdef` | Runtime assembly boundary. |
| `Game/Assets/CleanToContinue/Runtime/Core/CleaningTool.cs` | Three tool identities. |
| `Game/Assets/CleanToContinue/Runtime/Core/ToolSelectionModel.cs` | Current tool and change event. |
| `Game/Assets/CleanToContinue/Runtime/Progress/IProgressSource.cs` | Shared progress contract. |
| `Game/Assets/CleanToContinue/Runtime/Progress/StageProgressModel.cs` | Equal-weight total and one-shot 90% completion. |
| `Game/Assets/CleanToContinue/Runtime/Surface/CoverageGrid.cs` | Testable unique-cell coverage calculation. |
| `Game/Assets/CleanToContinue/Runtime/Surface/RuntimeMaskPainter.cs` | Double-buffered Web-safe mask stamping. |
| `Game/Assets/CleanToContinue/Runtime/Surface/SurfaceMaskLayer.cs` | Ray-hit UV cleaning for dust or polish. |
| `Game/Assets/CleanToContinue/Shaders/MaskStamp.shader` | Writes a circular value into a mask without compute shaders. |
| `Game/Assets/CleanToContinue/Shaders/CleanableSurface.shader` | Dust color and per-pixel smoothness restoration. |
| `Game/Assets/CleanToContinue/Runtime/Gap/GapDirtSpot.cs` | One cotton-swab dirt target. |
| `Game/Assets/CleanToContinue/Runtime/Gap/GapDirtGroup.cs` | Gap progress and target lookup. |
| `Game/Assets/CleanToContinue/Runtime/Input/StageInputController.cs` | Input System actions and pointer/UI guards. |
| `Game/Assets/CleanToContinue/Runtime/Input/EquipmentRotator.cs` | Bounded right-drag rotation. |
| `Game/Assets/CleanToContinue/Runtime/Input/StageInteractionController.cs` | Routes left-drag hits to the selected cleaning system. |
| `Game/Assets/CleanToContinue/Runtime/Highlight/HighlightController.cs` | One-shot unfinished-area pulse. |
| `Game/Assets/CleanToContinue/Runtime/Stage/StageController.cs` | Wires models, locks completion, and opens the memory panel. |
| `Game/Assets/CleanToContinue/Runtime/UI/ToolSelectorView.cs` | Three tool buttons, keyboard state, and selected visuals. |
| `Game/Assets/CleanToContinue/Runtime/UI/ProgressWheelView.cs` | Radial total and integer percent. |
| `Game/Assets/CleanToContinue/Runtime/UI/MemoryPanelView.cs` | Mouse memory image, line, and continue action. |
| `Game/Assets/CleanToContinue/Runtime/UI/MainMenuView.cs` | Start, settings, and credits panels. |
| `Game/Assets/CleanToContinue/Runtime/Flow/SceneFlow.cs` | Async scene transition with input lock. |
| `Game/Assets/CleanToContinue/Runtime/Flow/OpeningSequence.cs` | Ten-second opening and skip behavior. |
| `Game/Assets/CleanToContinue/Runtime/Audio/CleaningAudioController.cs` | Tool loop, selection, highlight, and completion audio. |
| `Game/Assets/CleanToContinue/Editor/VerticalSliceSceneBuilder.cs` | Reproducibly builds the three slice scenes and prefabs. |
| `Game/Assets/CleanToContinue/Editor/NumberedSceneBuilder.cs` | Creates and preserves the six numbered scene skeletons and build order. |
| `Game/Assets/CleanToContinue/Editor/WebBuildCommand.cs` | Deterministic Web build entry point. |
| `Game/Assets/CleanToContinue/Tests/EditMode/` | Pure rule and coverage tests. |
| `Game/Assets/CleanToContinue/Tests/PlayMode/` | Scene wiring and completion smoke tests. |
| `Game/Assets/CleanToContinue/Tests/PlayMode/CleanToContinue.PlayModeTests.asmdef` | PlayMode test assembly boundary. |

---

### Task 1: Runtime Assembly and Tool Selection

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/CleanToContinue.Runtime.asmdef`
- Create: `Game/Assets/CleanToContinue/Runtime/Core/CleaningTool.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Core/ToolSelectionModel.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/CleanToContinue.EditModeTests.asmdef`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/ToolSelectionModelTests.cs`

**Interfaces:**
- Produces: `CleaningTool { AirGun, CottonSwab, Cloth }`.
- Produces: `ToolSelectionModel.Selected`, `Select(CleaningTool)`, and `SelectionChanged`.

- [x] **Step 1: Write the failing selection tests**

```csharp
[Test]
public void StartsWithAirGun() =>
    Assert.That(new ToolSelectionModel().Selected, Is.EqualTo(CleaningTool.AirGun));

[Test]
public void SelectingToolChangesOnce()
{
    var model = new ToolSelectionModel();
    var calls = 0;
    model.SelectionChanged += _ => calls++;
    model.Select(CleaningTool.Cloth);
    model.Select(CleaningTool.Cloth);
    Assert.That(model.Selected, Is.EqualTo(CleaningTool.Cloth));
    Assert.That(calls, Is.EqualTo(1));
}
```

Use these assembly definitions:

```json
// CleanToContinue.Runtime.asmdef
{
  "name": "CleanToContinue.Runtime",
  "references": ["Unity.InputSystem", "Unity.ugui"],
  "autoReferenced": true
}
```

```json
// CleanToContinue.EditModeTests.asmdef
{
  "name": "CleanToContinue.EditModeTests",
  "references": ["CleanToContinue.Runtime", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "optionalUnityReferences": ["TestAssemblies"],
  "includePlatforms": ["Editor"]
}
```

- [x] **Step 2: Run the EditMode test and confirm it fails because the runtime types do not exist**

Run:

```powershell
& 'C:\Program Files\Unity Hub\6000.3.22f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\차명근\Documents\openaigamebuilders\Game' -runTests -testPlatform EditMode -testResults 'C:\Users\차명근\Documents\openaigamebuilders\TestResults\tool-selection.xml'
```

Expected: non-zero exit or failed test compilation naming `ToolSelectionModel`.

- [x] **Step 3: Implement the minimal model**

```csharp
public enum CleaningTool { AirGun, CottonSwab, Cloth }

public sealed class ToolSelectionModel
{
    public CleaningTool Selected { get; private set; } = CleaningTool.AirGun;
    public event System.Action<CleaningTool> SelectionChanged;

    public void Select(CleaningTool tool)
    {
        if (Selected == tool) return;
        Selected = tool;
        SelectionChanged?.Invoke(tool);
    }
}
```

- [x] **Step 4: Run all EditMode tests and inspect the Unity console through MCP**

Expected: tests pass; no new compilation errors.

- [x] **Step 5: Commit**

```powershell
git add Game/Assets/CleanToContinue
git commit -m "feat: add cleaning tool selection model"
```

---

### Task 2: Coverage and 90 Percent Completion Rules

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Progress/IProgressSource.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Progress/StageProgressModel.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Surface/CoverageGrid.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/CoverageGridTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/StageProgressModelTests.cs`

**Interfaces:**
- Produces: `IProgressSource.Tool`, `Progress01`, and `ProgressChanged`.
- Produces: `CoverageGrid.CreateFilled(int, int)`, `ApplyDisc(Vector2, float)`, and `Progress01`.
- Produces: `StageProgressModel.Progress01`, `IsComplete`, `Refresh()`, and `Completed`.

- [x] **Step 1: Write failing coverage tests**

```csharp
[Test]
public void RepeatingSameStrokeDoesNotDoubleCount()
{
    var grid = CoverageGrid.CreateFilled(32, 32);
    grid.ApplyDisc(new Vector2(0.5f, 0.5f), 0.1f);
    var once = grid.Progress01;
    grid.ApplyDisc(new Vector2(0.5f, 0.5f), 0.1f);
    Assert.That(grid.Progress01, Is.EqualTo(once));
}

[Test]
public void UvOutsideRangeIsClamped()
{
    var grid = CoverageGrid.CreateFilled(16, 16);
    Assert.DoesNotThrow(() => grid.ApplyDisc(new Vector2(-1f, 2f), 0.15f));
}
```

- [x] **Step 2: Write failing completion tests with a local fake progress source**

```csharp
[Test]
public void CompletesOnceAtNinetyPercent()
{
    var a = new FakeSource(CleaningTool.AirGun, 0.9f);
    var b = new FakeSource(CleaningTool.CottonSwab, 0.9f);
    var c = new FakeSource(CleaningTool.Cloth, 0.899f);
    var model = new StageProgressModel(new IProgressSource[] { a, b, c }, 0.9f);
    var calls = 0;
    model.Completed += () => calls++;
    model.Refresh();
    Assert.That(model.IsComplete, Is.False);
    c.Set(0.901f);
    model.Refresh();
    model.Refresh();
    Assert.That(model.IsComplete, Is.True);
    Assert.That(calls, Is.EqualTo(1));
}
```

Define this local fake at the bottom of `StageProgressModelTests.cs`:

```csharp
private sealed class FakeSource : IProgressSource
{
    public FakeSource(CleaningTool tool, float value) { Tool = tool; Progress01 = value; }
    public CleaningTool Tool { get; }
    public float Progress01 { get; private set; }
    public event System.Action ProgressChanged;
    public void Set(float value) { Progress01 = value; ProgressChanged?.Invoke(); }
}
```

- [x] **Step 3: Run both tests and verify they fail for missing types**

Expected: failures name `CoverageGrid`, `IProgressSource`, and `StageProgressModel`.

- [x] **Step 4: Implement unique-cell coverage and equal-weight stage progress**

Implementation rules:

```csharp
public interface IProgressSource
{
    CleaningTool Tool { get; }
    float Progress01 { get; }
    event System.Action ProgressChanged;
}
```

`CoverageGrid` stores a `bool[] remaining` and immutable `targetCount`. `ApplyDisc` clamps UV to `[0,1]`, clears only previously remaining cells inside the normalized radius, and returns the number newly cleaned. `Progress01` is `1f - remainingCount / (float)targetCount`; when `targetCount` is zero it returns `1f` without division.

`StageProgressModel.Refresh()` averages its three sources with equal weight, clamps to `[0,1]`, and invokes `Completed` only when the previous state was incomplete and the new total is at least `0.90f`.

- [x] **Step 5: Run all EditMode tests**

Expected: selection, coverage, 89.9%, 90.0%, and one-shot completion tests pass.

- [x] **Step 6: Commit**

```powershell
git add Game/Assets/CleanToContinue
git commit -m "feat: add cleaning progress rules"
```

---

### Task 3: Surface Masks and Lighting-Based Polish

**Files:**
- Create: `Game/Assets/CleanToContinue/Shaders/MaskStamp.shader`
- Create: `Game/Assets/CleanToContinue/Shaders/CleanableSurface.shader`
- Create: `Game/Assets/CleanToContinue/Runtime/Surface/RuntimeMaskPainter.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Surface/SurfaceMaskLayer.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/RuntimeMaskPainterTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/SurfaceMaskLayerTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/CleanToContinue.PlayModeTests.asmdef`

Create the PlayMode assembly before its first test:

```json
{
  "name": "CleanToContinue.PlayModeTests",
  "references": ["CleanToContinue.Runtime", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "optionalUnityReferences": ["TestAssemblies"]
}
```

**Interfaces:**
- Consumes: `CleaningTool`, `CoverageGrid`, and `IProgressSource`.
- Produces: `RuntimeMaskPainter.Initialize(int, Color)`, `Stamp(Vector2, float, float)`, `CurrentMask`, and `Dispose()`.
- Produces: `SurfaceMaskLayer.TryClean(CleaningTool, RaycastHit, float)` and `ForceFinish()`.

- [x] **Step 1: Write failing mask lifecycle and tool-routing tests**

```csharp
[UnityTest]
public IEnumerator WrongToolDoesNotChangeDustProgress()
{
    var layer = CreateTestSurface(CleaningTool.AirGun);
    var before = layer.Progress01;
    layer.TryClean(CleaningTool.Cloth, CreateCenterHit(layer), 0.1f);
    yield return null;
    Assert.That(layer.Progress01, Is.EqualTo(before));
}
```

Implement `CreateTestSurface` and `CreateCenterHit` as private helpers in the same test file. They create a primitive sphere on the `Cleanable` layer, add `SurfaceMaskLayer`, raycast from `(0,0,-3)` toward its center to obtain a real `RaycastHit.textureCoord`, and destroy the GameObject in teardown.

Add an EditMode test that initializes a 32×32 painter, verifies `CurrentMask` exists, disposes it, and verifies both temporary render textures are released without throwing.

- [x] **Step 2: Run tests and confirm missing implementation failures**

- [x] **Step 3: Implement a Web-compatible double-buffer mask stamp**

Use two `RenderTexture` instances with `RenderTextureFormat.ARGB32`, bilinear filtering, clamp wrapping, and no compute shader. `Stamp` sets `_BrushUV`, `_BrushRadius`, and `_WriteValue`, blits current → scratch through `MaskStamp.shader`, swaps the references, and updates the renderer property block.

The stamp fragment uses this exact rule:

```hlsl
float oldValue = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).r;
float distanceFromBrush = distance(input.uv, _BrushUV.xy);
float strength = 1.0 - smoothstep(_BrushRadius * 0.75, _BrushRadius, distanceFromBrush);
float value = lerp(oldValue, _WriteValue, strength);
return float4(value, value, value, 1.0);
```

- [x] **Step 4: Configure the URP Lit surface shader exactly**

Properties:

| Property | Type | Default |
|---|---|---:|
| `_BaseMap` | Texture2D | white |
| `_BaseColor` | Color | white |
| `_DustMask` | Texture2D | white |
| `_PolishRemainingMask` | Texture2D | white |
| `_DustColor` | Color | `#756F66` |
| `_DustOpacity` | Float | `0.55` |
| `_DirtySmoothness` | Float | `0.08` |
| `_CleanSmoothness` | Float | `0.72` |
| `_HighlightPulse` | Float | `0` |
| `_HighlightColor` | HDR Color | pale gold |

Node equations:

```text
dustRemaining = Sample(_DustMask).r
polishClean = 1 - Sample(_PolishRemainingMask).r
BaseColor = Lerp(_BaseMap * _BaseColor, _DustColor, dustRemaining * _DustOpacity)
Smoothness = Lerp(_DirtySmoothness, _CleanSmoothness, polishClean) * Lerp(1, 0.35, dustRemaining)
Emission = _HighlightColor * max(dustRemaining, 1 - polishClean) * _HighlightPulse
```

Use the Lit target so the restored smoothness reacts to the scene's real URP lights and reflections.

- [x] **Step 5: Implement `SurfaceMaskLayer`**

Create one instance for dust (`Tool = AirGun`, shader property `_DustMask`, stamp write value `0`) and one for polish remaining (`Tool = Cloth`, shader property `_PolishRemainingMask`, stamp write value `0`). Both start white, own a 64×64 `CoverageGrid`, use a 512×512 visual mask, and raise `ProgressChanged` only when a stroke cleans at least one new coverage cell. Store the property name as a serialized string so both layers can share one renderer and one material property block without overwriting each other.

Before setting a texture or highlight value, call `renderer.GetPropertyBlock(block)`, change only the owned property, then call `renderer.SetPropertyBlock(block)`. This preserves the other layer's mask and avoids last-writer-wins material state.

- [x] **Step 6: Run EditMode and PlayMode tests, then inspect a captured Scene view**

Expected: the same stroke is idempotent, wrong tools do nothing, and cloth strokes produce localized highlights under the Directional Light.

- [x] **Step 7: Commit**

```powershell
git add Game/Assets/CleanToContinue
git commit -m "feat: add surface dust and polish masks"
```

---

### Task 4: Cotton-Swab Gap Dirt and Space Highlight

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Gap/GapDirtSpot.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Gap/GapDirtGroup.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Highlight/HighlightController.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/GapDirtGroupTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/HighlightControllerTests.cs`

**Interfaces:**
- Consumes: `CleaningTool`, `IProgressSource`, and the two `SurfaceMaskLayer` instances.
- Produces: `GapDirtSpot.Apply(float)`, `Remaining01`, and `SetHighlight(float)`.
- Produces: `GapDirtGroup.TryClean(CleaningTool, Collider, float)`, `Progress01`, and `ForceFinish()`.
- Produces: `HighlightController.Pulse()` with a fixed 1.2-second duration.

- [x] **Step 1: Write failing gap progress tests**

```csharp
[Test]
public void CottonSwabReducesOnlyMatchingSpot()
{
    var group = CreateGroupWithTwoSpots();
    Assert.That(group.TryClean(CleaningTool.AirGun, group.Spots[0].Collider, 0.5f), Is.False);
    Assert.That(group.TryClean(CleaningTool.CottonSwab, group.Spots[0].Collider, 0.5f), Is.True);
    Assert.That(group.Progress01, Is.EqualTo(0.25f).Within(0.001f));
}
```

Define `CreateGroupWithTwoSpots` as a private helper in `GapDirtGroupTests.cs`; it creates a parent GameObject, two child spots with sphere colliders, assigns both to the group, and destroys the parent in teardown.

- [x] **Step 2: Write a failing highlight test**

At progress 0%, call `Pulse()`, verify unfinished surface and gap targets receive a positive highlight value, advance 1.3 seconds, and verify the value returns to zero. Repeat after completing one spot and verify that spot is not highlighted.

- [x] **Step 3: Run tests and verify the new types are missing**

- [x] **Step 4: Implement gap dirt**

Each spot starts at `Remaining01 = 1`, subtracts `cleaningPower * deltaTime`, scales its visible dirt child from `1` to `0.25`, emits a small particle burst at zero, and disables its cleaning collider only after completion. Group progress is `1 - average(Remaining01)`.

- [x] **Step 5: Implement the highlight pulse**

`Pulse()` restarts a 1.2-second unscaled-time coroutine. Use `sin(normalizedTime * PI)` for intensity, set `_HighlightPulse` through a `MaterialPropertyBlock` on surface renderers, and call `SetHighlight` only on gap spots whose `Remaining01 > 0`. Do not modify cleaning progress.

- [x] **Step 6: Run tests and inspect the pulse through Unity MCP capture**

Expected: highlight works at 0% and after partial cleaning, finished targets stay dark, and no progress changes.

- [x] **Step 7: Commit**

```powershell
git add Game/Assets/CleanToContinue
git commit -m "feat: add gap dirt and remaining-area highlight"
```

---

### Task 5: Input Routing and Mouse Rotation

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Input/StageInputController.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Input/EquipmentRotator.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Input/StageInteractionController.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/EquipmentRotatorTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/StageInteractionControllerTests.cs`
- Modify: `Game/ProjectSettings/TagManager.asset`

**Interfaces:**
- Consumes: `ToolSelectionModel`, surface layers, `GapDirtGroup`, `HighlightController`.
- Produces: pointer position, clean-held, rotate-held, highlight-performed, and numeric tool selection events.
- Produces: `EquipmentRotator.ApplyDrag(Vector2 delta)`.

- [ ] **Step 1: Write a failing bounded-rotation test**

```csharp
[Test]
public void PitchStaysInsideConfiguredBounds()
{
    var rotator = CreateRotator(-35f, 55f);
    rotator.ApplyDrag(new Vector2(0f, 10000f));
    Assert.That(rotator.Pitch, Is.InRange(-35f, 55f));
}
```

Define `CreateRotator` as a private helper in `EquipmentRotatorTests.cs`; it creates one GameObject, configures the pitch limits through a public `Configure(float minPitch, float maxPitch, float sensitivity)` method, and destroys the GameObject in teardown.

- [ ] **Step 2: Write failing interaction tests**

Verify left-drag with `AirGun` calls only the dust layer, left-drag with `Cloth` calls only polish, left-drag with `CottonSwab` calls only gap dirt, right-drag calls only rotation, and a pointer beginning over UI calls none of them.

- [ ] **Step 3: Run tests and confirm expected failures**

- [ ] **Step 4: Implement Input System actions in code**

Bindings:

```text
Point: <Pointer>/position
Clean: <Mouse>/leftButton
Rotate: <Mouse>/rightButton
Highlight: <Keyboard>/space
AirGun: <Keyboard>/1
CottonSwab: <Keyboard>/2
Cloth: <Keyboard>/3
```

Enable actions in `OnEnable`, disable and dispose them in `OnDisable`, and stop all continuous tool audio in `OnApplicationFocus(false)`.

Name user layer 8 `Cleanable` in `TagManager.asset`. Tests and the scene builder assign all interactive surface and gap colliders to this layer; desk and decoration remain outside it.

- [ ] **Step 5: Implement deterministic routing**

Raycast only against a `Cleanable` layer. While left is held, route the hit according to `ToolSelectionModel.Selected`. While right is held, feed pointer delta to `EquipmentRotator`; never clean and rotate in the same frame. Record whether the press began over UI with `EventSystem.current.IsPointerOverGameObject()` and keep it blocked until release.

- [ ] **Step 6: Run all tests and manually verify controls in Play Mode**

Expected: left cleans, right rotates, `Space` pulses, `1`/`2`/`3` change tools, and clicking UI never cleans behind it.

- [ ] **Step 7: Commit**

```powershell
git add Game/Assets/CleanToContinue Game/ProjectSettings/TagManager.asset
git commit -m "feat: add cleaning input and equipment rotation"
```

---

### Task 6: Stage UI, Audio, and Completion Reward

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Stage/StageController.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/UI/ToolSelectorView.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/UI/ProgressWheelView.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/UI/MemoryPanelView.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Audio/CleaningAudioController.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Audio/PrototypeAudioFactory.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/StageControllerTests.cs`

**Interfaces:**
- Consumes: selection and progress models plus three `IProgressSource` components.
- Produces: `StageController.Initialize()`, `CompleteStage()`, and `InputLocked`.
- Produces: `ProgressWheelView.Render(float)`, `ToolSelectorView.RenderSelection(CleaningTool)`, and `ToolSelectorView.RenderProgress(CleaningTool, float)`.

- [ ] **Step 1: Write a failing completion-lock test**

```csharp
[UnityTest]
public IEnumerator NinetyPercentCompletesOnceAndLocksInput()
{
    var stage = CreateStageControllerWithFakeSources();
    stage.SetProgress(0.899f);
    Assert.That(stage.InputLocked, Is.False);
    stage.SetProgress(0.9f);
    stage.SetProgress(1f);
    yield return null;
    Assert.That(stage.InputLocked, Is.True);
    Assert.That(stage.MemoryOpenCount, Is.EqualTo(1));
}
```

Define `CreateStageControllerWithFakeSources` and its three mutable `IProgressSource` fakes inside `StageControllerTests.cs`. Its `SetProgress(float)` helper sets all three sources to the same value and calls the real model's `Refresh`; `MemoryOpenCount` increments from the real memory-open event rather than bypassing `StageController`.

- [ ] **Step 2: Run the test and confirm missing UI/stage types**

- [ ] **Step 3: Implement the right-side UI**

Use a 1920×1080 reference resolution with `Scale With Screen Size`, match `0.5`. Place a radial `Image` and integer percent at top-right. Place three 88×88 buttons below it in AirGun, CottonSwab, Cloth order. Selected state uses a 4-pixel pale-gold border, 1.08 scale, and accessible text label; the unselected state remains readable. Each button has a small radial fill for its own progress and changes to a check mark at 100%. Button callbacks call `ToolSelectionModel.Select`; progress source events call `RenderProgress` for the matching tool.

Settings use exact `PlayerPrefs` keys `ctc.masterVolume`, `ctc.sfxVolume`, and `ctc.rotationSensitivity`; defaults are `0.8`, `1.0`, and `1.0`. Apply master volume through `AudioListener.volume`, multiply tool effects by the SFX value, and inject rotation sensitivity into `EquipmentRotator`.

- [ ] **Step 4: Implement stage completion**

When `StageProgressModel.Completed` fires: set `InputLocked`, stop tool loops, animate the wheel from current value to 100% over 0.35 seconds, call `ForceFinish()` on all layers, play one completion sound, darken the background, and open the mouse memory panel. The temporary continue action returns to `01.MainMenu`; Task 3 of the roadmap will change it to `04.Keyboard`.

- [ ] **Step 5: Add deterministic prototype audio**

Generate temporary clips at 44.1 kHz after the first user interaction: filtered noise for air gun, quiet high-frequency friction noise for cotton swab, low-pass friction noise for cloth, and a two-note sine chime for completion. Seed noise with a fixed integer so builds are reproducible. `CleaningAudioController` cross-fades only the selected tool loop while cleaning and stops on release, focus loss, UI interaction, or completion.

- [ ] **Step 6: Run PlayMode tests and inspect UI at 1920×1080 and 1366×768**

Expected: no clipping, number and radial fill agree, selection is distinguishable without sound, and completion opens once.

- [ ] **Step 7: Commit**

```powershell
git add Game/Assets/CleanToContinue
git commit -m "feat: add stage UI audio and memory reward"
```

---

### Task 7: Main Menu, Opening, and Mouse Scene Assembly

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Flow/SceneFlow.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Flow/OpeningSequence.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/UI/MainMenuView.cs`
- Create: `Game/Assets/CleanToContinue/Editor/CleanToContinue.Editor.asmdef`
- Create: `Game/Assets/CleanToContinue/Editor/VerticalSliceSceneBuilder.cs`
- Modify: `Game/Assets/CleanToContinue/Editor/NumberedSceneBuilder.cs`
- Modify: `Game/Assets/CleanToContinue/Scenes/01.MainMenu.unity`
- Modify: `Game/Assets/CleanToContinue/Scenes/02.Opening.unity`
- Modify: `Game/Assets/CleanToContinue/Scenes/03.Mouse.unity`
- Create: `Game/Assets/CleanToContinue/Prefabs/StageRoot.prefab`
- Create: `Game/Assets/CleanToContinue/Prefabs/PrototypeMouse.prefab`
- Modify: `Game/ProjectSettings/EditorBuildSettings.asset`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/VerticalSliceSceneTests.cs`

**Interfaces:**
- Consumes: every runtime component from Tasks 1–6.
- Produces: `SceneFlow.Load(string)`, the reusable `StageRoot` prefab, and three playable scenes.

- [ ] **Step 1: Write failing scene smoke tests**

Load each scene by name and assert:

```csharp
Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(expectedName));
Assert.That(Object.FindFirstObjectByType<EventSystem>(), Is.Not.Null);
```

For `01.MainMenu`, require Start, Settings, and Credits buttons. For `02.Opening`, require Skip and a timed transition component. For `03.Mouse`, require one `StageController`, three progress sources with distinct tools, one `EquipmentRotator`, and one memory panel.

- [ ] **Step 2: Run PlayMode tests and confirm the scenes are missing**

- [ ] **Step 3: Implement the scene builder**

Use this editor assembly definition:

```json
{
  "name": "CleanToContinue.Editor",
  "references": ["CleanToContinue.Runtime"],
  "includePlatforms": ["Editor"],
  "autoReferenced": true
}
```

Add menu item `Clean to Continue/Build Vertical Slice Scenes`. It must be idempotent: create or update project-owned objects without deleting user assets. Build:

```text
01.MainMenu
  Main Camera
  EventSystem
  Canvas
    Title
    StartButton
    SettingsButton
    CreditsButton
    SettingsPanel (hidden)
    CreditsPanel (hidden)

02.Opening
  Main Camera
  Directional Light
  DeskPlaceholder
  Canvas/OpeningLine
  Canvas/SkipButton

03.Mouse
  Main Camera
  Directional Light
  ReflectionProbe
  DeskPlaceholder
  StageRoot
  PrototypeMouse
    SurfaceCollider
    DustLayer
    PolishLayer
    GapDirtGroup
```

Use Unity primitives for the temporary desk and mouse so no third-party asset blocks the slice. Create a rounded mouse silhouette from scaled spheres/capsules with one cleanable UV-bearing top shell, a wheel, and four gap spots around the wheel/buttons. Keep generated project assets under `Assets/CleanToContinue`, register Undo when using MCP, and save all scenes explicitly.

- [ ] **Step 4: Wire scene flow**

Start loads `02.Opening`; Skip or the 10-second timer loads `03.Mouse`; the memory Continue button returns to `01.MainMenu`. Opening displays `정말 오랜만이다. 그런데… 이걸 먼저 치워야겠는데.` and the memory panel displays `그때는 바라보는 것만으로도 새로운 세계가 열렸다.` Settings exposes master volume, effects volume, and rotation sensitivity. Credits list the user, Codex, Unity, and the exact prototype line `Third-party assets: none in this prototype.`; replace that line only after an approved asset is actually integrated and recorded in `submission/ASSET_CREDITS.md`.

- [ ] **Step 5: Add build settings in exact order**

```text
0 Assets/CleanToContinue/Scenes/01.MainMenu.unity
1 Assets/CleanToContinue/Scenes/02.Opening.unity
2 Assets/CleanToContinue/Scenes/03.Mouse.unity
3 Assets/CleanToContinue/Scenes/04.Keyboard.unity
4 Assets/CleanToContinue/Scenes/05.Headset.unity
5 Assets/CleanToContinue/Scenes/06.Ending.unity
```

- [ ] **Step 6: Run scene smoke tests and inspect through MCP**

Use the Unity console, root GameObject/component read, and multi-angle capture. Expected: all tests pass, no missing scripts/materials, the mouse fills the central view, and right-side UI does not overlap it.

- [ ] **Step 7: Commit**

```powershell
git add Game/Assets/CleanToContinue Game/ProjectSettings/EditorBuildSettings.asset
git commit -m "feat: assemble mouse vertical slice scenes"
```

---

### Task 8: Web Build, Browser Verification, and Documentation

**Files:**
- Create: `Game/Assets/CleanToContinue/Editor/WebBuildCommand.cs`
- Modify: `docs/NONTECHNICAL_GUIDE.md`
- Modify: `docs/DEVELOPMENT_LOG.md`
- Modify: `docs/HUMAN_IN_THE_LOOP.md`
- Modify: `submission/CODEX_COLLABORATION.md`
- Modify: `submission/ASSET_CREDITS.md` only if approved external assets were actually used.

**Interfaces:**
- Consumes: the six-scene numbered build list with the first three scenes forming the playable vertical slice.
- Produces: menu command and batch entry `CleanToContinue.Editor.WebBuildCommand.BuildVerticalSlice`.

- [ ] **Step 1: Add a deterministic Web build command**

The command validates the six required numbered scenes, creates `Game/Builds/Web` if absent, and calls `BuildPipeline.BuildPlayer` for `BuildTarget.WebGL`. Treat any result other than `BuildResult.Succeeded` as an exception and log output size and duration.

- [ ] **Step 2: Run the full automated suite**

```powershell
& 'C:\Program Files\Unity Hub\6000.3.22f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\차명근\Documents\openaigamebuilders\Game' -runTests -testPlatform EditMode -testResults 'C:\Users\차명근\Documents\openaigamebuilders\TestResults\editmode.xml'
```

Expected: zero failed EditMode tests and zero compilation errors.

- [ ] **Step 3: Run the PlayMode suite**

```powershell
& 'C:\Program Files\Unity Hub\6000.3.22f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\차명근\Documents\openaigamebuilders\Game' -runTests -testPlatform PlayMode -testResults 'C:\Users\차명근\Documents\openaigamebuilders\TestResults\playmode.xml'
```

Expected: zero failed PlayMode tests and zero compilation errors.

- [ ] **Step 4: Build Web output**

```powershell
& 'C:\Program Files\Unity Hub\6000.3.22f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\차명근\Documents\openaigamebuilders\Game' -executeMethod CleanToContinue.Editor.WebBuildCommand.BuildVerticalSlice -logFile 'C:\Users\차명근\Documents\openaigamebuilders\work\web-build.log'
```

Expected: exit code `0`, `Game/Builds/Web/index.html` exists, and the log reports `Succeeded`.

- [ ] **Step 5: Start a local HTTP server**

```powershell
py -m http.server 8000 --directory 'C:\Users\차명근\Documents\openaigamebuilders\Game\Builds\Web'
```

Keep this terminal session open only while testing and stop it with `Ctrl+C` afterward.

- [ ] **Step 6: Verify the actual Chrome build**

Open `http://localhost:8000` in current desktop Chrome. Check: first click unlocks audio, `01.MainMenu` → `02.Opening` → `03.Mouse` works, Skip works, left/right drags do not conflict, all three tools work in arbitrary order, `Space` works at 0%, UI clicks do not clean, focus loss stops sound, 90% completes once, memory panel opens, Continue returns to menu, refresh starts cleanly, and no browser console errors appear.

- [ ] **Step 7: Repeat the browser matrix in Edge**

Open the same localhost URL in current desktop Edge and repeat the complete Step 6 checklist. Record Chrome and Edge separately.

- [ ] **Step 8: Update nontechnical and submission evidence**

For every implemented file, document: what it does, what the player sees, values the user can change, exact path, and how to verify it. Add the test XML paths, Web build log, MCP inspection result, browser matrix, relevant HIL IDs, and commit IDs to the development and collaboration logs. Do not claim Chrome or Edge success without actually opening that browser build.

- [ ] **Step 9: Run final repository checks**

```powershell
git diff --check
git status --short
git ls-files Game/Assets/ThirdParty
```

Expected: no whitespace errors, only intended project/docs changes, and no licensed raw asset files tracked.

- [ ] **Step 10: Commit and push the verified slice**

```powershell
git add Game/Assets/CleanToContinue Game/ProjectSettings/EditorBuildSettings.asset docs submission
git commit -m "feat: complete mouse cleaning web vertical slice"
git push origin main
```

Do not commit `Game/Builds/Web`, `TestResults`, Unity `Library`, logs, or third-party source assets.
