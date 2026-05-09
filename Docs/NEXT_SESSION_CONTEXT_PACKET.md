# Alien Crusher Handoff - 2026-05-10

## Current Progress Update
- MCP may still be unreliable, so the project now has a filesystem/Unity-batch validation path.
- ROUTE HOLD remains wired after LANE BREAK with a `LANE BREAK -> ROUTE OPEN` feedback beat, HUD guidance, route beacon, route trail pips, result badges/advice, and lobby/meta recommendations.
- Scene readiness validation is no longer menu-only: it can run through Unity `-executeMethod` and writes `Logs/AlienCrusherSceneValidation.log`.
- Latest completed batch validation against `Assets/Scenes/SampleScene.unity` from 2026-05-02 passed with `0 error(s), 0 warning(s)`.
- 2026-05-04 follow-up: validation/repair now also covers the ROUTE HOLD HUD arrow scaffold (`HudRouteArrow` with child `ArrowText`).
- 2026-05-04 map follow-up: runtime stage start now rebuilds the managed city map instead of reusing the same static layout forever. Map bounds, lot grid, target markers, spawn position, camera clamp, density, and prop variety now scale across the early stage ramp.
- Runtime map generation now opens stage-gated landmark districts: Stage 2 pocket park, Stage 3 market plaza, Stage 5 construction yard, Stage 6 power block, and Stage 7 skyline block.
- Runtime map generation now emits a compact `[AlienCrusher][MapLayout]` console line with stage, theme, map size, grid size, destructible count, prop counts, landmark count, and target marker positions for quick playtest tuning.
- Runtime map generation also emits `[AlienCrusher][MapLayout][WARN]` if it detects low destructible density, sparse starter-lane objects, low landmark count, missing/off-lane spawn, missing target markers, out-of-bounds targets, or targets too close to spawn.
- In editor/development builds, map layout testing has hotkeys: `F6` previous stage, `F7` next stage, `F8` reset to Stage 1, `F9` toggle map layout overlay, and `F10` sweep Stage 1 through the debug max stage with a short visual pause per stage. These restart or rebuild stages for layout testing without advancing saved progression.
- Added `Tools/Alien Crusher/Audit Runtime Map Layout` plus a batch entry point that sweeps Stage 1 through the debug max stage and writes `Logs/AlienCrusherMapLayoutAudit.log`.
- Added `Tools/AuditRuntimeMapLayoutStatic.ps1` as a Unity-free fallback audit for Stage 1-7 map growth formulas, spawn/target bounds, landmark placement, and minimum-count thresholds.
- ROUTE HOLD trail pips now scale their visible count by route distance and hide at very close range, reducing small-screen visual noise while keeping longer routes readable.
- LANE BREAK now starts a short route open beat: the announcement says `LANE BREAK -> ROUTE OPEN`, the HUD route indicator briefly says `OPEN`, and the active route marker/arrow/trail pulse harder for `routeOpenBeatSeconds`.
- ROUTE BONUS now opens a clearer Forward Smash payoff: copy says `ROUTE BONUS -> CLUSTER OPEN`, HUD route arrow switches to `SMASH`, and extra barrels/transformers spawn around the highlighted target.
- ROUTE HOLD progress now has a faster-read HUD state: progress text shows percent/remaining/countdown, the stage goal gauge temporarily becomes a ROUTE HOLD meter, and the route indicator shows `HOLD xx%`.
- Route reward clusters now vary by district: park rewards stay lower clutter, market routes add kiosk/vending chains, construction routes lean into barrels, power routes lean into transformers, and skyline routes add a high-value anchor.
- Failure result and lobby recommendation copy now start with one next-run first action for `OPENING FAILED`, `ROUTE HOLD MISSED`, `MID-RUN DRIFT`, `FINAL PUSH FAILED`, and `BOSS PHASE`.
- Editor/development playtests now emit `[AlienCrusher][Playtest]` console lines for `SWEEP_START`, `STAGE_START`, `ROUTE_OPEN`, `ROUTE_HOLD_CLEAR`, `ROUTE_BONUS`, `FORWARD_SMASH`, `STAGE_END`, and `SWEEP_END`, and append the same lines to `Logs/AlienCrusherPlaytestTelemetry.log`.
- `Tools/GeneratePlaytestTelemetrySummary.ps1` now parses that telemetry log into `Logs/AlienCrusherPlaytestTelemetrySummary.md`, grouping runs under sweep-level summaries when `F10` is used and adding a current tuning snapshot, stage trend, tuning candidate, first-pass experiment, and failure bucket action rollup.
- Rhythm is now an explicit design lens: playtests should judge opener -> pivot -> sustain -> payoff -> climax cadence, and neighboring stages should differ by rhythm problem rather than size alone.
- Added `Tools/AuditRouteHoldTuningStatic.ps1` as a Unity-free audit for ROUTE HOLD targets, pressure, deadlines, and distance-aware trail pip counts.
- `Tools/AuditRouteHoldTuningStatic.ps1` now reads its ROUTE HOLD, route open beat, route reward cluster, stage gate, boss stage, and stage timer defaults from the runtime C# fields before auditing, so tuning changes in `DummyFlowController`/`GameFlowSystem` do not silently drift from the audit.
- Added `Tools/RunStaticAudits.ps1` to run all Unity-free audits in one command and fail the process if any audit reports warnings.
- Added `Tools/InvokeUnityBatch.ps1` and `Tools/RunUnityBatchChecks.ps1` to make Unity batch validation less ambiguous. The wrapper uses batch/nographics mode, detects stale `Temp/UnityLockfile`, captures stdout/stderr, enforces a timeout, and fails if the expected report file timestamp does not advance.
- Added `Docs/GAME_UPDATE_ROADMAP.md` and updated the GDD core loop section to reflect the current LANE BREAK -> ROUTE OPEN -> ROUTE HOLD -> ROUTE BONUS / Forward Smash direction.
- Added `HudRouteArrow/ArrowText` to `SampleScene.unity` and `Tools/AuditSceneEssentialsStatic.ps1` to catch missing route HUD essentials without relying on Unity batch.
- 2026-05-05 validation follow-up: `Tools/RunUnityBatchChecks.ps1` passed. Scene validation and runtime map layout audit refreshed their report/log timestamps at 20:52 and 20:53 and ended with `0 error(s), 0 warning(s)`.
- Added `Tools/GenerateStagePlaytestChecklist.ps1` to generate `Logs/AlienCrusherStagePlaytestChecklist.md` before the Stage 1-7 hands-on pass.

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
  - ROUTE HOLD world trail now uses distance-aware active pip counts and smaller close-range pip scales. The route open beat also pulses the route marker, HUD indicator, arrow, and trail. ROUTE HOLD progress now drives HUD progress text, stage goal gauge fill/text, and `HOLD xx%` route indicator copy.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.ProgressionCore.cs`
  - LANE BREAK now starts `LANE BREAK -> ROUTE OPEN`, sets the route open beat timer, and resets that runtime timer at stage setup. ROUTE BONUS now spawns a district-flavored Forward Smash reward cluster around the highlighted target. Route milestones now also emit standardized playtest console telemetry.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`
  - Added `routeHoldTrailMinPipSpacing`, `routeHoldTrailCloseHideDistance`, `routeOpenBeatSeconds`, `routeRewardClusterRadius`, and `routeRewardClusterPropCount` tuning fields. Also stores the debug toggle for playtest telemetry logging.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.PlaytestTelemetry.cs`
  - Centralizes `[AlienCrusher][Playtest]` output for stage start/end and route milestone events so manual sweeps leave structured breadcrumbs in the Console, editor log, and `Logs/AlienCrusherPlaytestTelemetry.log`.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`
  - Calls runtime map rebuild at stage start before destructible reset and encounter setup. Result failure copy now starts with a bucket-specific first action and follows with a compact why line. Stage start/end now emit playtest telemetry summaries.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.MetaProgression.cs`
  - Lobby recommendations now reuse the last-run first action line before showing the meta upgrade recommendation and reason.
- `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`
  - Allows runtime map rebuilds to update camera clamp bounds.
- `Assets/Scripts/Editor/AlienCrusherSceneValidator.cs`
  - Validates the new ROUTE HOLD trail spacing/close-hide tuning fields.
- `Docs/GDD_ALIEN_CRUSHER.md`
  - Adds a current implementation note that the prototype's core loop has evolved into starter-lane crush, LANE BREAK, ROUTE HOLD, route reward, and result-driven growth.
- `Docs/GAME_UPDATE_ROADMAP.md`
  - Tracks current project status, immediate work, core loop fun improvements, future update milestones, and open risks.
- `Tools/AuditRuntimeMapLayoutStatic.ps1`
  - Unity-free map formula audit. It mirrors the runtime growth/landmark placement thresholds and writes `Logs/AlienCrusherMapLayoutStaticAudit.log`.
- `Tools/AuditRouteHoldTuningStatic.ps1`
  - Unity-free ROUTE HOLD tuning audit. It reads the relevant runtime C# default fields, mirrors stage target, route hold target, deadline, route open beat range, route reward cluster range, and trail active-pip formulas, then writes `Logs/AlienCrusherRouteHoldStaticAudit.log`.
- `Tools/RunStaticAudits.ps1`
  - Runs the map layout and ROUTE HOLD static audits together, using `-FailOnWarnings` for both.
- `Tools/InvokeUnityBatch.ps1`
  - Safer Unity batch wrapper. It runs one `-executeMethod`, writes an editor log, watches the expected report file, detects stale Unity lock files, and treats missing/stale reports as failures even if Unity exits with code `0`.
- `Tools/RunUnityBatchChecks.ps1`
  - Runs the scene validation batch and runtime map layout audit batch through `InvokeUnityBatch.ps1`.
- `Assets/Scripts/Editor/AlienCrusherSceneRepair.cs.meta`
  - Unity-generated meta file for the new editor script.
- `Assets/Scenes/SampleScene.unity`
  - Added `HudRouteIndicatorText` and `HudRouteArrow/ArrowText` with `UnityEngine.UI.Text` bindings under `HUD_Dummy`.
- `Tools/AuditSceneEssentialsStatic.ps1`
  - Unity-free scene essential audit for SampleScene object presence, Text bindings, and HudRouteArrow parent/child wiring.
- `Tools/RunStaticAudits.ps1`
  - Now also runs the scene essentials static audit before map layout and ROUTE HOLD audits.
- `Tools/GenerateStagePlaytestChecklist.ps1`
  - Generates a Stage 1-7 playtest checklist with validation status, map/grid growth, landmarks, ROUTE HOLD targets, route open beat timing, district reward identity, reward cluster expectations, route progress readability, failure-advice checks, route pressure, target distances, and observation prompts.
- `Logs/AlienCrusherSceneValidation.log`
  - Current validation report from 2026-05-05 20:52: `0 error(s), 0 warning(s)`.
- `Logs/AlienCrusherBatchValidationEditor.log`
  - Unity batch validation log refreshed on 2026-05-05 20:53.
- `Logs/AlienCrusherMapLayoutAudit.log`
  - Current runtime map layout audit report from 2026-05-05 20:53: Stage 1-7 `0 error(s), 0 warning(s)`.
- `Logs/AlienCrusherMapLayoutAuditEditor.log`
  - Unity batch map layout audit editor log refreshed on 2026-05-05 20:53.
- `Logs/AlienCrusherBatchRepairEditor.log`
  - Unity batch repair log.

## Current Unresolved Issues
- MCP connection is still assumed unreliable; continue using Unity batch commands and log files first.
- A titleless Unity process was observed during the 2026-05-04 follow-up; batch repair/validation did not refresh the 2026-05-02 logs. Clear the stale editor process before relying on fresh batch results.
- Unity batch validation is green again as of 2026-05-05, but previous 2026-05-04 attempts returned stale logs, a project-open lock fatal error, and one timeout. Keep using the wrapper so stale logs are caught.
- 2026-05-04 batch follow-up: direct Unity invocation surfaced `Aborting batchmode due to fatal error: It looks like another Unity instance is running with this project open.` A later wrapper run reached a real Unity process but timed out after 900 seconds without creating the editor log or refreshing the validation report. No Unity Editor process or `Temp/UnityLockfile` remained afterward. The 2026-05-05 wrapper run passed, but keep using `Tools/RunUnityBatchChecks.ps1` so stale-lock and stale-report failures remain explicit.
- Unity-free scene essentials static audit passed after the `HudRouteArrow/ArrowText` scene source fix with `Result: 0 error(s), 0 warning(s)`.
- Unity-free static map audit passed on 2026-05-05 with `Result: 0 error(s), 0 warning(s)`. This does not replace in-editor/playmode validation, but it catches formula regressions while Unity batch is unstable.
- Unity-free ROUTE HOLD static audit passed on 2026-05-05 with `Result: 0 error(s), 0 warning(s)`. It now parses the current C# default tuning fields before verifying route targets, route open beat timing, route pressure, and distance-aware trail pip counts across Stage 1-7.
- `Tools/RunStaticAudits.ps1` passed on 2026-05-05 with `Result: all static audits passed`.
- Playmode/mobile behavior still needs hands-on verification: route trail visibility, beacon distance readability, target count, reward timing, and reward single-trigger behavior.
- Trail pips are runtime primitives; verify they are not visually noisy on small Android screens.
- `DummyFlowController` remains a high-risk mega-controller split across many partials. Extracting ROUTE HOLD / stage route logic is still recommended after behavior is stable.
- Unity batch logs include a non-blocking temp allocator leak warning during editor shutdown; validation itself completed successfully.

## Recommended Next Session Work
1. Start with the safer Unity batch wrapper:
   `powershell -ExecutionPolicy Bypass -File Tools/RunUnityBatchChecks.ps1`
2. If it reports a stale Unity lock after confirming the project is not open in Unity, rerun:
   `powershell -ExecutionPolicy Bypass -File Tools/RunUnityBatchChecks.ps1 -ClearStaleUnityLock`
3. If validation reports any missing scene essential, run:
   `D:\Unity\6000.3.8f1\Editor\Unity.exe -batchmode -quit -projectPath D:\uni\spinball -executeMethod AlienCrusher.EditorTools.AlienCrusherSceneRepair.RepairCurrentSceneEssentialsBatch -logFile D:\uni\spinball\Logs\AlienCrusherBatchRepairEditor.log`
4. Confirm `HudRouteArrow/ArrowText` exists under `HUD_Dummy` after repair, then rerun validation.
5. Inspect `Logs/AlienCrusherMapLayoutAudit.log`; any `WARN:` line should be treated as a placement bug before visual polish.
6. If Unity batch is still unstable, run the fallback audit:
   `powershell -ExecutionPolicy Bypass -File Tools/AuditRuntimeMapLayoutStatic.ps1`
7. Run the ROUTE HOLD fallback audit:
   `powershell -ExecutionPolicy Bypass -File Tools/AuditRouteHoldTuningStatic.ps1`
8. Or run all Unity-free audits at once:
   `powershell -ExecutionPolicy Bypass -File Tools/RunStaticAudits.ps1`
9. Run in-editor playtest from Stage 1 through at least Stage 7. Use `F10` for an automatic Stage 1-7 sweep with a short pause per stage, or `F7` to jump forward, `F6` to jump back, `F8` to reset to Stage 1, and `F9` to hide/show the map layout overlay. Watch the overlay and `[AlienCrusher][MapLayout]` logs to verify size/grid/destructible/prop/landmark counts climb as expected.
10. Verify the map grows from a compact residential starter layout into denser/wider districts with more cars, props, commercial objects, barrels, transformers, landmark districts, and wider ROUTE HOLD targets.
11. Verify each run has a readable opener, pivot, sustain, payoff, and late squeeze or climax; Stage 2/3/5/6/7 should change the rhythm problem, not only the size or route distance.
12. Verify LANE BREAK appears, ROUTE OPEN beat is readable, HOLD beacon activates, route trail points to the active marker, ROUTE HOLD meter progresses clearly, and ROUTE HOLD reward opens the expected district SMASH target cluster once.
13. Tune `routeHoldWindowSeconds`, `routeHoldProgressThreshold`, `routeOpenBeatSeconds`, `routeRewardClusterRadius`, `routeRewardClusterPropCount`, `routeHoldTrailPipCount`, `routeHoldTrailMaxDistance`, `routeHoldTrailMinPipSpacing`, `routeHoldTrailCloseHideDistance`, and marker positions based on mobile readability.
14. If route pips are still too noisy, increase close-hide distance/min spacing or switch to fewer arrow-shaped pips.
15. After playtest stability, extract ROUTE HOLD / Stage Route logic out of `DummyFlowController` partials into a smaller dedicated runtime component or service.

## Next Session Paste Context Packet
```text
Project: D:\uni\spinball / Unity Alien Crusher / Unity 6000.3.8f1.
MCP may be unavailable; use filesystem, Unity batchmode, and logs first.
Latest completed work: ROUTE HOLD is wired after LANE BREAK, LANE BREAK triggers a short ROUTE OPEN beat, ROUTE HOLD progress is now shown as a faster-read HUD/gauge meter, ROUTE BONUS opens a district-flavored SMASH target cluster before normal Forward Smash resolution, failure result/lobby advice now starts with one next-run first action for the last failure bucket, and editor/development playtests now emit structured `[AlienCrusher][Playtest]` telemetry to both the Console and `Logs/AlienCrusherPlaytestTelemetry.log`. The telemetry stream now includes `SWEEP_START` and `SWEEP_END` around `F10` stage sweeps, and `Tools/GeneratePlaytestTelemetrySummary.ps1` now converts the log into `Logs/AlienCrusherPlaytestTelemetrySummary.md` with a current tuning snapshot, sweep-level summaries, stage trend rollups, tuning candidates, first-pass experiment suggestions, failure bucket action cues, and per-run breakdowns. HUD shows route/hold/smash guidance, route beacon, and distance-aware world-space trail pips toward Target_A/Target_B. Runtime map generation now resets/rebuilds the managed city layout on stage start using the current stage number, so stages grow from a compact starter district into wider, denser maps with more varied buildings, traffic props, commercial objects, barrels, transformers, stage-gated landmark districts, and wider target marker positions. Use `[AlienCrusher][MapLayout]` console logs, `Tools/Alien Crusher/Audit Runtime Map Layout`, the map layout overlay, `[AlienCrusher][Playtest]` filtering, the playtest telemetry log file, and the telemetry summary file to compare stage, theme, size, grid, destructible count, prop counts, landmark count, target positions, warnings, and route event order during playtest. In editor/development builds, use `F6`/`F7`/`F8` for quick stage cycling, `F9` to toggle the overlay, and `F10` to sweep Stage 1-7.
Latest validation: `Tools/RunUnityBatchChecks.ps1` passed on 2026-05-05. `Logs/AlienCrusherSceneValidation.log` refreshed at 21:14 with `Result: 0 error(s), 0 warning(s)`, and `Logs/AlienCrusherMapLayoutAudit.log` refreshed at 21:15 with Stage 1-7 `Result: 0 error(s), 0 warning(s)`. Unity-free scene essentials, static map audit, ROUTE HOLD static audit, `Tools/RunStaticAudits.ps1`, and `Tools/GenerateStagePlaytestChecklist.ps1` also passed on 2026-05-05. The route open beat/map rebuild/landmark/audit/route-hold trail/sweep telemetry changes still need a real in-editor or mobile-style playtest pass for feel.
Changed files: `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.ProgressionCore.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.UIFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.MetaProgression.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.PlaytestTelemetry.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`, `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`, `Assets/Scripts/Editor/AlienCrusherSceneValidator.cs`, `Assets/Scenes/SampleScene.unity`, `Tools/InvokeUnityBatch.ps1`, `Tools/RunUnityBatchChecks.ps1`, `Tools/AuditSceneEssentialsStatic.ps1`, `Tools/AuditRouteHoldTuningStatic.ps1`, `Tools/GenerateStagePlaytestChecklist.ps1`, `Tools/GeneratePlaytestTelemetrySummary.ps1`, `Docs/GAME_UPDATE_ROADMAP.md`, `Docs/GDD_ALIEN_CRUSHER.md`, plus editor validation/repair files from the ROUTE HOLD arrow pass and this handoff doc.
Useful Unity batch command: `powershell -ExecutionPolicy Bypass -File Tools/RunUnityBatchChecks.ps1`
Useful stale-lock retry command: `powershell -ExecutionPolicy Bypass -File Tools/RunUnityBatchChecks.ps1 -ClearStaleUnityLock`
Useful playtest checklist command: `powershell -ExecutionPolicy Bypass -File Tools/GenerateStagePlaytestChecklist.ps1`
Useful playtest telemetry summary command: `powershell -ExecutionPolicy Bypass -File Tools/GeneratePlaytestTelemetrySummary.ps1`
Useful static fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditRuntimeMapLayoutStatic.ps1`
Useful ROUTE HOLD fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditRouteHoldTuningStatic.ps1`
Useful combined fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/RunStaticAudits.ps1`
Next priority: run `Tools/GenerateStagePlaytestChecklist.ps1`, then do a real in-editor/mobile playtest from Stage 1 through Stage 7 and fill the generated checklist. After the sweep, run `Tools/GeneratePlaytestTelemetrySummary.ps1` and compare the markdown summary against the checklist notes. Confirm map growth, object variety, landmark district placement, opener -> pivot -> sustain -> payoff -> climax rhythm, LANE BREAK -> ROUTE OPEN -> ROUTE HOLD readability, route meter clarity, trail/beacon clarity, target distance, timer pressure, and that route reward opens one readable district SMASH cluster. Keep `Tools/RunUnityBatchChecks.ps1` and `Tools/RunStaticAudits.ps1` green after any tuning. If stable, extract ROUTE HOLD/stage route code out of `DummyFlowController`.
Known risks: MCP unreliable; no hands-on playmode/mobile pass yet; route pips may be visually noisy; `DummyFlowController` remains an architecture risk; Unity editor shutdown logs a non-blocking temp allocator warning.
```
