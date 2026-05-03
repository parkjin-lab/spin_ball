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
- they still need stronger silhouette distinction and feedback support

Need:
- one clear visual silhouette set per form
- one color family per form
- one impact / movement feedback profile per form
- one UI icon per form

Required assets:
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
- many objects still rely on primitive visuals and shared feedback language

Need:
- better distinction between breakable size tiers
- better visual states for damaged / cracking / near-destroyed structures
- stronger contrast between ground, props, small buildings, and big targets

Required assets:
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
- city fantasy still feels thin if only buildings dominate the map

Need:
- lightweight prop variety that makes the city feel alive and crushable
- props should support combo lanes and panic reactions

Required assets:
- compact car silhouette A
- compact car silhouette B
- van / bus silhouette
- street lamp silhouette
- traffic light silhouette
- roadside tree silhouette
- kiosk / vending silhouette
- bench / bus stop silhouette
- transformer / utility box silhouette
- explosive barrel silhouette

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

Need:
- distinct audio for hit quality, destruction scale, form actions, progression, and boss danger

Required assets:
- light hit
- medium hit
- heavy hit
- small break
- building collapse
- chain gain
- level up open
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
- boss down
- UI tap / confirm / locked / purchase fail

Why this matters now:
- code already contains many important moments, but silent or under-defined moments reduce impact
- destruction pleasure scales dramatically with sound even when visuals remain simple

---

## 2. Very Important Resources
These are not as urgent as the must-have group, but they should come immediately after.

### 2.1 UI Icon and Status Set
Need:
- icon set for stage pressure, route target, destruction, DP, overdrive, panic, seismic, boss state, weak point, shield, break window
- upgrade icons for meta upgrades and in-run upgrades
- status badge shapes for result and lobby panels

Required assets:
- `Icon_DP`
- `Icon_Stage`
- `Icon_NextStep`
- `Icon_BreakWindow`
- `Icon_Shield`
- `Icon_WeakPoint`
- `Icon_Overdrive`
- `Icon_Panic`
- `Icon_Seismic`
- `Icon_Retail`
- `Icon_Traffic`
- `Icon_Boss`

Why this matters now:
- UI text has been heavily simplified already
- the next gain in UX comes from fast icon recognition, not more text

### 2.2 Boss Encounter Identity Set
Status:
- Justice Sentinel logic exists
- phase, shield, drone, and break window systems are already implemented
- the encounter now needs a stronger authored identity

Required assets:
- sentinel body silhouette kit
- shield pylon silhouette kit
- phase 2 drone silhouette kit
- core exposed material / emissive state
- boss-only ring / warning particle set
- boss-only audio set
- boss-only HUD icon or badge

Why this matters now:
- the boss is a major progression promise
- if the boss looks too similar to normal buildings, the entire stage climb loses payoff

### 2.3 Environment Palette Sets
Need:
- at least 3 stage palette families so progression feels spatial, not only numeric

Recommended sets:
- district 1: civilian low-rise / clean suburb edge
- district 2: commercial strip / signage-heavy zone
- district 3: industrial utility zone / heavy traffic zone
- district 4+: fortified civic or defense zone

Required assets per district:
- ground material variant
- building palette variant
- prop color variant
- route marker tint
- fog / ambient tint preset

Why this matters now:
- stage progression is more satisfying when the world tone changes with difficulty

---

## 3. Resources Needed For Outgame Motivation
These support long-term retention rather than immediate moment-to-moment clarity.

### 3.1 Lobby / Meta Progression Resource Set
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

Why this matters now:
- the outgame flow is present in code
- visual motivation is still lighter than the actual system depth

### 3.2 Save / Progression Confirmation Resources
Need:
- lightweight UI moments that confirm long-term progress

Required assets:
- stage clear banner style
- unlock acquired badge
- DP gain burst
- new form target marker
- meta upgrade purchased pulse

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
- [ ] 3 district palette sets
- [ ] 8 to 10 street prop silhouettes
- [ ] 1 boss silhouette kit
- [ ] 1 shield pylon kit
- [ ] 1 drone kit

### VFX
- [ ] light hit burst
- [ ] medium hit burst
- [ ] heavy break burst
- [ ] collapse dust burst
- [ ] weak point hit burst
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
- [ ] UI feedback SFX set

### UI
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
