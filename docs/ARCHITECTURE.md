# Architecture

## Objective

Allow biological rules to change without coupling them to Unity scenes, artwork, UI, saving, monetisation, or platform SDKs.

## Layers

### Content definitions

Data for organisms, media, nutrients, tolerance curves, visuals, challenges, discoveries, events, and rewards.

### Simulation core

A deterministic engine-independent model responsible for the dish grid, environmental fields, resources, populations, growth, stress, death, dormancy, spread, competition, events, random streams, and outcomes.

The simulation core must not know about GameObjects, prefabs, particles, audio, advertising, or platform APIs.

### Application services

Coordinates experiment lifecycle, interventions, speed, save/load, progression, challenge evaluation, discoveries, analytics, and app lifecycle.

### Presentation

Renders the dish, colonies, overlays, particles, camera, UI, audio, and accessibility. It observes read-only simulation snapshots and never owns authoritative state.

### Platform adapters

Replaceable integrations for mobile lifecycle, notifications, achievements, cloud saves, ads, purchases, crash reporting, and analytics.

## Simulation space

Use a low-resolution two-dimensional grid. Cells can contain medium, moisture, temperature offset, light, oxygen proxy, nutrient pools, waste, population density, and contamination flags.

## Fixed-step pipeline

Apply interventions; update environment; calculate suitability; consume resources; apply growth/stress/death; spread populations; resolve competition; evaluate discoveries and outcomes; publish a snapshot.

## Determinism

Every experiment has a seed. Simulation randomness, world generation, events, and visual randomness use separate streams. Frame rate must not alter outcomes.

## Saving

Save schema version, content version, seed, tick, dish state, player actions, challenge state, discoveries, progression, and random-stream state. Never serialise transient Unity objects.