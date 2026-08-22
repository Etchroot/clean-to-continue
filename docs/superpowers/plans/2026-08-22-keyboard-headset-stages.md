# Keyboard and Headset Stages Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reuse the finished mouse-stage loop to build keyboard and multi-part headset stages, then connect Mouse → Keyboard → Headset → Ending with vertically stacked next-stage and main-menu buttons.

**Architecture:** A generic equipment-stage bootstrap and editor assembler will accept stage-specific prefab, memory text, and next-scene data. Every supported MeshRenderer gets independent dust and polish overlays/masks, while UI progress aggregates all sources belonging to the same tool.

**Tech Stack:** Unity 6, C#, URP, Unity UI, Input System, Unity Test Framework, Editor scene builder.

**Spec:** `docs/superpowers/specs/2026-08-22-keyboard-headset-stages-design.md`

## Global Constraints

- Browser-ready Unity Web build remains the final target.
- Use only AirGun and Cloth; do not restore CottonSwab gameplay.
- Preserve original equipment materials and user-owned scene objects.
- Complete automatically at 90% and force all visual masks clean on completion.
- Keep left click clean, right click rotate, and Space highlight.
- Prioritize a working three-stage flow before visual polish.

---

### Task 1: Dual-action memory panel

**Files:**
- Modify: `Game/Assets/CleanToContinue/Runtime/UI/MemoryPanelView.cs`
- Modify: `Game/Assets/CleanToContinue/Runtime/Flow/MouseStageBootstrap.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/StageControllerTests.cs`

**Interfaces:**
- Produces: `MemoryPanelView.Configure(..., Button nextButton, Button mainMenuButton, string nextScene, Sprite memorySprite, string memoryText)`
- Produces: `MemoryPanelView.NextSceneName` and `MemoryPanelView.MainMenuSceneName` read-only properties for scene-contract tests.
- Preserves: `OpenMouseMemory()` as the StageController completion entry point.

- [ ] **Step 1: Write the failing test**

Add a PlayMode test that configures two real Buttons and asserts their vertical order and configured targets:

```csharp
Assert.That(nextButton.GetComponent<RectTransform>().anchoredPosition.y,
    Is.GreaterThan(menuButton.GetComponent<RectTransform>().anchoredPosition.y));
Assert.That(view.NextSceneName, Is.EqualTo("04.Keyboard"));
Assert.That(view.MainMenuSceneName, Is.EqualTo("01.MainMenu"));
```

- [ ] **Step 2: Run PlayMode tests and verify RED**

Run: `Tools > Clean to Continue > Run PlayMode Tests`

Expected: compile/test failure because the dual-button Configure contract and target properties do not exist.

- [ ] **Step 3: Implement the minimal dual-button contract**

Store both buttons and scene names, bind `ContinueToNextStage` and `ReturnToMainMenu`, remove both listeners in `Configure` and `OnDestroy`, and keep `OpenMouseMemory()` responsible only for presenting the configured image/text.

- [ ] **Step 4: Run PlayMode tests and verify GREEN**

Expected: the new memory-panel test and all existing StageController tests pass.

### Task 2: Aggregate progress for multi-mesh equipment

**Files:**
- Modify: `Game/Assets/CleanToContinue/Runtime/UI/ToolSelectorView.cs`
- Modify: `Game/Assets/CleanToContinue/Runtime/Stage/StageController.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/ToolSelectorViewTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/ToolSelectorViewTests.cs.meta`

**Interfaces:**
- Produces: `ToolSelectorView.RenderAllProgress()` as a public method.
- Behavior: for every button tool, display `Average(source.Progress01)` across all non-null sources with that tool.

- [ ] **Step 1: Write the failing aggregation test**

Configure sources with AirGun values `0.25` and `0.75` plus Cloth value `0.2`, then assert:

```csharp
Assert.That(airFill.fillAmount, Is.EqualTo(0.5f).Within(0.001f));
Assert.That(clothFill.fillAmount, Is.EqualTo(0.2f).Within(0.001f));
```

- [ ] **Step 2: Run PlayMode tests and verify RED**

Expected: AirGun shows the last source value `0.75`, not the average `0.5`.

- [ ] **Step 3: Implement grouped averages**

Change `RenderAllProgress` to group `progressSources` by `CleaningTool`, calculate the mean, and call `RenderProgress` once per tool. Change `StageController.RenderProgressViews` to call this public summary method instead of writing each source sequentially.

- [ ] **Step 4: Run PlayMode tests and verify GREEN**

Expected: aggregation test and existing selector/controller tests pass.

### Task 3: Generic equipment bootstrap

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Flow/EquipmentStageBootstrap.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Flow/EquipmentStageBootstrap.cs.meta`
- Modify: `Game/Assets/CleanToContinue/Editor/VerticalSliceSceneBuilder.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/VerticalSliceSceneTests.cs`

**Interfaces:**
- Produces: `EquipmentStageBootstrap.Configure(..., string memoryText, string nextSceneName)` using the existing StageController/input/audio/UI dependencies.
- Consumes: arrays of `SurfaceMaskLayer`, two memory buttons, and the configured stage memory text.

- [ ] **Step 1: Write failing scene-contract tests**

For `03.Mouse`, `04.Keyboard`, and `05.Headset`, load each scene and assert one bootstrap, two tool categories, two memory buttons, the exact memory line, and next-scene name.

- [ ] **Step 2: Run PlayMode tests and verify RED**

Expected: Keyboard and Headset lack the generic bootstrap and completion controls.

- [ ] **Step 3: Implement generic bootstrap**

Move the wiring logic from `MouseStageBootstrap.Awake` into `EquipmentStageBootstrap`. Configure highlight, interaction, input, progress, tool selector, cursor, memory panel, StageController and audio from serialized references. Retain the method name `OpenMouseMemory()` only as the existing StageController callback; its content is stage-configured.

- [ ] **Step 4: Rebuild StageRoot prefab and scenes**

The editor builder creates StageRoot with `EquipmentStageBootstrap`, replacing the generated legacy mouse bootstrap. Do not delete user-owned root objects.

- [ ] **Step 5: Run PlayMode tests and verify GREEN**

Expected: Mouse behavior remains green and new bootstrap contract checks pass for generated scenes.

### Task 4: Multi-renderer equipment assembler

**Files:**
- Modify: `Game/Assets/CleanToContinue/Editor/VerticalSliceSceneBuilder.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/VerticalSliceSceneTests.cs`

**Interfaces:**
- Produces: stage definitions for Mouse, Keyboard, and Headset containing scene path, prefab path, authoring root name, playable name, memory text, next scene, scale multiplier and camera framing multiplier.
- Produces: one AirGun and one Cloth `SurfaceMaskLayer` per supported renderer.

- [ ] **Step 1: Write failing equipment structure tests**

For Keyboard and Headset, assert:

```csharp
Assert.That(playable.GetComponentsInChildren<SurfaceMaskLayer>(true)
    .Select(layer => layer.Tool).Distinct(),
    Is.EquivalentTo(new[] { CleaningTool.AirGun, CleaningTool.Cloth }));
Assert.That(playable.GetComponentsInChildren<MeshCollider>(true)
    .Count(collider => collider.enabled), Is.GreaterThanOrEqualTo(cleanableRendererCount));
```

Also verify every cleanable renderer has `DustOverlay` and `PolishOverlay` children and its original shared materials remain unchanged.

- [ ] **Step 2: Run PlayMode tests and verify RED**

Expected: `04.Keyboard` and `05.Headset` still contain only placeholder scene guides.

- [ ] **Step 3: Generalize renderer configuration**

Enumerate active `MeshRenderer` components with non-null `MeshFilter.sharedMesh`. For each renderer:

1. Keep the original renderer enabled and materials unchanged.
2. Create child overlay renderers using the same mesh.
3. Copy the original base texture/color into the polish overlay material instance through `SurfaceMaterialTransfer`.
4. Add an enabled MeshCollider using the same mesh to the renderer GameObject; disable broader BoxCollider/MeshCollider components that would intercept UV hits.
5. Add AirGun and Cloth SurfaceMaskLayer instances to the playable equipment root and configure each with its own overlay.

Throw `InvalidOperationException` with prefab path when no supported renderer exists.

- [ ] **Step 4: Generalize bounds-based placement**

Center the equipment around its pivot, calculate `rotationRadius = combinedBounds.extents.magnitude`, place pivot at `deskBounds.max.y + rotationRadius + margin`, and set the camera from the combined bounds using stage-definition framing multipliers.

- [ ] **Step 5: Copy missing user environment roots**

Before assembling Keyboard/Headset, copy `Desk` and `Wall` from `03.Mouse` only when the target scene lacks the same root name. Leave existing target roots untouched.

- [ ] **Step 6: Build all three equipment scenes**

Use:

```text
03.Mouse    Assets/ThirdParty/Mouse.prefab
04.Keyboard Assets/ThirdParty/Keyboard.prefab
05.Headset  Assets/ThirdParty/Headset Type1.prefab
```

Create the same rounded progress, tools and instruction panels. Create memory buttons at positive and negative Y positions so `NextStageButton` is visibly above `MainMenuButton`.

- [ ] **Step 7: Run PlayMode tests and verify GREEN**

Expected: all three scene-structure, material, collider, UI, and flow tests pass.

### Task 5: Unity integration verification

**Files:**
- Verify: `Game/Assets/CleanToContinue/Scenes/03.Mouse.unity`
- Verify: `Game/Assets/CleanToContinue/Scenes/04.Keyboard.unity`
- Verify: `Game/Assets/CleanToContinue/Scenes/05.Headset.unity`
- Verify: `Game/ProjectSettings/EditorBuildSettings.asset`

**Interfaces:**
- Consumes the three generated stages and build order.
- Produces a playable Mouse → Keyboard → Headset → Ending flow.

- [ ] **Step 1: Run the scene builder once**

Run: `Clean to Continue > Build Vertical Slice Scenes`

Expected console marker names all three equipment scenes and reports no unsupported renderer.

- [ ] **Step 2: Run full EditMode tests**

Run: `Tools > Clean to Continue > Run EditMode Tests`

Expected: zero failures.

- [ ] **Step 3: Run full PlayMode tests with Game View focused**

Run: `Tools > Clean to Continue > Run PlayMode Tests`

Expected: zero failures.

- [ ] **Step 4: Manually play the complete flow**

Verify Keyboard and Headset can be rotated without Desk penetration; all visible parts respond to AirGun and Cloth; Space highlights remaining dirt; completion opens the correct memory; next-stage and main-menu buttons load their declared scenes.

- [ ] **Step 5: Record only confirmed follow-up polish**

After the user reports actual play results, adjust stage-specific scale, camera distance, brush radius and light intensity. Defer unrelated visual polish until the complete flow is working.
