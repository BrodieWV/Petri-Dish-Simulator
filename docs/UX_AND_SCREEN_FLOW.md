# UX and Screen Flow

## Primary flow

Launch → Laboratory Hub → Experiment Setup or Existing Dish → Dish → Outcome/Observations → Laboratory Hub

The Laboratory Hub is the main workspace between experiments. It must be functional before broader Phase 3 expansion.

## Laboratory Hub

The Hub shows the currently selected dish, culture summary, recent notes, navigation, and primary actions.

### Navigation

- **Lab** — return to the Laboratory Hub.
- **New Experiment** — open Experiment Setup.
- **Compare** — open comparison when supported; otherwise show a clear requirement such as needing at least two dishes.
- **Journal** — open the journal when implemented; until then use an explicit temporary/unavailable state rather than a dead button.
- **Collection** — open discovered/unlocked content when implemented; until then use an explicit temporary state.
- **Challenges** — open challenge content when implemented; until then use an explicit temporary state.
- **Settings** — open settings.

### Primary Hub actions

- **Open Dish** — enter the full experiment view for the selected dish.
- **+ Start New Experiment** — same destination as New Experiment.
- **Compare** — same comparison flow, using the selected dish as context where possible.
- **Left/right dish selectors** — move between existing dishes. Before multiple-dish persistence exists, these controls must have an intentional disabled/no-op state and remain structurally ready for later dish cycling.

## Hub 3D dish inspection

The Hub uses the real 3D petri-dish presentation and allows lightweight visual inspection without modifying simulation state.

### Desktop and Unity Editor

- Mouse drag over the dish or dish viewport orbits the view.
- Mouse wheel zooms in and out.
- Reset View restores the approved default angle and framing.

### Touch/tablet

- One-finger drag over the dish orbits the view.
- Pinch zooms in and out where practical.

### Camera behaviour

- Orbit is centred on the dish presentation pivot.
- Horizontal orbit may rotate freely where presentation allows.
- Vertical pitch is constrained to useful inspection angles and must not invert or lose the dish.
- Zoom has safe minimum and maximum limits.
- Camera/view motion must never modify deterministic simulation state.
- Input beginning over UI controls must not orbit or zoom the dish.
- The dish must remain framed and readable at supported landscape resolutions.
- Motion should be smooth and responsive, with reset available when needed.

The Hub provides quick visual inspection. **Open Dish** remains the route to the full experiment interface and simulation controls.

## Setup

Select organism, medium, dish, nutrients, initial environment, review difficulty, name culture, and start. Advanced settings remain collapsed initially.

## Dish screen

The dish occupies most of the display. The status area shows culture condition, time, speed, limiting factor, objective, and alerts. The intervention tray provides temperature, moisture, light, airflow, nutrients, and inspection.

Use temporary development sliders for live tuning before final controls are chosen.

## Inspection

Show local density, growth trend, moisture, nutrients, stress, recent events, and a plain-language observation.

## Outcome

Show outcome, timeline, causes, interventions, discoveries, score, restart same seed, modify setup, and return to the Laboratory Hub.

## Navigation and lifecycle acceptance

Before Phase 3 expansion:

- Hub → Open Dish → Lab/Hub works without creating duplicate presentation objects.
- New Experiment entry points resolve to the same setup flow.
- No visible Hub control is silently dead.
- Scene changes preserve the intended authoritative experiment state.
- Play/Stop and scene transitions do not leave duplicate cameras, EventSystems, RenderTextures, or presentation objects.

## Accessibility

Do not rely on colour alone. Support larger text, reduced motion, haptic toggle, safe areas, muted play, and later plain/scientific language modes.
