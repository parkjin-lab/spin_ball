# Alien Crusher - Game Update Roadmap

Last updated: 2026-05-17

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
- LANE BREAK and ROUTE HOLD are wired as the current mid-run tempo layer: route targets, `LANE BREAK -> ROUTE OPEN` feedback, HUD guidance, world beacon, route trail pips, route reward, FORWARD SMASH cluster payoff, result badges, and lobby/meta recommendations are connected.
- Failure result and lobby recommendation copy now start with one bucket-specific first action before explaining the upgrade reason.
- Editor/development playtests now emit `[AlienCrusher][Playtest]` console lines and append the same route telemetry to `Logs/AlienCrusherPlaytestTelemetry.log` for `SWEEP_START`, stage start, route open, route hold clear, route bonus, forward smash, stage end, and `SWEEP_END`.
- `Tools/GeneratePlaytestTelemetrySummary.ps1` can convert the telemetry log into a markdown report with a current tuning snapshot, rhythm snapshot, `Tune Next` decision block, sweep-level summaries, stage trend rollups, tuning candidates, first-pass experiment suggestions, failure bucket actions, and per-run breakdowns for faster Stage 1-7 review.
- Runtime map rebuilds happen at stage start. Stage 1-7 currently grow from a compact starter layout into wider, denser maps with expanded target marker spacing and stage-gated landmarks.
- Stage-gated landmark districts currently include Stage 2 pocket park, Stage 3 market plaza, Stage 5 construction yard, Stage 6 power block, and Stage 7 skyline block.
- Stage 4+ boss flow exists around Justice Sentinel, shield pylons, core exposure, break windows, phase 2 drones, pressure pulses, and defeat cascade.
- Validation tools now include Unity scene validation/repair, Unity runtime map layout audit entry points, Unity-free static audits, and safer Unity batch wrappers.

### Current Validation Status
- Latest scene validation report from 2026-05-05 21:14 in `Logs/AlienCrusherSceneValidation.log` shows `0 error(s), 0 warning(s)`.
- `Assets/Scenes/SampleScene.unity` contains `HudRouteArrow` with child `ArrowText`, and `Tools/AuditSceneEssentialsStatic.ps1` verifies those scene essentials with `0 warning(s)`.
- Unity-free static map audit passes Stage 1-7 formula checks with `0 warning(s)`.
- Unity-free ROUTE HOLD tuning audit passes with `0 warning(s)` and reads current default tuning values, including route open beat timing, from runtime C# fields before auditing.
- `Tools/RunStaticAudits.ps1` passes the current Unity-free audit set and fails if an expected report is missing or not refreshed during the run.
- Unity-free static audits now include a playtest telemetry wiring check so runtime `F10` event names and telemetry summary parser expectations stay aligned before manual tuning starts.
- Runtime Unity map layout batch report from 2026-05-05 21:15 in `Logs/AlienCrusherMapLayoutAudit.log` covers Stage 1-7 with `0 error(s), 0 warning(s)`.
- `Tools/RunUnityBatchChecks.ps1` passed both scene validation and runtime map layout audit with refreshed report/log timestamps.
- `Tools/GenerateStagePlaytestChecklist.ps1` generates `Logs/AlienCrusherStagePlaytestChecklist.md`, combining current validation results, Stage 1-7 map growth, route targets, route pressure, target distances, and hands-on observation prompts.
- `Tools/GeneratePlaytestTelemetrySummary.ps1` now includes a rhythm snapshot, but no real Stage 1-7 sweep evidence has been captured yet.
- As of 2026-05-17, no real `F10` sweep telemetry log exists yet. The next required evidence artifacts are `Logs/AlienCrusherPlaytestTelemetry.log`, regenerated `Logs/AlienCrusherPlaytestTelemetrySummary.md`, and populated Stage 1 / 4 / 7 checklist notes.

### Current Main Risk
The prototype has enough systems to be interesting, and the automated validation loop is now green again. The remaining risk is play feel: real editor/mobile playtests must still confirm that route readability, map growth, reward timing, HUD scaffolding, and the opener -> pivot -> sustain -> payoff -> climax rhythm all feel good in motion instead of flattening into constant pressure.

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

Required next artifacts:
- `Logs/AlienCrusherPlaytestTelemetry.log`
- regenerated `Logs/AlienCrusherPlaytestTelemetrySummary.md` with `Tune Next` based on real run data
- populated Stage 1 / 4 / 7 notes in `Logs/AlienCrusherStagePlaytestChecklist.md` or linked screenshots/videos

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

Done when:
- each stage band has one short note on readability, route pressure, map identity, and rhythm identity
- at least one screenshot or written observation exists for Stage 1, Stage 4, and Stage 7
- `Logs/AlienCrusherStagePlaytestChecklist.md` has notes for Stage 1-7 or links to the matching screenshot/video captures

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
Baseline district route payoff identity is now implemented:
- Stage 2 park: forgiving bench/tree/barrel payoff
- Stage 3-4 market: kiosk/vending/barrel chain payoff
- Stage 5 construction: barrel-heavy yard blast payoff
- Stage 6 power block: transformer-heavy power surge payoff
- Stage 7 skyline: transformer/barrel cluster plus high-value skyline anchor

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
- The route loop may still flatten into uniform pressure if district-to-district rhythm variation is too weak.
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
