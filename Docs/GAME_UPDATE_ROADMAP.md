# Alien Crusher - Game Update Roadmap

Last updated: 2026-08-22

This document tracks the current project state, the next production priorities, and the update direction for making the core loop more fun. It should be read with:
- `Docs/GDD_ALIEN_CRUSHER.md`
- `Docs/NEXT_SESSION_CONTEXT_PACKET.md`
- `Docs/AUTOMATION_RUNBOOK.md`
- `Docs/GAME_DESIGN_GAP_POLICY.md`
- `Docs/CURRENT_STAGE_RESOURCE_REQUIREMENTS.md`
- `Docs/STAGE_08_10_CONTENT_EXPANSION.md`
- `Docs/CURRENT_STAGE_RESOURCE_PRODUCTION_BOARD.md`

---

## 1. Current Project State

### Implemented Playable Foundation
- Stage flow exists from lobby into stage start, HUD, level-up choice, result, restart, and next stage.
- Destruction progression exists through score, chain timing, ball growth, landing shockwave, overdrive, combo rush, retail frenzy, strip clear, traffic panic, seismic bursts, and result feedback.
- Form and meta progression exist around the current runtime forms `Sphere`, `Spike`, `Ram`, `Saucer`, and `Crusher`, plus meta upgrades such as `SizeCore`, `ImpactCore`, and `DpAmplifier`.
- LANE BREAK and ROUTE HOLD are wired as the current mid-run tempo layer: route targets, `LANE BREAK -> ROUTE OPEN` feedback, HUD guidance, world beacon, route trail pips, route reward, FORWARD SMASH cluster payoff, result badges, and lobby/meta recommendations are connected.
- Failure result and lobby recommendation copy now start with one bucket-specific first action before explaining the upgrade reason.
- Editor/development playtests now emit `[AlienCrusher][Playtest]` console lines and append the same route telemetry to `Logs/AlienCrusherPlaytestTelemetry.log` for `SWEEP_START`, stage start, route open, route hold clear, route bonus, forward smash, stage end, and `SWEEP_END`. ROUTE HOLD telemetry now also samples target distance, closest/average/farthest distance, in-range percentage, and elapsed route time.
- `Tools/GeneratePlaytestTelemetrySummary.ps1` can convert the telemetry log into a markdown report with a current tuning snapshot, rhythm snapshot, `Tune Next` decision block, sweep-level summaries, stage trend rollups, route adherence metrics, tuning candidates, first-pass experiment suggestions, failure bucket actions, and per-run breakdowns for faster Stage 1-7 review.
- Runtime map rebuilds happen at stage start. Stage 1-7 preserve the original compact-to-skyline curve; Stage 8-10 continue growth from 66m/20x20 to 74m/22x22 without shrinking the earlier layouts.
- Stage-gated landmark districts now include Stage 2 pocket park, Stage 3 market plaza, Stage 4 Sentinel checkpoint, Stage 5 construction yard, Stage 6 power block, Stage 7 skyline block, Stage 8 transit hub, Stage 9 harbor yard, and Stage 10 civic core.
- The Stage 8-10 pack adds three distinct late-run destruction rhythms: linear shuttle chain, volatile container/fuel setup, and ring-to-center uplink finale. Stage 9-10 objective density is capped at the Stage 8 baseline so spatial growth does not become a raw workload spike; other balance timing remains evidence-locked.
- Stage 4+ boss flow exists around Justice Sentinel, shield pylons, core exposure, break windows, phase 2 drones, pressure pulses, and defeat cascade. Runtime silhouette kits `BOSS_Sentinel_Body_Kit`, `BOSS_Shield_Pylon_Kit`, and `BOSS_Phase2_Drone_Kit` now overlay those three roles so the main body, shield blockers, and phase-2 drones read as separate counts before HUD text.
- Boss climax feedback now uses named runtime VFX `VFX_Boss_Warning_Ring` and `VFX_Boss_Defeat_Cascade` plus a short vertical break-window burst so inbound warning, CORE EXPOSED, and Sentinel-down release have distinct visual weight. Audio C clips `SFX_Boss_Warning`, `SFX_Boss_Break`, and `SFX_Boss_Down` are reused, not replaced. Shield/exposed/broken still use Destruction B cyan / hot orange / dark cracks. Boss timing, HP, and pulse intervals are unchanged.
- Route payoff cluster frames now use named runtime VFX `VFX_RouteCluster_Marker` on a `RouteClusterMarker` host so an opened ROUTE BONUS reads as a mint-slate ring, not a filled disc. Target_A/B, HOLD pips, and `PAL_RouteMarker_Tints` stay the louder nav signal. `routeRewardClusterRadius` and `routeRewardClusterPropCount` are unchanged.
- Combo rise and Overdrive now have named runtime pulses: `VFX_Combo_Rise_Pulse` is a tight lime-gold upward tick burst on CRUSH RUSH, and `VFX_Overdrive_Pulse` is an orange speed ring with flame chevrons around the ball. Audio C `comboRiseClip` stays on the existing combo-rush hook. Combo thresholds, overdrive duration/damage, HUD layout, Icons C Overdrive, Boss C climax VFX, and form kits were not restyled.
- Forward Smash cash-out now uses named runtime VFX `VFX_ForwardSmash_Confirm`: a mint-white impact star plus a short broken ring at the smashed follow-up target. Audio A `SFX_Route_Bonus` stays on the existing `PlayTotalDestructionFeedback` hook. Combo/Overdrive pulses, cluster marker, and Boss C climax VFX were not restyled. Route radius, payoff counts, and smash damage are unchanged.
- ROUTE HOLD success now uses named runtime VFX `VFX_RouteHold_Success`: a gold-cyan lock ring plus four ground dashes at the ball, with short aim pips and a marker ping toward the opened ROUTE BONUS / Forward Smash target. Distinct from combo lime ticks, Overdrive orange chevrons, the mint smash star, and Boss C climax VFX. Audio A `routeBonusClip` stays on the existing BONUS/smash hook. HOLD duration, route timing, payoff counts, and marker tints are unchanged.
- LANE BREAK -> ROUTE OPEN now uses named runtime VFX `VFX_RouteOpen_Trail`: magenta path dashes that race toward the beacon plus an orchid ping. Distinct from HOLD gold-cyan lock ring, smash mint star, combo lime ticks, Overdrive orange chevrons, and Boss C climax VFX. Audio A `routeOpenClip` stays on the existing `PlayRouteOpenCue` hook. Route timing, payoff counts, and marker tints are unchanged.
- `PAL_RouteMarker_Tints` now locks `Target_A` / `Target_B`, `routeColor`, and HOLD trail pips to a magenta/orchid nav family so route markers stay the highest-contrast signal over district palettes and the new boss kit colors.
- Core rhythm palettes `PAL_District_StarterResidential`, `PAL_District_MarketPlaza`, `PAL_District_SentinelCheckpoint`, and `PAL_District_SkylineBlock` now tint Stage 1/3/4/7 ground, walls, and landmark pads so those stages do not share one city color. Building tier kits, Icons A/B/C, and route/boss/form numbers are unchanged.
- Secondary palettes `PAL_District_PocketPark`, `PAL_District_ConstructionYard`, and `PAL_District_PowerBlock` now tint Stage 2/5/6 so the park cut reads calmer, the yard reads blast-payoff, and the power block reads transformer risk. Palette B Stage 1/3/4/7 families and `PAL_RouteMarker_Tints` stay as-is.
- Ambient stage bands now use named runtime set `PAL_Ambient_StageBands` so Stage 1/4/7 no longer share one cool-gray fill. Ambient stays mid-value so building tiers, orchid/magenta route markers, and payoff props stay readable. District palettes A/B/C, combo/Overdrive pulses, Forward Smash confirm, and Boss C climax VFX were not restyled.
- FeedbackSystem now exposes assignable audio hooks for hit weight, destruction size, combo rise, route open/hold/bonus beats, boss warnings/break/down, and level-up moments. Route/failure, hit/break, and climax/progression drafts (`SFX_Boss_Warning`, `SFX_Boss_Break`, `SFX_Boss_Down`, `SFX_Combo_Rise`, `SFX_LevelUp_Open`) load at runtime if Inspector slots are empty.
- Successful lobby unlocks, meta purchases, and form equips now play named draft `SFX_Progression_Confirm`, distinct from the existing dry `SFX_Progression_Locked` fail cue. DP/cost numbers, form stats, and Icons A-D were not restyled. `Toast_ProgressionSaved` still covers save confirm without a modal.
- Boss-stage success now uses named result badge `Badge_Boss_Clear`, a steel plate with a downward chevron and slit eye, so Sentinel victory does not share the mint district-clear plate. `Badge_Result_Clear`, Icons A-D, HOLD pulse, smash confirm, combo pulses, Boss C climax VFX, and ambient bands were not restyled.
- HUD route/progress/gauge text now uses shorter mobile-safe runtime copy with best-fit safeguards for the main HUD readouts. Run-essential draft icons `Icon_DP`, `Icon_Stage`, `Icon_NextStep`, and `Icon_Route` now appear on the play-mode HUD strip and beside the matching lobby/result labels. Route/boss readability draft icons `Icon_BreakWindow`, `Icon_Shield`, `Icon_WeakPoint`, and `Icon_Boss` now sit on a top-right HUD strip, swap beside Sentinel status, and mark elite weak-point copy without changing HUD strings. Upgrade/chaos status draft icons `Icon_Overdrive`, `Icon_Panic`, `Icon_Seismic`, `Icon_Retail`, and `Icon_Traffic` sit on a compact strip below Icons A and beside chain / upgrade / TRAFFIC labels. Result/lobby draft badges `Badge_Result_Clear`, `Badge_Result_Failure`, `Badge_Locked`, and `Badge_Recommended` mark success, fail, lock, and recommendation without restyling Icons A/B/C or district palettes. Outgame DP economy now uses `UI_DP_GainBurst` plus `SFX_Progression_Locked` so result earn, lobby spend, and insufficient-DP fails read before copy. Form/meta choice and result-to-lobby payoff now use `UI_FormCard_StateSet`, `UI_MetaNode_SizeCore`, `UI_MetaNode_ImpactCore`, `UI_MetaNode_DpAmplifier`, `Badge_FormReady`, `Badge_MetaReady`, `Banner_StageUnlocked`, and `Toast_ProgressionSaved` so lock/unlock, recommended nodes, stage unlock, and save confirm read without restyling Icons A-D. Icon_DP and Icons A-D were not restyled.
- Sphere now has a designed starter identity: runtime kit `FORM_Sphere_Body_Kit` (cool-green body plus emissive belt), lobby thumbnail `Icon_Form_Sphere`, and SPHERE PULSE cues `Icon_Skill_SpherePulse` plus an in-world pulse mark. Form stats, unlock cost, and pulse cooldown/damage are unchanged.
- Ram and Saucer now have route-helper identities: `FORM_Ram_Body_Kit` reads as a forward amber wedge with side horns, `FORM_Saucer_Body_Kit` reads as a wide cyan disc. Lobby icons `Icon_Form_Ram` / `Icon_Form_Saucer` sit on those cards. Skill numbers are unchanged.
- Spike and Crusher now have damage-fantasy identities: `FORM_Spike_Body_Kit` reads as a lean acid-tipped needle crown, `FORM_Crusher_Body_Kit` reads as layered steel bulk with a flat plate and blue seams. Lobby icons `Icon_Form_Spike` / `Icon_Form_Crusher` sit on those cards. Skill numbers are unchanged.
- All five forms already share matching silhouette + lobby/skill icon + material languages, so Form Identity D had no remaining gaps.
- Building size tiers now use named runtime materials `MAT_Building_Small`, `MAT_Building_Mid`, `MAT_Building_Large`, and `MAT_Boss_Structure` so easy plaster props, city-concrete fillers, dark durable masses, and steel-blue boss structures read at gameplay distance before they break. HP, break thresholds, and spawn counts are unchanged.
- Combat states now use named runtime materials `MAT_Damage_CrackOverlay`, `MAT_WeakPoint_Glow`, `MAT_Shielded_Pylon`, and `MAT_Exposed_Core` so damaged buildings, elite weak points, protected pylons, and an exposed Sentinel core never share one look. Shield counts, break thresholds, and boss timing are unchanged.
- Break feedback now uses named runtime VFX `VFX_Debris_Light`, `VFX_Debris_Heavy`, `VFX_Smoke_Damage`, and `VFX_WeakPoint_Hit` so small chips, heavy collapse, near-break smoke, and crit flashes stay distinct without covering route markers. HP and spawn counts are unchanged.
- Traffic now uses named runtime kits `PROP_Car_Compact_A`, `PROP_Car_Compact_B`, and `PROP_Van_Bus` so moving cars and parked panic clusters read as vehicles instead of boxes. Traffic count, speed, HP, and spawn rate are unchanged.
- Roadside rhythm props now use named runtime kits `PROP_StreetLamp`, `PROP_TrafficLight`, `PROP_RoadsideTree`, and `PROP_Bench` so Stage 1-3 streets have thin lamps/lights, gappy trees, and low benches that do not hide route markers. Spawn counts and HP are unchanged.
- Market and utility props now use named runtime kits `PROP_Kiosk`, `PROP_Vending`, `PROP_BusStop`, `PROP_Transformer`, and `PROP_ExplosiveBarrel` so Stage 2-6 streets and ROUTE BONUS clusters signal chain density versus payoff danger before impact. Spawn counts, HP, and explosion radii are unchanged.
- Residential filler props now use named runtime kits `PROP_Fence`, `PROP_Mailbox`, and `PROP_Shed` on the existing residential hooks. Fences read as thin rails, mailboxes as post-and-box, sheds as roofed backyard boxes. Spawn counts and HP are unchanged so Stage 1 does not gain extra clutter. Route markers stay readable.
- Validation tools now include Unity scene validation/repair, Unity runtime map layout audit entry points, Unity-free static audits, and safer Unity batch wrappers.

### Current Validation Status
- Latest scene validation report from 2026-05-05 21:14 in `Logs/AlienCrusherSceneValidation.log` shows `0 error(s), 0 warning(s)`.
- `Assets/Scenes/SampleScene.unity` contains `HudRouteArrow` with child `ArrowText`, and `Tools/AuditSceneEssentialsStatic.ps1` verifies those scene essentials with `0 warning(s)`.
- Unity-free static map audit passes Stage 1-7 formula checks with `0 warning(s)`.
- Unity-free ROUTE HOLD tuning audit passes with `0 warning(s)` and reads current default tuning values, including route open beat timing, from runtime C# fields before auditing.
- `Tools/RunStaticAudits.ps1` passes the current Unity-free audit set and fails if an expected report is missing or not refreshed during the run.
- `Tools/RunPlaytestReadinessPrep.ps1` now runs the autonomous pre-playtest prep loop in one command: static audits, Stage 1-7 checklist generation, optional production checklist generation, telemetry summary generation, and Evidence Gate report-only readiness.
- `Tools/RunPlaytestReadinessPrep.ps1` now ends with a "Next Autonomous Work While Waiting" block so recurring agents can keep improving readiness, reports, resource planning, and handoff docs without violating the no-evidence tuning lock.
- `Tools/GenerateAutonomousWorkBacklog.ps1` generates `Logs/AlienCrusherAutonomousWorkBacklog.md`, a current safe-work list for unattended agents while real playtest evidence is still missing.
- `Tools/GenerateResourceProductionBacklog.ps1` merges generated production checklists into `Logs/AlienCrusherResourceProductionBacklog.md`, so unattended agents can prioritize audio, route payoff, boss identity, district palette, and UI/icon work without touching tuning.
- `Logs/AlienCrusherResourceProductionBacklog.md` now includes `## Recommended Production Batch Order` and `## Production Batch Focus`, consolidating 5 recommended batches, 33 total production batches, and 108 individual resource items. Use the recommended batch order when assigning unattended resource work so assets are produced in readable gameplay groups instead of isolated one-offs.
- `Tools/GenerateArchitectureExtractionPlan.ps1` maps `DummyFlowController` partial ownership into `Logs/AlienCrusherArchitectureExtractionPlan.md`, so architecture planning can proceed without changing gameplay behavior before Evidence Green.
- `Tools/GenerateAutomationStatusSummary.ps1` writes `Logs/AlienCrusherAutomationStatusSummary.md`, a one-page heartbeat artifact for progress, validation, current blockers, next safe work, resource item count, and production batch count.
- Unity-free static audits now include a playtest telemetry wiring check so runtime `F10` event names and telemetry summary parser expectations stay aligned before manual tuning starts.
- Runtime Unity map layout batch report from 2026-05-05 21:15 in `Logs/AlienCrusherMapLayoutAudit.log` covers Stage 1-7 with `0 error(s), 0 warning(s)`.
- `Tools/RunUnityBatchChecks.ps1` passed both scene validation and runtime map layout audit with refreshed report/log timestamps.
- `Tools/GenerateStagePlaytestChecklist.ps1` generates disposable readiness output at `Logs/AlienCrusherStagePlaytestChecklist.md`, while durable human observations should be recorded in `Docs/AlienCrusherStagePlaytestNotes.md`.
- The Stage 1-7 checklist now includes a progression save smoke pass so DP, selected form, stage unlock, meta upgrade state, and repaired save persistence are checked before rhythm tuning.
- `Tools/GenerateAudioResourceAssignmentChecklist.ps1` generates disposable audio assignment output at `Logs/AlienCrusherAudioResourceAssignmentChecklist.md`, mapping current `FeedbackSystem` slots to suggested SFX names, folders, and assignment priority.
- `Tools/GenerateFormIdentityProductionChecklist.ps1` generates disposable form identity output at `Logs/AlienCrusherFormIdentityProductionChecklist.md`, mapping runtime forms to unlock cost, skill fantasy, silhouette, icon, material, and failure-problem targets.
- `Tools/GenerateDestructionReadabilityChecklist.ps1` generates disposable destruction readability output at `Logs/AlienCrusherDestructionReadabilityChecklist.md`, mapping material, VFX, weak-point, shield, exposed-core, and break-audio targets to current runtime destruction systems.
- `Tools/GenerateStreetPropVarietyChecklist.ps1` generates disposable street prop variety output at `Logs/AlienCrusherStreetPropVarietyChecklist.md`, mapping traffic, roadside, commercial, utility, and residential prop targets to current runtime map/traffic hooks.
- `Tools/GenerateUiIconStatusChecklist.ps1` generates disposable UI icon/status output at `Logs/AlienCrusherUiIconStatusChecklist.md`, mapping HUD, route, upgrade, boss, result, and recommendation states to icon and badge targets.
- `Tools/GenerateBossIdentityProductionChecklist.ps1` generates disposable boss identity output at `Logs/AlienCrusherBossIdentityProductionChecklist.md`, mapping Justice Sentinel, shield pylon, core exposure, phase 2 drone, warning, break, and defeat beats to production targets.
- `Tools/GenerateDistrictPaletteProductionChecklist.ps1` generates disposable district palette output at `Logs/AlienCrusherDistrictPaletteProductionChecklist.md`, mapping starter, park, market, Sentinel checkpoint, construction, power, skyline, route tint, and ambient palette targets to runtime districts.
- `Tools/GenerateOutgameProgressionChecklist.ps1` generates disposable outgame progression output at `Logs/AlienCrusherOutgameProgressionChecklist.md`, mapping DP gain, form card states, meta nodes, result badges, stage unlock banners, and save confirmation targets to the current lobby/result systems.
- `Tools/GenerateRoutePayoffLayoutChecklist.ps1` generates disposable route payoff layout output at `Logs/AlienCrusherRoutePayoffLayoutChecklist.md`, mapping ROUTE BONUS, district payoff layouts, cluster markers, and Forward Smash confirmation to current route reward code paths.
- `Tools/GeneratePlaytestTelemetrySummary.ps1` now includes a rhythm snapshot, but no real Stage 1-7 sweep evidence has been captured yet.
- As of 2026-06-08, no real `F10` sweep telemetry log exists yet. The next required evidence artifacts are `Logs/AlienCrusherPlaytestTelemetry.log`, regenerated `Logs/AlienCrusherPlaytestTelemetrySummary.md`, populated Stage 1 / 4 / 7 notes in `Docs/AlienCrusherStagePlaytestNotes.md`, and a completed progression save smoke result.
- `Docs/GAME_DESIGN_GAP_POLICY.md` now records the sub-agent gap review and sets policy for evidence gates, tuning lock, ROUTE HOLD route-readability, sensory rhythm, mobile HUD readability, landmark value, Stage 4 identity, and production gates.
- `Tools/TestPlaytestEvidenceGate.ps1` now provides the blocking Evidence Green check for real telemetry, summary freshness, Stage 1-7 marker coverage, and populated playtest notes. Use `-ReportOnly` when checking readiness before evidence exists.
- `Tools/TestPlaytestEvidenceGateRegression.ps1` now keeps the Evidence Green gate itself covered by fixture telemetry and temporary notes, and `Tools/RunStaticAudits.ps1` runs it with the rest of the Unity-free audit chain.
- `Tools/TestPlaytestReadinessPrep.ps1` now protects the autonomous readiness prep runner with a non-recursive `-SkipStaticAudits` smoke pass inside `Tools/RunStaticAudits.ps1`.
- `Tools/TestAutonomousReportGenerators.ps1` now protects the autonomous backlog, resource backlog, architecture plan, and status summary generators with direct marker checks inside `Tools/RunStaticAudits.ps1`.
- Evidence Green now also requires the tracked progression save smoke result, so real rhythm tuning cannot start while save/load persistence is unverified.
- `Tools/AuditFeedbackAudioHooksStatic.ps1` now checks that rhythm-critical feedback events still have assignable audio clip hooks, and `Tools/RunStaticAudits.ps1` includes it in the Unity-free audit chain.
- `Tools/AuditResourceSlotDocsStatic.ps1` now checks that current `FeedbackSystem` audio clip fields stay documented in the resource requirements and production board, so code-side audio slots and production needs do not drift apart.
- `Tools/AuditProgressionSaveSafetyStatic.ps1` now checks the JSON save/backup/meta-bound/stage-bound/default/migration contract so long-term progression cannot silently lose backup fallback, persisted save repair, safe progression bounds, or legacy PlayerPrefs migration coverage.
- `Tools/AuditMobileHudReadabilityStatic.ps1` now checks that core HUD route/progress/gauge copy stays compact and that main HUD text fields keep mobile best-fit safeguards.
- Stage 4 now has a Sentinel checkpoint landmark in the runtime map layout and the static map audit tracks the new landmark tier/count expectations.
- `Tools/AuditPlaytestTelemetryWiringStatic.ps1` now protects the route-adherence telemetry contract so route distance metrics stay wired into both runtime logs and summary output.
- `Tools/AuditRuntimeMapLayoutStatic.ps1` now records landmark value metadata beyond count: role, target relationship, payoff object mix, entry lane, and exit lane for every active landmark.

### Current Main Risk
The prototype has enough systems to be interesting, and the automated validation loop is now green again. The remaining risk is play feel: real editor/mobile playtests must still confirm that route readability, map growth, reward timing, HUD scaffolding, and the opener -> pivot -> sustain -> payoff -> climax rhythm all feel good in motion instead of flattening into constant pressure. The current design policy treats this as an evidence problem first and blocks rhythm/payoff/boss tuning until real Stage 1-7 playtest telemetry exists.

---

## 2. Immediate Work Queue

### P0 - Restore Validation Confidence
1. Keep `Tools/RunUnityBatchChecks.ps1` passing before any new scene/map changes.
2. Keep `Tools/RunStaticAudits.ps1` passing before committing gameplay tuning.
3. Require both scene validation and runtime map layout audit reports to refresh during Unity batch runs.
4. Treat stale or missing report files as validation failures, even if Unity exits with code `0`.

Done when:
- `Logs/AlienCrusherSceneValidation.log` reports `0 error(s), 0 warning(s)`.
- `Logs/AlienCrusherMapLayoutAudit.log` exists and covers Stage 1-7.
- Unity batch logs are from the current run, not stale files.

### P0 - Stage 1-7 Editor Playtest
Before entering play mode, run the autonomous readiness prep:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/RunPlaytestReadinessPrep.ps1
```

If an asset/resource pass is next, include the production checklists in the same run:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/RunPlaytestReadinessPrep.ps1 -IncludeProductionChecklists
```

Or generate individual checklists:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/GenerateStagePlaytestChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateAudioResourceAssignmentChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateFormIdentityProductionChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateDestructionReadabilityChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateStreetPropVarietyChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateUiIconStatusChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateBossIdentityProductionChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateDistrictPaletteProductionChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateOutgameProgressionChecklist.ps1
powershell -ExecutionPolicy Bypass -File Tools/GenerateRoutePayoffLayoutChecklist.ps1
```

Required next artifacts:
- `Logs/AlienCrusherPlaytestTelemetry.log`
- regenerated `Logs/AlienCrusherPlaytestTelemetrySummary.md` with `Tune Next` based on real run data
- optional audio assignment pass in `Logs/AlienCrusherAudioResourceAssignmentChecklist.md`
- optional form identity production pass in `Logs/AlienCrusherFormIdentityProductionChecklist.md`
- optional destruction readability pass in `Logs/AlienCrusherDestructionReadabilityChecklist.md`
- optional street prop variety pass in `Logs/AlienCrusherStreetPropVarietyChecklist.md`
- optional UI icon/status pass in `Logs/AlienCrusherUiIconStatusChecklist.md`
- optional boss identity pass in `Logs/AlienCrusherBossIdentityProductionChecklist.md`
- optional district palette pass in `Logs/AlienCrusherDistrictPaletteProductionChecklist.md`
- optional outgame progression pass in `Logs/AlienCrusherOutgameProgressionChecklist.md`
- optional route payoff layout pass in `Logs/AlienCrusherRoutePayoffLayoutChecklist.md`
- resource batch focus review in `Logs/AlienCrusherResourceProductionBacklog.md` before starting asset work
- populated Stage 1 / 4 / 7 notes in `Docs/AlienCrusherStagePlaytestNotes.md` or linked screenshots/videos
- progression save smoke result for DP, selected form, stage unlock, meta upgrade state, and repaired save persistence

First `F10` sweep minimum markers:
- `SWEEP_START`
- `STAGE_START` and `STAGE_END` for Stage 01, Stage 04, and Stage 07
- `SWEEP_END`

If any marker is missing, fix telemetry/sweep wiring before tuning.

Use `F10` sweep or manual `F6/F7/F8/F9` controls to verify:
- stage size/grid/destructible counts grow as expected
- landmark districts appear at the intended stages
- camera clamp follows the rebuilt map bounds
- target markers stay readable and reachable
- LANE BREAK -> ROUTE OPEN beat is visible and then ROUTE HOLD reads clearly
- route trail pips are not noisy on a small mobile-style viewport
- ROUTE HOLD reward fires once and feels like a meaningful payoff
- each run has a readable opener, pivot, sustain, payoff, and late squeeze/climax beat
- Stage 2/3/5/6/7 change the rhythm problem, not only the map size or target distance
- console filter `[AlienCrusher][Playtest]` shows the expected route event order per run
- `Logs/AlienCrusherPlaytestTelemetry.log` keeps the same event order from `SWEEP_START` through `SWEEP_END`
- `Tools/GeneratePlaytestTelemetrySummary.ps1` produces a readable current tuning snapshot, rhythm snapshot, sweep summary, stage trend rollup, tuning candidates, first-pass experiment suggestions, failure bucket actions, and per-run breakdown after the sweep
- DP, selected form, highest unlocked stage, and meta upgrade state survive exit/re-enter play mode before tuning decisions are made

Done when:
- each stage band has one short note on readability, route pressure, map identity, and rhythm identity
- at least one screenshot or written observation exists for Stage 1, Stage 4, and Stage 7
- `Docs/AlienCrusherStagePlaytestNotes.md` has notes for Stage 1-7 or links to the matching screenshot/video captures

After the first real sweep, tune in this order:
1. opening / first pivot readability
2. route hold sustain readability
3. payoff / smash close readability
4. stage-specific rhythm presets
5. boss breathing windows

Rule:
- choose one dominant broken beat
- choose one variable family
- retest only the affected stages before widening the pass

### P0 - ROUTE HOLD Readability Tuning
Tune only after playtest evidence:
- `routeHoldWindowSeconds`
- `routeHoldProgressThreshold`
- `routeOpenBeatSeconds`
- `routeRewardClusterRadius`
- `routeRewardClusterPropCount`
- `routeHoldTrailPipCount`
- `routeHoldTrailMaxDistance`
- `routeHoldTrailMinPipSpacing`
- `routeHoldTrailCloseHideDistance`
- Target_A/Target_B placement rules

Done when:
- close-range route pips hide cleanly
- far target guidance remains readable
- route pressure feels urgent without turning into pure distance tax

### P0 - Game Design Gap Policy
Use `Docs/GAME_DESIGN_GAP_POLICY.md` as the guardrail for any next gameplay/design work.

Immediate policy requirements:
1. Keep the tuning lock active until real Stage 1-7 evidence exists.
2. Choose one broken beat and one variable family per tuning pass.
3. Treat ROUTE HOLD as both a count goal and a route-reading goal.
4. Prioritize audio hooks, mobile HUD readability, and failure beat feedback as core rhythm work, not polish.
5. Expand map validation from landmark count to landmark gameplay value.

Done when:
- `Tools/TestPlaytestEvidenceGate.ps1` passes without `-ReportOnly`
- Stage 4 has a runtime Sentinel checkpoint landmark and documented boss-approach identity instead of sharing only the market escalation feel
- HUD/audio/failure feedback tasks are tracked as P0/P1 design work

### P0 - Autonomous Continuity While The Creator Is Away

Recurring agents may continue work without new creator input only inside the safe pre-evidence lanes:

1. Keep readiness automation readable and green.
2. Improve generated checklist/report diagnostics.
3. Update handoff docs with the latest verified next step.
4. Expand resource planning from existing runtime hooks and generated checklist gaps.
5. Add static/regression coverage for tooling changes.

Do not use autonomous time to tune route timing, payoff counts, target placement, stage rhythm presets, or boss pressure before Evidence Green.

Done when:
- `Tools/RunPlaytestReadinessPrep.ps1` clearly prints both the required human evidence and safe autonomous work.
- `Logs/AlienCrusherResourceProductionBacklog.md` exists and identifies the highest-value resource tasks that support rhythm readability without changing gameplay numbers.
- `Logs/AlienCrusherArchitectureExtractionPlan.md` exists and identifies safe extraction order before any `DummyFlowController` behavior refactor.
- `Logs/AlienCrusherAutomationStatusSummary.md` exists and summarizes progress, validation, blockers, resource order, and architecture order for the next unattended agent.
- `Docs/NEXT_SESSION_CONTEXT_PACKET.md` names the next safe task for an unattended agent.
- readiness prep regression protects those instructions.

---

## 3. Current Core Loop Definition

The current core loop is no longer just "destroy objects for score." The implemented loop should now be treated as:

```text
Stage Start
  -> Crush the starter lane
  -> Trigger LANE BREAK
  -> Follow the opened ROUTE HOLD beacon
  -> Keep destruction tempo until ROUTE HOLD succeeds
  -> Trigger ROUTE BONUS / Forward Smash setup
  -> Convert the opened cluster into score, DP, growth, or boss pressure
  -> Read result feedback and upgrade toward the next run
```

### Design Intent
LANE BREAK is the opening tempo check. ROUTE HOLD is the mid-run control test. ROUTE BONUS and Forward Smash should be the visible reward that makes the player feel, "I read the route well, so the city opened up."

### Player Skill Question
Each run should ask one clear question:

> Can I find the best crush lane, keep speed through the route, and turn that momentum into the next big destruction cluster?

### Rhythm Lens
The loop only works if a run changes state in readable beats rather than sitting at one pressure level the whole time.

- **Opener**: first crush lane is obvious and satisfying to commit to
- **Pivot**: `LANE BREAK -> ROUTE OPEN` makes the next objective feel like a real turn, not extra UI noise
- **Sustain**: `ROUTE HOLD` asks for control and commitment without becoming pure distance tax
- **Payoff**: `ROUTE BONUS` / Forward Smash feels earned because it follows a readable route problem
- **Climax**: finish lane or boss phase compresses the prior beats into one last push

If a stage feels flat, boring, or samey, treat that as a rhythm bug before treating it as a content shortage.

---

## 4. Making The Core Loop More Fun

### Near-Term Experiments

#### 1. Route Open Moment
After LANE BREAK, the baseline two-second "route opened" beat is now implemented:
- announcement copy: `LANE BREAK -> ROUTE OPEN`
- HUD objective/hint shifts briefly to `ROUTE OPEN`
- active Target_A/Target_B marker, HUD route indicator, arrow, and route trail pulse harder during the beat
- scene/static validation now checks the `routeOpenBeatSeconds` tuning range

Success signal:
- player can immediately tell that the next objective changed

#### 2. Reward Cluster Emphasis
Baseline reward cluster emphasis is now implemented:
- ROUTE BONUS copy shifts to `ROUTE BONUS -> CLUSTER OPEN`
- Forward Smash target copy now says `SMASH CLUSTER OPEN`
- HUD route arrow switches to `SMASH` and points at the highlighted forward target
- route reward spawns extra barrel/transformer payoff props around the next Forward Smash target
- scene/static validation now checks route reward cluster radius and prop count

Success signal:
- ROUTE HOLD success visibly creates or reveals something worth chasing

#### 3. Route Progress Readability
Baseline ROUTE HOLD progress readability is now implemented:
- HUD progress text switches to a ROUTE HOLD percentage, remaining wreck count, and countdown during the objective
- stage goal gauge temporarily becomes the ROUTE HOLD meter before returning to normal stage progress
- route indicator shows `HOLD xx%` while pointing at the active beacon
- close-range pips still hide so the meter does the heavy readability work

Success signal:
- player can read "almost there" without parsing a sentence

#### 4. District Route Puzzles
Baseline district route payoff identity is now implemented as named layouts:
- Stage 2 `PAYOFF_ParkCut_Layout`: open bench/tree/barrel cut with a readable center chase lane
- Stage 3-4 `PAYOFF_MarketChain_Layout`: tight kiosk/vending/barrel chain along the smash path
- Stage 5 `PAYOFF_YardBlast_Layout`: wide barrel-heavy blast corners plus one utility
- Stage 6 `PAYOFF_PowerSurge_Layout`: long transformer corridor with barrel punctuation
- Stage 7 `PAYOFF_SkylineBreach_Layout`: asymmetric anchor-first tower with a side cluster

Success signal:
- later stages feel different by route decision, not only by size

#### 5. Shorter Failure Advice
Baseline shorter failure advice is now implemented. Result and lobby advice both start with the next run's first action:
- `OPENING FAILED`: hit dense low-rise rows first
- `ROUTE HOLD MISSED`: after LANE BREAK, stay on the beacon route
- `MID-RUN DRIFT`: choose the next cluster before speed drops
- `FINAL PUSH FAILED`: ignore side props and force the goal lane
- `BOSS PHASE`: break pylons, then burst the exposed core

Success signal:
- failure advice is actionable in one glance

#### 6. Stage Rhythm Presets
After the first real Stage 1-7 sweep, test small stage-specific rhythm presets instead of one global tempo profile:
- Guardrail: do not implement these presets before real `F10` telemetry and Stage 1/4/7 notes identify the dominant broken beat.
- Stage 2: slightly longer `routeOpenBeatSeconds`
- Stage 3-4: faster opening pressure and denser sustain
- Stage 5: larger payoff spacing and louder release
- Stage 6: longer hold window for long-route commitment
- Stage 7: tighter late-run squeeze before the skyline climax

Success signal:
- neighboring stages feel like different tempo problems before new content is added

#### 7. Payoff Layout Rhythm
Keep district payoff identity, but vary the layout rhythm as much as the prop set:
- market payoff: tighter chainable clusters
- construction payoff: wider blast spacing
- power payoff: longer transformer corridor
- skyline payoff: asymmetric anchor-first cluster

Success signal:
- payoff beats feel different in motion, not just visually different in screenshots

#### 8. Boss Breathing Windows
Boss pressure should read as "breathe -> burst -> breathe" instead of constant harassment:
- widen break windows slightly if core exposure is too hard to parse
- delay pressure pulses slightly if the punish window is drowned out
- make drone-break aftermath a clearer low-pressure punish beat

Success signal:
- the boss feels like the run's climax instead of a longer version of the normal route

---

## 5. Future Update Direction

### Milestone 1 - Verified Prototype Loop
Goal:
- make validation trustworthy and prove Stage 1-7 can be played without route/map/HUD confusion

Includes:
- clean scene validation
- Unity runtime map audit report
- Stage 1-7 playtest notes
- ROUTE HOLD readability tuning

### Milestone 2 - Core Loop Fun Pass
Goal:
- make LANE BREAK -> ROUTE OPEN -> ROUTE HOLD -> ROUTE BONUS the primary satisfying loop

Includes:
- verified opener -> pivot -> sustain -> payoff -> climax cadence in Stage 1-7 playtests
- one explicit post-sweep tuning decision per pass instead of broad multi-variable adjustments
- route open feedback beat tuning after playtest
- reward cluster visibility
- route progress mini-readability
- shorter result advice

### Milestone 3 - District Identity Pass
Goal:
- make map growth feel like new route problems, not just larger maps

Includes:
- stage landmark behavior rules
- target marker distance bands
- district-specific prop payoff
- map overlay/audit thresholds and value records for landmark role, target relationship, payoff object mix, entry lane, and exit lane

### Milestone 4 - Resource Feedback Pass
Goal:
- make the existing systems feel intentional and juicy with minimal asset cost

Highest-return resources:
- core audio starter pack, now mapped to the assignable `FeedbackSystem` clip slots
- form silhouettes and icons
- destruction material tiers
- street prop variety
- boss identity silhouettes and warning effects

### Milestone 5 - Form And Meta Purpose Pass
Goal:
- connect each form/meta upgrade to a real failure problem

Direction:
- `Ram`: route recovery and mid-run drift
- `Crusher`: final push and boss pressure
- `Saucer`: navigation and target reach
- `Spike`: weak point and dense-object puncture
- `Sphere`: stable default starter lane control

### Milestone 6 - Architecture Stabilization
Goal:
- reduce risk in the large `DummyFlowController` partial surface after behavior stabilizes

Extraction candidates:
- ROUTE HOLD / Stage Route logic
- map layout debug and audit hooks
- result advice and failure bucket logic
- HUD route indicator/trail rendering

---

## 6. Open Risks

- Unity batch has recently shown stale log, lock, and timeout behavior, although the 2026-05-05 wrapper run passed cleanly.
- Static audits pass formula checks but cannot validate real play feel.
- Real-evidence gate automation exists, but it will fail until a real Stage 1-7 sweep and populated notes exist.
- The route loop may still flatten into uniform pressure if district-to-district rhythm variation is too weak.
- ROUTE HOLD may still feel like a timed destruction count unless route adherence is measured separately from destroyed count.
- ROUTE HOLD route-adherence telemetry now exists, but it still needs the first real Stage 1-7 sweep before any tuning decision can use it.
- Route and failure rhythm now have draft clips on the matching events; hit/break/boss/level-up audio and mobile HUD screenshot review are still open.
- Progression save recovery now falls back per file, persists sanitized repairs after load, clamps meta/stage bounds, and removes duplicate meta-upgrade entries; this still needs an in-editor smoke test with a real save file before release.
- Stage 4 has first-pass Sentinel checkpoint identity plus runtime boss silhouette kits for the main body, shield pylons, and phase-2 drones; it still needs playtest/screenshot confirmation that those three roles stay countable at mobile distance without HUD text.
- Landmark value records now exist in static audit/checklist output, but they still need real visual confirmation that the roles are legible during play.
- Route trail pips may be visually noisy on small Android screens.
- Current implementation form names differ from older GDD form fantasy names; status documents should use runtime names until the design naming pass is resolved.
- `DummyFlowController` remains a large partial mega-controller and should not absorb more route/gameplay surface indefinitely.

---

## 7. Documentation Update Rules

When this roadmap changes:
- update `Docs/NEXT_SESSION_CONTEXT_PACKET.md` with the latest validation status
- keep `Docs/GDD_ALIEN_CRUSHER.md` focused on product design, not every temporary validation issue
- keep `Docs/GAME_DESIGN_GAP_POLICY.md` focused on current design gaps, guardrails, and decision policy
- keep resource priorities in `Docs/CURRENT_STAGE_RESOURCE_REQUIREMENTS.md` and `Docs/CURRENT_STAGE_RESOURCE_PRODUCTION_BOARD.md`
- avoid declaring a Unity batch pass unless the report file timestamp advanced during that run
