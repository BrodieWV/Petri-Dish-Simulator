# Task Feature List

This file records features the user wants implemented in Petri Dish Simulator. It is a product backlog, not a commitment that every item belongs in the current milestone.

## Status labels

- **Requested** — captured but not yet designed or scheduled
- **Designed** — behaviour and acceptance criteria are defined
- **In progress** — implementation has started
- **Implemented** — code exists but may still require review and testing
- **Verified** — reviewed and tested in Unity
- **Deferred** — intentionally postponed

## Active features

### F-001 — Rotate and zoom around the petri dish

**Status:** Designed  
**Priority:** High  
**Area:** Camera and dish interaction  
**Milestone:** M6.5 / Phase 3.0

The player should be able to rotate the viewing angle around the real 3D petri dish and zoom closer or farther away from the Laboratory Hub.

#### Intended controls

Mobile/tablet:

- One-finger drag over the dish rotates/orbits the view.
- Pinch gesture zooms in and out.
- Touches beginning over UI controls must not move the dish view.

Desktop and Unity Editor:

- Mouse drag over the dish or dish viewport rotates/orbits the view.
- Mouse wheel zooms.
- A reset-view control or equivalent action returns to the default camera position.

#### Behaviour requirements

- Rotation/orbit must use the presentation pivot and must not rotate or alter authoritative simulation state.
- Zoom must have minimum and maximum limits.
- Pitch must remain within useful inspection angles.
- The camera/view must not pass through or lose the dish.
- The dish should remain framed and readable at all supported landscape aspect ratios.
- Camera movement must not change simulation state.
- Touches/clicks beginning over UI controls must not move the camera.
- Camera controls must coexist with current and future dish inspection behavior.
- Motion should be smooth but responsive.
- Reset View must restore a consistent approved framing.

#### Current implementation context

The Laboratory Hub now uses a reusable real 3D petri-dish display with a dedicated presentation camera and rotation pivot. This feature should build on that existing structure rather than introducing a fake 2D rotation effect or a second dish implementation.

#### Acceptance criteria

- Player can orbit the Hub dish using mouse input.
- Player can zoom between the default whole-dish view and a useful closer inspection view.
- Touch drag/pinch support works where practical on the supported input path.
- Camera/view remains within configured pitch/yaw/distance limits.
- Reset View restores the approved default framing.
- UI interaction does not trigger camera movement.
- Camera movement does not affect deterministic simulation results.
- Dish remains visible and correctly framed at supported landscape resolutions.

### F-002 — Laboratory Hub controls and navigation

**Status:** Designed  
**Priority:** High  
**Area:** UI navigation and screen flow  
**Milestone:** M6.5 / Phase 3.0

Every visible Laboratory Hub control must behave intentionally before broader Phase 3 feature expansion begins.

#### Required behaviour

- `Lab` returns to the Laboratory Hub.
- Sidebar `New Experiment` opens Experiment Setup.
- `+ Start New Experiment` opens the same Experiment Setup flow.
- `Open Dish` opens the selected dish's full experiment view.
- Sidebar and bottom `Compare` controls enter the same comparison flow or a clear requirement/unavailable state until comparison is implemented.
- `Journal`, `Collection`, and `Challenges` open implemented screens or explicit temporary states rather than silently doing nothing.
- `Settings` opens Settings.
- Navigation must not recreate the old Phase 2 experiment UI over the Laboratory Hub.
- Hub → Open Dish → return to Lab must preserve the intended experiment state.
- Scene transitions must not create duplicate EventSystems, cameras, RenderTextures, or presentation objects.

#### Acceptance criteria

- Every visible control produces its documented result.
- Duplicate controls for the same action resolve to the same destination/behavior.
- Unfinished destinations have clear, honest temporary states instead of fake feature implementations.
- Returning to the Hub restores a valid Hub presentation without duplicate runtime UI.
- Automated tests cover navigation logic where practical, with manual approved-scene verification for Unity presentation behavior.

### F-003 — Laboratory Hub dish selection controls

**Status:** Designed  
**Priority:** High  
**Area:** Hub dish selection / future multi-dish support  
**Milestone:** M6.5 / Phase 3.0 foundation

The left/right controls below the selected dish should be structurally functional now and become real dish cycling when multiple-dish persistence is implemented.

#### Behaviour requirements

- With one dish, controls use a clear disabled/no-op state and the dish counter remains truthful.
- With multiple dishes in the future, left/right changes the selected dish without changing another dish's authoritative state.
- Selected-dish title, organism, medium, culture summary, notes context, 3D presentation, and counter update together.
- Selection logic must be separated from future multi-dish persistence so the Hub can adopt the real data source without UI reconstruction.

#### Acceptance criteria for M6.5

- Current single-dish state is intentional and not a dead/broken interaction.
- Selector controls and presentation binding have a clear extension point for the next multiple-dish milestone.
- No fake additional dishes are created simply to demonstrate the arrows.

## Adding future features

Add each requested feature with:

- Stable feature ID
- Name
- Status
- Priority
- Area
- User intent
- Behaviour requirements
- Dependencies
- Acceptance criteria
