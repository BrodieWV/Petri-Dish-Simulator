# Milestones

## M0 — Phase 0 documentation — Complete

Core loop, MVP scope, simulation boundaries, acceptance checks, scientific simplifications, determinism, saving, and agent rules are explicit.

## M1 — Static dish scene — Complete

Responsive dish view, safe mobile layout, readable UI, temporary live-tuning controls, and verified phone layout.

## M2 — Deterministic simulation core — Complete

Fixed-step clock, seeded setup, temperature, moisture, nutrients, population state, inspection, saves, and simulation regression tests.

## M3 — First living colony — Complete

One organism grows in favourable conditions, slows near tolerance limits, declines outside survival ranges, consumes nutrients, and produces visibly different outcomes.

## M4 — Player intervention loop — Complete

Temperature and moisture controls, delayed effects, limiting-factor feedback, pause/speed controls, inspection, accessibility feedback, and same-seed/new-seed reset.

## M5 — Vertical slice — Functionally complete; presentation polish pending

Complete setup-to-outcome flow, one guided experiment, saving, onboarding, informative failure, phone-safe layout, and automated verification.

Remaining presentation work includes final materials, audio, haptics, and final art.

## M6 — Data-driven experimental framework — Automated implementation complete; review active

- Validated organism and medium definitions
- Named real organisms with simplified educational behaviour
- Save-compatible organism and medium selection
- Two organisms and two media proving distinct behaviour
- Live colony texture connected to the approved 3D dish
- Nutrient intervention
- Content validation and regression tests
- Laboratory Hub presentation foundation using the reusable 3D dish

Exit: the current vertical slice runs through external definitions, two organisms and two media produce meaningfully different outcomes, and all tests pass.

Qualified microbiology review and representative manual/device verification remain useful before final release sign-off.

## M6.5 — Laboratory Hub interaction and navigation — Next

Make the current Laboratory Hub functional before beginning M7 content/system expansion.

Required outcomes:

- Every visible Hub button has intentional behaviour.
- Lab navigation returns to the Hub.
- New Experiment entry points open experiment setup.
- Open Dish enters the selected experiment view.
- Compare entry points open comparison or a clear requirements/unavailable state.
- Journal, Collection, Challenges, and Settings are wired to implemented screens or explicit temporary states rather than dead controls.
- Left/right dish selectors are functional at the UI layer and ready for multiple dishes.
- The real 3D dish supports constrained orbit/rotation and zoom.
- Desktop/Editor input supports mouse drag, wheel zoom, and reset view.
- Touch input supports drag orbit and pinch zoom where practical.
- UI input does not trigger dish-camera movement.
- Camera controls do not modify simulation state.
- Dish framing is stable across supported landscape resolutions.
- Hub → Open Dish → Lab navigation is verified.
- Play/Stop and scene transitions do not leak presentation resources or duplicate EventSystems/cameras.

Exit: the current UI behaves like a functioning game interface and the dish can be inspected naturally before multiple-dish, comparison, progression, and content expansion are added.

## M7 — Content-complete MVP — Not started

Four organisms, four media, twelve guided experiments, ten challenges, journal, progression, discovery flow, cloning into multiple dishes, content validation, comparison, transfer/lineage, contamination, and responsive screens.

Implementation note: the serialized Laboratory Hub presentation foundation is merged. M6.5 now converts its mock/placeholder interactions into functional navigation and 3D dish inspection. M7 simulation, persistence, progression, journal, challenge, unlock, comparison, transfer, and contamination systems remain not started until M6.5 is verified.

## M8 — Mobile release candidate — Not started

Performance, save integrity, accessibility, tutorial, analytics, monetisation boundaries, privacy, and store requirements pass.
