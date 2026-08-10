# Laboratory Hub UI foundation

The serialized Phase 3 presentation foundation lives at
`Assets/PetriDish/Scenes/LaboratoryHub.unity`. The existing
`PetriDishVerticalSlice.unity` remains the first enabled build scene and is not
modified by this work.

## Current visual direction

The hub uses a clean modern laboratory presentation with warm off-white
backgrounds, white cards, pale cool-grey work surfaces, charcoal typography,
lightweight dividers, subtle shadows, and restrained teal interaction accents.
Healthy, warning, and severe colours remain semantic. The organism and dish
illustration provide the strongest colour in the workspace.

The selected dish is the visual focal point. Its camera-free illustration uses a
translucent glass rim, soft agar, restrained colony clusters, and layered depth
directly on the white presentation surface rather than inside a grey preview box.
The scene includes a background-only camera with no culling or RenderTexture
target so Unity never exposes its `Display 1 / No cameras rendering` overlay.

## Scope

The hub provides editable mock presentation for:

- one selected dish, with `Dish A` shown at `1 / 1`;
- a large deliberate illustrated petri-dish placeholder with no preview-camera dependency;
- *Bacillus subtilis* on Nutrient Agar at 18 h and 42% coverage;
- a `Growing well` status and a 26°C, 42% moisture, nutrients-OK summary;
- disabled previous/next controls ready for later multi-dish browsing;
- Lab Notes for observation, discovery, challenge, and recent-update mock entries;
- persistent desktop/tablet navigation without a separate Dishes destination;
- compact phone-landscape navigation and a serialized Lab Notes drawer;
- a cohesive culture/environment summary with fewer framed metric boxes;
- observation-style Lab Notes with whitespace, accent rules, and lightweight dividers;
- an aligned action dock for Start New Experiment and Compare, plus Open Dish in the culture card;
- quiet header access to Journal and Settings.

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
2. Enter Play Mode at 1920x1080 and confirm the persistent wider rail, dominant selected
   dish, translucent rim, unboxed white presentation, Lab Notes, integrated action dock,
   and hover/press states. Confirm no `Display 1 / No cameras rendering` text appears.
3. At 1366x768 confirm dish metadata, environmental summary, Open Dish, and the
   disabled `Dish A  1 / 1` control remain readable without clipping.
4. At 1136x640 or a phone wider than 1.95:1, confirm navigation labels collapse,
   the selected dish stays primary, Start New Experiment remains reachable, and
   the Lab Notes button toggles the drawer and the aligned action dock remains reachable.
5. Confirm previous and next dish controls remain disabled while `1 / 1` is shown.
6. Select Start New Experiment, Open Dish, Compare, Journal, Settings, and rail
   actions; confirm mock-data feedback appears and no Console errors occur.
7. Confirm portrait is not presented as a supported layout.

Product-owner visual review on representative landscape devices remains required
before this presentation foundation is treated as shipping M7 UI.

## Shared 3D dish presentation

The selected-culture preview instantiates `Assets/PetriDish/Presentation/Prefabs/PetriDishDisplay.prefab`. The prefab nests the existing `Assets/PetriDish/Art/models/PetriDish.fbx` below a `RotationPivot`, retaining its embedded glass, agar, and colony-surface materials. Its camera and neutral presentation lights stay outside the pivot so future orbit, zoom, and reset controls can be added without restructuring the model.

The Hub uses the existing `DishRenderer` once in the editor builder to bake `LaboratoryHubMockColony.asset`, then binds that saved texture through the existing `ColonySurfacePresenter` and a `MaterialPropertyBlock`. Runtime presentation owns one 768 x 768 RenderTexture, detaches and releases it on disable/destruction, and participates in the existing Play Mode teardown guard. The displayed culture remains mock presentation data and is not connected to Phase 2 simulation or saves.

The navigation rail is a masked vertical `ScrollRect`; Settings remains inside its content and reachable on compact landscape sizes.

## Runtime scene ownership

`LaboratoryHub.unity` carries a `PetriDishRuntimeScene` marker with the `NonExperiment` role. `PetriDishRuntime` and its simulation controller may remain persistent across scene transitions, but `RuntimeBootstrap` attaches experiment presentation only when the active scene owns it. Entering the Hub detaches and removes any generated legacy experiment Canvas, renderer bindings, and generated EventSystem; entering an existing Phase 2 scene initializes its responsive binder or backward-compatible legacy presentation normally.

Future non-experiment scenes should declare the same `NonExperiment` role instead of relying on scene-name checks. Existing responsive Phase 2 experiment scenes remain compatible because their scene-owned binder is treated as an experiment signal. Any future scene that needs generated legacy presentation must explicitly declare the `Experiment` role; unmarked scenes do not receive legacy UI.

## Final visual polish

The Hub applies a consistent 1.25 typography scale to builder-owned text. Its selected-dish camera uses Hub-specific 1.35 framing with a small upward composition offset while retaining the shared `PetriDishDisplay` pivot, model, materials, and RenderTexture lifecycle. Growing state is shown as a compact healthy badge beside the dish identity, the redundant header Settings and footer prompt are removed, disabled dish-navigation arrows remain legible, and Current Observation receives restrained emphasis. The navigation rail remains masked and scrollable.
