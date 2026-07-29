# Document Index

This index routes contributors to the current source documents. When documents disagree,
follow the source-of-truth order in `AGENTS.md`.

## Product and scope

- `VISION.md` — product promise and audience
- `GAME_DESIGN_DOCUMENT.md` — approved game structure and player loop
- `MILESTONES.md` — milestone acceptance boundaries
- `ROADMAP.md` — phased delivery sequence
- `PRINCIPLES.md` — product and design principles
- `DECISIONS.md` — accepted architectural and product decisions

## Architecture and simulation

- `ARCHITECTURE.md` — layers, dependencies, determinism, and saving
- `DATA_MODEL.md` — static definitions, runtime state, IDs, and versions
- `SIMULATION_MODEL.md` — organism, medium, environment, and resource rules
- `TEST_STRATEGY.md` — automated and manual verification approach
- `RISKS.md` — scientific, technical, product, and operational risks

## Design and content

- `design/VERTICAL_SLICE_SPECIFICATION.md` — The Comfortable Range experiment
- `design/STARTING_BALANCE_VALUES.md` — provisional Phase 1 balance source
- `design/SCREEN_WIREFRAMES_AND_BEHAVIOUR.md` — responsive UI behaviour
- `content/VERTICAL_SLICE_COPY_DECK.md` — approved player-facing experiment copy
- `CONTENT_BIBLE.md` — content structure and scientific labelling
- `SCIENTIFIC_ACCURACY_AND_SAFETY_GUIDE.md` — educational and safety boundaries

## Unity production

- `production/IMPLEMENTATION_STATUS_M1_M5.md` — current implemented state and limitations
- `production/DETERMINISM_AND_SAVE_REGRESSION.md` — save and replay guarantees
- `production/PROJECT_STRUCTURE.md` — intended Unity folder responsibilities
- `production/UNITY_SCENE_AND_PREFAB_SPEC.md` — scene and prefab contracts
- `production/ASSET_REGISTER.md` — required visual and audio assets
- `production/CODEX_REVIEW_BRIEF.md` — repository review expectations

## Phase 2 definition framework

- Organism definitions: `Assets/PetriDish/Content/OrganismDefinition.cs`
- Medium definitions: `Assets/PetriDish/Content/MediumDefinition.cs`
- Definition catalog: `Assets/PetriDish/Content/SimulationDefinitionCatalog.cs`
- Default assets: `Assets/PetriDish/Content/Resources/PetriDish/`
- Regression tests: `Assets/Tests/Editor/SimulationDefinitionTests.cs`

The only production definitions currently approved are Rapid Bacterium and Nutrient
Agar. Additional organisms and media require separate reviewed content work.
