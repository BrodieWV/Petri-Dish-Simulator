# Roadmap

## Current status

Phase 1 is complete. Phase 2 automated implementation is complete and the Laboratory Hub presentation foundation is merged. The immediate next milestone is Phase 3.0: make the Laboratory Hub and dish inspection experience function correctly before expanding Phase 3 systems.

## Phase 0 — Foundation — Complete

Define vision, scope, game design, architecture, simulation model, data model, UX, content, progression, analytics, tests, risks, decisions, agent instructions, and backlog.

## Phase 1 — Living Dish Prototype — Complete

One dish, one medium, one organism, temperature, moisture, deterministic simulation, colony growth and decline, speed controls, feedback, seeded reset, saving, inspection, accessibility, and one guided experiment.

Exit achieved: players can produce visibly different outcomes by changing conditions, and the Unity Edit Mode suite passes on the locked production editor.

## Phase 2 — Multi-Organism Experimental Slice — Implementation complete; review active

- Validated data-driven organism and medium definitions
- Two named real organisms and two media with meaningfully distinct behaviour
- Organism and medium selection
- 3D petri-dish presentation with the live 2D colony texture on the colony surface
- Nutrient intervention
- Save compatibility across selected organisms and media
- Content validation and regression tests
- Laboratory Hub presentation foundation using the real 3D dish

Exit: players can select organisms and media, run meaningfully different experiments, intervene with temperature, moisture, and nutrients, and understand why outcomes differ.

The integrated branch passes the complete automated suite. Qualified scientific review and representative manual/device verification remain useful, but Phase 3 expansion must not begin by adding more content or systems until the current Laboratory Hub interaction and navigation are functional.

## Phase 3.0 — Laboratory Hub Functionalisation — Next

Correct and complete the existing Laboratory Hub before expanding Phase 3 gameplay systems.

Scope:

- Wire all visible Laboratory Hub navigation and action buttons to their intended destinations or honest temporary states.
- `Lab` returns to the Laboratory Hub.
- `New Experiment` and `+ Start New Experiment` enter the experiment setup flow.
- `Open Dish` enters the selected dish's full experiment view.
- `Compare` entry points open the comparison flow or a clear unavailable/requirements state until multiple dishes exist.
- `Journal`, `Collection`, `Challenges`, and `Settings` open their implemented destinations or explicit temporary states rather than dead controls.
- Left/right dish controls are structurally functional and ready to switch dishes once multiple-dish persistence exists.
- The 3D dish is directly inspectable from the Hub using constrained orbit/rotation and zoom.
- Desktop/Editor controls: mouse drag to orbit, mouse wheel to zoom, reset to the default view.
- Touch controls: one-finger drag to orbit and pinch to zoom where practical.
- Camera movement must not affect simulation state.
- UI interactions must not accidentally move the dish camera.
- Orbit/zoom must remain within safe pitch, yaw, distance, and framing limits.
- The dish remains correctly framed at supported landscape resolutions.
- Hub → Open Dish → return to Lab navigation is regression tested.
- Play/Stop and scene changes must not leak cameras, RenderTextures, EventSystems, or presentation objects.

Exit: every visible control behaves intentionally, navigation is coherent, the real 3D dish can be inspected comfortably, and the existing UI is stable enough to support Phase 3 expansion.

## Phase 3 — Comparative Experiments and Progression

After Phase 3.0 is verified, expand to four organisms and four media, multiple dishes, colony transfer and cloning, source lineage, comparison tools, full guided set, challenges, unlock tree, journal, collection, contamination, cosmetics, achievements, and statistics.

Recommended order after Phase 3.0:

1. Multiple dishes and independent experiment state.
2. Content expansion to four organisms and four media.
3. Comparison/history tools.
4. Colony transfer, cloning, and lineage.
5. Guided experiments, challenges, journal, collection, and unlocks.
6. Contamination, cosmetics, achievements, and statistics.

## Phase 4 — Advanced Biology Systems

Evolution and mutation, antibacterial and antifungal treatments, resistance, contamination, multi-species competition, expanded environmental variables, and advanced educational content.

These systems require stable organism definitions, lineage tracking, and multi-dish saves before implementation.

## Phase 5 — Mobile Production

Optimisation, touch UX, accessibility, suspend/resume, offline-safe saves, tutorials, localisation framework, analytics, crash reporting, store compliance, and optional rewarded ads.

## Phase 6 — Launch and Learning

Closed testing, retention analysis, tuning, tutorial improvements, performance fixes, content correction, and monetisation validation.

## Later expansions

Fungi Growing Simulator, Petri Terrarium, multi-species food webs, advanced microscopy, genetics education edition, PC laboratory mode, and reusable biological simulation assets.
