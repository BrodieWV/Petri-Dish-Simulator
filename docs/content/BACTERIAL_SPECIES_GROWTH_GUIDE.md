# Bacterial Species Growth and Appearance Guide

## Purpose

This document defines how each selected bacterial species should grow, spread, age, and appear during gameplay. It is a visual and simulation-design reference, not a laboratory cultivation guide. Speeds are relative gameplay tiers and must be tuned for readability and balance rather than treated as strain-specific measurements.

## Shared bacterial colony model

Each inoculation creates one or more founding patches according to the selected exposure style. A colony is not rendered as one uniformly scaling circle. Each occupied surface region should track local biomass, age, nutrients, moisture, oxygen access, stress, waste, dormancy, and species identity.

Most compact colonies should display four overlapping visual states:

1. **Founding microcolonies:** faint, scattered, nearly transparent dots at deposited cell locations.
2. **Active frontier:** younger outer growth entering unused medium; usually smoother, thinner, and more responsive than the centre.
3. **Mature interior:** denser biomass with the species' characteristic colour, elevation, texture, folds, or pigment.
4. **Old or stressed core:** slower growth, altered colour or opacity, drying, cracking, dormancy, sporulation, or local collapse depending on species.

Mass growth speed means how quickly visible biomass and thickness accumulate. Spread speed means how quickly the occupied footprint advances across the surface. These values are intentionally separate.

---

## Bacillus subtilis — The Survivor

### Growth habit

- **Overall form:** starts as compact round microcolonies, then develops a broad irregular colony with lobed or scalloped edges.
- **Expansion mechanism:** moderate radial expansion through division and local surface movement; the active edge advances faster than the centre thickens.
- **Mass growth speed:** fast.
- **Spread speed:** medium-fast on ordinary medium; faster and thinner on wetter or softer surfaces.
- **Vertical development:** begins flat and translucent, then becomes low-raised, opaque, wrinkled, and locally ridged.
- **Merger behaviour:** nearby colonies fuse readily; their joining line should disappear gradually rather than instantly.
- **Old-age behaviour:** nutrient-poor interior becomes dry, matte, and increasingly dormant; severe stress triggers visible spore-rich zones.

### Appearance

- **Young colour:** translucent grey-white to pale cream.
- **Mature colour:** cream, ivory, or light tan.
- **Stressed colour:** dull beige or greyed cream.
- **Texture:** initially smooth and moist; later matte, finely wrinkled, then strongly folded under favourable biofilm-forming conditions.
- **Edge:** smooth when young, becoming uneven, lobed, or slightly dendritic as local conditions diverge.
- **Height:** low at first; mature ridges and central folds create noticeable relief.
- **Surface response:** dry areas reduce shine and deepen cracking; wet areas encourage broader, thinner spread.

### What gameplay should show

**Founding stage**

- Each exposure deposit produces several faint points rather than an immediately solid circle.
- Closely spaced points merge into a translucent patch.
- The player can still recognise whether the starting exposure was a point, droplet, streak, or spray.

**Expansion stage**

- A pale active rim pushes outward.
- The centre becomes opaque before the edge.
- Growth accelerates toward locally richer and wetter cells, causing asymmetry.

**Maturation stage**

- Wrinkles emerge from several local centres, not as a single perfect radial pattern.
- Ridges thicken where biomass is crowded.
- The active perimeter remains smoother than the mature interior.

**Stress and survival stage**

- Drying or starvation stops perimeter advance first.
- Portions of the interior shift to a dusty, matte spore-state overlay.
- Rehydration restores growth from surviving edge pockets and dormant regions instead of making the entire colony resume simultaneously.

### Exposure-style differences

- **Needle touch:** one compact colony with strong radial age zoning.
- **Droplet:** many internal microcolonies merge; the whole droplet footprint clouds before strong outward expansion.
- **Streak:** dense streak sections become continuous wrinkled growth; isolated sections form separate colonies.
- **Spray:** numerous cream colonies compete and merge into an uneven field.

### Key simulation identity

Bacillus subtilis should be the clearest demonstration that a colony has a young frontier, a mature interior, and a stress-resistant dormant population.

---

## Deinococcus radiodurans — The Repair Specialist

### Growth habit

- **Overall form:** compact, rounded colonies with relatively regular borders.
- **Expansion mechanism:** primarily local division with little dramatic surface migration.
- **Mass growth speed:** slow-medium.
- **Spread speed:** slow.
- **Vertical development:** dense, smooth, convex colonies rather than broad films.
- **Merger behaviour:** touching colonies join slowly and may retain visible lobes for some time.
- **Old-age behaviour:** mature colonies deepen in pigment; damaged regions pause before recovering from surviving pockets.

### Appearance

- **Young colour:** very pale peach or translucent pink.
- **Mature colour:** salmon, coral, pink-red, or orange-red.
- **Stressed colour:** muted brick-red or patchy pale pink where pigment production and growth decline.
- **Texture:** smooth, dense, slightly glossy, and moist-looking.
- **Edge:** entire and rounded under even conditions; only mildly irregular under local stress.
- **Height:** convex, producing small domed colonies.
- **Damage response:** temporary dulling, local translucency, and halted edge movement rather than explosive death effects.

### What gameplay should show

**Founding stage**

- Tiny pale-pink points become visible later than the fast-growing species.
- Deposits remain distinct for longer, making the original exposure pattern easy to read.

**Expansion stage**

- Colonies widen slowly while becoming denser and redder.
- The colour increase should be more visually dramatic than the footprint increase.
- Uniform medium produces unusually neat circular colonies.

**Damage stage**

- Radiation, dehydration, or oxidative stress adds a cellular-damage load.
- The colony stops expanding and loses some saturation.
- Damage should appear as mottled internal zones rather than a single health bar effect.

**Repair stage**

- Once conditions improve, surviving zones brighten first.
- Recovery spreads through the existing colony before outward expansion resumes.
- Repair consumes stored energy, so a repaired colony may remain smaller than an unstressed control colony.

**Late stage**

- Colonies remain compact and strongly pigmented.
- Crowding creates clusters of touching coral domes rather than one highly spreading mat.

### Exposure-style differences

- **Needle touch:** one dense red-orange dome.
- **Droplet:** a field of small domes inside the original droplet boundary; later partial coalescence.
- **Streak:** bead-like colonies follow the streak, retaining separation longer than Bacillus.
- **Spray:** scattered pink-red dots produce a visually readable deposition map.

### Key simulation identity

Deinococcus radiodurans should look slow, compact, pigmented, and capable of pausing for repair. Its signature is recovery of existing biomass rather than rapid territorial spread.

---

## Rhodopseudomonas palustris — The Metabolic Shapeshifter

### Growth habit

- **Overall form:** thin, spreading films and low colonies whose appearance depends strongly on light and oxygen.
- **Expansion mechanism:** moderate local division combined with gradual film formation in wet areas.
- **Mass growth speed:** medium when the environmental mode is suitable; very slow when the light-oxygen-carbon combination is poor.
- **Spread speed:** medium in wet conditions; slow on dry or firm surfaces.
- **Vertical development:** mostly flat to low-raised; biomass accumulates as a coloured film rather than heavy folds.
- **Merger behaviour:** films merge seamlessly, although older source patches remain darker.
- **Old-age behaviour:** central regions become dark burgundy-brown, then thin or patchy as resources decline.

### Appearance

- **Young colour:** translucent beige-pink.
- **Photosynthetic colour:** rose, magenta-brown, purple-red, or burgundy.
- **Respiratory colour:** lighter tan-pink with reduced purple pigmentation.
- **Stressed colour:** dull brown-grey or patchy colourless film.
- **Texture:** smooth, wet, slightly glossy, and thin.
- **Edge:** diffuse and feathered in wet zones; more compact and defined on firmer medium.
- **Height:** very low.
- **Lighting response:** illuminated low-oxygen regions gradually deepen in purple-red pigment; shaded areas remain paler.

### What gameplay should show

**Founding stage**

- Deposited points initially appear as nearly invisible wet spots.
- Under favourable light and low oxygen, pigment appears before strong thickness.

**Mode-selection stage**

- The same colony should visibly select between two major presentations:
  - **Photosynthetic mode:** darker purple-red, thin but territorially active.
  - **Respiratory mode:** paler, somewhat denser, with reduced pigment.
- Mode transitions should move across the colony according to local conditions, not recolour it instantly.

**Expansion stage**

- A thin tinted frontier spreads preferentially through moist channels.
- Brightness gradients and covers can create sharply different neighbouring colony zones.
- Older source locations remain darker, revealing the colony's history.

**Stress stage**

- An unsuitable oxygen-light combination stalls expansion.
- Pigment fades unevenly.
- Persistent stress creates transparent gaps within the film rather than deep cracking.

**Recovery stage**

- Corrected conditions produce a wave of returning pigment followed by renewed edge movement.

### Exposure-style differences

- **Needle touch:** a small coloured disc that later extends as a thin film.
- **Droplet:** pigment develops across much of the droplet footprint because many founders are distributed through it.
- **Streak:** produces ribbon-like burgundy growth with lighter gaps where deposition was sparse.
- **Spray:** scattered pink films expand and merge into irregular tinted islands.

### Key simulation identity

Rhodopseudomonas palustris should make environmental state visible through pigment. Its footprint, thickness, and colour must change independently so that metabolic switching is readable without opening a statistics panel.

---

## Streptomyces violaceoruber — The Chemical Territory Builder

### Growth habit

- **Overall form:** filamentous bacterial network resembling a very fine fungal colony; expands through branching substrate mycelium and later develops aerial mycelium and spores.
- **Expansion mechanism:** directional tip growth with repeated branching rather than a uniformly pushing solid rim.
- **Mass growth speed:** slow initially, medium after the network establishes.
- **Spread speed:** medium; exploratory filaments may advance ahead of dense visible biomass.
- **Vertical development:** substrate network first, then powdery or velvety aerial growth over mature areas.
- **Merger behaviour:** compatible branches interweave; competing colonies can maintain a visible boundary.
- **Old-age behaviour:** older sectors become dry and powdery as spores accumulate; pigment varies with substrate and colony state.

### Appearance

- **Young colour:** translucent off-white threads.
- **Mature substrate colour:** violet, wine-red, muted purple, or occasionally orange-brown depending on the gameplay medium.
- **Aerial growth colour:** grey, grey-white, or cream.
- **Stressed colour:** pale beige-grey with reduced aerial growth.
- **Texture:** fine branching web at first; later velvety, chalky, or powdery.
- **Edge:** radiating filaments, branching fans, and irregular exploratory tips.
- **Height:** low substrate mat with visibly raised fuzzy aerial regions.
- **Chemical interaction:** competitor-facing edges may become denser, more pigmented, and less expansive while producing inhibition effects.

### What gameplay should show

**Founding stage**

- Several hair-thin filaments emerge from each founding point.
- Threads branch and leave gaps between them; the colony must not begin as a filled circle.

**Network stage**

- Branches explore outward faster than the interior fills.
- Secondary branching gradually closes empty spaces.
- Nutrient-rich directions create larger fans and more branch tips.

**Maturation stage**

- Pigment appears beneath the surface network.
- Aerial mycelium rises first in older, well-fed sectors as a grey-white velvet.
- Spore-rich areas become dry and powdery.

**Competition stage**

- Detection of another colony redirects branches toward or along the contact zone.
- Antibiotic investment creates a visible clear or weakened zone in susceptible competitors.
- The producing colony's frontier temporarily slows and may become darker or denser near the boundary.

**Late stage**

- The colony shows a layered history: sparse explorer filaments at the edge, dense coloured substrate growth behind them, and powdery aerial growth in the old interior.

### Exposure-style differences

- **Needle touch:** a single radial branching network.
- **Droplet:** many overlapping networks form a dense central mat with multiple outward fans.
- **Streak:** filamentous growth follows the streak but repeatedly sends branches sideways into fresh medium.
- **Spray:** isolated starburst colonies expand until their networks interweave.

### Key simulation identity

Streptomyces violaceoruber should never look like a recoloured circular bacterial blob. Its defining visual sequence is branching exploration, network thickening, substrate pigmentation, then grey or cream aerial sporulation.

---

## Relative gameplay comparison

| Species | Mass growth | Surface spread | Mature height | Edge character | Dominant visual change |
|---|---|---|---|---|---|
| Bacillus subtilis | Fast | Medium-fast | Medium | Lobed and wrinkled | Smooth cream growth becomes folded and spore-dry |
| Deinococcus radiodurans | Slow-medium | Slow | Medium, domed | Smooth and regular | Pale colonies deepen to coral-red and pause for repair |
| Rhodopseudomonas palustris | Medium, mode-dependent | Medium | Very low | Diffuse film | Pigment changes reveal metabolic mode |
| Streptomyces violaceoruber | Slow then medium | Medium | Low mat plus aerial layer | Filamentous and branching | White threads become pigmented, velvety, and powdery |

## Implementation notes

- Growth speed values should be species multipliers applied to local environmental suitability, not fixed animation durations.
- Spread and mass accumulation require separate parameters.
- Colour should be derived from age, metabolic state, stress, medium, and local density.
- Colony edges require species-specific masks or procedural rules; avoid using one shared circular expansion shader for all species.
- Exposure style determines founder placement. Species biology determines what those founders do afterward.
- The player should be able to infer growth history by looking at the colony without needing microscopic mode or numerical overlays.
