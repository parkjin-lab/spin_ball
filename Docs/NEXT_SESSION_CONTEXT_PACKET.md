# Alien Crusher Handoff - 2026-05-04

## Current Progress Update
- MCP may still be unreliable, so the project now has a filesystem/Unity-batch validation path.
- ROUTE HOLD remains wired after LANE BREAK with HUD guidance, route beacon, route trail pips, result badges/advice, and lobby/meta recommendations.
- Scene readiness validation is no longer menu-only: it can run through Unity `-executeMethod` and writes `Logs/AlienCrusherSceneValidation.log`.
- Latest completed batch validation against `Assets/Scenes/SampleScene.unity` from 2026-05-02 passed with `0 error(s), 0 warning(s)`.
- 2026-05-04 follow-up: validation/repair now also covers the ROUTE HOLD HUD arrow scaffold (`HudRouteArrow` with child `ArrowText`). A fresh Unity batch run is still needed after the current editor/batch process issue is cleared.
- 2026-05-04 map follow-up: runtime stage start now rebuilds the managed city map instead of reusing the same static layout forever. Map bounds, lot grid, target markers, spawn position, camera clamp, density, and prop variety now scale across the early stage ramp.
- Runtime map generation now emits a compact `[AlienCrusher][MapLayout]` console line with stage, theme, map size, grid size, destructible count, prop counts, and target marker positions for quick playtest tuning.
- Runtime map generation also emits `[AlienCrusher][MapLayout][WARN]` if it detects low destructible density, sparse starter-lane objects, missing/off-lane spawn, missing target markers, out-of-bounds targets, or targets too close to spawn.
- In editor/development builds, map layout testing has hotkeys: `F6` previous stage, `F7` next stage, `F8` reset to Stage 1, `F9` toggle map layout overlay, and `F10` sweep Stage 1 through the debug max stage with a short visual pause per stage. These restart or rebuild stages for layout testing without advancing saved progression.

## Work Completed Immediately Before This Handoff
- Extended `Tools/Alien Crusher/Validate Current Scene` so it also writes a validation report file.
- Added batch entry point:
  - `AlienCrusher.EditorTools.AlienCrusherSceneValidator.ValidateCurrentSceneBatch`
- Added `Tools/Alien Crusher/Repair Scene Essentials` plus batch entry point:
  - `AlienCrusher.EditorTools.AlienCrusherSceneRepair.RepairCurrentSceneEssentialsBatch`
- Repaired `SampleScene.unity` by adding the missing `HudRouteIndicatorText` under `HUD_Dummy`.
- Re-ran Unity batch validation after repair; all checked core systems, route markers, ROUTE HOLD tuning fields, and HUD text bindings pass.

## Changed Files
- `Assets/Scripts/Editor/AlienCrusherSceneValidator.cs`
  - Added batch validation support, default scene loading, report-file output, batch exit codes, and ROUTE HOLD HUD arrow checks.
- `Assets/Scripts/Editor/AlienCrusherSceneRepair.cs`
  - Targeted editor repair utility for scene essentials, currently ensuring the ROUTE HOLD HUD route indicator and route arrow scaffold exist.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`
  - Stage-aware runtime map reset/rebuild flow. Managed map children are cleared safely, then regenerated with larger bounds and more varied lots/props by stage. Emits `[AlienCrusher][MapLayout]` summary logs and `[AlienCrusher][MapLayout][WARN]` safety warnings.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`
  - Added editor/development hotkeys for fast map layout stage cycling: `F6`, `F7`, `F8`, `F9` overlay toggle, and `F10` automatic Stage 1-7 sweep with a short per-stage pause.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`
  - Calls runtime map rebuild at stage start before destructible reset and encounter setup.
- `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`
  - Allows runtime map rebuilds to update camera clamp bounds.
- `Assets/Scripts/Editor/AlienCrusherSceneRepair.cs.meta`
  - Unity-generated meta file for the new editor script.
- `Assets/Scenes/SampleScene.unity`
  - Added `HudRouteIndicatorText` with `UnityEngine.UI.Text` binding under `HUD_Dummy`.
- `Logs/AlienCrusherSceneValidation.log`
  - Latest validation report: `0 error(s), 0 warning(s)`.
- `Logs/AlienCrusherBatchValidationEditor.log`
  - Unity batch validation log.
- `Logs/AlienCrusherBatchRepairEditor.log`
  - Unity batch repair log.

## Current Unresolved Issues
- MCP connection is still assumed unreliable; continue using Unity batch commands and log files first.
- A titleless Unity process was observed during the 2026-05-04 follow-up; batch repair/validation did not refresh the 2026-05-02 logs. Clear the stale editor process before relying on fresh batch results.
- Unity batch validation still needs a fresh successful run after the map rebuild change; the latest attempts did not update the validation logs, and the direct waited Unity process returned `-2147483645`.
- Playmode/mobile behavior still needs hands-on verification: route trail visibility, beacon distance readability, target count, reward timing, and reward single-trigger behavior.
- Trail pips are runtime primitives; verify they are not visually noisy on small Android screens.
- `DummyFlowController` remains a high-risk mega-controller split across many partials. Extracting ROUTE HOLD / stage route logic is still recommended after behavior is stable.
- Unity batch logs include a non-blocking temp allocator leak warning during editor shutdown; validation itself completed successfully.

## Recommended Next Session Work
1. Start with batch validation:
   `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherSceneValidator.ValidateCurrentSceneBatch -logFile D:\uni\spinball\Logs\AlienCrusherBatchValidationEditor.log`
2. If validation reports a missing scene essential, run:
   `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherSceneRepair.RepairCurrentSceneEssentialsBatch -logFile D:\uni\spinball\Logs\AlienCrusherBatchRepairEditor.log`
3. Confirm `HudRouteArrow/ArrowText` exists under `HUD_Dummy` after repair, then rerun validation.
4. Run in-editor playtest from Stage 1 through at least Stage 7. Use `F10` for an automatic Stage 1-7 sweep with a short pause per stage, or `F7` to jump forward, `F6` to jump back, `F8` to reset to Stage 1, and `F9` to hide/show the map layout overlay. Watch the overlay and `[AlienCrusher][MapLayout]` logs to verify size/grid/destructible/prop counts climb as expected. Treat any warning text or `[AlienCrusher][MapLayout][WARN]` line as a placement bug to tune before visual polish.
5. Verify the map grows from a compact residential starter layout into denser/wider districts with more cars, props, commercial objects, barrels, transformers, and wider ROUTE HOLD targets.
6. Verify LANE BREAK appears, HOLD beacon activates, route trail points to the active marker, ROUTE HOLD target/time is readable, and ROUTE HOLD reward fires once.
7. Tune `routeHoldWindowSeconds`, `routeHoldProgressThreshold`, `routeHoldTrailPipCount`, `routeHoldTrailMaxDistance`, and marker positions based on mobile readability.
8. If route pips are too noisy, reduce pip count/opacity or switch to fewer arrow-shaped pips.
9. After playtest stability, extract ROUTE HOLD / Stage Route logic out of `DummyFlowController` partials into a smaller dedicated runtime component or service.

## Next Session Paste Context Packet
```text
Project: D:\uni\spinball / Unity Alien Crusher / Unity 6000.3.8f1.
MCP may be unavailable; use filesystem, Unity batchmode, and logs first.
Latest completed work: ROUTE HOLD is wired after LANE BREAK. HUD shows route/hold guidance, route beacon, and world-space trail pips toward Target_A/Target_B. Runtime map generation now resets/rebuilds the managed city layout on stage start using the current stage number, so stages grow from a compact starter district into wider, denser maps with more varied buildings, traffic props, commercial objects, barrels, transformers, and wider target marker positions. Use `[AlienCrusher][MapLayout]` console logs and the map layout overlay to compare stage, theme, size, grid, destructible count, prop counts, target positions, and warnings during playtest. In editor/development builds, use `F6`/`F7`/`F8` for quick stage cycling, `F9` to toggle the overlay, and `F10` to sweep Stage 1-7.
Latest validation: Unity batch validation completed successfully on 2026-05-02 with `Result: 0 error(s), 0 warning(s)`. Fresh 2026-05-04 batch attempts did not update logs, and the waited Unity process returned `-2147483645`, so the map rebuild change still needs an in-editor compile/playmode validation pass.
Changed files: `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`, `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`, plus editor validation/repair files from the ROUTE HOLD arrow pass and this handoff doc.
Useful validation command: `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherSceneValidator.ValidateCurrentSceneBatch -logFile D:\uni\spinball\Logs\AlienCrusherBatchValidationEditor.log`
Next priority: rerun repair/validation, confirm `HudRouteArrow/ArrowText` is present, then do a real in-editor/mobile playtest from Stage 1 through Stage 7. Confirm map growth, object variety, LANE BREAK -> ROUTE HOLD readability, trail/beacon clarity, target distance, timer pressure, and that route reward fires once. Then tune route hold values. If stable, extract ROUTE HOLD/stage route code out of `DummyFlowController`.
Known risks: MCP unreliable; no hands-on playmode/mobile pass yet; route pips may be visually noisy; `DummyFlowController` remains an architecture risk; Unity editor shutdown logs a non-blocking temp allocator warning.
```
