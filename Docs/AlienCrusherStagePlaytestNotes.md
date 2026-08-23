# Alien Crusher Stage Playtest Notes

Last updated: 2026-08-23

Use this tracked file for human observations from the first real Stage 1-7 `F10` sweep. The generated checklist in `Logs/AlienCrusherStagePlaytestChecklist.md` is a disposable readiness artifact and can be regenerated at any time.

## Sweep Metadata

- Date / build:
- Tester:
- Viewport / device:
- Checklist generated at:
- Telemetry log:
- Telemetry summary:

## Minimum Evidence Gate

- [ ] `Logs/AlienCrusherPlaytestTelemetry.log` exists after the sweep.
- [ ] `Logs/AlienCrusherPlaytestTelemetrySummary.md` was regenerated after the telemetry log.
- [ ] `SWEEP_START` exists.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 01.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 02.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 03.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 04.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 05.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 06.
- [ ] `STAGE_START` and `STAGE_END` exist for Stage 07.
- [ ] `SWEEP_END` exists.
- [ ] `Tools/TestPlaytestEvidenceGate.ps1` passes without `-ReportOnly`.

## Progression Save Smoke Pass

- [ ] Starting DP / selected form / highest unlocked stage / previewed meta upgrade noted.
- [ ] DP or stage progress changes after a clear.
- [ ] Affordable form unlock or meta upgrade purchase persists, or locked/need-DP state is readable when not affordable.
- [ ] Exit and re-enter play mode keeps DP, selected form, stage unlock, and meta upgrade state.
- [ ] Edited/restored save data with impossible values is repaired and does not reappear after reload.
- [ ] Save/load result:

## Stage Notes

Each note field should be a short observation, not a one-word status. Write enough detail to explain what was readable, confusing, fair, unfair, distinct, or flat.

Evidence-quality examples:
- Readability: "Opening lane was readable because the low-rise row pulled forward, but the route marker disappeared behind tall props after LANE BREAK."
- Route pressure: "HOLD felt fair until the last 10 seconds; the player reached the beacon but ran out of nearby crushable objects."
- Map identity: "Stage 4 reads as a Sentinel checkpoint because pylons and barricades frame the route before the boss beat."
- Rhythm identity: "Opener is calm, pivot is clear, sustain is noisy, payoff lands late; this stage's broken beat is sustain."
- Screenshot/video reference: "screenshot path, video timestamp, or short capture note that can be checked later."

### Stage 01

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

### Stage 02

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

### Stage 03

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

### Stage 04

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

### Stage 05

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

### Stage 06

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

### Stage 07

- Readability:
- Route pressure:
- Map identity:
- Rhythm identity:
- Screenshot/video reference:

## Post-Sweep Decision

- Primary bottleneck / dominant broken beat:
- Tune these fields first / one variable family to change next:
- Current values copied from `Tune Next`:
- Chosen first-pass experiment:
- Retest stage(s):
- Do not touch yet:

## Do Not Tune Yet

- [ ] Stage-specific rhythm presets
- [ ] Payoff layout rhythm
- [ ] Boss breathing windows
- [ ] Multiple variable families at once

## Qualitative Playtest (2026-08-23)

This is creator feel notes only. It is **not** a full `F10` telemetry sweep. Stage 01-07 evidence fields above stay empty on purpose. Do not treat this section as Evidence Green.

- Date: 2026-08-23
- Tester: 진웅 박
- Scope: qualitative Stage 1 playtest; no telemetry log captured
- Feel: fun
- Growth UI pops too often
- HUD / copy text is too long and hard to read
- Map feels too small and ends too fast
- Destruction pleasure needs more variety
- UI still looks unfinished

Follow-up batches after this note: HUD/copy/toast readability first. Map size, route timing, and destruction variety stay later. No invented `F10` markers.
