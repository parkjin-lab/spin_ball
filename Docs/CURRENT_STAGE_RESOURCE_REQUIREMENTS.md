# Alien Crusher - Current Stage Resource Requirements

## Purpose
This document defines the minimum and recommended resources needed to make the current playable prototype feel intentionally designed, readable, and fun.

It is based on the systems already implemented in code:
- form switching: `Sphere`, `Spike`, `Ram`, `Saucer`, `Crusher`
- stage progression and route pressure
- destructible city blocks and small street props
- traffic panic / retail frenzy / strip clear chains
- overdrive, landing shockwave, seismic bursts
- stage boss encounter: Justice Sentinel
- lobby meta progression and stage unlock flow

The goal at this stage is not asset volume.
The goal is to secure the specific resources that most improve:
- readability
- destructive pleasure
- progression clarity
- boss encounter identity
- outgame motivation

## Resource Policy For This Project
Because this project intentionally keeps a lightweight Unity-native art direction, most resources should follow these rules:
- prefer primitive-based or low-poly silhouette-driven visuals
- use color, motion, scale, particles, material variation, and camera feedback before requesting complex custom meshes
- reserve custom art effort for objects that define gameplay meaning
- only add UI that supports the current step of player decision-making

In practice, this means:
- common buildings can stay procedural / simple
- critical gameplay identifiers should get dedicated visual treatment
- sound and feedback resources are now high-value, because many core loops already exist

## Stage 08-10 Content Pack Resources

The late-run expansion is playable with Unity primitives, but these dedicated resources now have high value:

- Stage 08 Transit Hub: shuttle silhouette, route display material, gate silhouette, shuttle break SFX
- Stage 09 Harbor Yard: container variants, fuel hazard material, crane silhouette, fuel-chain SFX
- Stage 10 Civic Core: pylon silhouette, energy-band material, uplink tower, pylon/uplink break SFX
- three district palette sets with distinct ground, structure, hazard, and emissive colors
- one minimap or stage-select district icon for each new stage

Production policy:
- prioritize silhouette and destruction readability over mesh detail
- keep dangerous fuel objects visually distinct from ordinary containers
- make Civic Core pylons and the uplink readable as setup and payoff at gameplay camera distance
- validate the primitive baseline before replacing it with authored assets

See Docs/STAGE_08_10_CONTENT_EXPANSION.md for gameplay intent and acceptance criteria.
## Current Resource Need Summary
Priority order at the current stage:
1. gameplay readability resources
2. impact and destruction feedback resources
3. boss identity resources
4. outgame progression resources
5. stage theme variation resources
6. polish-only resources

---

## 1. Must-Have Resources Now
These are the highest-value resources for the current build.
Without them, the game may work but feel unfinished or confusing.

### 1.1 Form Identity Resources
Status:
- forms already exist in code and partially exist visually
- Sphere now has a runtime belt kit, lobby/HUD icon, and SPHERE PULSE cue
- Ram and Saucer now have route-helper silhouette kits, lobby icons, and skill marks
- Spike and Crusher now have damage-fantasy silhouette kits, lobby icons, and skill marks

Need:
- one clear visual silhouette set per form
- one color family per form
- one impact / movement feedback profile per form
- one UI icon per form

Required assets:
- `FORM_Sphere_Body_Kit` cool-green body plus emissive equatorial belt
- `FORM_Ram_Body_Kit` amber wedge nose with side horns
- `FORM_Saucer_Body_Kit` cyan rim disc with pale underside
- `FORM_Spike_Body_Kit` lean dark core with acid radial needles
- `FORM_Crusher_Body_Kit` layered steel shell, flat frontal plate, and blue pressure seams
- `Icon_Form_Sphere` solid circle with orbit band (`Assets/Resources/UI/Icons/`)
- `Icon_Form_Ram` wedge arrow inside circle (`Assets/Resources/UI/Icons/`)
- `Icon_Form_Saucer` flat disk with motion streak (`Assets/Resources/UI/Icons/`)
- `Icon_Form_Spike` circle with four to six spikes (`Assets/Resources/UI/Icons/`)
- `Icon_Form_Crusher` blocky mass with impact crack (`Assets/Resources/UI/Icons/`)
- `Icon_Skill_SpherePulse` pulse-ring skill cue (`Assets/Resources/UI/Icons/`)
- `Icon_Skill_RamBreach` forward shove cue (`Assets/Resources/UI/Icons/`)
- `Icon_Skill_SaucerDash` wide-disc dash cue (`Assets/Resources/UI/Icons/`)
- `Icon_Skill_SpikeBurst` radial puncture cue (`Assets/Resources/UI/Icons/`)
- `Icon_Skill_CrusherSlam` heavy slam cue (`Assets/Resources/UI/Icons/`)
- `Form Icon - Sphere`
- `Form Icon - Spike`
- `Form Icon - Ram`
- `Form Icon - Saucer`
- `Form Icon - Crusher`
- lightweight form accent mesh or addon pieces for each form
- material presets for each form

Design direction:
- `Sphere`: smooth, alien, stable, default invader body
- `Spike`: aggressive, puncture-focused, sharp radial silhouette
- `Ram`: forward-heavy, wedge or horn emphasis
- `Saucer`: flatter, hovering, sci-fi disk profile
- `Crusher`: heavy, dense, industrial or siege form

Why this matters now:
- the player must instantly feel that a form switch changes playstyle
- outgame unlock desire is weak unless each form looks and sounds meaningfully different

### 1.2 Destruction Readability Resources
Status:
- destruction systems exist
- building size tiers now have named runtime materials so easy / filler / durable / boss structures read before they break
- combat states now have named runtime materials so damaged, weak-point, shielded, and exposed-core reads stay distinct

Need:
- collapse audio pairing after break VFX
- stronger contrast between ground, props, small buildings, and big targets once street-prop kits expand

Required assets:
- `MAT_Building_Small` pale plaster / cheap low-rise (`Assets/Art/Materials/Destruction/`)
- `MAT_Building_Mid` standard city concrete filler (`Assets/Art/Materials/Destruction/`)
- `MAT_Building_Large` dark heavy mass for durable targets (`Assets/Art/Materials/Destruction/`)
- `MAT_Boss_Structure` cool steel-blue armor, not a normal large building (`Assets/Art/Materials/Destruction/`)
- `MAT_Damage_CrackOverlay` dark split lines for damaged / near-break (`Assets/Art/Materials/Destruction/`)
- `MAT_WeakPoint_Glow` small gold target pip with halo (`Assets/Art/Materials/Destruction/`)
- `MAT_Shielded_Pylon` cool cyan barrier on live shield pylons (`Assets/Art/Materials/Destruction/`)
- `MAT_Exposed_Core` hot orange-white open core window (`Assets/Art/Materials/Destruction/`)
- `VFX_Debris_Light` short cheap chips on small hits and prop breaks (`Assets/Art/VFX/Destruction/`)
- `VFX_Debris_Heavy` heavier local collapse burst (`Assets/Art/VFX/Destruction/`)
- `VFX_Smoke_Damage` near-break smoke on mid/large structures (`Assets/Art/VFX/Destruction/`)
- `VFX_WeakPoint_Hit` sharp gold/orange crit flash (`Assets/Art/VFX/Destruction/`)
- material set for `small building`, `mid building`, `large building`, `boss-related structure`
- crack overlay / damage tint presets
- smoke burst variant
- debris burst variant
- heavy collapse burst variant
- weak point glow material
- shielded material variant
- exposed-core material variant

Why this matters now:
- fun depends on knowing what is easy to crush, what is risky, and what is a payoff target
- current code already supports multiple destruction moments, so the missing value is presentation clarity

### 1.3 Street Prop Variety Resources
Status:
- street props and traffic systems exist
- moving and parked traffic now use named runtime kits so cars and vans read as crushable vehicles, not boxes
- roadside lamps, traffic lights, trees, and benches now have named runtime kits for a quick-break street cadence
- market and utility props now have named runtime kits so chain density and payoff danger read before impact
- residential extras now have named runtime kits `PROP_Fence`, `PROP_Mailbox`, and `PROP_Shed` on the existing residential hooks, without adding Stage 1 spawn count

Need:
- residential filler extras after the market / utility set
- props should support combo lanes and panic reactions

Required assets:
- `PROP_Car_Compact_A` low hatchback crush target (`Assets/Art/Props/Traffic/`)
- `PROP_Car_Compact_B` same footprint, notchback roof (`Assets/Art/Props/Traffic/`)
- `PROP_Van_Bus` longer blocky van/bus (`Assets/Art/Props/Traffic/`)
- `PROP_StreetLamp` thin post with a visible cap (`Assets/Art/Props/Street/`)
- `PROP_TrafficLight` lamp variant with red/amber/green head blocks (`Assets/Art/Props/Street/`)
- `PROP_RoadsideTree` trunk plus a gappy canopy cluster (`Assets/Art/Props/Street/`)
- `PROP_Bench` low seat/back silhouette (`Assets/Art/Props/Street/`)
- `PROP_Kiosk` stacked stall with awning and counter (`Assets/Art/Props/Street/`)
- `PROP_Vending` upright cabinet with a readable face panel (`Assets/Art/Props/Street/`)
- `PROP_BusStop` thin roof / panel / posts, not a building (`Assets/Art/Props/Street/`)
- `PROP_Transformer` utility tank plus pipe/cap hazard (`Assets/Art/Props/Utilities/`)
- `PROP_ExplosiveBarrel` drum with a strong danger band (`Assets/Art/Props/Utilities/`)
- `PROP_Fence` thin rails that do not hide route pips (`Assets/Art/Props/Street/`)
- `PROP_Mailbox` tiny post-and-box suburb read (`Assets/Art/Props/Street/`)
- `PROP_Shed` roofed backyard box, smaller than a building (`Assets/Art/Props/Street/`)

Important note:
These do not need high-detail custom models.
They need:
- readable shape
- readable scale
- readable break reaction

Why this matters now:
- early-stage fun improves when the player can continuously crush many low-cost targets
- small prop chains are already part of the score and pacing systems

### 1.4 Audio Starter Pack
Status:
- current build has many gameplay states that should sound different
- audio is now one of the highest leverage missing pieces
- `FeedbackSystem` now exposes assignable one-shot slots for hit weight, break scale, route beats, boss beats, level-up, and failure beats
- failure now has a dedicated runtime downbeat, so defeat should sound like a clear rhythm punctuation instead of silently falling into the result screen
- hit/break weight drafts now load at runtime when Inspector slots are empty: `SFX_Hit_Light`, `SFX_Hit_Medium`, `SFX_Hit_Heavy`, `SFX_Break_Small`, `SFX_Break_LargeCollapse` (`Assets/Audio/SFX/Impact/` + Resources copies)
- climax/progression drafts now load at runtime when Inspector slots are empty: `SFX_Boss_Warning`, `SFX_Boss_Break`, `SFX_Boss_Down` (`Assets/Audio/SFX/Boss/`), `SFX_Combo_Rise` (`Assets/Audio/SFX/Skills/`), `SFX_LevelUp_Open` (`Assets/Audio/SFX/UI/`)
- CRUSH RUSH now adds named runtime pulse `VFX_Combo_Rise_Pulse` (lime-gold upward ticks at the ball) while still reusing `comboRiseClip`; OVERDRIVE start adds `VFX_Overdrive_Pulse` (orange speed ring plus flame chevrons). Combo thresholds, overdrive duration/damage, and HUD layout stay unchanged.

Need:
- distinct audio for hit quality, destruction scale, form actions, progression, and boss danger
- short failure stingers that distinguish ordinary route/time failure from boss-phase failure

Required assets:
- `SFX_Hit_Light` -> `hitLightClip`
- `SFX_Hit_Medium` -> `hitMediumClip`
- `SFX_Hit_Heavy` -> `hitHeavyClip`
- `SFX_Break_Small` -> `breakSmallClip`
- `SFX_Break_LargeCollapse` -> `breakLargeClip`
- `SFX_Combo_Rise` -> `comboRiseClip`
- `SFX_Route_Open` -> `routeOpenClip`
- `SFX_Route_HoldWarning` -> `routeHoldWarningClip`
- `SFX_Route_Bonus` -> `routeBonusClip`
- `SFX_Boss_Warning` -> `bossWarningClip`
- `SFX_Boss_Break` -> `bossBreakClip`
- `SFX_Boss_Down` -> `bossDownClip`
- `SFX_LevelUp_Open` -> `levelUpClip`
- `SFX_Failure_Warning` -> `failureWarningClip`
- `SFX_Failure_Boss` -> `failureBossClip`

Next audio slots still needed after the current hook surface:
- level up confirm
- DP reward / outgame reward
- form unlock
- overdrive start
- panic jackpot
- retail frenzy trigger
- seismic burst
- landing shockwave
- boss shield up
- boss core exposed
- boss phase transition
- UI tap / confirm / locked / purchase fail

Why this matters now:
- code already contains many important moments, but silent or under-defined moments reduce impact
- destruction pleasure scales dramatically with sound even when visuals remain simple
- rhythm depends on contrast: warning, payoff, and failure beats must be audibly different

---

## 2. Very Important Resources
These are not as urgent as the must-have group, but they should come immediately after.

### 2.1 UI Icon and Status Set
Status:
- run-essential draft icons `Icon_DP`, `Icon_Stage`, `Icon_NextStep`, and `Icon_Route` already sit on the HUD strip and lobby/result labels
- route/boss readability draft icons `Icon_BreakWindow`, `Icon_Shield`, `Icon_WeakPoint`, and `Icon_Boss` now sit on a top-right HUD strip, swap beside Sentinel status, and mark elite weak-point copy
- upgrade/chaos status draft icons `Icon_Overdrive`, `Icon_Panic`, `Icon_Seismic`, `Icon_Retail`, and `Icon_Traffic` now sit on a compact HUD/lobby strip and beside chain, upgrade, and TRAFFIC labels
- result/lobby draft badges `Badge_Result_Clear`, `Badge_Result_Failure`, `Badge_Locked`, and `Badge_Recommended` now mark success, fail, lock, and recommendation at a glance
- leftover boss-stage next-run icon `Icon_Boss_Sentinel` now marks lobby Stage 4+ select and result next-action when the next run is a Sentinel encounter
- leftover next-run spend-change pulse `VFX_SpendChange_Ready` now names the last lobby form or meta spend on stage start
- leftover lobby stage-select confirm pulse `VFX_StageSelect_Confirm` now marks Stage prev/next on the stage readout
- leftover meta-purchase confirm pulse `VFX_MetaUpgrade_Confirm` now marks a successful Size / Impact / DP buy on that node
- Icons A/B/C and district palettes were not restyled

Need:
- icon set for stage pressure, route target, destruction, DP, overdrive, panic, seismic, boss state, weak point, shield, break window
- upgrade icons for meta upgrades and in-run upgrades
- status badge shapes for result and lobby panels

Required assets:
- `Icon_DP`
- `Icon_Stage`
- `Icon_NextStep`
- `Icon_Route` (`Assets/Resources/UI/Icons/`)
- `Icon_BreakWindow`
- `Icon_Shield`
- `Icon_WeakPoint`
- `Icon_Overdrive`
- `Icon_Panic`
- `Icon_Seismic`
- `Icon_Retail`
- `Icon_Traffic`
- `Icon_Boss`
- `Icon_Boss_Sentinel`
- `Badge_Result_Clear`
- `Badge_Boss_Clear`
- `Badge_Result_Failure`
- `Badge_Locked`
- `Badge_Recommended` (`Assets/Resources/UI/Badges/`)
- `UI_DP_GainBurst` (`Assets/Resources/UI/Rewards/`)
- `SFX_Progression_Locked` (`Assets/Audio/SFX/UI/SFX_Progression_Locked.wav`)
- `SFX_Progression_Confirm` (`Assets/Audio/SFX/UI/SFX_Progression_Confirm.wav`)
- `UI_FormCard_StateSet` (`Assets/Resources/UI/Lobby/`)
- `UI_MetaNode_SizeCore`
- `UI_MetaNode_ImpactCore`
- `UI_MetaNode_DpAmplifier` (`Assets/Resources/UI/Meta/`)
- `Badge_FormReady`
- `Badge_MetaReady` (`Assets/Resources/UI/Badges/`)
- `Banner_StageUnlocked`
- `Toast_ProgressionSaved` (`Assets/Resources/UI/Rewards/`)
- `VFX_FormEquip_Confirm` champagne lobby form-equip lock-in ring
- `VFX_SpendChange_Ready` jade next-run spend-change ready plate
- `VFX_StageSelect_Confirm` ice-slate lobby stage-select confirm brackets
- `VFX_MetaUpgrade_Confirm` copper diamond pulse when a meta upgrade is purchased

Why this matters now:
- UI text has been heavily simplified already
- the next gain in UX comes from fast icon recognition, not more text

### 2.2 Boss Encounter Identity Set
Status:
- Justice Sentinel logic exists
- phase, shield, drone, and break window systems are already implemented
- runtime silhouette kits now separate the main body, shield pylons, and phase-2 drones from ordinary city props
- climax warning/break/down VFX now punctuate inbound, CORE EXPOSED, and Sentinel down as three different silhouettes
- result boss-clear badge now ships as `Badge_Boss_Clear`; leftover `Icon_Boss_Sentinel` now marks lobby/result when the next run is a Sentinel stage. Remaining identity work is optional boss-specific armor/core materials (`MAT_Boss_*`) and leftover expose burst

Required assets:
- `BOSS_Sentinel_Body_Kit` sentinel body silhouette kit
- `BOSS_Shield_Pylon_Kit` shield pylon silhouette kit
- `BOSS_Phase2_Drone_Kit` phase 2 drone silhouette kit
- `VFX_Boss_Warning_Ring` threat-pulse / inbound danger ring (`Assets/Art/VFX/Boss/`)
- `VFX_Boss_Defeat_Cascade` Sentinel-down / city-collapse release (`Assets/Art/VFX/Boss/`)
- `SFX_Boss_Warning` threat pulse / inbound warning
- `SFX_Boss_Break` break-window open
- `SFX_Boss_Down` Justice Sentinel defeat
- boss armor / shield pylon / exposed-core material set
- core exposed material / emissive state
- boss core expose burst VFX
- `Badge_Boss_Clear` steel result badge for Sentinel victory (`Assets/Resources/UI/Badges/`)
- `Icon_Boss_Sentinel` tall steel Sentinel body for lobby/result next-run boss identity (`Assets/Resources/UI/Icons/`)

Generated production checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateBossIdentityProductionChecklist.ps1`

Why this matters now:
- the boss is a major progression promise
- if the boss looks too similar to normal buildings, the entire stage climb loses payoff

### 2.3 Environment Palette Sets
Status:
- `PAL_RouteMarker_Tints` already locks Target_A / Target_B / routeColor / HOLD pips
- core rhythm palettes `PAL_District_StarterResidential`, `PAL_District_MarketPlaza`, `PAL_District_SentinelCheckpoint`, and `PAL_District_SkylineBlock` now tint Stage 1/3/4/7 ground, walls, and landmark pads
- secondary palettes `PAL_District_PocketPark`, `PAL_District_ConstructionYard`, and `PAL_District_PowerBlock` now tint Stage 2/5/6 ground, walls, and landmark pads
- building tier kits, Icons A/B/C, and Palette B Stage 1/3/4/7 families were not restyled
- remaining work is ambient stage bands
- named runtime set `PAL_Ambient_StageBands` now drives `RenderSettings.ambientLight` per stage band without restyling district palettes or `PAL_RouteMarker_Tints`

Need:
- at least 3 stage palette families so progression feels spatial, not only numeric

Recommended sets:
- district 1: civilian low-rise / clean suburb edge
- district 2: commercial strip / signage-heavy zone
- district 3: industrial utility zone / heavy traffic zone
- district 4+: fortified civic or defense zone
- named runtime landmark palettes: pocket park, market plaza, Sentinel checkpoint, construction yard, power block, skyline block

Required assets per district:
- ground material variant
- building palette variant
- prop color variant
- `PAL_RouteMarker_Tints` shared route marker / `routeColor` / HOLD trail tint set
- fog / ambient tint preset
- `PAL_Ambient_StageBands` stage-band ambient fill (`Assets/Art/Palettes/Lighting/`)

`PAL_RouteMarker_Tints` runtime values:
- Marker `Target_A` / `Target_B`: `(1.00, 0.58, 0.94)` bright orchid
- Paint `routeColor`: `(0.94, 0.18, 0.70)` deep magenta
- Trail HOLD pips: `(1.00, 0.76, 0.98)` pink-white

Generated production checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateDistrictPaletteProductionChecklist.ps1`

Why this matters now:
- stage progression is more satisfying when the world tone changes with difficulty
- route rhythm needs different visual tones, not only different object counts

### 2.4 Route Payoff Layout Set
Need:
- district-specific reward cluster layouts for ROUTE BONUS and Forward Smash
- payoff spacing rules that make each district feel like a different route problem
- a route cluster marker that frames the opened cluster without hiding target guidance

Required assets:
- `PAYOFF_ParkCut_Layout` park cut payoff layout
- `PAYOFF_MarketChain_Layout` market chain payoff layout
- `PAYOFF_YardBlast_Layout` construction yard blast payoff layout
- `PAYOFF_PowerSurge_Layout` power surge payoff layout
- `PAYOFF_SkylineBreach_Layout` skyline breach payoff layout
- `VFX_RouteCluster_Marker` mint-slate ring frame for opened ROUTE BONUS clusters (`Assets/Art/VFX/Route/`)
- `RouteClusterMarker` runtime host for that frame
- `VFX_ForwardSmash_Confirm` mint-white impact star and short broken ring on FORWARD SMASH (`Assets/Art/VFX/Route/`)
- `VFX_RouteHold_Success` gold-cyan lock ring when ROUTE HOLD completes and flips toward ROUTE BONUS (`Assets/Art/VFX/Route/`)
- `VFX_RouteOpen_Trail` magenta path dashes when LANE BREAK flips to ROUTE OPEN (`Assets/Art/VFX/Route/`)
- `VFX_LaneBreak_Residual` ivory-ash residual crack at the wreck that completed LANE BREAK (`Assets/Art/VFX/Route/`)
- `VFX_RouteChase_Pulse` cobalt wedges from the opened ROUTE BONUS cluster toward the next smash target (`Assets/Art/VFX/Route/`)
- `VFX_RouteHold_Warning` rose inward ticks at the beacon when ROUTE HOLD is closing (`Assets/Art/VFX/Route/`)
- `VFX_RouteBonus_Success` amethyst bloom plus upward petals when ROUTE BONUS opens at the cluster (`Assets/Art/VFX/Route/`)
- Forward Smash confirmation VFX/SFX

Generated production checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateRoutePayoffLayoutChecklist.ps1`

Why this matters now:
- ROUTE HOLD needs a visible reward, not only a counter or score bonus
- the payoff beat is where rhythm turns route reading into destructive pleasure

---

## 3. Resources Needed For Outgame Motivation
These support long-term retention rather than immediate moment-to-moment clarity.

### 3.1 Lobby / Meta Progression Resource Set
Status:
- `UI_DP_GainBurst` and `SFX_Progression_Locked` now mark earn, spend, and insufficient DP on result/lobby
- form cards, meta nodes, stage-unlock banners, and save toasts now ship as readable visuals (`UI_FormCard_StateSet`, `UI_MetaNode_SizeCore`, `UI_MetaNode_ImpactCore`, `UI_MetaNode_DpAmplifier`, `Badge_FormReady`, `Badge_MetaReady`, `Banner_StageUnlocked`, `Toast_ProgressionSaved`)
- Icons A-D and district palettes were not restyled
- confirm audio now ships as `SFX_Progression_Confirm` on successful unlock, purchase, and equip
- leftover next-run spend-change pulse `VFX_SpendChange_Ready` names the last lobby form or meta spend when the next run starts
- leftover lobby stage-select confirm pulse `VFX_StageSelect_Confirm` marks Stage prev/next on the stage readout
- leftover meta-purchase confirm pulse `VFX_MetaUpgrade_Confirm` marks a successful Size / Impact / DP buy on that node

Need:
- cleaner visuals for form unlock targets and meta purchases
- clear difference between `available now`, `later`, `locked by stage`, `equipped`

Required assets:
- form card background variants
- meta upgrade chip / node visuals
- locked state overlay
- ready state highlight
- equipped state highlight
- stage unlocked banner element
- progression toast badge element

Generated production checklist:
- `powershell -ExecutionPolicy Bypass -File Tools/GenerateOutgameProgressionChecklist.ps1`

Why this matters now:
- the outgame flow is present in code
- visual motivation is still lighter than the actual system depth
- every clear should create a readable next-run reason, not only add DP silently

### 3.2 Save / Progression Confirmation Resources
Need:
- lightweight UI moments that confirm long-term progress
- smoke-test evidence that stage unlock, DP balance, form unlocks, and meta upgrades survive save/load
- corrupt-primary save recovery check before any release-style playtest build
- stage-bound recovery check that rejects impossible lobby/current/cleared stage values from edited or corrupted saves
- meta-bound recovery check that rejects negative DP, invalid selected forms, duplicate/invalid unlocked forms, and negative upgrade levels
- meta-upgrade recovery check that trims upgrade IDs and rejects duplicate upgrade entries
- repair persistence check that confirms sanitized save data is written back after load

Required assets:
- stage clear banner style
- unlock acquired badge
- `UI_DP_GainBurst` DP gain / spend / insufficient burst
- `SFX_Progression_Locked` insufficient-DP / locked purchase cue
- `SFX_Progression_Confirm` purchase / unlock / equip confirmation cue
- `VFX_FormEquip_Confirm` champagne lock-in ring when a lobby form is equipped
- `VFX_SpendChange_Ready` jade ready plate on the next run after a lobby form or meta spend
- `VFX_StageSelect_Confirm` ice-slate bracket pulse when a lobby stage is selected
- `VFX_MetaUpgrade_Confirm` copper diamond pulse when a meta upgrade is purchased
- `UI_FormCard_StateSet` form card lock / ready / equipped frame
- `UI_MetaNode_SizeCore` Size Core node chip
- `UI_MetaNode_ImpactCore` Impact Core node chip
- `UI_MetaNode_DpAmplifier` DP Amplifier node chip
- `Badge_FormReady` result form-ready next-action badge
- `Badge_MetaReady` result meta-ready next-action badge
- `Banner_StageUnlocked` stage clear / highest-stage banner
- `Toast_ProgressionSaved` save confirmation toast
- new form target marker
- meta upgrade purchased pulse

Validation support:
- `powershell -ExecutionPolicy Bypass -File Tools/AuditProgressionSaveSafetyStatic.ps1`

---

## 4. Resources That Can Stay Dummy For Now
These do not need immediate production time.

Can remain dummy:
- high-detail character art
- realistic building interiors
- cinematic cutscene assets
- complex texture packs
- high-end skybox variants
- polished menu backgrounds
- advanced logo treatment
- localization art variants
- premium VFX layering beyond gameplay readability

Reason:
- these do not currently improve the core destroy-loop as much as form readability, boss identity, prop variety, and sound

---

## 5. Suggested Production Backlog
This is the recommended creation order.

### Phase A - Immediate
Make the current game feel good.
- form icons
- form silhouette addons
- destruction material tiers
- basic street prop silhouettes
- core gameplay SFX pack
- boss exposed / shield / weak point materials

### Phase B - Readability Upgrade
Make the stage and boss easier to read.
- boss identity set
- UI status icon set
- district palette set A/B/C
- heavy collapse particle variants
- warning and mission marker variants

### Phase C - Motivation Upgrade
Make progression more desirable.
- lobby card visuals
- unlock banners
- stage clear reward visuals
- meta purchase feedback assets
- result screen reward tokens

### Phase D - Prestige Polish
Add flavor after the loop is solid.
- richer thematic sky / fog profiles
- stage-specific decorative meshes
- form-specific idle VFX
- premium transition flourishes

---

## 6. Practical Asset Checklist
This section can be used as a working production checklist.

### Art
- [ ] 5 form icons
- [ ] 5 form silhouette enhancement kits
- [ ] 4 destruction material tiers
- [ ] 1 weak point material
- [ ] 1 shield material
- [ ] 1 exposed-core material
- [x] `PAL_District_StarterResidential` Stage 1 opener ground/wall/pad set
- [x] `PAL_District_MarketPlaza` Stage 3 density ground/wall/pad set
- [x] `PAL_District_SentinelCheckpoint` Stage 4 warning ground/wall/pad set
- [x] `PAL_District_SkylineBlock` Stage 7 climax ground/wall/pad set
- [x] `PAL_District_PocketPark` Stage 2 calmer mint-path park cut
- [x] `PAL_District_ConstructionYard` Stage 5 dusty blast-payoff yard
- [x] `PAL_District_PowerBlock` Stage 6 electric-teal transformer block
- [x] `PAL_Ambient_StageBands` stage-band ambient fill
- [ ] 3 district palette sets
- [ ] 8 to 10 street prop silhouettes
- [x] `PROP_Car_Compact_A` runtime compact hatchback traffic kit
- [x] `PROP_Car_Compact_B` runtime compact notchback traffic kit
- [x] `PROP_Van_Bus` runtime van/bus traffic kit
- [x] `PROP_StreetLamp` runtime thin-post lamp kit
- [x] `PROP_TrafficLight` runtime signal-head lamp variant
- [x] `PROP_RoadsideTree` runtime canopy-cluster tree kit
- [x] `PROP_Bench` runtime low seat/back bench kit
- [x] `PROP_Kiosk` runtime market stall kit
- [x] `PROP_Vending` runtime face-panel cabinet kit
- [x] `PROP_BusStop` runtime thin shelter kit
- [x] `PROP_Transformer` runtime utility tank kit
- [x] `PROP_ExplosiveBarrel` runtime danger-band drum kit
- [x] `PROP_Fence` runtime thin-rail fence kit
- [x] `PROP_Mailbox` runtime post-and-box mailbox kit
- [x] `PROP_Shed` runtime roofed backyard shed kit
- [x] `BOSS_Sentinel_Body_Kit` runtime sentinel body silhouette kit
- [x] `BOSS_Shield_Pylon_Kit` runtime shield pylon silhouette kit
- [x] `BOSS_Phase2_Drone_Kit` runtime phase 2 drone silhouette kit
- [x] `VFX_Boss_Warning_Ring` rust-amber inbound / pulse danger ring
- [x] `VFX_Boss_Defeat_Cascade` steel-white Sentinel-down release
- [x] `VFX_RouteCluster_Marker` mint-slate ROUTE BONUS cluster frame
- [x] `VFX_ForwardSmash_Confirm` mint-white FORWARD SMASH cash-out star
- [x] `VFX_RouteHold_Success` gold-cyan ROUTE HOLD success lock ring
- [x] `VFX_RouteOpen_Trail` magenta ROUTE OPEN path dashes
- [x] `VFX_LaneBreak_Residual` ivory-ash LANE BREAK residual crack
- [x] `VFX_RouteChase_Pulse` cobalt ROUTE BONUS chase wedges
- [x] `VFX_RouteHold_Warning` rose ROUTE HOLD CLOSING ticks
- [x] `VFX_RouteBonus_Success` amethyst ROUTE BONUS cluster bloom
- [x] `VFX_Combo_Rise_Pulse` lime-gold upward ticks on CRUSH RUSH
- [x] `VFX_Overdrive_Pulse` orange speed ring and flame chevrons on OVERDRIVE

### VFX
- [ ] light hit burst
- [ ] medium hit burst
- [ ] heavy break burst
- [ ] collapse dust burst
- [ ] weak point hit burst
- [x] `VFX_Boss_Warning_Ring` rust-amber inbound / pulse danger ring
- [x] `VFX_Boss_Defeat_Cascade` steel-white Sentinel-down release
- [x] `VFX_RouteCluster_Marker` mint-slate ROUTE BONUS cluster frame
- [x] `VFX_ForwardSmash_Confirm` mint-white FORWARD SMASH cash-out star
- [x] `VFX_RouteHold_Success` gold-cyan ROUTE HOLD success lock ring
- [x] `VFX_RouteOpen_Trail` magenta ROUTE OPEN path dashes
- [x] `VFX_LaneBreak_Residual` ivory-ash LANE BREAK residual crack
- [x] `VFX_RouteChase_Pulse` cobalt ROUTE BONUS chase wedges
- [x] `VFX_RouteHold_Warning` rose ROUTE HOLD CLOSING ticks
- [x] `VFX_RouteBonus_Success` amethyst ROUTE BONUS cluster bloom
- [x] `VFX_Combo_Rise_Pulse` lime-gold upward ticks on CRUSH RUSH
- [x] `VFX_Overdrive_Pulse` orange speed ring and flame chevrons on OVERDRIVE
- [ ] shield break burst
- [ ] core exposed burst
- [ ] panic jackpot burst
- [ ] retail frenzy burst
- [ ] seismic resonance burst
- [ ] landing shockwave burst

### Audio
- [ ] hit SFX set
- [ ] destruction SFX set
- [ ] form action SFX set
- [ ] progression / reward SFX set
- [ ] boss warning / phase / break / death SFX set
- [ ] route open / route hold warning / route bonus SFX set
- [ ] ordinary failure warning stinger
- [ ] boss-phase failure stinger
- [ ] UI feedback SFX set

### UI
- [x] `Icon_DP` run-essential money pip
- [x] `Icon_Stage` run-essential stage skyline
- [x] `Icon_NextStep` run-essential next-action chevron
- [x] `Icon_Route` run-essential orchid path-to-beacon
- [x] `Icon_BreakWindow` gold open cracked ring
- [x] `Icon_Shield` cyan shield plate with pylon ticks
- [x] `Icon_WeakPoint` gold bullseye with glow dot
- [x] `Icon_Boss` sentinel eye inside steel frame
- [x] `Icon_Boss_Sentinel` tall steel Sentinel body for lobby/result next-run identity
- [x] `Icon_Overdrive` orange speed ring with flame notch
- [x] `Icon_Panic` car with radial warning spikes
- [x] `Icon_Seismic` ground crack wave
- [x] `Icon_Retail` shop awning with burst star
- [x] `Icon_Traffic` road lane plus small car
- [x] `Badge_Result_Clear` mint success plate with upward shard
- [x] `Badge_Boss_Clear` steel Sentinel-down plate with downward chevron
- [x] `Badge_Result_Failure` rust fail plate with broken route notch
- [x] `Badge_Fail_Opening` rust stacked-bar plate for OPENING FAILED
- [x] `Badge_Fail_Hold` rust trail-and-beacon plate for ROUTE HOLD MISSED
- [x] `Badge_Fail_Drift` rust offset-chevron plate for MID-RUN DRIFT
- [x] `Badge_Fail_Push` rust notched-wedge plate for FINAL PUSH FAILED
- [x] `Badge_Fail_Boss` steel cracked-eye plate for BOSS PHASE fail
- [x] `Badge_Locked` steel lock plate
- [x] `Badge_Recommended` gold focus chevron
- [x] `UI_DP_GainBurst` teal reward burst for earn / spend / insufficient
- [x] `SFX_Progression_Locked` muted locked-purchase cue
- [x] `SFX_Progression_Confirm` bright unlock / purchase / equip cue
- [x] `VFX_FormEquip_Confirm` champagne lobby form-equip lock-in ring
- [x] `VFX_SpendChange_Ready` jade next-run spend-change ready plate
- [x] `VFX_StageSelect_Confirm` ice-slate lobby stage-select confirm brackets
- [x] `VFX_MetaUpgrade_Confirm` copper lobby meta-purchase confirm diamond
- [x] `UI_FormCard_StateSet` form card lock / ready / equipped frame
- [x] `UI_MetaNode_SizeCore` Size Core node chip
- [x] `UI_MetaNode_ImpactCore` Impact Core node chip
- [x] `UI_MetaNode_DpAmplifier` DP Amplifier node chip
- [x] `Badge_FormReady` result form-ready next-action badge
- [x] `Badge_MetaReady` result meta-ready next-action badge
- [x] `Banner_StageUnlocked` stage unlock banner
- [x] `Toast_ProgressionSaved` save confirmation toast
- [ ] stage / DP / route / break / shield / weak point icon set
- [ ] form card frame set
- [ ] meta upgrade chip set
- [ ] locked / ready / equipped state visuals
- [ ] unlock / stage clear / reward badge set

---

## 7. Recommended Team Decision Right Now
If only one resource category can be advanced next, choose:
1. audio
2. form silhouette and icon pass
3. boss identity pass

That order gives the best return on effort for the current codebase.

## 8. Final Guidance
At the current stage, this project does not need more generic content.
It needs better distinction between already existing gameplay meanings.

The most important resource question is:
"What helps the player instantly understand what to smash, what to fear, what to chase, and what they just earned?"

Any resource that answers that question is high priority.
Any resource that does not can wait.
