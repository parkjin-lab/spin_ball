# Alien Crusher Handoff - 2026-05-25

## Validation Snapshot
- Latest Unity batch validation: 2026-05-05 (`Tools/RunUnityBatchChecks.ps1` passed; scene and map audit logs refreshed)
- Latest static audit refresh: 2026-05-25 (`Tools/RunStaticAudits.ps1`, feedback audio hook audit, playtest evidence gate regression)
- Real Stage 1-7 playtest telemetry: still not captured as of 2026-05-25; the summary pipeline is ready but still waiting on the first true `F10` sweep
- No telemetry log or unparseable telemetry means the summary is a readiness artifact only; do not tune rhythm, payoff, boss, or route timing from it.
- Latest design policy review: 2026-05-25 sub-agent gap review produced `Docs/GAME_DESIGN_GAP_POLICY.md`; `Tools/TestPlaytestEvidenceGate.ps1` now checks the real evidence gate and `Tools/TestPlaytestEvidenceGateRegression.ps1` protects that gate with fixture coverage. Feedback audio hook points now exist in `FeedbackSystem`, but audio clips/assets still need selection and playtest balancing. Treat mobile HUD readability, ROUTE HOLD route-adherence evidence, landmark gameplay value, Stage 4 boss-approach identity, and real evidence gates as the next design policy backlog.

## Immediate First Action
1. Run `powershell -ExecutionPolicy Bypass -File Tools/GenerateStagePlaytestChecklist.ps1`
2. Run one Unity `F10` sweep and capture Stage 1 / 4 / 7 notes in `Docs/AlienCrusherStagePlaytestNotes.md`
3. Re-run `powershell -ExecutionPolicy Bypass -File Tools/GeneratePlaytestTelemetrySummary.ps1`, then compare the rhythm snapshot against the checklist notes
4. Pick one dominant broken beat, one variable family, and the exact stages to retest before making any broader tuning pass
5. Before tuning, compare the decision against `Docs/GAME_DESIGN_GAP_POLICY.md`

Done only when:
- `Logs/AlienCrusherPlaytestTelemetry.log` exists
- regenerated `Logs/AlienCrusherPlaytestTelemetrySummary.md` contains `Tune Next` from real run data
- Stage 1 / 4 / 7 notes or screenshot/video references exist in `Docs/AlienCrusherStagePlaytestNotes.md`
- the chosen tuning pass satisfies the evidence/tuning-lock policy in `Docs/GAME_DESIGN_GAP_POLICY.md`
- `Tools/TestPlaytestEvidenceGate.ps1` passes without `-ReportOnly`

## After First Sweep, Tune In This Order
1. opening / first pivot readability
2. route hold sustain readability
3. payoff / smash close readability
4. stage-specific rhythm presets
5. boss breathing windows

Rule:
- choose one dominant broken beat
- choose one variable family
- retest only the affected stages before widening the pass

## Current Progress Update
- MCP may still be unreliable, so the project now has a filesystem/Unity-batch validation path.
- ROUTE HOLD remains wired after LANE BREAK with a `LANE BREAK -> ROUTE OPEN` feedback beat, HUD guidance, route beacon, route trail pips, result badges/advice, and lobby/meta recommendations.
- Scene readiness validation is no longer menu-only: it can run through Unity `-executeMethod` and writes `Logs/AlienCrusherSceneValidation.log`.
- Latest completed Unity batch validation against `Assets/Scenes/SampleScene.unity` is the 2026-05-05 wrapper pass; latest static/checklist/readiness refresh is 2026-05-20.
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
- `Tools/GeneratePlaytestTelemetrySummary.ps1` now parses that telemetry log into `Logs/AlienCrusherPlaytestTelemetrySummary.md`, grouping runs under sweep-level summaries when `F10` is used and adding a current tuning snapshot, rhythm snapshot, `Tune Next` decision block, stage trend, tuning candidate, first-pass experiment, and failure bucket action rollup.
- Rhythm is now an explicit design lens: playtests should judge opener -> pivot -> sustain -> payoff -> climax cadence, and neighboring stages should differ by rhythm problem rather than size alone.
- No real Stage 1-7 sweep telemetry has been captured yet, so the next important evidence is still the first true `F10` playtest pass.
- 2026-05-25 design gap review: four sub-agent roles reviewed rhythm/core loop, map/content growth, feedback/sensory design, and production/validation policy. The result is `Docs/GAME_DESIGN_GAP_POLICY.md`, which turns the findings into guardrails: evidence before tuning, one broken beat at a time, ROUTE HOLD as route-readability plus count goal, audio/HUD/failure feedback as rhythm work, landmark gameplay value audits, Stage 4 boss-approach identity, and evidence gate automation.
- Added `Tools/TestPlaytestEvidenceGate.ps1` as the blocking Evidence Green check. It verifies real telemetry, summary freshness, Stage 1-7 `STAGE_START`/`STAGE_END` coverage, `SWEEP_START`/`SWEEP_END`, populated stage notes, and optional post-sweep decision fields. Use `-ReportOnly` before the first real sweep.
- Added `Tools/TestPlaytestEvidenceGateRegression.ps1` and wired it into `Tools/RunStaticAudits.ps1` so the Evidence Green checker itself is tested without requiring a live Unity playtest.
- Added assignable feedback audio hooks in `FeedbackSystem` for hit/destruction weight, combo rise, route open/hold/bonus, boss warning/break/down, and level-up beats. Hooks are null-safe until clips are assigned.
- Added `Tools/AuditFeedbackAudioHooksStatic.ps1` and wired it into `Tools/RunStaticAudits.ps1` so rhythm-critical feedback events keep their audio hook surface.
- Added `Tools/AuditRouteHoldTuningStatic.ps1` as a Unity-free audit for ROUTE HOLD targets, pressure, deadlines, and distance-aware trail pip counts.
- `Tools/AuditRouteHoldTuningStatic.ps1` now reads its ROUTE HOLD, route open beat, route reward cluster, stage gate, boss stage, and stage timer defaults from the runtime C# fields before auditing, so tuning changes in `DummyFlowController`/`GameFlowSystem` do not silently drift from the audit.
- Added `Tools/RunStaticAudits.ps1` to run all Unity-free audits in one command and fail the process if any audit reports warnings.
- Added `Tools/InvokeUnityBatch.ps1` and `Tools/RunUnityBatchChecks.ps1` to make Unity batch validation less ambiguous. The wrapper uses batch/nographics mode, detects stale `Temp/UnityLockfile`, captures stdout/stderr, enforces a timeout, and fails if the expected report file timestamp does not advance.
- Added `Docs/GAME_UPDATE_ROADMAP.md` and updated the GDD core loop section to reflect the current LANE BREAK -> ROUTE OPEN -> ROUTE HOLD -> ROUTE BONUS / Forward Smash direction.
- Added `HudRouteArrow/ArrowText` to `SampleScene.unity` and `Tools/AuditSceneEssentialsStatic.ps1` to catch missing route HUD essentials without relying on Unity batch.
- 2026-05-05 validation follow-up: `Tools/RunUnityBatchChecks.ps1` passed. Scene validation and runtime map layout audit refreshed their report/log timestamps at 21:14 and 21:15 and ended with `0 error(s), 0 warning(s)`.
- Added `Tools/GenerateStagePlaytestChecklist.ps1` to generate `Logs/AlienCrusherStagePlaytestChecklist.md` before the Stage 1-7 hands-on pass; durable human observations live in `Docs/AlienCrusherStagePlaytestNotes.md`.

## Work Completed Immediately Before This Handoff
- Added rhythm design review support across the design artifacts instead of leaving rhythm as an implicit feel goal.
- Updated `Docs/GDD_ALIEN_CRUSHER.md` with explicit rhythm design principles and stage rhythm variation rules.
- Updated `Docs/GAME_UPDATE_ROADMAP.md` so the core loop is reviewed through opener -> pivot -> sustain -> payoff -> climax cadence.
- Updated `Tools/GenerateStagePlaytestChecklist.ps1` so Stage 1-7 playtests now ask for a rhythm pass and a per-stage rhythm identity note.
- Updated `Tools/GeneratePlaytestTelemetrySummary.ps1` so telemetry summaries expose a rhythm snapshot and make it obvious when no real sweep evidence exists yet.
- Refreshed this handoff so the next session can start from checklist -> `F10` sweep -> telemetry summary without reconstructing context.
- Added first-sweep run-sheet expectations, no-log telemetry gates, and static audit report freshness checks so autonomous prep work can continue without touching gameplay tuning.
- Added sub-agent design gap policy so future work can distinguish evidence-required tuning from autonomous readiness/design-policy work.
- Added a playtest evidence gate script so tuning lock can be enforced by tooling, not only by reading docs.
- Added the first audio-hook scaffold for the sensory rhythm pass: feedback events can now trigger assignable one-shot clips, and the boss defeat flow has a dedicated downbeat call.

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
  - Tracks current project status, immediate work, core loop fun improvements, future update milestones, and open risks. Now links the design gap policy and marks evidence-gated tuning as the current design guardrail.
- `Docs/GAME_DESIGN_GAP_POLICY.md`
  - New sub-agent review synthesis and policy document for missing design pieces: real evidence gates, ROUTE HOLD route-readability, sensory rhythm, mobile HUD readability, landmark value, Stage 4 identity, resource priority, and production gates.
- `Docs/AlienCrusherStagePlaytestNotes.md`
  - Minimum evidence gate now requires Stage 01-07 `STAGE_START`/`STAGE_END` coverage and the playtest evidence gate pass.
- `Tools/AuditRuntimeMapLayoutStatic.ps1`
  - Unity-free map formula audit. It mirrors the runtime growth/landmark placement thresholds and writes `Logs/AlienCrusherMapLayoutStaticAudit.log`.
- `Tools/AuditRouteHoldTuningStatic.ps1`
  - Unity-free ROUTE HOLD tuning audit. It reads the relevant runtime C# default fields, mirrors stage target, route hold target, deadline, route open beat range, route reward cluster range, and trail active-pip formulas, then writes `Logs/AlienCrusherRouteHoldStaticAudit.log`.
- `Tools/AuditPlaytestTelemetryWiringStatic.ps1`
  - Unity-free playtest telemetry wiring audit. It checks that runtime `F10`/route event emitters and `Tools/GeneratePlaytestTelemetrySummary.ps1` still share the same expected event contract.
- `Tools/RunStaticAudits.ps1`
  - Runs scene essentials, static map layout, ROUTE HOLD tuning, and playtest telemetry wiring audits with warning failure enabled, and fails if expected reports are missing or stale.
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
  - Uses the current PowerShell host for child audit scripts, then runs scene essentials, map layout, ROUTE HOLD, and telemetry wiring audits with report freshness checks.
- `Tools/GenerateStagePlaytestChecklist.ps1`
  - Generates a Stage 1-7 playtest checklist with validation status, map/grid growth, landmarks, ROUTE HOLD targets, route open beat timing, district reward identity, reward cluster expectations, route progress readability, failure-advice checks, route pressure, target distances, and observation prompts.
- `Tools/TestPlaytestEvidenceGate.ps1`
  - Verifies Evidence Green after a real sweep: telemetry log exists, telemetry summary is fresh, Stage 1-7 markers are present, notes are populated, and optional decision fields are filled.
- `Tools/TestPlaytestEvidenceGateRegression.ps1`
  - Uses fixture telemetry, generated summary output, and temporary playtest notes to verify the Evidence Green checker continues to accept valid evidence.
- `Tools/RunStaticAudits.ps1`
  - Now includes the playtest evidence gate regression in the Unity-free audit chain.
- `Assets/Scripts/Runtime/Systems/FeedbackSystem.cs`
  - Adds silent-safe audio hook fields and one-shot calls for rhythm-critical feedback events.
- `Assets/Scripts/Runtime/Systems/DummyFlowController.StageEncounter.cs`
  - Routes Justice Sentinel defeat through the dedicated boss-down feedback beat.
- `Tools/AuditFeedbackAudioHooksStatic.ps1`
  - Unity-free audit for the feedback audio hook surface and boss-down wiring.
- `Tools/RunStaticAudits.ps1`
  - Now includes the feedback audio hook audit in the Unity-free audit chain.
- `Logs/AlienCrusherSceneValidation.log`
  - Current validation report from 2026-05-05 21:14: `0 error(s), 0 warning(s)`.
- `Logs/AlienCrusherBatchValidationEditor.log`
  - Unity batch validation log refreshed on 2026-05-05 21:14.
- `Logs/AlienCrusherMapLayoutAudit.log`
  - Current runtime map layout audit report from 2026-05-05 21:15: Stage 1-7 `0 error(s), 0 warning(s)`.
- `Logs/AlienCrusherMapLayoutAuditEditor.log`
  - Unity batch map layout audit editor log refreshed on 2026-05-05 21:15.
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
- `Tools/RunStaticAudits.ps1` passed on 2026-05-20 with `Result: all static audits passed`; the wrapper now also checks report freshness, telemetry event/parser wiring, telemetry summary regression, and readiness report regression.
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
   `powershell -ExecutionPolicy Bypass -File Tools/InvokeUnityBatch.ps1 -ExecuteMethod AlienCrusher.EditorTools.AlienCrusherSceneRepair.RepairCurrentSceneEssentialsBatch -EditorLogPath Logs/AlienCrusherBatchRepairEditor.log -ExpectedReportPath ""`
4. Confirm `HudRouteArrow/ArrowText` exists under `HUD_Dummy` after repair, then rerun validation.
5. Inspect `Logs/AlienCrusherMapLayoutAudit.log`; any `WARN:` line should be treated as a placement bug before visual polish.
6. If Unity batch is still unstable, run the fallback audit:
   `powershell -ExecutionPolicy Bypass -File Tools/AuditRuntimeMapLayoutStatic.ps1`
7. Run the ROUTE HOLD fallback audit:
   `powershell -ExecutionPolicy Bypass -File Tools/AuditRouteHoldTuningStatic.ps1`
8. Or run all Unity-free audits at once:
   `powershell -ExecutionPolicy Bypass -File Tools/RunStaticAudits.ps1`
9. Generate the hands-on checklist:
   `powershell -ExecutionPolicy Bypass -File Tools/GenerateStagePlaytestChecklist.ps1`
10. Run in-editor playtest from Stage 1 through at least Stage 7. Use `F10` for an automatic Stage 1-7 sweep with a short pause per stage, or `F7` to jump forward, `F6` to jump back, `F8` to reset to Stage 1, and `F9` to hide/show the map layout overlay. Watch the overlay and `[AlienCrusher][MapLayout]` logs to verify size/grid/destructible/prop/landmark counts climb as expected.
11. Re-run the telemetry summary and confirm `Tune Next` is based on real telemetry:
   `powershell -ExecutionPolicy Bypass -File Tools/GeneratePlaytestTelemetrySummary.ps1`
12. Verify the map grows from a compact residential starter layout into denser/wider districts with more cars, props, commercial objects, barrels, transformers, landmark districts, and wider ROUTE HOLD targets.
13. Verify each run has a readable opener, pivot, sustain, payoff, and late squeeze or climax; Stage 2/3/5/6/7 should change the rhythm problem, not only the size or route distance.
14. Verify LANE BREAK appears, ROUTE OPEN beat is readable, HOLD beacon activates, route trail points to the active marker, ROUTE HOLD meter progresses clearly, and ROUTE HOLD reward opens the expected district SMASH target cluster once.
15. Choose one dominant broken beat from the checklist plus telemetry summary, then pick one variable family to change next.
16. Tune `routeHoldWindowSeconds`, `routeHoldProgressThreshold`, `routeOpenBeatSeconds`, `routeRewardClusterRadius`, `routeRewardClusterPropCount`, `routeHoldTrailPipCount`, `routeHoldTrailMaxDistance`, `routeHoldTrailMinPipSpacing`, `routeHoldTrailCloseHideDistance`, and marker positions based on mobile readability, but only for the chosen beat family first.
17. Retest only the affected stages before widening into stage-specific presets or boss-window tuning.
18. If route pips are still too noisy, increase close-hide distance/min spacing or switch to fewer arrow-shaped pips.
19. After playtest stability, extract ROUTE HOLD / Stage Route logic out of `DummyFlowController` partials into a smaller dedicated runtime component or service.

## Next Session Paste Context Packet
```text
Project: D:\uni\spinball / Unity Alien Crusher / Unity 6000.3.8f1.
MCP may be unavailable; use filesystem, Unity batchmode, and logs first.
Latest completed work: ROUTE HOLD is wired after LANE BREAK, LANE BREAK triggers a short ROUTE OPEN beat, ROUTE HOLD progress is now shown as a faster-read HUD/gauge meter, ROUTE BONUS opens a district-flavored SMASH target cluster before normal Forward Smash resolution, failure result/lobby advice now starts with one next-run first action for the last failure bucket, and editor/development playtests now emit structured `[AlienCrusher][Playtest]` telemetry to both the Console and `Logs/AlienCrusherPlaytestTelemetry.log`. The telemetry stream now includes `SWEEP_START` and `SWEEP_END` around `F10` stage sweeps, and `Tools/GeneratePlaytestTelemetrySummary.ps1` now converts the log into `Logs/AlienCrusherPlaytestTelemetrySummary.md` with a current tuning snapshot, rhythm snapshot, `Tune Next` decision block, sweep-level summaries, stage trend rollups, tuning candidates, first-pass experiment suggestions, failure bucket action cues, and per-run breakdowns. Rhythm is now an explicit design lens, so the next pass should judge opener -> pivot -> sustain -> payoff -> climax cadence rather than only route readability. HUD shows route/hold/smash guidance, route beacon, and distance-aware world-space trail pips toward Target_A/Target_B. Runtime map generation now resets/rebuilds the managed city layout on stage start using the current stage number, so stages grow from a compact starter district into wider, denser maps with more varied buildings, traffic props, commercial objects, barrels, transformers, stage-gated landmark districts, and wider target marker positions. `Docs/GAME_DESIGN_GAP_POLICY.md` now records the 2026-05-25 sub-agent design review and blocks rhythm/payoff/boss tuning until real Stage 1-7 evidence exists. Use `[AlienCrusher][MapLayout]` console logs, `Tools/Alien Crusher/Audit Runtime Map Layout`, the map layout overlay, `[AlienCrusher][Playtest]` filtering, the playtest telemetry log file, the telemetry summary file, and the design gap policy to compare stage, theme, size, grid, destructible count, prop counts, landmark value, target positions, warnings, and route event order during playtest. In editor/development builds, use `F6`/`F7`/`F8` for quick stage cycling, `F9` to toggle the overlay, and `F10` to sweep Stage 1-7.
Latest validation: `Tools/RunUnityBatchChecks.ps1` passed on 2026-05-05. `Logs/AlienCrusherSceneValidation.log` refreshed at 21:14 with `Result: 0 error(s), 0 warning(s)`, and `Logs/AlienCrusherMapLayoutAudit.log` refreshed at 21:15 with Stage 1-7 `Result: 0 error(s), 0 warning(s)`. Unity-free scene essentials, static map audit, ROUTE HOLD static audit, playtest telemetry wiring audit, playtest telemetry summary regression, readiness report generator regression, `Tools/RunStaticAudits.ps1`, `Tools/GenerateStagePlaytestChecklist.ps1`, and `Tools/GeneratePlaytestTelemetrySummary.ps1` were refreshed again on 2026-05-20. As of 2026-05-25, no real `F10` sweep telemetry log exists yet. The route open beat/map rebuild/landmark/audit/route-hold trail/sweep telemetry changes still need the first real in-editor or mobile-style `F10` sweep for feel.
Changed files: `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.ProgressionCore.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.UIFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.MetaProgression.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.PlaytestTelemetry.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`, `Assets/Scripts/Runtime/Systems/CameraFollowSystem.cs`, `Assets/Scripts/Editor/AlienCrusherSceneValidator.cs`, `Assets/Scenes/SampleScene.unity`, `Tools/InvokeUnityBatch.ps1`, `Tools/RunUnityBatchChecks.ps1`, `Tools/AuditSceneEssentialsStatic.ps1`, `Tools/AuditRouteHoldTuningStatic.ps1`, `Tools/AuditPlaytestTelemetryWiringStatic.ps1`, `Tools/TestPlaytestTelemetrySummary.ps1`, `Tools/TestReadinessReports.ps1`, `Tools/GenerateStagePlaytestChecklist.ps1`, `Tools/GeneratePlaytestTelemetrySummary.ps1`, `Docs/AlienCrusherStagePlaytestNotes.md`, `Docs/GAME_UPDATE_ROADMAP.md`, `Docs/GAME_DESIGN_GAP_POLICY.md`, `Docs/GDD_ALIEN_CRUSHER.md`, plus editor validation/repair files from the ROUTE HOLD arrow pass and this handoff doc.
Useful Unity batch command: `powershell -ExecutionPolicy Bypass -File Tools/RunUnityBatchChecks.ps1`
Useful stale-lock retry command: `powershell -ExecutionPolicy Bypass -File Tools/RunUnityBatchChecks.ps1 -ClearStaleUnityLock`
Useful playtest checklist command: `powershell -ExecutionPolicy Bypass -File Tools/GenerateStagePlaytestChecklist.ps1`
Useful playtest telemetry summary command: `powershell -ExecutionPolicy Bypass -File Tools/GeneratePlaytestTelemetrySummary.ps1`
Useful playtest telemetry wiring audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditPlaytestTelemetryWiringStatic.ps1`
Useful playtest telemetry summary regression command: `powershell -ExecutionPolicy Bypass -File Tools/TestPlaytestTelemetrySummary.ps1`
Useful readiness report regression command: `powershell -ExecutionPolicy Bypass -File Tools/TestReadinessReports.ps1`
Useful playtest evidence gate command: `powershell -ExecutionPolicy Bypass -File Tools/TestPlaytestEvidenceGate.ps1`
Useful playtest evidence readiness command: `powershell -ExecutionPolicy Bypass -File Tools/TestPlaytestEvidenceGate.ps1 -ReportOnly`
Useful playtest evidence gate regression command: `powershell -ExecutionPolicy Bypass -File Tools/TestPlaytestEvidenceGateRegression.ps1`
Useful feedback audio hook audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditFeedbackAudioHooksStatic.ps1`
Useful static fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditRuntimeMapLayoutStatic.ps1`
Useful ROUTE HOLD fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/AuditRouteHoldTuningStatic.ps1`
Useful combined fallback audit command: `powershell -ExecutionPolicy Bypass -File Tools/RunStaticAudits.ps1`
Next priority: run `Tools/GenerateStagePlaytestChecklist.ps1`, then do a real in-editor/mobile playtest from Stage 1 through Stage 7 and fill `Docs/AlienCrusherStagePlaytestNotes.md`. After the sweep, run `Tools/GeneratePlaytestTelemetrySummary.ps1` and `Tools/TestPlaytestEvidenceGate.ps1`, then compare the markdown summary against the checklist notes and `Docs/GAME_DESIGN_GAP_POLICY.md`. Confirm map growth, object variety, landmark gameplay value, Stage 4 boss-approach identity, opener -> pivot -> sustain -> payoff -> climax rhythm, LANE BREAK -> ROUTE OPEN -> ROUTE HOLD readability, route meter clarity, trail/beacon clarity, target distance, timer pressure, mobile HUD readability, audio/feedback gaps, and that route reward opens one readable district SMASH target cluster. Then choose one dominant broken beat, one variable family, and the retest stages before touching broader stage presets or boss windows. Keep `Tools/RunUnityBatchChecks.ps1` and `Tools/RunStaticAudits.ps1` green after any tuning. If stable, extract ROUTE HOLD/stage route code out of `DummyFlowController`.
Known risks: MCP unreliable; no hands-on playmode/mobile pass yet; route pips may be visually noisy; `DummyFlowController` remains an architecture risk; Unity editor shutdown logs a non-blocking temp allocator warning.
```
