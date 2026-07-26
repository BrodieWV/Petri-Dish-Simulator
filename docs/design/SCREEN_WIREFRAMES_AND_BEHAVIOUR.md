# Screen Wireframes and Behaviour

These are functional wireframes. Final art may change appearance, but hierarchy and behaviour should remain stable unless documented.

## Global mobile rules

- Design for 320–600 CSS-pixel-equivalent widths and common phone safe areas.
- Minimum touch target: 44 × 44 points.
- Do not rely on hover.
- Do not place critical information under the dish or behind transient effects.
- Colour is never the only state indicator.
- During development, expose temporary live-tuning sliders for panel width, dish scale, margins, text size, icon scale, and control spacing.

## Main Menu

```text
┌──────────────────────────────┐
│ PETRI DISH SIMULATOR         │
│                              │
│      [Live dish preview]     │
│                              │
│ [Continue Experiment]        │
│ [New Experiment]             │
│ [Guided Experiments]         │
│ [Challenges]                 │
│ [Journal]   [Collection]     │
│                    [Settings]│
└──────────────────────────────┘
```

Behaviour:

- `Continue Experiment` appears only when a valid active save exists.
- Live preview uses a static snapshot, not a running background simulation in Phase 1.
- First launch highlights Guided Experiments.

## Guided Experiment Selection

```text
┌──────────────────────────────┐
│ ‹ Guided Experiments         │
│ Progress: 1 / 12             │
│                              │
│ [01 The Comfortable Range]   │
│  Temperature • Beginner ★    │
│                              │
│ [02 A Drying Edge] 🔒        │
│ [03 Food Runs Out] 🔒        │
│                              │
│ [Experiment details panel]   │
│                  [Start]     │
└──────────────────────────────┘
```

Cards show title, lesson, organism, estimated duration, completion rating, and lock reason.

## Experiment Setup

For the first guided experiment, setup is mostly preselected.

```text
┌──────────────────────────────┐
│ ‹ Experiment Setup           │
│                              │
│ Organism  [Rapid Bacterium]  │
│ Medium    [Nutrient Agar]    │
│ Dish      [Standard Round]   │
│                              │
│ Starting conditions          │
│ Temperature 18 °C            │
│ Moisture    72%              │
│                              │
│ Lesson: temperature range    │
│ Difficulty: Beginner         │
│                  [Start Dish]│
└──────────────────────────────┘
```

Locked guided values remain visible and explain why they are fixed. Sandbox later allows free selection.

## Dish Screen

```text
┌──────────────────────────────┐
│ Objective: Reach 55%    II 1×│
│ Condition: Growing slowly    │
│ Limiting: Too cold           │
│                              │
│       ┌──────────────┐       │
│       │              │       │
│       │  PETRI DISH  │       │
│       │              │       │
│       └──────────────┘       │
│ Coverage 12%   Health 91%    │
│                              │
│ [Temp] [Moisture] [Inspect]  │
│ ┌ Temperature ─────────────┐ │
│ │ 18 °C  [−]──slider──[+]  │ │
│ │ Preferred: undiscovered   │ │
│ └──────────────────────────┘ │
└──────────────────────────────┘
```

### Top bar

- Objective summary
- Pause/resume
- Speed control
- Pause menu access

### Status strip

- Condition label
- Limiting factor
- Coverage
- Health or growth trend

### Dish interaction

- Tap colony or medium to inspect.
- Pinch zoom only if the dish cannot remain legible at fixed scale.
- Inspection marker must not obscure colony state.

### Intervention tray

Only unlocked interventions appear. Selecting one opens its control panel. Closing the panel does not pause the experiment unless required by accessibility settings.

### Speed control

Cycles 1×, 2×, 4×. Pause remains separate. Tutorial prompts may temporarily return to 1×.

## Tutorial Prompt

Prompts should not permanently cover the dish.

```text
┌──────────────────────────────┐
│ The culture is growing, but  │
│ temperature is limiting it.  │
│ Try increasing it gradually. │
│ [Show me]       [Continue]   │
└──────────────────────────────┘
```

- `Show me` highlights the relevant control without performing the action.
- Prompt can collapse into a small objective chip.
- Repeated prompts become shorter.

## Inspection Panel

```text
┌──────────────────────────────┐
│ Inspecting: Colony centre  × │
│ Density        High          │
│ Growth         Increasing ↑  │
│ Moisture       69%           │
│ Nutrients      58%           │
│ Stress         Low           │
│                              │
│ Observation                 │
│ Cells are growing rapidly,   │
│ but local nutrients are      │
│ beginning to fall.           │
└──────────────────────────────┘
```

Use qualitative labels alongside numbers. Advanced values remain hidden until later progression.

## Pause Menu

- Resume
- Restart from checkpoint
- Restart same seed
- Settings
- Leave experiment

Leaving warns only when unsaved progress would be lost.

## Discovery Overlay

```text
┌──────────────────────────────┐
│ DISCOVERY                    │
│ A Comfortable Range          │
│                              │
│ Growth accelerated when the  │
│ culture stayed near 28 °C.   │
│                              │
│ Educational simplification   │
│ [Open Journal]   [Continue]  │
└──────────────────────────────┘
```

Discovery overlays pause the tutorial by default and may be changed in settings later.

## Outcome Screen

```text
┌──────────────────────────────┐
│ CULTURE SUCCESSFUL ★         │
│ Coverage 57%   Health 78%    │
│                              │
│ What happened                │
│ • Warm conditions sped growth│
│ • Heat increased drying      │
│ • Moisture restored recovery │
│                              │
│ Discoveries: 2               │
│ Rewards: 2 Knowledge, 1 Star │
│                              │
│ [View Timeline]              │
│ [Retry Seed] [Continue]      │
└──────────────────────────────┘
```

The causal summary is more important than the score.

## Experiment Timeline

Timeline entries include:

- Temperature changes
- Moisture additions
- Condition changes
- Discovery moments
- Coverage milestones
- Stress and recovery phases

The timeline should use simulation time, not wall-clock time.

## Journal Entry

Each entry contains:

- Observation
- Likely cause
- Why it matters
- Try next
- Accuracy label
- Related organism and medium

## Responsive priorities

When vertical space is limited:

1. Preserve dish size.
2. Collapse secondary text.
3. Make intervention panel a bottom sheet.
4. Keep objective, condition, pause, and active control visible.
5. Never reduce touch targets below minimum.

## Accessibility behaviours

- State icons accompany labels.
- Reduced motion replaces pulses with static edge indicators.
- Large text moves controls into stacked bottom sheets.
- Haptics are optional.
- Audio descriptions are not required for MVP, but UI text must expose all critical state.
