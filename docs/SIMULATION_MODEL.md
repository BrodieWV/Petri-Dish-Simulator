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

## Safety

Use broad or fictionalised archetypes and avoid actionable real-world pathogen cultivation details.