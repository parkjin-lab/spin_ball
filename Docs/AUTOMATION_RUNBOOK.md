# Alien Crusher Automation Runbook

Last updated: 2026-06-10

Use this runbook when the creator is busy and agents should keep the project moving without making risky design-tuning decisions.

## Operating Principle

Fun work is currently rhythm work: opener -> pivot -> sustain -> payoff -> climax.

Before Evidence Green, agents should improve the project's ability to observe, explain, and prepare that rhythm. They should not tune rhythm numbers yet.

Evidence Green means:
- a real editor/development `F10` Stage 1-7 sweep exists in `Logs/AlienCrusherPlaytestTelemetry.log`
- `Logs/AlienCrusherPlaytestTelemetrySummary.md` was regenerated after the sweep
- `Docs/AlienCrusherStagePlaytestNotes.md` has meaningful Stage 1-7 notes
- Progression Save Smoke Pass has a concrete save/load result
- `Tools/TestPlaytestEvidenceGate.ps1` passes without `-ReportOnly`

## Default Autonomous Loop

1. Run:
   ```powershell
   powershell -ExecutionPolicy Bypass -File Tools/RunPlaytestReadinessPrep.ps1
   ```
2. If resource or production planning is the next safe lane, run:
   ```powershell
   powershell -ExecutionPolicy Bypass -File Tools/RunPlaytestReadinessPrep.ps1 -IncludeProductionChecklists
   ```
3. Read `Logs/AlienCrusherAutonomousWorkBacklog.md`.
4. Read `Logs/AlienCrusherResourceProductionBacklog.md` if the next safe lane is resource planning.
5. Read `Logs/AlienCrusherArchitectureExtractionPlan.md` if the next safe lane is architecture planning.
6. Read `Logs/AlienCrusherAutomationStatusSummary.md` for the current progress, validation, blocker, and next to-do snapshot.
7. Choose one safe autonomous task from the backlog.
8. Make the smallest useful tooling, documentation, checklist, or static-audit improvement.
9. Run focused validation, then `Tools/RunStaticAudits.ps1`.
10. Update `Docs/NEXT_SESSION_CONTEXT_PACKET.md` and `Docs/GAME_UPDATE_ROADMAP.md` if the next safe task changed.

## Safe Work Before Evidence Green

- Improve readiness prep output and evidence-gate diagnostics.
- Improve generated checklist wording and missing-evidence instructions.
- Keep `Tools/RunStaticAudits.ps1` and readiness regression tests green.
- Expand resource production planning from existing runtime hooks.
- Generate or refine `Logs/AlienCrusherResourceProductionBacklog.md` from existing production checklists.
- Generate or refine `Logs/AlienCrusherArchitectureExtractionPlan.md` from `DummyFlowController` partial ownership.
- Generate or refine `Logs/AlienCrusherAutomationStatusSummary.md` so heartbeats have one concise current-state artifact.
- Add static or regression coverage for scripts and reports.
- Prepare architecture extraction notes for `DummyFlowController` ownership boundaries.
- Refresh handoff docs so the next agent starts from current facts.

## Blocked Work Before Evidence Green

- Route timing tuning.
- ROUTE HOLD threshold or target-count tuning.
- Stage-specific rhythm presets.
- Payoff cluster radius/count tuning.
- Target placement policy changes.
- Boss pressure or breathing-window tuning.
- Broad multi-variable balance passes.

## Sub-Agent Roles

- Core loop explorer: check whether opener, pivot, sustain, payoff, and climax are observable in code/docs.
- Automation explorer: check whether the next unattended task is explicit and covered by a script or doc.
- Resource planner: turn generated production checklists into prioritized missing resource tables.
- Architecture scout: map `DummyFlowController` partial responsibilities and propose extraction order, without changing behavior.

## Decision Rule

If a proposed autonomous task does not help the player understand what to smash, what to chase, what changed, or what they earned, defer it.

If the task changes play feel, wait for Evidence Green.
