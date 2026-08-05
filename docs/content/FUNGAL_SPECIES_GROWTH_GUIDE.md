# Fungal Species Growth and Appearance Guide

## Purpose

This document defines how each selected fungal species should grow, spread, mature, fruit, age, and appear during gameplay. It is a visual and simulation-design reference, not a cultivation guide. Relative speeds and phase timings are gameplay values that should be tuned for readability and balance.

## Shared fungal growth model

Fungal colonies should be represented as branching networks rather than expanding solid discs. Circular outlines can emerge because hyphal tips radiate outward under uniform conditions, but the visible colony should still contain directional threads, branch density, old interior zones, and species-specific structures.

Each local region should track hyphal density, active tip density, branch age, moisture, available substrate, oxygen, light exposure, stress, reproductive state, and species identity.

Mass growth speed means how quickly the colony fills space with dense mycelium or fruiting structures. Spread speed means how quickly leading hyphal tips extend into new substrate. A fungus may spread quickly while remaining visually thin.

---

## Neurospora crassa — The Biological Clock

### Growth habit

- **Overall form:** extremely fast radial hyphal fans with a sparse advancing edge and a denser branched interior.
- **Expansion mechanism:** strong apical tip growth followed by repeated subapical branching.
- **Mass growth speed:** fast.
- **Spread speed:** very fast; one of the fastest selected fungi.
- **Vertical development:** low substrate mycelium followed by upright aerial hyphae and conidiation.
- **Merger behaviour:** neighbouring fronts fuse into a continuous network; the original colony centres remain readable through age and colour bands.
- **Old-age behaviour:** older zones become more aerial, orange, dry-looking, and spore-rich while interior transport threads remain visible beneath them.

### Appearance

- **Young colour:** nearly transparent to white.
- **Mature vegetative colour:** white to pale cream.
- **Reproductive colour:** bright orange to burnt orange from conidia.
- **Stressed colour:** pale, thin, grey-white, with interrupted orange development.
- **Texture:** fine cottony threads at first; later fuzzy and powdery in reproductive bands.
- **Edge:** long leader hyphae with visible gaps, followed by a dense branching zone.
- **Height:** low at the frontier; taller and fuzzier in older bands.
- **Rhythm:** regular light-dark conditions create repeating bands of dense orange sporulation.

### What gameplay should show

**Founding stage**

- Several white hyphae emerge from each founder.
- One or more leader tips rapidly move ahead of the central biomass.
- The initial exposure shape remains briefly visible before radial growth dominates.

**Rapid expansion stage**

- Thin leader hyphae advance first.
- Branches appear behind them and fill the sector.
- The centre does not simply enlarge; it develops a transport network and older aerial zones.

**Clock-expression stage**

- Regular environmental cycles create visible concentric or sector-shaped orange bands.
- Constant light creates more continuous orange development but weaker rhythmic separation.
- Constant darkness leaves broad pale vegetative regions.
- A disrupted cycle produces compressed, widened, or missing bands rather than resetting the whole colony visually.

**Stress stage**

- Dry or nutrient-poor sectors stop branching before leading tips fully halt.
- The frontier becomes sparse and uneven.
- Recovery begins from surviving tips and internal branch points.

**Late stage**

- The colony becomes a historical record of environmental cycles: pale growth zones alternating with orange spore bands.

### Exposure-style differences

- **Needle touch:** one rapidly expanding radial fan with clear banding.
- **Droplet:** many founders create a dense pale centre before synchronised outward fans emerge.
- **Streak:** the streak becomes a long launch line for overlapping fans.
- **Spray:** separate fast-growing orange-white colonies collide early and form complex overlapping bands.

### Key simulation identity

Neurospora crassa should feel fast and rhythmic. Its defining feature is not simply orange colour but the appearance of time as repeated growth and sporulation bands.

---

## Pleurotus ostreatus — The Wood Recycler

### Growth habit

- **Overall form:** dense white mycelium moving through and over fibrous substrate, followed by local knots, pins, and layered oyster-shaped fruiting bodies.
- **Expansion mechanism:** branching hyphae penetrate substrate fibres and form rope-like cords across gaps.
- **Mass growth speed:** medium-fast during colonisation; very high locally during fruit-body enlargement.
- **Spread speed:** medium.
- **Vertical development:** low mycelial mat during colonisation, then strongly three-dimensional fruiting clusters.
- **Merger behaviour:** separate mycelial islands fuse readily and redistribute growth through the connected network.
- **Old-age behaviour:** exhausted mycelium becomes thinner, yellowed, or dry; old fruit bodies flatten, curl, darken, and lose firmness.

### Appearance

- **Young colour:** translucent white threads.
- **Mature mycelium:** bright white, dense, cottony, and sometimes rope-like.
- **Primordia:** tiny white knots becoming grey-beige pins.
- **Fruiting bodies:** pale grey, blue-grey, beige-grey, or brown-grey fan-shaped caps with white undersides.
- **Stressed colour:** cream, pale yellow, or dull grey.
- **Texture:** cottony mycelium, fibrous cords, then smooth to slightly velvety caps with fine gills underneath.
- **Edge:** irregular and substrate-guided rather than perfectly radial.
- **Height:** low during colonisation; high when fruiting.
- **Substrate response:** growth follows fibres, cracks, and wood-chip contacts, making substrate geometry visibly important.

### What gameplay should show

**Founding stage**

- White hyphae attach to nearby fibres and branch along them.
- On disconnected wood chips, several isolated colonies appear rather than one surface disc.

**Colonisation stage**

- Thin threads become dense white patches.
- Patches connect through visible cords.
- The colony penetrates downward as well as spreading across the visible surface, represented by whitening within translucent substrate or progress beneath chip surfaces.

**Consolidation stage**

- Fully occupied regions become bright, thick, and coherent.
- Free surface expansion slows while internal resource conversion continues.
- Small dense knots form where fruiting conditions are locally favourable.

**Fruiting stage**

- Knots become pins, then layered fan-shaped mushrooms.
- Fresh-air limitations produce elongated stems and undersized caps.
- Low humidity causes dry edges and stalled caps.
- Excess water produces glossy, heavy-looking surfaces and local collapse risk.

**Late stage**

- Mature caps broaden and overlap.
- Spore release appears as a subtle pale deposit beneath clusters.
- Spent substrate loses brightness and structural integrity.

### Exposure-style differences

- **Needle touch:** one colonisation island that follows adjacent fibres.
- **Droplet:** several dense white patches appear within the wetted area and then connect.
- **Streak:** a white mycelial strip spreads sideways into the wood substrate.
- **Mixed substrate inoculation:** many internal colonies connect into the fastest full-block colonisation pattern.

### Key simulation identity

Pleurotus ostreatus should demonstrate that fungal success is not only surface area. It must colonise a three-dimensional substrate network and then switch from hidden resource capture to visible fruiting.

---

## Trichoderma harzianum — The Fungal Hunter

### Growth habit

- **Overall form:** very fast, low white mycelial growth that rapidly becomes dense and produces vivid green spore patches.
- **Expansion mechanism:** aggressive branching and redirection toward nearby fungal hyphae or nutrient-rich zones.
- **Mass growth speed:** very fast.
- **Spread speed:** very fast.
- **Vertical development:** initially low; later forms powdery raised sporulation cushions.
- **Merger behaviour:** self-colonies fuse quickly; contact with another fungus triggers overgrowth, coiling, lysis effects, or a contested boundary.
- **Old-age behaviour:** green spore zones become darker, duller, and dusty; exhausted regions thin behind the advancing front.

### Appearance

- **Young colour:** transparent to bright white.
- **Mature vegetative colour:** white to cream.
- **Reproductive colour:** pale green, emerald green, then dark forest green in dense spore areas.
- **Stressed colour:** yellow-green, grey-green, or sparse white.
- **Texture:** fine and flat at the frontier; cottony behind it; granular or powdery where conidia form.
- **Edge:** rapidly advancing, irregular, and locally pointed toward targets.
- **Height:** low mycelium with medium raised spore cushions.
- **Interaction response:** contact-facing sectors become denser and more structured than unopposed sectors.

### What gameplay should show

**Founding stage**

- A faint white web appears quickly.
- Multiple founders merge before they become individually thick.

**Expansion stage**

- The active frontier covers territory faster than dense biomass accumulates.
- Branch tips visibly redirect toward another fungal colony when chemical detection is active.
- Rich zones become dense white earlier than poor zones.

**Attack stage**

- Trichoderma hyphae contact and trace along target hyphae.
- Local coiling or close wrapping is shown in magnified mode.
- At normal scale, the target edge becomes pale, broken, or recessed while Trichoderma overgrows it.
- The attack zone should advance unevenly according to moisture and target resistance.

**Sporulation stage**

- White mature patches develop discrete green islands.
- Islands expand and merge into a powdery green field.
- Light and surface exposure can influence where green appears, giving the colony a mottled appearance.

**Late stage**

- Older interior becomes dark green and dusty.
- The frontier may remain white, making age visually obvious.

### Exposure-style differences

- **Needle touch:** one white radial colony that rapidly develops green sectors.
- **Droplet:** the droplet becomes a dense white patch, then mottled green from several points.
- **Streak:** a fast white-green strip sends broad lateral fronts across the plate.
- **Spray:** many green-centred colonies merge into an aggressive mat.

### Key simulation identity

Trichoderma harzianum should look faster and more predatory than the other fungi. Its signature is a white attacking frontier followed by conspicuous green sporulation.

---

## Pilobolus crystallinus — The Spore Cannon

### Growth habit

- **Overall form:** low, inconspicuous vegetative mycelium within dung-like substrate followed by upright transparent sporangiophores capped with black sporangia.
- **Expansion mechanism:** substrate-limited mycelial colonisation; reproductive structures become the dominant visible feature.
- **Mass growth speed:** medium.
- **Spread speed:** slow across the plate; long-distance spread occurs through projectile sporangia rather than crawling growth.
- **Vertical development:** very high relative to colony footprint because stalks rise above the substrate.
- **Merger behaviour:** vegetative colonies merge quietly; reproductive stalks remain individually readable.
- **Old-age behaviour:** fired stalks collapse and become translucent; missed sporangia remain stuck to nearby surfaces.

### Appearance

- **Young colour:** nearly invisible white hyphae within the substrate.
- **Mature stalk:** clear to glassy, sometimes with a swollen translucent base.
- **Sporangium:** glossy black or very dark brown.
- **Stressed colour:** cloudy, pale beige, or collapsed translucent structures.
- **Texture:** wet, glass-like stalks contrasted with firm black caps.
- **Edge:** vegetative edge is diffuse and mostly hidden.
- **Height:** tall, slender reproductive stalks.
- **Light response:** stalks bend and orient toward the strongest useful light direction.

### What gameplay should show

**Colonisation stage**

- Surface change is subtle: slight whitening or damp texturing within the substrate.
- The player should initially wonder whether growth has begun, encouraging inspection or time-lapse use.

**Stalk initiation stage**

- Tiny clear bumps emerge from mature regions.
- Bumps elongate into upright glassy stalks.
- Dark sporangia form at their tips.

**Aiming stage**

- Stalks slowly bend toward the dominant light source.
- Different local shadows produce different launch angles.
- A visible pressure or readiness cue can be represented by swelling at the stalk base rather than a numerical countdown.

**Launch stage**

- Individual black sporangia fire rapidly along the stalk's current aim.
- Successful impacts leave adhesive dark dots on target vegetation or surfaces.
- Misses stick to dish walls, lids, or unsuitable substrate.

**Secondary-colony stage**

- A landed sporangium does not immediately become a large colony.
- It creates a delayed founding point if the destination becomes suitable, clearly separating dispersal from germination.

### Exposure-style differences

- **Needle touch:** a small local patch producing a clustered launch battery.
- **Droplet:** stalks appear across the droplet footprint and aim in slightly different directions.
- **Mixed substrate exposure:** reproductive stalks emerge from multiple internal locations.
- **Spore impact:** one adhesive dark point becomes a new delayed colony.

### Key simulation identity

Pilobolus crystallinus should shift the meaning of spread. Its colony footprint is modest, but its reproduction can cross the entire dish through aimed ballistic dispersal.

---

## Schizophyllum commune — The Split-Gill Architect

### Growth habit

- **Overall form:** persistent white wood-decay mycelium that develops radial fans, corded networks, and small shelf-like fruiting bodies after compatible networks unite.
- **Expansion mechanism:** branching hyphae penetrate wood and extend across its surface; reproductive progression depends on mating compatibility and environmental state.
- **Mass growth speed:** medium-slow.
- **Spread speed:** medium.
- **Vertical development:** low mycelial network followed by small but clearly raised fan-shaped fruiting structures.
- **Merger behaviour:** ordinary contact does not always produce the same result; compatible mycelia fuse into a more capable reproductive network, while incompatible contacts remain as seams or interwoven boundaries.
- **Old-age behaviour:** mature mycelium becomes tougher and less bright; fruit bodies dry, close, or curl during unfavourable moisture and reopen when conditions improve.

### Appearance

- **Young colour:** translucent white.
- **Mature mycelium:** white, cream, or pale grey.
- **Fruiting bodies:** small fan-shaped shelves, white to grey-beige, with split or curled gill-like folds underneath.
- **Stressed colour:** dull beige-grey or slightly yellowed white.
- **Texture:** fine web becoming tough, felted, and locally corded; fruit bodies are fuzzy above and folded beneath.
- **Edge:** irregular radial fans guided by wood grain and cracks.
- **Height:** low network with low-medium fruiting shelves.
- **Moisture response:** fruit bodies visibly close, curl, or stiffen when dry and expand when moisture returns.

### What gameplay should show

**Founding stage**

- Each exposure founder creates a separate pale mycelial genotype marker internally, even when the player cannot yet distinguish them visually.
- Threads follow grain lines and invade cracks.

**Expansion stage**

- Radial fans overlap and form a felted mat.
- Stronger cords connect established regions.
- Growth on wood should appear partly embedded rather than painted on top.

**Compatibility encounter stage**

- When two colonies meet, the contact line is evaluated.
- Compatible networks gradually lose the boundary, thicken, and gain a new reproductive-state visual.
- Incompatible networks retain a seam, interlock without full integration, or redirect along the boundary.

**Fruiting stage**

- Small pale knots arise from the compatible mature network.
- Knots flatten into fan-shaped shelves.
- Underside folds separate and become visibly split as the fruit body matures.

**Dry-rewet stage**

- Dry fruit bodies curl inward and become rigid-looking rather than instantly dying.
- Rewetting causes a gradual reopening animation.
- Repeated cycles leave older shelves darker and more weathered.

### Exposure-style differences

- **Single needle touch:** one genotype forms a broad wood-following network but cannot complete all compatibility-dependent progression alone.
- **Two-point inoculation:** two networks advance toward each other, making the contact outcome central to play.
- **Droplet:** multiple founders create a dense patch but may still represent only one compatible identity depending on the selected scenario.
- **Spray:** many small wood-bound fans produce several compatibility boundaries and possible fruiting centres.

### Key simulation identity

Schizophyllum commune should make network identity and compatibility visible. It is not the fastest fungus; its reward is architectural complexity, durable wood colonisation, and moisture-responsive fruiting bodies.

---

## Relative gameplay comparison

| Species | Mass growth | Hyphal spread | Mature height | Edge character | Dominant visual change |
|---|---|---|---|---|---|
| Neurospora crassa | Fast | Very fast | Medium | Long sparse leaders | White fans develop rhythmic orange bands |
| Pleurotus ostreatus | Medium-fast | Medium | High when fruiting | Fibre-guided and corded | White wood colonisation produces grey oyster clusters |
| Trichoderma harzianum | Very fast | Very fast | Medium | Aggressive and target-directed | White frontier becomes powdery green |
| Pilobolus crystallinus | Medium | Slow locally | Very high stalks | Hidden or diffuse | Clear stalks aim and fire black sporangia |
| Schizophyllum commune | Medium-slow | Medium | Low-medium | Wood-guided radial fans | Compatible white networks form split-gill shelves |

## Implementation notes

- Hyphal tip movement and biomass infill should be separate systems.
- Fungal fronts need directional branches, not a single expanding alpha mask.
- Branch age should drive density, aerial growth, reproductive state, colour, and texture.
- Species-specific reproductive structures should be spawned from eligible mature regions rather than from the colony centre by default.
- Substrate geometry should influence Pleurotus and Schizophyllum strongly, while surface light direction should influence Neurospora patterning and Pilobolus aiming.
- Exposure style determines the founder map; compatibility, branching rules, and environmental response determine the final colony form.
- At normal gameplay zoom, growth must remain readable as shape, colour, height, and texture changes. Microscopic mode can reveal individual hyphae, coiling, branch fusion, and spore structures.
