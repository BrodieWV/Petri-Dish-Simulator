# Laboratory Hub UI foundation

The serialized Phase 3 presentation foundation lives at
`Assets/PetriDish/Scenes/LaboratoryHub.unity`. The existing
`PetriDishVerticalSlice.unity` remains the first enabled build scene and is not
modified by this work.

## Scope

The hub provides editable mock presentation for:

- one dominant featured/live dish;
- three active dish summaries;
- laboratory activity, discoveries, challenge progress, and recent unlocks;
- persistent desktop/tablet navigation;
- compact phone-landscape navigation, horizontal active-dish cards, and an
  activity drawer;
- primary New Experiment, Active Dishes, Compare, Journal, and Open Dish actions.

All values and button results are placeholders. This foundation does not add
multi-dish persistence, lineage, colony transfer, a comparison engine, journal
storage, unlock logic, challenges, contamination, or new simulation rules.

## Reusable assets

The theme is `Assets/PetriDish/UI/Styles/PetriDishUITheme.asset`. Surface,
signal, typography, spacing, navigation-width, column-width, and compact
breakpoint values remain Inspector-editable.

Reusable serialized prefabs are grouped below
`Assets/PetriDish/UI/Prefabs` into Common, Navigation, Experiments, and
Laboratory. Screen-specific mock values stay in `LaboratoryHub.unity`.

Run `Petri Dish > Build Laboratory Hub` to create missing reusable assets and
safely rebuild only the hub scene. Existing prefab and theme assets are reused,
so repeated runs do not duplicate or reset them.

## Manual verification

1. Open `Assets/PetriDish/Scenes/LaboratoryHub.unity`.
2. Enter Play Mode at 1920x1080 and confirm the three-column layout, persistent
   labelled rail, dominant featured dish, vertical active cards, activity cards,
   and button hover/press feedback.
3. Use a landscape tablet resolution such as 1366x768 and confirm all primary
   actions remain readable and reachable.
4. Use a compact landscape resolution such as 1136x640 or a phone wider than
   1.95:1. Confirm navigation labels collapse, active cards become horizontal,
   the featured dish remains primary, and the Activity button toggles the drawer.
5. Select New Experiment, Open Dish, Compare, Journal, and navigation actions;
   confirm the temporary mock-data feedback appears and no Console errors occur.
6. Confirm portrait is not presented as a supported layout for this foundation.

This presentation-only asset requires product-owner visual review on target
landscape devices before M7 implementation can use it as a shipping hub.
