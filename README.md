# Petri Dish Simulator

**Working title:** Petri Dish Simulator  
**Project type:** Casual educational ecosystem simulation game  
**Initial platform:** Mobile, with a PC development build  
**Engine target:** Unity  
**Project phase:** Phase 0 — definition and pre-production

## Concept

Petri Dish Simulator lets players create a small living ecosystem by selecting an organism, growth medium, food source, and environmental conditions. The player then observes growth, competition, stress, adaptation, reproduction, dormancy, and collapse.

The first release is a game, not a laboratory simulator. It should teach real biological relationships through understandable cause and effect while remaining visually active, forgiving, and enjoyable for younger or casual players.

## Player fantasy

> “I created this tiny world, changed its environment, and discovered why it survived, changed, or failed.”

## Core loop

1. Choose a dish, medium, organism, and optional food source.
2. Set temperature, moisture, light, airflow, and nutrient levels.
3. Start the culture.
4. Observe visible changes and organism feedback.
5. Intervene by adjusting the environment or adding resources.
6. Complete discoveries, challenges, and collection goals.
7. Save successful cultures, unlock new organisms, and try more complex ecosystems.

## Initial scope

The first playable version focuses on a single dish containing one primary organism type. The simulation uses simplified traits and environmental tolerances rather than species-level laboratory accuracy.

Initial organisms:

- Fast-growing bacterium
- Filamentous fungus
- Slime mould
- Yeast-like colony

Initial media:

- Nutrient agar
- Wood-chip substrate
- Moist soil gel
- Low-nutrient agar

Initial environmental controls:

- Temperature
- Moisture
- Light cycle and intensity
- Airflow
- Food or nutrient input

## Design pillars

- Visible cause and effect
- A living dish at all times
- Science-inspired, not science-faking
- Easy to start, difficult to master
- Failure produces information
- Expandable from children’s game to advanced simulation

## Phase 0 documents

- `docs/VISION.md`
- `docs/GAME_DESIGN_DOCUMENT.md`
- `docs/ARCHITECTURE.md`
- `docs/ROADMAP.md`
- `docs/MILESTONES.md`
- `AGENTS.md`
- `docs/PRINCIPLES.md`
- `docs/SIMULATION_MODEL.md`
- `docs/DATA_MODEL.md`
- `docs/UX_AND_SCREEN_FLOW.md`
- `docs/CONTENT_BIBLE.md`
- `docs/ART_AND_AUDIO_DIRECTION.md`
- `docs/PROGRESSION_AND_ECONOMY.md`
- `docs/ANALYTICS.md`
- `docs/TEST_STRATEGY.md`
- `docs/RISKS.md`
- `docs/DECISIONS.md`
- `docs/production/UNITY_BUILD_BRIEF.md`
- `docs/production/CODEX_REVIEW_BRIEF.md`
- `docs/production/PHASE_1_BACKLOG.md`
- `docs/research/GITHUB_REFERENCE_NOTES.md`

## Current decision

Do not begin with a chemically exact simulation. Build a deterministic, data-driven ecological model that produces understandable outcomes and can later accept more detailed biology modules.
