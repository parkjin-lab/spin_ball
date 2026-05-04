# Alien Crusher - Game Update Roadmap

Last updated: 2026-05-05

This document tracks the current project state, the next production priorities, and the update direction for making the core loop more fun. It should be read with:
- `Docs/GDD_ALIEN_CRUSHER.md`
- `Docs/NEXT_SESSION_CONTEXT_PACKET.md`
- `Docs/CURRENT_STAGE_RESOURCE_REQUIREMENTS.md`
- `Docs/CURRENT_STAGE_RESOURCE_PRODUCTION_BOARD.md`

---

## 1. Current Project State

### Implemented Playable Foundation
- Stage flow exists from lobby into stage start, HUD, level-up choice, result, restart, and next stage.
- Destruction progression exists through score, chain timing, ball growth, landing shockwave, overdrive, combo rush, retail frenzy, strip clear, traffic panic, seismic bursts, and result feedback.
- Form and meta progression exist around the current runtime forms `Sphere`, `Spike`, `Ram`, `Saucer`, and `Crusher`, plus meta upgrades such as `SizeCore`, `ImpactCore`, and `DpAmplifier`.
- LANE BREAK and ROUTE HOLD are wired as the current mid-run tempo layer: route targets, HUD guidance, world beacon, route trail pips, route reward, result badges, and lobby/meta recommendations are connected.
- Runtime map rebuilds happen at stage start. Stage 1-7 currently grow from a compact starter layout into wider, denser maps with expanded target marker spacing and stage-gated landmarks.
- Stage-gated landmark districts currently include Stage 2 pocket park, Stage 3 market plaza, Stage 5 construction yard, Stage 6 power block, and Stage 7 skyline block.
- Stage 4+ boss flow exists around Justice Sentinel, shield pylons, core exposure, break windows, phase 2 drones, pressure pulses, and defeat cascade.
- Validation tools now include Unity scene validation/repair, Unity runtime map layout audit entry points, Unity-free static audits, and safer Unity batch wrappers.

### Current Validation Status
- Latest scene validation report from 2026-05-05 00:27 in `Logs/AlienCrusherSceneValidation.log` shows `0 error(s), 0 warning(s)`.
- `Assets/Scenes/SampleScene.unity` contains `HudRouteArrow` with child `ArrowText`, and `Tools/AuditSceneEssentialsStatic.ps1` verifies those scene essentials with `0 warning(s)`.
- Unity-free static map audit passes Stage 1-7 formula checks with `0 warning(s)`.
- Unity-free ROUTE HOLD tuning audit passes with `0 warning(s)` and reads current default tuning values from runtime C# fields before auditing.
- `Tools/RunStaticAudits.ps1` passes the current Unity-free audit set.
- Runtime Unity map layout batch report from 2026-05-05 00:28 in `Logs/AlienCrusherMapLayoutAudit.log` covers Stage 1-7 with `0 error(s), 0 warning(s)`.
- `Tools/RunUnityBatchChecks.ps1` passed both scene validation and runtime map layout audit with refreshed report/log timestamps.
- `Tools/GenerateStagePlaytestChecklist.ps1` generates `Logs/AlienCrusherStagePlaytestChecklist.md`, combining current validation results, Stage 1-7 map growth, route targets, route pressure, target distances, and hands-on observation prompts.

### Current Main Risk
The prototype has enough systems to be interesting, and the automated validation loop is now green again. The remaining risk is play feel: real editor/mobile playtests must still confirm that route readability, map growth, reward timing, and HUD scaffolding feel good in motion.

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
Before entering play mode, generate the checklist:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/GenerateStagePlaytestChecklist.ps1
```

Use `F10` sweep or manual `F6/F7/F8/F9` controls to verify:
- stage size/grid/destructible counts grow as expected
- landmark districts appear at the intended stages
- camera clamp follows the rebuilt map bounds
- target markers stay readable and reachable
- LANE BREAK opens ROUTE HOLD clearly
- route trail pips are not noisy on a small mobile-style viewport
- ROUTE HOLD reward fires once and feels like a meaningful payoff

Done when:
- each stage band has one short note on readability, route pressure, and map identity
- at least one screenshot or written observation exists for Stage 1, Stage 4, and Stage 7
- `Logs/AlienCrusherStagePlaytestChecklist.md` has notes for Stage 1-7 or links to the matching screenshot/video captures

### P0 - ROUTE HOLD Readability Tuning
Tune only after playtest evidence:
- `routeHoldWindowSeconds`
- `routeHoldProgressThreshold`
- `routeHoldTrailPipCount`
- `routeHoldTrailMaxDistance`
- `routeHoldTrailMinPipSpacing`
- `routeHoldTrailCloseHideDistance`
- Target_A/Target_B placement rules

Done when:
- close-range route pips hide cleanly
- far target guidance remains readable
- route pressure feels urgent without turning into pure distance tax

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

---

## 4. Making The Core Loop More Fun

### Near-Term Experiments

#### 1. Route Open Moment
After LANE BREAK, add a stronger two-second "route opened" beat:
- HUD copy: `LANE BREAK -> ROUTE OPEN`
- stronger Target_A/Target_B pulse
- small camera/feedback pop
- route trail appears with a short delay instead of blending into ordinary HUD noise

Success signal:
- player can immediately tell that the next objective changed

#### 2. Reward Cluster Emphasis
Make ROUTE HOLD feel less like a score bonus and more like opening the next smash cluster:
- emphasize spawned barrels/transformers as route payoff objects
- make Forward Smash target copy and marker more direct
- result text should say what the route opened, not only how many points it gave

Success signal:
- ROUTE HOLD success visibly creates or reveals something worth chasing

#### 3. Route Progress Readability
Turn ROUTE HOLD progress into a faster-read mini meter or stronger HUD state:
- use color/pulse on existing route text or stage gauge
- keep numeric target/time, but do not rely on text alone
- avoid persistent pips at close range

Success signal:
- player can read "almost there" without parsing a sentence

#### 4. District Route Puzzles
Treat landmark districts as route behavior changes:
- Stage 2 park: forgiving open route and low clutter
- Stage 3 market: dense small-object chain route
- Stage 5 construction: explosive setup route
- Stage 6 power block: transformer payoff route
- Stage 7 skyline: long route into a high-value tower cluster

Success signal:
- later stages feel different by route decision, not only by size

#### 5. Shorter Failure Advice
Result and lobby advice should always answer the next run's first action:
- `OPENING FAILED`: hit the low-density starter lane first
- `ROUTE HOLD MISSED`: stay inside the beacon route after LANE BREAK
- `MID-RUN DRIFT`: choose the next cluster before speed drops
- `FINAL PUSH FAILED`: save burst/form power for the last target group
- `BOSS PHASE`: break pylons, then hit exposed core

Success signal:
- failure advice is actionable in one glance

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
- make LANE BREAK -> ROUTE HOLD -> ROUTE BONUS the primary satisfying loop

Includes:
- route open feedback beat
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
- map overlay/audit thresholds for landmark value, not only count

### Milestone 4 - Resource Feedback Pass
Goal:
- make the existing systems feel intentional and juicy with minimal asset cost

Highest-return resources:
- core audio starter pack
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
- Route trail pips may be visually noisy on small Android screens.
- Current implementation form names differ from older GDD form fantasy names; status documents should use runtime names until the design naming pass is resolved.
- `DummyFlowController` remains a large partial mega-controller and should not absorb more route/gameplay surface indefinitely.

---

## 7. Documentation Update Rules

When this roadmap changes:
- update `Docs/NEXT_SESSION_CONTEXT_PACKET.md` with the latest validation status
- keep `Docs/GDD_ALIEN_CRUSHER.md` focused on product design, not every temporary validation issue
- keep resource priorities in `Docs/CURRENT_STAGE_RESOURCE_REQUIREMENTS.md` and `Docs/CURRENT_STAGE_RESOURCE_PRODUCTION_BOARD.md`
- avoid declaring a Unity batch pass unless the report file timestamp advanced during that run
