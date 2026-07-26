# Challenge Catalogue

## Purpose

Provide replayable experiments that reuse core systems while teaching distinct relationships. Values are provisional until simulation tuning begins.

## Challenge structure

Each challenge defines:

- Starting organism, medium, dish, and seed rules
- Starting environmental conditions
- Available interventions
- Primary objective
- Optional objectives
- Failure or ending conditions
- Scoring factors
- Discovery links
- Accuracy label

## C01 — Hold the Line

- Organism: Rapid Bacterium
- Medium: Nutrient Agar
- Objective: Maintain 35–55% coverage for 90 simulation seconds.
- Constraint: Only three temperature changes and two moisture doses.
- Lesson: Stability can be harder than maximum growth.
- Score: Time stable, health, unused interventions.

## C02 — Cold Start

- Organism: Rapid Bacterium
- Medium: Nutrient Agar
- Start: 10 °C
- Objective: Reach 40% coverage without exceeding 31 °C.
- Lesson: Recovery and gradual temperature correction.
- Optional: Never enter the `Too hot` state.

## C03 — Drying Edge

- Organism: Rapid Bacterium
- Medium: Low-Nutrient Agar
- Start: Strong edge evaporation.
- Objective: Reach 45% coverage while keeping edge moisture above 30%.
- Lesson: Spatial moisture patterns.
- Optional: Use no more than three moisture doses.

## C04 — Food Runs Out

- Organism: Rapid Bacterium
- Medium: Low-Nutrient Agar
- Objective: Reach 35% coverage and maintain health for 30 seconds after nutrients become limiting.
- Lesson: Growth consumes resources.
- Optional: Finish with at least 10% nutrients remaining.

## C05 — Too Much of a Good Thing

- Organism: Yeast-like Culture
- Medium: Nutrient Agar
- Start: High nutrients.
- Objective: Prevent waste stress while reaching 50% coverage.
- Lesson: More nutrients can produce crowding and waste.
- Available controls: Nutrients, airflow, temperature.

## C06 — Wake the Culture

- Organism: Yeast-like Culture
- Medium: Low-Nutrient Agar
- Start: Dormant culture.
- Objective: Trigger recovery without causing heat or saturation stress.
- Lesson: Dormancy and delayed recovery.
- Score: Recovery time and final health.

## C07 — Threads Through Timber

- Organism: Filamentous Fungus
- Medium: Wood-Chip Substrate
- Objective: Connect three separated nutrient pockets.
- Lesson: Branching growth and substrate structure.
- Optional: Use less than a specified nutrient supplement.

## C08 — Keep the Wood Damp

- Organism: Filamentous Fungus
- Medium: Wood-Chip Substrate
- Objective: Maintain continuous growth for two simulated days.
- Constraint: Moisture distributes unevenly.
- Lesson: Retention, local drying, and slow response.

## C09 — Follow the Food

- Organism: Slime Mould
- Medium: Low-Nutrient Agar
- Objective: Reach two food sources and form a connecting network.
- Lesson: Gradient-driven movement.
- Score: Network length, time, and wasted branching.

## C10 — The Efficient Path

- Organism: Slime Mould
- Medium: Nutrient Agar
- Objective: Connect four food nodes with less than a maximum network mass.
- Lesson: Distributed path optimisation.
- Note: This is a gameplay abstraction inspired by observed behaviour, not a precise laboratory recreation.

## C11 — Uninvited Guest

- Organism: Rapid Bacterium
- Medium: Moist Soil Gel
- Event: Contaminant appears after a deterministic trigger.
- Objective: Preserve at least 40% primary-culture coverage for 60 seconds.
- Lesson: Competition and environmental trade-offs.
- Available controls affect both organisms differently.

## C12 — Shared Dish

- Organisms: Rapid Bacterium and Yeast-like Culture
- Medium: Nutrient Agar
- Objective: Keep both populations above 15% coverage.
- Lesson: Coexistence and limiting factors.
- Failure: Either population collapses.

## C13 — Bright Window

- Organism: Light-sensitive archetype or Rapid Bacterium variant
- Medium: Nutrient Agar
- Objective: Maintain growth during a changing light cycle.
- Lesson: Light stress and indirect warming.
- Unlock: After light control is introduced.

## C14 — Breathing Room

- Organism: Yeast-like Culture
- Medium: Nutrient Agar
- Objective: Use airflow to reduce waste pressure without drying the dish.
- Lesson: One intervention can have opposing effects.

## C15 — Condensation Cycle

- Organism: Filamentous Fungus
- Medium: Moist Soil Gel
- Objective: Maintain suitable moisture through repeated warming and cooling.
- Lesson: Condensation, evaporation, and delayed effects.

## C16 — Minimum Intervention

- Organism: Player choice from unlocked beginner organisms
- Medium: Nutrient Agar
- Objective: Reach a healthy stable culture with no more than four interventions.
- Lesson: Observation before action.
- Score: Fewer actions, stability, and health.

## C17 — Same Seed, New Plan

- Organism: Rapid Bacterium
- Medium: Nutrient Agar
- Seed: Fixed weekly seed.
- Objective: Improve on the player’s previous score.
- Lesson: Determinism enables controlled comparison.
- Online services are not required; scoring can remain local.

## C18 — Edge to Edge

- Organism: Filamentous Fungus
- Medium: Wood-Chip Substrate
- Objective: Reach opposite sides of the dish while keeping central health above 50%.
- Lesson: Expansion can leave old regions resource-limited.

## C19 — Recovery Window

- Organism: Rapid Bacterium
- Medium: Nutrient Agar
- Event: Alternating short heat and cold faults.
- Objective: Recover after each event without entering critical condition.
- Lesson: Stress accumulation and recovery time.

## C20 — Balanced Ecosystem

- Organisms: Two compatible archetypes
- Medium: Moist Soil Gel
- Objective: Maintain both populations, moisture, and nutrients within target bands.
- Lesson: Multi-variable equilibrium.
- Position: Late initial-content challenge.

## Difficulty tiers

### Beginner

One dominant variable, generous limits, direct feedback, unlimited pause.

### Intermediate

Two interacting variables, limited interventions, delayed effects, partial hints.

### Advanced

Multiple organisms or spatial gradients, strict efficiency objectives, incomplete exact feedback.

## Challenge star model

- One star: Complete the primary objective.
- Two stars: Complete one optional objective.
- Three stars: Complete all optional objectives or exceed an efficiency threshold.

No challenge requires advertising or payment to earn three stars.

## Content rollout recommendation

Vertical slice:

- Guided experiment `The Comfortable Range`

First playable build:

- C01 Hold the Line
- C02 Cold Start
- C03 Drying Edge

MVP:

- Ten challenges selected from C01–C16

Post-launch:

- Multi-species, weekly-seed, and advanced equilibrium challenges
