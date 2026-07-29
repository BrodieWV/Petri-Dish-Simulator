# Simulation Model

## Core entities

- Dish: size, boundaries, evaporation, and grid.
- Medium: water retention, nutrients, diffusion, structure, and compatibility.
- Environment: temperature, moisture, light, airflow/oxygen proxy, nutrients, waste, and time.
- Organism definition: tolerance and behaviour data.
- Population: local density, health, stress, dormancy, maturity, energy, and trend.
- Colony: a connected visual grouping derived from population state.

## Organism traits

Preferred and survivable temperature/moisture ranges, light sensitivity, oxygen preference, nutrient needs, growth and consumption rates, waste, spread style, dormancy, recovery, competition, inhibition, and visual profile.

The first Phase 2 framework implements starting health and colony shape; preferred,
growth, and survival temperature ranges; preferred, growth, and survival moisture
thresholds; nutrient suitability; health/stress response; growth/death; resource demand;
carrying capacity; and local spread. These values come from the selected
`OrganismDefinition`; the central simulation contains only general algorithms and
normalised grid rules.

The default `rapid-bacterium` definition reproduces the previous vertical-slice balance.
It is an educationalised archetype rather than species-level cultivation guidance.

## Medium traits

The selected `MediumDefinition` supplies starting and maximum moisture/nutrients,
optional moisture and nutrient diffusion, moisture response, radial evaporation,
edge-drying multiplier, heat-driven evaporation, and spread resistance. The default
`nutrient-agar` definition reproduces the previous vertical-slice values: both diffusion
rates and spread resistance are zero.

Structure, compatibility, multiple nutrient pools, and waste remain later Phase 2 work
and are not implied by this first framework.

## Suitability

Each environmental factor uses a tunable response curve. Ideal conditions approach 1.0; limiting conditions reduce growth; lethal ranges cause damage or death. The model exposes the strongest limiting factor.

## Resource flow

Growth consumes nutrients and moisture, metabolism produces waste, waste diffuses or decays, and dead biomass may return some nutrients.

## Spread archetypes

- Bacterium: fast radial expansion.
- Fungus: branching edge-biased growth.
- Slime mould: movement toward food gradients.
- Yeast-like culture: dense budding clusters.

## Events

Condensation, dry edge, nutrient pocket, bloom, dormancy, waste buildup, contamination, colony collision, and resource crash.

## Scientific labels

Observed relationship, educational simplification, gameplay abstraction, or fictional.

Definition assets include a scientific/simplification note as content metadata. They do
not include presentation colours, textures, animation, or audio. A stable visual-profile
ID is presentation metadata only and is not copied into authoritative simulation values.

## Safety

Use broad or fictionalised archetypes and avoid actionable real-world pathogen cultivation details.
