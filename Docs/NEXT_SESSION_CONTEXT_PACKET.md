# Alien Crusher Handoff - 2026-05-04

## Current Progress Update
- MCP may still be unreliable, so the project now has a filesystem/Unity-batch validation path.
- ROUTE HOLD remains wired after LANE BREAK with HUD guidance, route beacon, route trail pips, result badges/advice, and lobby/meta recommendations.
- Scene readiness validation is no longer menu-only: it can run through Unity `-executeMethod` and writes `Logs/AlienCrusherSceneValidation.log`.
- Latest completed batch validation against `Assets/Scenes/SampleScene.unity` from 2026-05-02 passed with `0 error(s), 0 warning(s)`.
- 2026-05-04 follow-up: validation/repair now also covers the ROUTE HOLD HUD arrow scaffold (`HudRouteArrow` with child `ArrowText`). A fresh Unity batch run is still needed after the current editor/batch process issue is cleared.
- 2026-05-04 map follow-up: runtime stage start now rebuilds the managed city map instead of reusing the same static layout forever. Map bounds, lot grid, target markers, spawn position, camera clamp, density, and prop variety now scale across the early stage ramp.
- Runtime map generation now opens stage-gated landmark districts: Stage 2 pocket park, Stage 3 market plaza, Stage 5 construction yard, Stage 6 power block, and Stage 7 skyline block.
- Runtime map generation now emits a compact `[AlienCrusher][MapLayout]` console line with stage, theme, map size, grid size, destructible count, prop counts, landmark count, and target marker positions for quick playtest tuning.
- Runtime map generation also emits `[AlienCrusher][MapLayout][WARN]` if it detects low destructible density, sparse starter-lane objects, low landmark count, missing/off-lane spawn, missing target markers, out-of-bounds targets, or targets too close to spawn.
- In editor/development builds, map layout testing has hotkeys: `F6` previous stage, `F7` next stage, `F8` reset to Stage 1, `F9` toggle map layout overlay, and `F10` sweep Stage 1 through the debug max stage with a short visual pause per stage. These restart or rebuild stages for layout testing without advancing saved progression.
- Added `Tools/Alien Crusher/Audit Runtime Map Layout` plus a batch entry point that sweeps Stage 1 through the debug max stage and writes `Logs/AlienCrusherMapLayoutAudit.log`.
- Added `Tools/AuditRuntimeMapLayoutStatic.ps1` as a Unity-free fallback audit for Stage 1-7 map growth formulas, spawn/target bounds, landmark placement, and minimum-count thresholds.
- ROUTE HOLD trail pips now scale their visible count by route distance and hide at very close range, reducing small-screen visual noise while keeping longer routes readable.
- Added `Tools/AuditRouteHoldTuningStatic.ps1` as a Unity-free audit for ROUTE HOLD targets, pressure, deadlines, and distance-aware trail pip counts.
- `Tools/AuditRouteHoldTuningStatic.ps1` now reads its ROUTE HOLD, stage gate, boss stage, and stage timer defaults from the runtime C# fields before auditing, so tuning changes in `DummyFlowController`/`GameFlowSystem` do not silently drift from the audit.
- Added `Tools/RunStaticAudits.ps1` to run all Unity-free audits in one command and fail the process if any audit reports warnings.

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
- `Assets/Scripts/Editor/AlienCrusherMapLayoutAuditor.cs`
  - Editor/batch audit utility for regenerating Stage 1 through the debug max stage and recording `[AlienCrusher][MapLayout]` summaries/warnings into `Logs/AlienCrusherMapLayoutAudit.log`.
- `Assets/Scripts/Editor/AlienCrusherMapLayoutAuditor.cs.meta`
  - Unity meta file for the new map layout audit utility.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`
  - Stage-aware runtime map reset/rebuild flow. Managed map children are cleared safely, then regenerated with larger bounds, more varied lots/props, stage-gated landmark districts, and landmark-count validation. Emits `[AlienCrusher][MapLayout]` summary logs and `[AlienCrusher][MapLayout][WARN]` safety warnings. Also exposes an editor-only audit hook used by `AlienCrusherMapLayoutAuditor`.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`
  - Added editor/development hotkeys for fast map layout stage cycling: `F6`, `F7`, `F8`, `F9` overlay toggle, and `F10` automatic Stage 1-7 sweep with a short per-stage pause.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.UIFlow.cs`
  - ROUTE HOLD world trail now uses distance-aware active pip counts and smaller close-range pip scales.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`
  - Added `routeHoldTrailMinPipSpacing` and `routeHoldTrailCloseHideDistance` tuning fields.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`
  - Calls runtime map rebuild at stage start before destructible reset and encounter setup.
- `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`
  - Allows runtime map rebuilds to update camera clamp bounds.
- `Assets/Scripts/Editor/AlienCrusherSceneValidator.cs`
  - Validates the new ROUTE HOLD trail spacing/close-hide tuning fields.
- `Tools/AuditRuntimeMapLayoutStatic.ps1`
  - Unity-free map formula audit. It mirrors the runtime growth/landmark placement thresholds and writes `Logs/AlienCrusherMapLayoutStaticAudit.log`.
- `Tools/AuditRouteHoldTuningStatic.ps1`
  - Unity-free ROUTE HOLD tuning audit. It reads the relevant runtime C# default fields, mirrors stage target, route hold target, deadline, and trail active-pip formulas, then writes `Logs/AlienCrusherRouteHoldStaticAudit.log`.
- `Tools/RunStaticAudits.ps1`
  - Runs the map layout and ROUTE HOLD static audits together, using `-FailOnWarnings` for both.
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
- Unity batch validation still needs a fresh log-backed successful run after the map rebuild/landmark/audit changes. A 2026-05-04 validation attempt returned process code `0`, but the validation log files still did not update from 2026-05-02 and a titleless Unity process had to be cleared afterward. A first map audit batch attempt returned `-2147483645` and did not create audit logs.
- Unity-free static map audit passed on 2026-05-04 with `Result: 0 error(s), 0 warning(s)`. This does not replace in-editor/playmode validation, but it catches formula regressions while Unity batch is unstable.
- Unity-free ROUTE HOLD static audit passed on 2026-05-04 with `Result: 0 error(s), 0 warning(s)`. It now parses the current C# default tuning fields before verifying route targets, route pressure, and distance-aware trail pip counts across Stage 1-7.
- `Tools/RunStaticAudits.ps1` passed on 2026-05-04 with `Result: all static audits passed`.
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
4. Run the map layout audit:
   `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherMapLayoutAuditor.AuditRuntimeMapLayoutBatch -logFile D:\uni\spinball\Logs\AlienCrusherMapLayoutAuditEditor.log`
5. Inspect `Logs/AlienCrusherMapLayoutAudit.log`; any `WARN:` line should be treated as a placement bug before visual polish.
6. If Unity batch is still unstable, run the fallback audit:
   `powershell -ExecutionPolicy Bypass -File Tools/AuditRuntimeMapLayoutStatic.ps1`
7. Run the ROUTE HOLD fallback audit:
   `powershell -ExecutionPolicy Bypass -File Tools/AuditRouteHoldTuningStatic.ps1`
8. Or run all Unity-free audits at once:
   `powershell -ExecutionPolicy Bypass -File Tools/RunStaticAudits.ps1`
9. Run in-editor playtest from Stage 1 through at least Stage 7. Use `F10` for an automatic Stage 1-7 sweep with a short pause per stage, or `F7` to jump forward, `F6` to jump back, `F8` to reset to Stage 1, and `F9` to hide/show the map layout overlay. Watch the overlay and `[AlienCrusher][MapLayout]` logs to verify size/grid/destructible/prop/landmark counts climb as expected.
10. Verify the map grows from a compact residential starter layout into denser/wider districts with more cars, props, commercial objects, barrels, transformers, landmark districts, and wider ROUTE HOLD targets.
11. Verify LANE BREAK appears, HOLD beacon activates, route trail points to the active marker, ROUTE HOLD target/time is readable, and ROUTE HOLD reward fires once.
12. Tune `routeHoldWindowSeconds`, `routeHoldProgressThreshold`, `routeHoldTrailPipCount`, `routeHoldTrailMaxDistance`, `routeHoldTrailMinPipSpacing`, `routeHoldTrailCloseHideDistance`, and marker positions based on mobile readability.
13. If route pips are still too noisy, increase close-hide distance/min spacing or switch to fewer arrow-shaped pips.
14. After playtest stability, extract ROUTE HOLD / Stage Route logic out of `DummyFlowController` partials into a smaller dedicated runtime component or service.

## Next Session Paste Context Packet
```text
Project: D:\uni\spinball / Unity Alien Crusher / Unity 6000.3.8f1.
MCP may be unavailable; use filesystem, Unity batchmode, and logs first.
Latest completed work: ROUTE HOLD is wired after LANE BREAK. HUD shows route/hold guidance, route beacon, and distance-aware world-space trail pips toward Target_A/Target_B. Runtime map generation now resets/rebuilds the managed city layout on stage start using the current stage number, so stages grow from a compact starter district into wider, denser maps with more varied buildings, traffic props, commercial objects, barrels, transformers, stage-gated landmark districts, and wider target marker positions. Use `[AlienCrusher][MapLayout]` console logs, `Tools/Alien Crusher/Audit Runtime Map Layout`, and the map layout overlay to compare stage, theme, size, grid, destructible count, prop counts, landmark count, target positions, and warnings during playtest. In editor/development builds, use `F6`/`F7`/`F8` for quick stage cycling, `F9` to toggle the overlay, and `F10` to sweep Stage 1-7.
Latest validation: Unity batch validation completed successfully on 2026-05-02 with `Result: 0 error(s), 0 warning(s)`. A fresh 2026-05-04 validation batch attempt returned process code `0`, but the validation logs still did not update from 2026-05-02 and a titleless Unity process had to be cleared afterward. A first map audit batch attempt returned `-2147483645` and did not create audit logs. Unity-free static map audit, ROUTE HOLD static audit, and `Tools/RunStaticAudits.ps1` passed on 2026-05-04. The ROUTE HOLD static audit now reads its default tuning values from the runtime C# fields before running, but the map rebuild/landmark/audit/route-hold trail changes still need an in-editor compile/playmode validation pass.
Changed files: `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`, `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`, plus editor validation/repair files from the ROUTE HOLD arrow pass and this handoff doc.
Useful validation command: `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherSceneValidator.ValidateCurrentSceneBatch -logFile D:\uni\spinball\Logs\AlienCrusherBatchValidationEditor.log`
Useful map audit command: `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherMapLayoutAuditor.AuditRuntimeMapLayoutBatch -logFile D:\uni\spinball\Logs\AlienCrusherMapLayoutAuditEditor.log`
Useful static fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditRuntimeMapLayoutStatic.ps1`
Useful ROUTE HOLD fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditRouteHoldTuningStatic.ps1`
Useful combined fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/RunStaticAudits.ps1`
Next priority: rerun repair/validation, map layout audit, and combined static audits, confirm `HudRouteArrow/ArrowText` is present, then do a real in-editor/mobile playtest from Stage 1 through Stage 7. Confirm map growth, object variety, landmark district placement, LANE BREAK -> ROUTE HOLD readability, trail/beacon clarity, target distance, timer pressure, and that route reward fires once. Then tune route hold values. If stable, extract ROUTE HOLD/stage route code out of `DummyFlowController`.
Known risks: MCP unreliable; no hands-on playmode/mobile pass yet; route pips may be visually noisy; `DummyFlowController` remains an architecture risk; Unity editor shutdown logs a non-blocking temp allocator warning.
```
