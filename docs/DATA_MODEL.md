# Data Model

## Goals

Data-driven content, versioned saves, deterministic experiments, validation, and separation of static definitions from live state.

## Definitions

### OrganismDefinition

Stable ID, display name, scientific label, tolerances, growth, metabolism, nutrients, spread, competition, dormancy, visuals, audio, discoveries, and unlocks.

### MediumDefinition

Stable ID, category, water retention, nutrient capacity, diffusion, evaporation, structure, visuals, compatibility, and educational note.

### DishDefinition

Stable ID, geometry, grid profile, boundaries, evaporation, theme, and unlocks.

### InterventionDefinition

Stable ID, environmental effect, delay, duration, cost, cooldown, restrictions, and feedback.

### ChallengeDefinition

Starting setup, allowed actions, objectives, failure, scoring, hints, rewards, and educational objective.

### DiscoveryDefinition

Trigger, observation, explanation, simplification label, reward, and related entries.

## Runtime state

ExperimentState stores schema/content versions, seed, tick, dish/grid state, populations, environment, events, challenge state, intervention history, random state, and outcome.

PlayerProfile stores settings, unlocks, discoveries, challenge results, achievements, statistics, cosmetics, and tutorial progress.

## Rules

IDs never depend on display names, are never reused, and must validate. Save schema changes require migration fixtures and safe failure behaviour.