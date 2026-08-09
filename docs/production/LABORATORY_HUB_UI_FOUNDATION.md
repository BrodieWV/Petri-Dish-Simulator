# Laboratory Hub UI foundation

The serialized Phase 3 presentation foundation lives at
`Assets/PetriDish/Scenes/LaboratoryHub.unity`. The existing
`PetriDishVerticalSlice.unity` remains the first enabled build scene and is not
modified by this work.

## Current visual direction

The hub uses a clean modern laboratory presentation with warm off-white
backgrounds, white cards, pale cool-grey work surfaces, charcoal typography,
thin cool-grey borders, subtle shadows, and restrained teal interaction accents.
Healthy, warning, and severe colours remain semantic. The organism and dish
illustration provide the strongest colour in the workspace.

## Scope

The hub provides editable mock presentation for:

- one selected dish, with `Dish A` shown at `1 / 1`;
- a deliberate illustrated petri-dish placeholder with no camera dependency;
- *Bacillus subtilis* on Nutrient Agar at 18 h and 42% coverage;
- a `Growing well` status and a 26°C, 42% moisture, nutrients-OK summary;
- disabled previous/next controls ready for later multi-dish browsing;
- Lab Notes for observation, discovery, challenge, and recent-update mock entries;
- persistent desktop/tablet navigation without a separate Dishes destination;
- compact phone-landscape navigation and a serialized Lab Notes drawer;
- prominent Start New Experiment, Compare, Open Dish, Journal, and Settings actions.

All values and button results are placeholders. This foundation does not add
multi-dish persistence, swipe gestures, lineage, colony transfer, a comparison
engine, journal storage, unlock logic, challenges, contamination, live dish data
binding, or new simulation rules.

## Reusable assets

The theme is `Assets/PetriDish/UI/Styles/PetriDishUITheme.asset`. Laboratory
surfaces, borders, shadow, semantic signals, typography, spacing, navigation
width, notes width, and compact breakpoints remain Inspector-editable.

Reusable serialized prefabs are grouped below
`Assets/PetriDish/UI/Prefabs` into Common, Navigation, Experiments, and
Laboratory. Screen-specific mock values stay in `LaboratoryHub.unity`.

Run `Petri Dish > Build Laboratory Hub` to create missing reusable assets and
safely rebuild only the hub scene. Existing prefab and theme assets are reused,
so repeated runs do not duplicate or reset them.

## Manual verification

1. Open `Assets/PetriDish/Scenes/LaboratoryHub.unity`.
2. Enter Play Mode at 1920x1080 and confirm the persistent rail, dominant selected
   dish, contained illustration, Lab Notes, primary action, and hover/press states.
3. At 1366x768 confirm dish metadata, environmental summary, Open Dish, and the
   disabled `Dish A  1 / 1` control remain readable without clipping.
4. At 1136x640 or a phone wider than 1.95:1, confirm navigation labels collapse,
   the selected dish stays primary, Start New Experiment remains reachable, and
   the Lab Notes button toggles the drawer.
5. Confirm previous and next dish controls remain disabled while `1 / 1` is shown.
6. Select Start New Experiment, Open Dish, Compare, Journal, Settings, and rail
   actions; confirm mock-data feedback appears and no Console errors occur.
7. Confirm portrait is not presented as a supported layout.

Product-owner visual review on representative landscape devices remains required
before this presentation foundation is treated as shipping M7 UI.
