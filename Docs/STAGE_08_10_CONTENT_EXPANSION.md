# Alien Crusher - Stage 08-10 Content Expansion

Last updated: 2026-08-19

## Goal

Stages 8-10 form a late-run three-act content pack. The pack expands map size and object variety while preserving the proven Stage 1-7 layout curve. Each stage adds a different destruction rhythm instead of only raising health or density.

## Content Arc

| Stage | District | Map/Grid | Primary Rhythm | Landmark Payoff |
|---|---|---:|---|---|
| 08 | Transit Hub | 66m / 20x20 | Read lanes, commit to a shuttle chain, rotate out | Three destructible shuttles, route displays, four gates |
| 09 | Harbor Yard | 70m / 21x21 | Prime a container lane, trigger fuel, cash out | Six containers, four explosive fuel props, crane silhouette |
| 10 | Civic Core | 74m / 22x22 | Circle, break pylons, expose the center | Four ring pylons and a high-value uplink tower |

Stage 1-7 remains unchanged at 44-62m and 13x13-19x19. Late expansion adds 4m and one grid row/column per stage.

## Fun Policy

- New stages must introduce a new decision pattern, not only more hit points.
- Stage 08 teaches a straight, readable chain lane.
- Stage 09 turns the chain into a volatile setup-and-payoff beat.
- Stage 10 changes the route into a ring-to-center finale.
- Existing route, combo, boss, and timing values stay locked until real telemetry exists.
- Stage 9-10 objective density uses the Stage 8 baseline cap of 168 destructibles, preventing map growth from becoming a raw ROUTE HOLD workload spike.
- Late-stage objects reuse current destructible, explosive, and reactive systems so the pack remains playable without external assets.

## Runtime Content

### Stage 08 - Transit Hub

Gameplay intent:
- create long visual lanes that invite a committed acceleration line
- use shuttles as medium-heavy chain anchors
- use gates as short recovery beats after a shuttle break

Production resources:
- transit shuttle silhouette
- route display material
- gate/turnstile silhouette
- transit arrival and heavy shuttle break SFX
- cyan route-line palette variant

### Stage 09 - Harbor Yard

Gameplay intent:
- let players choose between safe container damage and a risky fuel-chain cash-out
- create a broad outer-district destination before returning to the route
- make explosive sequencing visually obvious

Production resources:
- two container silhouette variants
- fuel drum hazard material
- crane silhouette
- container collapse and fuel ignition SFX
- harbor asphalt, rust, and warning-stripe palette

### Stage 10 - Civic Core

Gameplay intent:
- produce a clear ring-to-center finale
- make pylons readable as the setup beat and the uplink as the payoff beat
- provide a visual climax without changing boss tuning speculatively

Production resources:
- civic pylon silhouette
- energy-band emissive material
- uplink tower/crown silhouette
- pylon break and uplink collapse SFX
- white civic stone, dark metal, cyan/red energy palette

## Acceptance Criteria

Automated:
- Stage 01-10 map sizes and grid counts never regress.
- Spawn, route targets, and every active landmark remain inside map bounds.
- Landmark route relationships pass the static map audit.
- The generated playtest checklist includes Transit Hub, Harbor Yard, and Civic Core.

Human evidence, when Unity access is available:
- capture one screenshot each for Stage 08, 09, and 10
- confirm the intended lane/volatile/ring rhythm is visible without reading documentation
- record whether each landmark creates a distinct route decision
- record frame pacing and object-count spikes on Stage 10
- do not tune health, timers, or boss pressure until these observations and telemetry exist

## Next Expansion Candidates

After this pack is visually and rhythmically validated:
1. Add one district-specific reactive behavior per stage.
2. Add a late-run event deck that selects one modifier from a small authored pool.
3. Add a second late-run boss or boss variant only after Stage 10 route clarity is stable.
4. Add alternate landmark layouts using deterministic seeds, with the authored layout kept as the baseline.