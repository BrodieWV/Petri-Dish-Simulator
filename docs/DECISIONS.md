# Decision Log

## D-001 — Unity is the initial engine
Accepted for mobile support and the existing studio workflow.

## D-002 — Build the child-friendly version first
Accepted to constrain systems and create a path toward advanced editions.

## D-003 — Use broad organism archetypes — Superseded
Originally accepted to avoid false species precision and safety risk. Superseded by D-013 after product direction changed to named real organisms with simplified educational behaviour.

## D-004 — Deterministic low-resolution 2D grid
Accepted for spatial patterns, debugging, seeded challenges, and mobile performance.

## D-005 — Separate simulation from Unity presentation
Accepted for testing, saves, and future expansion.

## D-006 — Simplified initial variables
Temperature, moisture, light, airflow/oxygen proxy, and general nutrients.

## D-007 — Failure must provide information
Accepted because collapse is part of experimentation.

## D-008 — No runtime AI dependency in MVP
Accepted for offline reliability, predictability, cost, and child safety.

## D-009 — No code in Phase 0 package
Accepted; this package defines implementation.

## D-010 — Temporary live-tuning controls
Accepted for rapid UI and simulation adjustment during development.

## D-011 — Unity 6.5 is the production baseline
Accepted on 27 July 2026 and amended on the same date by product-owner direction. The repository targets Unity `6000.5.3f1`. This supersedes the earlier `6000.3.20f1` baseline. Unity 2022, Unity 2023, and earlier Unity 6 editor versions are not supported project baselines.

## D-012 — The standard dish uses a circular active simulation mask
Accepted on 28 July 2026 after phone-simulator review exposed a mismatch between the round presentation and the square grid boundary. The deterministic 48 × 48 storage grid remains, but cells outside the visible agar circle are inactive and excluded from growth, interventions, snapshots, inspection, coverage, and aggregate metrics. Edge drying is measured radially from the agar boundary so authoritative simulation state matches the player-visible round dish.

## D-013 — Use named real organisms with simplified educational behaviour
Accepted on 30 July 2026. The game will use real organism names and recognisable biological traits without attempting laboratory-grade prediction. Each organism definition must include a scientific name, player-facing name, source-backed traits, confidence information, and explicit simplification notes. Values remain tunable for gameplay, and the game must not imply that simulated outcomes reproduce real laboratory results.

## D-014 — Use a hybrid 2D simulation and 3D presentation
Accepted on 30 July 2026. The deterministic 2D grid and generated colony texture remain authoritative. A reusable 3D petri-dish model presents that texture on a dedicated colony surface while the existing 2D UI remains until a later UI pass. Simulation behaviour must not depend on the 3D scene hierarchy or materials.

## D-015 — Multiple dishes and colony transfer follow stable definitions
Accepted on 30 July 2026. Nutrient intervention, cloning colonies into additional dishes, and comparative environments are planned after organism and medium definitions are stable. Evolution, treatment resistance, antibacterial and antifungal systems require lineage tracking and are deferred until the multi-dish save model is proven.

## D-016 — Organism and medium balance is definition-driven

Accepted on 28 July 2026 for the first Phase 2 content framework. Biological organism
values and medium environmental values are authored in validated ScriptableObject
definitions and copied into immutable runtime simulation values. Stable IDs and
definition versions are saved with the experiment. Schema-version-2 saves migrate only
to the original Rapid Bacterium and Nutrient Agar defaults; incompatible or missing
definitions fail without replacing the active experiment. Presentation assets remain
separate from biological simulation values.
