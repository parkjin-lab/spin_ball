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

Sphere starter shipped:
- `FORM_Sphere_Body_Kit` runtime cool-green body plus emissive belt
- `Icon_Form_Sphere` lobby/HUD circle-with-orbit-band thumbnail
- `Icon_Skill_SpherePulse` SPHERE PULSE ring cue
- draft sprites at `Assets/Resources/UI/Icons/`
- in-world pulse mark `VFX_SpherePulse_Mark` plus belt flash; cooldown/damage unchanged

Ram / Saucer route helpers shipped:
- `FORM_Ram_Body_Kit` dark shell, amber wedge plate, and side horns
- `FORM_Saucer_Body_Kit` wide cyan rim over a pale disc
- `Icon_Form_Ram` / `Icon_Form_Saucer` lobby thumbnails
- `Icon_Skill_RamBreach` / `Icon_Skill_SaucerDash` action-button cues
- in-world marks `VFX_RamBreach_Mark` and `VFX_SaucerDash_Mark`; skill numbers unchanged

Spike / Crusher damage fantasies shipped:
- `FORM_Spike_Body_Kit` lean dark core, longer forward/up needles, acid tips
- `FORM_Crusher_Body_Kit` layered steel bulk, flat frontal plate, blue pressure seams
- `Icon_Form_Spike` / `Icon_Form_Crusher` lobby thumbnails
- `Icon_Skill_SpikeBurst` / `Icon_Skill_CrusherSlam` action-button cues
- in-world marks `VFX_SpikeBurst_Mark` and `VFX_CrusherSlam_Mark`; skill numbers unchanged

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
- `Assets/Resources/UI/Icons/`

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

Building tiers shipped:
- `MAT_Building_Small` pale plaster for props and low-rises
- `MAT_Building_Mid` warm city concrete for standard fillers
- `MAT_Building_Large` charcoal mass for durable targets
- `MAT_Boss_Structure` cool steel-blue armor for Sentinel / gate / pylon hosts
- runtime assignment only; HP, break thresholds, and spawn counts unchanged
- Stage 1 is mostly plaster + concrete; Stage 4 adds dark mass plus steel-blue boss structures at the Sentinel checkpoint

Combat states shipped:
- `MAT_Damage_CrackOverlay` dark split lines that appear as buildings take damage
- `MAT_WeakPoint_Glow` gold pip plus halo so elite crit spots are not ordinary props
- `MAT_Shielded_Pylon` cool cyan barrier on live pylon panes / caps
- `MAT_Exposed_Core` hot orange-white chest/core window when shields drop
- runtime assignment only; HP, break thresholds, shield counts, and timing unchanged
- Stage 1 smash shows gold weak points and crack overlays; Stage 4 shows cyan protected pylons vs hot exposed core

Break feedback shipped:
- `VFX_Debris_Light` short plaster chips on small hits and street-prop breaks
- `VFX_Debris_Heavy` heavier local burst on large/mid collapse, kept short so Target_A/B and HOLD pips stay readable
- `VFX_Smoke_Damage` near-break smoke on mid/large structures
- `VFX_WeakPoint_Hit` tight gold/orange crit flash, distinct from ordinary chips
- runtime VFX only; HP, spawn counts, and timing unchanged

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

Traffic silhouettes shipped:
- `PROP_Car_Compact_A` low hatchback, hood / cabin / hatch read
- `PROP_Car_Compact_B` same footprint with a notchback roof and trunk
- `PROP_Van_Bus` longer blocky cabin, readable from chase camera
- applied by `EnsureTrafficVehicleRuntime` and `RegisterTrafficVehicle` so Stage 1-3 moving cars and parked panic clusters share the set
- collider size, traffic count, speed, HP, and spawn rate unchanged
- roofs stay low so Target_A / Target_B and HOLD trail pips stay visible

Light roadside props shipped:
- `PROP_StreetLamp` thin post plus warm cap, used by even `Lamp_*` serials
- `PROP_TrafficLight` same pole hook with a narrow signal-head stack
- `PROP_RoadsideTree` trunk plus a gappy four-sphere canopy, not a leaf wall
- `PROP_Bench` low seat/back kit on the existing 1-HP bench host
- spawn counts, HP, and hit volumes unchanged; legacy Pole/Trunk/host colliders stay
- posts stay thin and benches stay low so Target_A / Target_B and HOLD trail pips stay readable

Market and utility props shipped:
- `PROP_Kiosk` stacked stall with awning, window, and counter
- `PROP_Vending` upright cabinet with a face panel, glass, and slot
- `PROP_BusStop` thin posts, roof, and back panel — a shelter, not a building
- `PROP_Transformer` steel tank with yellow/black hazard and pipe caps
- `PROP_ExplosiveBarrel` drum with lid, hoop, and a strong orange danger band
- applied by the existing commercial/utility Ensure* hooks, including Stage 2-6 streets and ROUTE BONUS clusters
- spawn counts, HP, explosion radius, and hit volumes unchanged
- silhouettes stay under building height so Target_A / Target_B and HOLD trail pips stay readable

Residential filler props shipped:
- `PROP_Fence` thin posts plus two rails, not a solid slab
- `PROP_Mailbox` tiny post-and-box with a flag
- `PROP_Shed` roofed backyard box with a door, smaller than a building
- applied by the existing `EnsureResidentialFenceRuntime` / `EnsureResidentialMailboxRuntime` / `EnsureResidentialShedRuntime` hooks
- spawn counts, HP, and hit volumes unchanged so Stage 1 starter lane does not get extra clutter
- rails stay thin and sheds stay low so Target_A / Target_B and HOLD trail pips stay readable

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
- failure beat set

Expected output:
- impact scale is felt through sound
- different gameplay states stop feeling samey
- boss and progression moments gain payoff
- defeat has a clear audible downbeat before the result screen

Must-cover events:
- light hit
- medium hit
- heavy hit
- small break
- heavy collapse
- chain up
- route open
- route hold warning
- route bonus
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
- ordinary failure
- boss-phase failure
- UI click / locked / confirm / fail

Current `FeedbackSystem` slot map:
- `hitLightClip`, `hitMediumClip`, `hitHeavyClip` (`SFX_Hit_Light` / `SFX_Hit_Medium` / `SFX_Hit_Heavy` in `Assets/Audio/SFX/Impact/`)
- `breakSmallClip`, `breakLargeClip` (`SFX_Break_Small` / `SFX_Break_LargeCollapse` in `Assets/Audio/SFX/Impact/`)
- `comboRiseClip` (`SFX_Combo_Rise` in `Assets/Audio/SFX/Skills/`)
- `VFX_Combo_Rise_Pulse` lime-gold upward ticks on CRUSH RUSH
- `VFX_Overdrive_Pulse` orange speed ring and flame chevrons on OVERDRIVE
- `routeOpenClip`, `routeHoldWarningClip`, `routeBonusClip`
- `bossWarningClip`, `bossBreakClip`, `bossDownClip` (`SFX_Boss_Warning` / `SFX_Boss_Break` / `SFX_Boss_Down` in `Assets/Audio/SFX/Boss/`)
- `levelUpClip` (`SFX_LevelUp_Open` in `Assets/Audio/SFX/UI/`)
- `failureWarningClip`, `failureBossClip`

Suggested file targets:
- `Assets/Audio/SFX/Impact/`
- `Assets/Audio/SFX/Skills/`
- `Assets/Audio/SFX/Boss/`
- `Assets/Audio/SFX/Failure/`
- `Assets/Audio/SFX/UI/`

Done when:
- silent critical moments no longer exist
- small hit / big hit / collapse are clearly separable by ear
- boss phase changes are recognizable without looking at UI
- every current `FeedbackSystem` slot is assigned a clip or has an intentional placeholder noted in the scene/prefab

---

## 3. Secondary Queue
These should start after the immediate queue is stable.

### B0. Outgame Progression UX Pass
Priority: `P0`

Deliverables:
- form card state set
- meta node state set for Size Core / Impact Core / DP Amplifier
- `UI_DP_GainBurst` DP gain / spend / insufficient burst
- `SFX_Progression_Locked` insufficient-DP / locked purchase cue
- `SFX_Progression_Confirm` purchase / unlock / equip confirmation cue
- `UI_FormCard_StateSet` form card lock / ready / equipped frame
- `UI_MetaNode_SizeCore` Size Core node chip
- `UI_MetaNode_ImpactCore` Impact Core node chip
- `UI_MetaNode_DpAmplifier` DP Amplifier node chip
- `Badge_FormReady` result form-ready next-action badge
- `Badge_MetaReady` result meta-ready next-action badge
- `Banner_StageUnlocked` stage unlocked banner
- `Toast_ProgressionSaved` lightweight progression saved toast
- confirm / locked audio cues (`SFX_Progression_Confirm`, `SFX_Progression_Locked`)

Generated checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateOutgameProgressionChecklist.ps1`

Done when:
- result and lobby answer what was earned, what can be bought, and why the next run is different
- recommended form/meta spend targets read visually before reading detailed text
- insufficient DP and locked states are unmistakable on mobile

### B0.5 Route Payoff Layout Pass
Priority: `P0`

Deliverables:
- `PAYOFF_ParkCut_Layout` park cut payoff layout
- `PAYOFF_MarketChain_Layout` market chain payoff layout
- `PAYOFF_YardBlast_Layout` construction yard blast payoff layout
- `PAYOFF_PowerSurge_Layout` power surge payoff layout
- `PAYOFF_SkylineBreach_Layout` skyline breach payoff layout
- `VFX_RouteCluster_Marker` mint-slate ring frame for opened ROUTE BONUS clusters
- `RouteClusterMarker` runtime host for that frame
- `VFX_ForwardSmash_Confirm` mint-white impact star and short broken ring on FORWARD SMASH
- `VFX_RouteHold_Success` gold-cyan lock ring when ROUTE HOLD completes and flips toward ROUTE BONUS
- `VFX_RouteOpen_Trail` magenta path dashes when LANE BREAK flips to ROUTE OPEN
- `VFX_LaneBreak_Residual` ivory-ash residual crack at the wreck that completed LANE BREAK
- `VFX_RouteChase_Pulse` cobalt wedges from the opened ROUTE BONUS cluster toward the next smash target
- Forward Smash confirmation VFX/SFX

Generated checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateRoutePayoffLayoutChecklist.ps1`

Done when:
- ROUTE BONUS visibly opens a district-specific reward cluster
- Forward Smash payoff feels earned by route reading, not like a random extra explosion
- Stage 5/6/7 payoff layouts differ by spacing and chase direction, not only by prop names

### B1. Boss Identity Pass
Priority: `P0`

Deliverables:
- `BOSS_Sentinel_Body_Kit` Justice Sentinel silhouette kit
- `BOSS_Shield_Pylon_Kit` shield pylon kit
- `BOSS_Phase2_Drone_Kit` phase 2 drone kit
- `VFX_Boss_Warning_Ring` inbound / pressure-pulse danger ring
- `VFX_Boss_Defeat_Cascade` Sentinel-down / city-collapse release
- short vertical break-window burst on CORE EXPOSED (not a second warning ring)
- `SFX_Boss_Warning` threat pulse / inbound warning
- `SFX_Boss_Break` break-window open
- `SFX_Boss_Down` Justice Sentinel defeat
- boss armor / shield / exposed-core material set
- boss core expose burst VFX
- `Badge_Boss_Clear` steel result badge for Sentinel victory
- `Icon_Boss_Sentinel` tall steel Sentinel body for lobby/result next-run boss identity

Generated checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateBossIdentityProductionChecklist.ps1`

Done when:
- `BOSS_Sentinel_Body_Kit`, `BOSS_Shield_Pylon_Kit`, and `BOSS_Phase2_Drone_Kit` keep the three Stage 4+ roles countable from silhouette and color language alone
- phase 1 / break window / phase 2 are visually separable
- the boss loop reads as breathe -> burst -> punish -> release in Stage 4+ playtests

### B2. UI Icon Pass
Priority: `P1`

Deliverables:
- `Icon_DP` teal cracked-diamond money pip
- `Icon_Stage` stacked district-block skyline
- `Icon_NextStep` forward chevron with target notch
- `Icon_Route` orchid path-to-beacon (echoes `PAL_RouteMarker_Tints`)
- `Icon_BreakWindow` gold open cracked ring
- `Icon_Shield` cyan shield plate with pylon ticks
- `Icon_WeakPoint` gold bullseye with glow dot
- `Icon_Boss` sentinel eye inside steel frame
- `Icon_Boss_Sentinel` tall steel Sentinel body for lobby/result next-run identity
- `Icon_Overdrive` orange speed ring with flame notch
- `Icon_Panic` car with radial warning spikes
- `Icon_Seismic` ground crack wave
- `Icon_Retail` shop awning with burst star
- `Icon_Traffic` road lane plus small car
- `Badge_Result_Clear` mint success plate with upward shard
- `Badge_Boss_Clear` steel Sentinel-down plate with downward chevron
- `Badge_Result_Failure` rust fail plate with broken route notch
- `Badge_Locked` steel lock plate
- `Badge_Recommended` gold focus chevron
- draft sprites at `Assets/Resources/UI/Icons/`
- draft badges at `Assets/Resources/UI/Badges/`

Done when:
- HUD and lobby rely less on text-only recognition
- result and lobby states scan faster on mobile

### B3. District Palette Pass
Priority: `P0`

Deliverables:
- `PAL_District_StarterResidential` Stage 1 clean-road opener
- `PAL_District_MarketPlaza` Stage 3 warm stall-density pivot
- `PAL_District_SentinelCheckpoint` Stage 4 steel/amber boss-approach warning
- `PAL_District_SkylineBlock` Stage 7 night-plaza climax
- `PAL_District_PocketPark` Stage 2 calmer park-cut green
- `PAL_District_ConstructionYard` Stage 5 dusty blast-payoff yellow/black
- `PAL_District_PowerBlock` Stage 6 electric teal transformer risk
- `PAL_RouteMarker_Tints` global route marker tint set for `Target_A`, `Target_B`, `routeColor`, and HOLD trail pips
- `PAL_Ambient_StageBands` stage-band ambient fill for `RenderSettings.ambientLight`
- ambient stage-band tone set

Generated checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateDistrictPaletteProductionChecklist.ps1`

Done when:
- stage difficulty also feels like spatial escalation
- screenshots from different stage bands no longer look interchangeable
- `PAL_RouteMarker_Tints` keeps `Target_A`, `Target_B`, `routeColor`, and HOLD trail pips as the highest-contrast nav signal over every district and the Stage 4+ boss kits

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
| Overdrive / Seismic / Landing Shockwave | distinct burst VFX and audio (`VFX_Overdrive_Pulse` plus `VFX_Combo_Rise_Pulse` on CRUSH RUSH) | separates special states from normal combat |
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
- [ ] route open / route hold warning / route bonus SFX
- [ ] ordinary failure / boss-phase failure SFX
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
1. assign temporary clips for every current `FeedbackSystem` slot, especially failure beats
2. create primitive-based form silhouette kits and icon placeholders
3. create destruction material tiers
4. create street prop primitive kit

This is the fastest path to making the current build look more intentional without waiting on a full asset pipeline.
