# Unity Build Brief

## Objective

Build Milestones M1–M5 without changing approved scope.

## Project shape

Bootstrap, Main Menu, Experiment, and optional Simulation Laboratory scenes; separate Content, Simulation, Application, Presentation, Platform, Development, and Tests areas.

## Implementation order

Project settings; responsive dish; simulation clock and seed; environment/resources/population; one organism; lifecycle and spread; renderer; feedback; interventions; setup/outcome; saving; guided experiment; polish and profiling.

## Constraints

No authoritative state in prefabs, no frame-dependent simulation, no direct platform calls from game systems, no hard-coded generic organism IDs, no scene-object save references, no unseeded randomness, and validated tunable values.

## Development tools

Seed entry, speed, grid inspection, overlays, presets, snapshot save/load, determinism comparison, and live tuning.

## Vertical-slice done

A clean install launches one guided experiment, shows a living colony, accepts interventions, reaches and explains an outcome, awards a discovery, saves progress, and returns to menu.