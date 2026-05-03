# Alien Crusher - Current Stage Resource Production Board

## Goal
Turn the current resource requirements into an immediately usable production board.

This document answers four practical questions:
- what must be produced first
- what can be mocked with Unity primitives now
- what should be postponed
- what output is expected from each asset task

Related docs:
- `Docs/CURRENT_STAGE_RESOURCE_REQUIREMENTS.md`
- `Docs/GDD_ALIEN_CRUSHER.md`

---

## 1. Current Production Strategy
At the current prototype stage, we should not aim for asset completeness.
We should aim for:
- stronger gameplay readability
- stronger destruction payoff
- stronger boss identity
- stronger outgame motivation with minimal art cost

Decision rule:
- if a resource improves `what to smash / what to fear / what to chase / what you earned`, make it now
- if a resource is mostly decorative, postpone it

---

## 2. Immediate Work Queue
These are the next resource tasks with the best return.

### A1. Form Identity Pass
Priority: `P0`

Deliverables:
- 5 form icons
- 5 form silhouette addon kits
- 5 form color/material presets

Expected output:
- player can distinguish all forms at a glance
- form selection in lobby feels desirable
- form swap during run feels meaningful

Unity-native fallback allowed now:
- sphere: keep primitive sphere, add emissive ring / band
- spike: radial cone array
- ram: wedge nose + side horns
- saucer: flattened sphere + ring disk
- crusher: layered heavy shell + frontal plate

Suggested file targets:
- `Assets/Art/Forms/`
- `Assets/Resources/UI/Forms/`
- `Assets/Settings/Materials/Forms/`

Done when:
- every form has unique silhouette from gameplay camera distance
- every form has unique icon in UI
- every form has unique color family

### A2. Destruction Readability Pass
Priority: `P0`

Deliverables:
- small / medium / large / boss material tiers
- weak point material
- shield material
- exposed-core material
- crack/damage tint presets

Expected output:
- players can tell easy targets from high-value targets
- damage states become readable before destruction
- boss objects are visually distinct from ordinary buildings

Unity-native fallback allowed now:
- no texture painting required
- use color, emission, fresnel-like fake rim, alpha smoke particles, scale punch

Suggested file targets:
- `Assets/Art/Materials/Destruction/`
- `Assets/Art/VFX/Destruction/`

Done when:
- ground and buildings never blend together visually
- large structures look meaningfully tougher than small ones
- weak points and shield states read instantly

### A3. Street Prop Variety Pass
Priority: `P0`

Deliverables:
- 8 to 10 lightweight prop silhouettes
- 3 vehicle body variants minimum
- 5 breakable roadside prop variants minimum

Expected output:
- city feels alive before the player reaches large buildings
- early gameplay has enough crushable density
- retail frenzy / panic / strip clear systems gain stronger visual support

Unity-native fallback allowed now:
- car: cubes + wedges + cylinders
- van/bus: elongated box silhouette
- lamp: cylinder + capsule
- kiosk: stacked cube silhouette
- tree: cylinder + low-poly sphere cluster
- transformer: box + pipe silhouette
- explosive barrel: cylinder + color band

Suggested file targets:
- `Assets/Art/Props/Traffic/`
- `Assets/Art/Props/Street/`
- `Assets/Art/Props/Utilities/`

Done when:
- early map sections contain enough low-cost crush targets
- props are readable even without custom textures
- at least 3 prop categories visibly react differently when broken

### A4. Core Audio Starter Pack
Priority: `P0`

Deliverables:
- hit set
- destruction set
- skill set
- boss set
- UI set

Expected output:
- impact scale is felt through sound
- different gameplay states stop feeling samey
- boss and progression moments gain payoff

Must-cover events:
- light hit
- heavy hit
- small break
- heavy collapse
- chain up
- overdrive start
- level up open
- level up confirm
- panic jackpot
- retail frenzy
- seismic burst
- landing shockwave
- boss shield
- boss core open
- boss phase transition
- boss down
- UI click / locked / confirm / fail

Suggested file targets:
- `Assets/Audio/SFX/Impact/`
- `Assets/Audio/SFX/Skills/`
- `Assets/Audio/SFX/Boss/`
- `Assets/Audio/SFX/UI/`

Done when:
- silent critical moments no longer exist
- small hit / big hit / collapse are clearly separable by ear
- boss phase changes are recognizable without looking at UI

---

## 3. Secondary Queue
These should start after the immediate queue is stable.

### B1. Boss Identity Pass
Priority: `P1`

Deliverables:
- Justice Sentinel silhouette kit
- shield pylon kit
- phase 2 drone kit
- boss-only warning particle pass
- boss-only audio layer refinement

Done when:
- boss no longer reads as a large building with special rules
- phase 1 / break window / phase 2 are visually separable

### B2. UI Icon Pass
Priority: `P1`

Deliverables:
- DP icon
- stage icon
- route icon
- break window icon
- shield icon
- weak point icon
- overdrive icon
- panic icon
- seismic icon
- boss icon

Done when:
- HUD and lobby rely less on text-only recognition
- result and lobby states scan faster on mobile

### B3. District Palette Pass
Priority: `P1`

Deliverables:
- district 1 palette
- district 2 palette
- district 3 palette
- district 4+ fortified palette

Done when:
- stage difficulty also feels like spatial escalation
- screenshots from different stage bands no longer look interchangeable

---

## 4. Deferred Queue
These are useful later but not critical now.

Priority: `P2`
- cinematic splash background
- premium logo pass
- detailed skybox work
- decorative façade variety beyond readability need
- non-essential ambient props
- polished menu scene art
- premium-only VFX layering

Reason:
- these will not improve current core fun as much as readability, feedback, and boss payoff

---

## 5. Unity-Only Mock Plan
If we continue without custom external art, use this temporary production plan.

### Forms
- build all form silhouettes from primitive combinations
- assign unique materials and emissive accents
- generate temporary icons from in-editor screenshots

### Buildings
- use 4 material tiers only
- derive all building classes from scale + color + emissive weak point differences

### Props
- build vehicles and street props from cubes, cylinders, capsules, spheres
- use break reaction timing and particles to separate categories

### UI
- use flat icon placeholders or letter-icons first
- only replace with final icons once hierarchy and UX are stable

### Audio
- if no final pack exists, source temp placeholders internally and keep event list fixed
- do not wait for polished audio before hooking logic

---

## 6. Resource-to-System Mapping
This is the fastest way to understand why each asset matters.

| System | Resource Needed | Why |
|---|---|---|
| Form unlock / selection | icons, silhouettes, materials | makes unlock motivation real |
| Core destruction loop | material tiers, damage VFX, hit SFX | improves readability and smash payoff |
| Early-stage fun | small props, vehicles, break reactions | gives enough crush density before big buildings |
| Retail Frenzy / Strip Clear | shop props, signage, frenzy burst, frenzy SFX | supports combo lane fantasy |
| Traffic Panic | car variants, panic SFX, chain reaction prop feedback | makes chaos feel systemic |
| Overdrive / Seismic / Landing Shockwave | distinct burst VFX and audio | separates special states from normal combat |
| Boss | sentinel kit, pylon kit, drone kit, boss-only VFX/SFX | creates stage climax identity |
| Lobby / Meta | form cards, meta nodes, badges, unlock banners | strengthens return loop |

---

## 7. Suggested Weekly Execution Order
If one person is executing alone, use this order.

### Day 1
- form silhouette pass
- form color/material pass
- icon placeholder pass

### Day 2
- destruction material tiers
- weak point / shield / exposed-core material pass

### Day 3
- street prop primitive kit
- traffic vehicle variants

### Day 4
- core gameplay SFX hookup list
- boss identity primitive kit

### Day 5
- UI icon placeholders
- lobby / result / progression reward badge placeholders

This order keeps the game becoming more fun every day instead of waiting for a large art batch.

---

## 8. Production Checklist By Ownership
Use this if tasks need to be split.

### Gameplay Readability Owner
- [ ] form silhouette pass
- [ ] destruction material tiers
- [ ] weak point/shield/core state pass
- [ ] icon placeholder set

### City Feel Owner
- [ ] small prop kit
- [ ] traffic vehicle kit
- [ ] district palette pass

### Juice / Feedback Owner
- [ ] hit / break / collapse SFX
- [ ] frenzy / panic / seismic / overdrive SFX
- [ ] boss warning / phase / break / death SFX
- [ ] VFX variant pass for destruction moments

### Progression UX Owner
- [ ] lobby form card states
- [ ] meta upgrade chip states
- [ ] unlock / stage clear / reward badges

---

## 9. Immediate Recommendation
If we act on this document right away, the best next practical step is:
1. create primitive-based form silhouette kits and icon placeholders
2. create destruction material tiers
3. create street prop primitive kit

This is the fastest path to making the current build look more intentional without waiting on a full asset pipeline.
