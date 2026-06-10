# Alien Crusher - Game Design Gap Policy

Last updated: 2026-06-10

## Purpose

This document records the current design gaps found by role-based sub-agent review and turns them into production policy.

The project already has a clear direction:

```text
Stage Start
  -> Starter lane crush
  -> LANE BREAK
  -> ROUTE OPEN
  -> ROUTE HOLD
  -> ROUTE BONUS / Forward Smash
  -> Result feedback and growth choice
```

The main problem is not lack of systems. The main problem is that the fun rhythm is still a design hypothesis until real Stage 1-7 playtest evidence proves it.

## Sub-Agent Review Roles

- Rhythm/Core Loop Review: checked whether the current loop has real opener -> pivot -> sustain -> payoff -> climax cadence.
- Map/Content Growth Review: checked whether map growth creates new route decisions, not only bigger layouts.
- Feedback/Sensory Review: checked HUD, camera, audio, haptics, failure feedback, and mobile readability.
- Production/Validation Review: checked whether future tuning decisions have enough evidence gates and document rules.

## Executive Diagnosis

Alien Crusher is close to a stronger prototype because its current loop is no longer plain score chasing. It has route state changes, growing maps, failure buckets, telemetry, and validation tools.

The weak points are:

- Real `F10` Stage 1-7 telemetry is still missing, so rhythm quality is unproven.
- ROUTE HOLD may feel like a timed destruction count instead of a true route-reading test; first-pass route-adherence telemetry now exists, but evidence has not been captured yet.
- Landmarks now have first-pass static value records for role, route relationship, payoff mix, entry lane, and exit lane, but this still needs playtest/screenshot confirmation.
- Stage 4 now has a first-pass Sentinel checkpoint landmark, but its boss-approach readability is still unproven until screenshot/playtest evidence confirms it reads without HUD text.
- Audio hooks now exist as a first-pass runtime layer, but clips/assets and balance are still missing.
- HUD has first-pass mobile text safeguards, but real device/screenshot readability is still unproven.
- Failure feedback explains the problem, but does not yet make the failure beat strongly felt through sound/haptics/screen rhythm.
- Production rules now enforce "do not tune before evidence" through `Tools/TestPlaytestEvidenceGate.ps1`, including telemetry, notes, and progression save smoke results.

## P0 Policy - Evidence Before Rhythm Tuning

Do not tune route timing, stage-specific rhythm presets, payoff layouts, target placement policy, or boss breathing windows until a real Stage 1-7 playtest exists.

Required evidence:

- `Logs/AlienCrusherPlaytestTelemetry.log` exists from an editor/development run.
- `Logs/AlienCrusherPlaytestTelemetrySummary.md` was regenerated after the telemetry log.
- `SWEEP_START` and `SWEEP_END` exist.
- Stage 01-07 each have `STAGE_START` and `STAGE_END`.
- Stage 01, Stage 04, and Stage 07 have screenshot/video references or concrete written observations.
- `Docs/AlienCrusherStagePlaytestNotes.md` contains meaningful notes for readability, route pressure, map identity, and rhythm identity.
- `Docs/AlienCrusherStagePlaytestNotes.md` contains a completed Progression Save Smoke Pass `Save/load result`.
- `Logs/AlienCrusherPlaytestTelemetrySummary.md` route-adherence lines are reviewed for closest target distance, average distance, farthest distance, in-range percentage, and elapsed route time.
- `Tools/TestPlaytestEvidenceGate.ps1` passes without `-ReportOnly`.

Allowed before evidence:

- documentation updates
- static audit improvements
- telemetry wiring fixes
- readiness/report generator fixes
- scene validation/repair work
- non-tuning UI bug fixes
- autonomous backlog/runbook updates that make the next safe unattended task clearer

Blocked before evidence:

- `routeHoldWindowSeconds`
- `routeHoldProgressThreshold`
- `routeOpenBeatSeconds`
- `routeRewardClusterRadius`
- `routeRewardClusterPropCount`
- stage rhythm presets
- payoff layout rhythm changes
- boss pressure/breathing window tuning
- route target placement rules

## P0 Policy - One Broken Beat At A Time

After a real sweep, choose exactly one dominant broken beat:

1. Opener: first lane is unclear or unsatisfying.
2. Pivot: LANE BREAK -> ROUTE OPEN does not read as a state change.
3. Sustain: ROUTE HOLD is unreadable, unfair, or feels like distance tax.
4. Payoff: ROUTE BONUS / Forward Smash does not feel earned or visible.
5. Climax: final push or boss phase is noisy, flat, or too constant.

Then choose one variable family only:

- opener density / starter lane
- route guidance / marker readability
- route hold timing / progress threshold
- payoff cluster radius / count / role
- stage identity / landmark route policy
- boss breathing / warning / punish windows
- HUD/audio/haptic feedback

Retest only the affected stages before widening the pass.

## P0 Policy - Route Hold Must Become A Route Test

ROUTE HOLD currently has strong HUD support and telemetry, but the risk is that it feels like "destroy enough things before a timer ends" rather than "read and hold the route."

Future implementation should separate two concepts:

- Count goal: how many objects must be destroyed to clear the hold.
- Route goal: whether the player is reading, approaching, and staying oriented around the beacon.

Telemetry now records:

- distance to active route marker at `ROUTE_OPEN`
- closest distance reached before `ROUTE_HOLD_CLEAR`
- average and farthest marker distance during active hold
- in-range sample percentage during active hold
- elapsed route time before clear or stage end
- whether reward cluster was reached after `ROUTE_BONUS`

Design rule:

- If the player succeeds by ignoring the route marker, the route design failed.
- If the player follows the marker but cannot find enough crushable objects, the map layout failed.
- If both happen inconsistently, tune readability before tuning numbers.

## P0 Policy - Sensory Feedback Must Carry Rhythm

The game needs sound and feedback to make pressure and release readable.

Minimum audio hook policy:

- hit light / medium / heavy
- small break / large collapse
- combo rise
- route open
- route hold warning
- route bonus
- Forward Smash
- boss shield up
- boss core exposed
- boss phase transition
- boss down
- UI tap / locked / fail

Failure feedback policy:

- `OPENING FAILED`: short drop, low-impact thud, no long punishment.
- `ROUTE HOLD MISSED`: warning pulse, medium haptic, route marker fade/drop.
- `MID-RUN DRIFT`: speed-down feel, softened combo tone.
- `FINAL PUSH FAILED`: timer compression tone, short hard stop.
- `BOSS PHASE`: heavier warning, distinct boss low tone, delayed result reveal.

Accessibility policy:

- Add or preserve paths for Reduced Shake, Reduced Flash, and Haptics Off before stacking more feedback layers.
- FEEL/MMFeedbacks should be reserved first for high-value moments: route bonus, level up, boss core exposure, boss down, and failure beat.

## P0 Policy - Mobile HUD Readability

HUD policy for mobile portrait:

- primary state should read as one label plus one gauge/icon
- route indicator should stay one line
- result advice should fit as one first action plus one short reason
- long explanatory text should move to generated docs or debug logs, not combat HUD

Required viewport checks when UI changes:

- 1080x1920
- 720x1600
- 1080x2400
- one notch/safe-area profile

Fail the pass if:

- route indicator overlaps other HUD text
- result advice needs more than two short lines before the action is clear
- button labels or HUD counters overflow their containers
- text replaces an icon where icon recognition would be faster

## P1 Policy - Map Growth Must Create New Decisions

Static map audits currently prove map growth, target bounds, landmark counts, minimum density, and first-pass landmark value records. That is necessary but not sufficient.

Landmark policy:

- Stage 2 Pocket Park: forgiving route recovery and low-pressure chain.
- Stage 3 Market Plaza: tight chainable props and quick sustain.
- Stage 4 Boss Approach: add a defense-front or pylon-foreshadow identity instead of only extending market feel.
- Stage 5 Construction Yard: wider blast spacing and large payoff setup.
- Stage 6 Power Block: longer route commitment and transformer corridor payoff.
- Stage 7 Skyline Block: asymmetric high-value anchor and late pressure.

Target placement policy:

- landmark-entry target: pulls the player into a new district
- payoff-exit target: exits through the reward cluster
- return-pressure target: asks the player to recover direction under time pressure

At least one Stage 5-7 target should make the player read a landmark route rather than only travel farther.

Future audit policy:

- do not only count landmarks
- record landmark role, route proximity, payoff object mix, entry lane, exit lane, and target relationship
- treat static landmark value records as a review scaffold, not as proof that the landmark reads in motion

## P1 Policy - Stage 4 Needs Its Own Identity

Stage 4 is a design risk because it sits between Stage 3 market identity and Stage 4+ boss systems.

Stage 4 should become the "boss approach" stage. Current first pass:

- runtime map generation adds a Sentinel checkpoint landmark tier at Stage 4
- checkpoint pieces include pylon foreshadowing, barricades, warning beacons, and a gate block
- static map audit now mirrors the Stage 4 landmark center and minimum landmark-count tier

Stage 4 still needs evidence that it achieves the goal:

- introduce defense-front silhouettes or pylon foreshadow props
- leave more deliberate breathing space near the central route
- teach shield/core visual language before the real boss pressure peaks
- avoid simply being a larger market stage

Success signal:

- without reading the HUD, the player can tell Stage 4 is preparing them for a different kind of pressure.

## P1 Policy - Resource Work Should Follow Gameplay Meaning

Do not add generic content first. Add assets that clarify decisions.

Priority order:

1. Audio starter hooks and temporary clips.
2. HUD/status icon set for route, boss, weak point, DP, stage pressure.
3. Form silhouettes and form icons.
4. Destruction tier materials and damage states.
5. Role-based street prop silhouettes.
6. Boss identity silhouettes and warning VFX.
7. District palette sets.

Street prop roles:

- chain filler: easy low-resistance objects for flow
- explosive prep: barrels and transformers that promise payoff
- guide props: visual lanes toward route decisions
- hazard props: interrupt or punish lazy pathing
- anchor props: high-value targets that define the payoff

## P1 Policy - Production Gates

Recommended gate tiers:

- Readiness Green: `Tools/RunStaticAudits.ps1` passes.
- Runtime Green: `Tools/RunUnityBatchChecks.ps1` refreshes scene and map reports.
- Evidence Green: `Tools/TestPlaytestEvidenceGate.ps1` verifies real telemetry, summary freshness, Stage 1-7 markers, stage notes, and progression save smoke result.
- Decision Green: `Post-Sweep Decision` is filled with bottleneck, variable family, current values, experiment, retest stages, and do-not-touch list.

Tuning should require Readiness Green and Evidence Green.
Scene/map structural changes should require Readiness Green and Runtime Green.
Documentation/readiness changes can proceed with Readiness Green only.

## P2 Policy - Boss Is The Climax, Not Constant Harassment

Boss systems already have enough ingredients: shield, pylons, core exposure, phase 2 drones, pressure pulses, and defeat cascade.

Future boss tuning should follow:

- breathe: readable low-pressure approach
- warn: clear shield/core/phase signal
- burst: short danger or punish window
- release: obvious opening after the player solves the beat
- climax: final cascade that pays off the run

Do not tune boss pressure until the base Stage 1-7 route loop has real evidence.

## Immediate Backlog From This Review

### P0

- Run the first real Stage 1-7 `F10` sweep.
- Keep `Tools/TestPlaytestEvidenceGate.ps1` as the blocking gate for telemetry/summary/notes coverage.
- Fill the Progression Save Smoke Pass before accepting Evidence Green.
- Keep tuning lock active until evidence exists.
- Source temporary clips for the existing runtime audio hook surface.
- Confirm HUD mobile overflow/readability with screenshots after the new static text safeguards.

### P1

- Confirm landmark value records against real screenshots/playtest notes and refine the audit only from evidence.
- Confirm Stage 4 Sentinel checkpoint readability with screenshot/playtest evidence and refine only from evidence.
- Use the new route-adherence telemetry fields during the first sweep review, then decide whether ROUTE HOLD is a path-readability problem or a timer/target-count problem.
- Convert HUD state priority toward icon/gauge recognition.
- Add failure beat sensory policy to implementation backlog.

### P2

- FEEL/MMFeedbacks presets for high-value moments.
- Reduced Shake / Reduced Flash / Haptics Off accessibility pass.
- Stage palette sets and role-based prop silhouette expansion.
- Boss breathing window tuning after base route loop evidence.

## Working Rule

When in doubt, ask:

```text
Does this change make the player better understand what to smash, what to chase, what changed, and what they earned?
```

If the answer is no, it is not the next important game design change.
