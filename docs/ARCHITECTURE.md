# Architecture

## Objective

Allow biological rules to change without coupling them to Unity scenes, artwork, UI, saving, monetisation, or platform SDKs.

## Layers

### Content definitions

Data for organisms, media, nutrients, tolerance curves, visuals, challenges, discoveries, events, and rewards.

Phase 2 organism and medium simulation values are authored as `OrganismDefinition` and
`MediumDefinition` ScriptableObjects under `Assets/PetriDish/Content`. A
`SimulationDefinitionCatalog` owns the available definitions, enforces stable unique IDs,
and identifies the default vertical-slice pair.

Definition assets are validated before a simulation starts or a save is restored. The
simulation copies their biological values into immutable runtime value objects at
construction, so changing a ScriptableObject during play cannot alter an existing
deterministic experiment. Presentation data such as colours, textures, animation, and
audio is not stored in these simulation definitions. Definitions expose only a stable
visual-profile ID; a later presentation registry will resolve that ID without introducing
art references into authoritative simulation state.

### Simulation core

A deterministic engine-independent model responsible for the dish grid, environmental fields, resources, populations, growth, stress, death, dormancy, spread, competition, events, random streams, and outcomes.

The simulation core must not know about GameObjects, prefabs, particles, audio, advertising, or platform APIs.

### Application services

Coordinates experiment lifecycle, interventions, speed, save/load, progression, challenge evaluation, discoveries, analytics, and app lifecycle.

### Presentation

Renders the dish, colonies, overlays, particles, camera, UI, audio, and accessibility. It observes read-only simulation snapshots and never owns authoritative state.

`DishRenderer` remains the sole owner of the generated colony `Texture2D`. It updates that
same texture from read-only simulation snapshots and publishes an event only when the
texture object itself is recreated. `ColonySurfacePresenter` observes that presentation
texture and assigns it to a configured `MeshRenderer` shader property through one cached
`MaterialPropertyBlock`. It does not copy pixels, instantiate materials per frame, mutate
shared imported materials, retain snapshots, or access `PetriSimulation`.

`ColonySurfacePresenter` also owns presentation-only scale, offset, and horizontal/vertical
flip controls for correcting model-UV alignment. It writes the live texture to the configured
shader texture property and the alignment to that property's conventional `_ST` vector in
the same cached `MaterialPropertyBlock`. The built-in Standard shader therefore uses
`_MainTex` and `_MainTex_ST`. These controls do not change the generated texture, model UVs,
shared material, simulation state, flat fallback, or inspection surface.

The custom presenter Inspector adds `Auto Centre`, `Auto Fit`, and `Reset Alignment`
actions. Auto Centre derives an offset from the target mesh's UV0 bounds while preserving
the selected scale and flips. Auto Fit uses the larger UV0 extent to calculate one uniform
scale, preserving the circular texture rather than stretching it, and then centres it.
Reset Alignment restores scale 1,1, offset 0,0, and disabled flips. Editor calculations use
read-only mesh data and do not require a model import-setting change; runtime calls require
a readable mesh and fail without replacing an existing live texture binding otherwise.

`RuntimeBootstrap` binds scene `ColonySurfacePresenter` components to its runtime-created
`DishRenderer` after initial UI construction and after later scene loads. The existing 2D
`RawImage` remains available as a visual fallback and as the current normalized tap input
surface. Hiding its image changes only its alpha, preserving inspection raycasts until a
separate reviewed 3D raycast-mapping task is approved.

The current project uses Unity's built-in render pipeline. The presenter defaults to the
Standard shader texture property `_MainTex`, but the property name is serialized and
validated so another compatible shader can be configured explicitly.

### Platform adapters

Replaceable integrations for mobile lifecycle, notifications, achievements, cloud saves, ads, purchases, crash reporting, and analytics.

## Simulation space

Use a low-resolution two-dimensional grid. Cells can contain medium, moisture, temperature offset, light, oxygen proxy, nutrient pools, waste, population density, and contamination flags.

## Fixed-step pipeline

Apply interventions; update environment; calculate suitability; consume resources; apply growth/stress/death; spread populations; resolve competition; evaluate discoveries and outcomes; publish a snapshot.

The Phase 2 medium step optionally diffuses moisture and nutrients through deterministic
cardinal-neighbour averaging. Both default diffusion rates are zero, preserving the
vertical-slice baseline. Organism carrying capacity and medium spread resistance are
applied by the simulation core without any presentation dependency.

## Determinism

Every experiment has a seed. Simulation randomness, world generation, events, and visual randomness use separate streams. Frame rate must not alter outcomes.

The selected organism ID, medium ID, and both definition versions are part of the
deterministic experiment identity. Saves may resume only when those exact definitions are
available. Balance changes require a definition-version increment and an explicit
migration or controlled incompatibility message.

## Saving

Save schema version, content version, seed, tick, dish state, player actions, challenge state, discoveries, progression, and random-stream state. Never serialise transient Unity objects.

Simulation save schema version 3 stores stable organism and medium IDs plus their
definition versions. Experiment save schema version 3 resolves those IDs through the
validated catalog before constructing the restored simulation. Existing experiment and
simulation schema-version-2 saves migrate to the original `rapid-bacterium` and
`nutrient-agar` definitions because those saves predate content selection. Missing,
duplicate, malformed, or version-mismatched definitions fail before replacing the running
experiment.
