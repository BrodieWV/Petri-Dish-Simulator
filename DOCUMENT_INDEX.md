# Document Index

This index identifies the authoritative document for common project questions.

## Start here

| Question | Authoritative file |
|---|---|
| How should agents work? | `AGENTS.md` |
| What is the game and who is it for? | `docs/VISION.md` |
| What are the non-negotiable project decisions? | `docs/DECISIONS.md` |
| What phase is active and what comes later? | `docs/ROADMAP.md` |
| What milestone is active? | `docs/MILESTONES.md` |
| What should be built next? | `docs/production/PHASE_2_BACKLOG.md` |
| What is already implemented? | `docs/production/IMPLEMENTATION_STATUS_M1_M5.md` |
| How does the simulation work? | `docs/SIMULATION_MODEL.md` |
| How is the project structured technically? | `docs/ARCHITECTURE.md` |
| What data is stored? | `docs/DATA_MODEL.md` |
| What are the current balance assumptions? | `docs/design/STARTING_BALANCE_VALUES.md` |
| What scientific and safety rules apply? | `docs/SCIENTIFIC_ACCURACY_AND_SAFETY_GUIDE.md` |
| How is the project tested? | `docs/TEST_STRATEGY.md` |

## Core product documents

- `README.md`
- `docs/VISION.md`
- `docs/PRINCIPLES.md`
- `docs/GAME_DESIGN_DOCUMENT.md`
- `docs/ROADMAP.md`
- `docs/MILESTONES.md`
- `docs/DECISIONS.md`
- `docs/RISKS.md`

## Technical documents

- `docs/ARCHITECTURE.md`
- `docs/DATA_MODEL.md`
- `docs/SIMULATION_MODEL.md`
- `docs/TEST_STRATEGY.md`
- `docs/ANALYTICS.md`

## Phase 2 definition framework

- `Assets/PetriDish/Content/OrganismDefinition.cs` — organism identity, scientific metadata, and simulation values
- `Assets/PetriDish/Content/MediumDefinition.cs` — medium identity, scientific metadata, and environmental values
- `Assets/PetriDish/Content/SimulationDefinitionCatalog.cs` — validated definition lookup and defaults
- `Assets/PetriDish/Content/Resources/PetriDish/` — current Rapid Bacterium and Nutrient Agar defaults
- `Assets/Tests/Editor/SimulationDefinitionTests.cs` — definition, determinism, and save migration coverage

## Content and design

- `data/CONTENT_CATALOG.md`
- `docs/CONTENT_BIBLE.md`
- `docs/PROGRESSION_AND_ECONOMY.md`
- `docs/ART_AND_AUDIO_DIRECTION.md`
- `docs/UX_AND_SCREEN_FLOW.md`
- `docs/content/CHALLENGE_CATALOGUE.md`
- `docs/content/VERTICAL_SLICE_COPY_DECK.md`
- `docs/design/SCREEN_WIREFRAMES_AND_BEHAVIOUR.md`
- `docs/design/STARTING_BALANCE_VALUES.md`
- `docs/design/VERTICAL_SLICE_SPECIFICATION.md`

## Production and handoff

- `docs/production/PHASE_2_BACKLOG.md` — active backlog
- `docs/production/IMPLEMENTATION_STATUS_M1_M5.md` — current implementation and handoff
- `docs/production/ASSET_REGISTER.md`
- `docs/production/DETERMINISM_AND_SAVE_REGRESSION.md`
- `docs/production/PROJECT_STRUCTURE.md`
- `docs/production/REPOSITORY_READINESS_AUDIT.md`
- `docs/production/UNITY_BUILD_BRIEF.md`
- `docs/production/UNITY_SCENE_AND_PREFAB_SPEC.md`
- `docs/production/CODEX_REVIEW_BRIEF.md`
- `docs/production/CODEX_M1_M2_IMPLEMENTATION_PROMPT.md` — historical implementation prompt

## Archive and research

- `docs/archive/PHASE_1_BACKLOG.md` — completed Phase 1 backlog
- `docs/research/GITHUB_REFERENCE_NOTES.md`

## Current status

Phase 1 is complete. Phase 2 is active. The immediate code task is the data-driven organism and medium framework. The product owner is concurrently working locally on the reusable 3D petri-dish presentation.
