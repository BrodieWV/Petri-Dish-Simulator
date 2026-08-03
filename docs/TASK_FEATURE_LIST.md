# Task Feature List

This file records features the user wants implemented in Petri Dish Simulator. It is a product backlog, not a commitment that every item belongs in the current milestone.

## Status labels

- **Requested** — captured but not yet designed or scheduled
- **Designed** — behaviour and acceptance criteria are defined
- **In progress** — implementation has started
- **Implemented** — code exists but may still require review and testing
- **Verified** — reviewed and tested in Unity
- **Deferred** — intentionally postponed

## Requested features

### F-001 — Rotate and zoom around the petri dish

**Status:** Requested  
**Priority:** High  
**Area:** Camera and dish interaction

The player should be able to rotate the viewing angle around the petri dish and zoom closer or farther away.

#### Intended controls

Mobile:

- One-finger drag over the dish rotates the view.
- Pinch gesture zooms in and out.
- Optional two-finger drag adjusts the viewing angle if one-finger drag conflicts with dish inspection.
- Double-tap resets or focuses the camera.

Desktop and Unity Editor:

- Left or middle mouse drag rotates the view.
- Mouse wheel zooms.
- A reset-view button returns to the default camera position.

#### Behaviour requirements

- Rotation must orbit around the centre of the dish rather than rotating the dish simulation itself.
- Zoom must have minimum and maximum limits.
- The camera must not pass through the dish or clip through the agar.
- The dish should remain framed and readable at all supported aspect ratios.
- Camera movement must not change simulation state.
- Touches beginning over UI controls must not move the camera.
- Camera controls must coexist with tap-to-inspect behaviour.
- Motion should be smoothed but remain responsive.
- A reduced-motion option should disable or reduce camera inertia.

#### Design considerations

The current dish renderer is a flat UI texture. Full orbit rotation will require the dish presentation to become a world-space 3D or 2.5D object, or use a controlled perspective effect. This feature should therefore be designed alongside the production dish renderer rather than added as a superficial rotation of the existing UI image.

#### Preliminary acceptance criteria

- Player can orbit at least partially around the dish using touch and mouse input.
- Player can zoom between a whole-dish view and a close inspection view.
- Camera remains within configured pitch, yaw, and distance limits.
- Reset view restores a consistent default framing.
- UI interaction does not trigger camera movement.
- Camera movement does not affect deterministic simulation results.

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
