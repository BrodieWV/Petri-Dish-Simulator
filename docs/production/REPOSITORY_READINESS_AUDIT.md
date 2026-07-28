# Repository Readiness Audit

## Audit date

Updated 27 July 2026

## Current status

The repository contains the Phase 0 foundation and a first-pass implementation of Milestones M1–M5. Product, simulation, content, UX, safety, and agent boundaries are defined. It has been imported and compiled using the locked Unity 6.5 editor, and the complete Edit Mode suite passes.

## Locked Unity setup decisions

- Editor baseline: Unity 6.5
- Exact project marker: `6000.5.3f1`
- Project location: repository root
- Layout: portrait-first mobile
- UI: GameObject-based Unity UI (`com.unity.ugui`)
- Rendering: built-in rendering for the first prototype unless Codex identifies a clear need for URP
- Runtime AI: none for MVP

Do not downgrade the project to Unity 2022, Unity 2023, or an earlier Unity 6 editor.

## Available foundations

### Product definition

- Vision
- Game design document
- Principles
- Decision log
- Roadmap
- Milestones
- Progression and economy
- Analytics

### Simulation definition

- Layered architecture
- Runtime and definition data models
- Deterministic simulation requirement
- Initial environmental model
- Rapid Bacterium and Nutrient Agar starting values
- Fixed first guided experiment
- Golden scenarios and test strategy

### Presentation definition

- Screen hierarchy and behavioural wireframes
- Mobile safe-area requirements
- Accessibility rules
- Scene hierarchy
- Prefab responsibilities
- Asset register
- Art and audio direction
- Complete tutorial and outcome copy

### Production definition

- Agent rules
- Phase 1 backlog
- Unity build brief
- Codex review brief
- M1/M2 Codex implementation prompt
- M1–M5 implementation handoff
- Acceptance criteria for M0–M7

### Content and safety

- Four initial organism archetypes
- Four initial media
- Twelve guided experiment concepts
- Twenty challenge concepts
- Accuracy labels
- Scientific review workflow
- Prohibited unsafe content

## Readiness checks

| Area | Status | Notes |
|---|---|---|
| Game vision | Ready | Clear audience and product ladder |
| Vertical slice | Implemented first pass | One guided experiment in code |
| Simulation scope | Implemented first pass | Simplified deterministic grid |
| Starting balance | Ready for tuning | Values explicitly provisional |
| Architecture | Implemented first pass | Simulation separated from presentation |
| UI flow | Functional first pass | Final visual design remains open |
| Unity hierarchy | Runtime generated | Production prefabs remain optional |
| Art production | Placeholder | Final style exploration required |
| Audio production | Pending | Asset list exists |
| Tutorial copy | Implemented in part | Full copy verification pending |
| Testing | Deferred | Determinism and golden scenarios defined |
| Safety | Ready | Strong scope and review controls |
| Monetisation | Deferred correctly | Not part of prototype |
| Online services | Deferred correctly | Not required for core play |

## Remaining Unity production decisions

1. Minimum Android API level and target-device baseline
2. Whether to adopt the Unity Input System for production controls
3. Final text rendering and redistributable font
4. Whether the visual target justifies moving from built-in rendering to URP
5. Build Profile setup for Android and desktop development builds

## Required next review

1. Open with Unity `6000.5.3f1`.
2. Allow Unity 6 to resolve core packages.
3. Compile all scripts.
4. Run `Petri Dish > Setup Vertical Slice Project`.
5. Record compiler errors and API migration warnings.
6. Have Codex correct Unity 6 compatibility issues.
7. Run the guided experiment in Play Mode.
8. Review architecture, saving, generated UI, and balance.

## Known implementation uncertainties

### Colony rendering method

The current version uses CPU texture updates. Codex should compare this with a shader-driven or GPU-assisted field renderer before larger content is added.

### Global versus local temperature

The vertical slice uses global temperature. The grid architecture can later support gradients.

### Moisture field timing

The first pass includes local moisture and edge drying. Values require tuning after running in Unity.

### Health display

The player-facing condition combines internal signals. Usability testing should determine the best terminology.

## Risks

- Unity 6 API or package differences may produce initial import errors.
- Runtime-generated UI can become difficult to maintain if it grows without conversion to prefabs or UI documents.
- Provisional balance values may produce poor pacing until tuned in Play Mode.
- CPU texture updates may become expensive on lower-end mobile devices.
- Committing generated Unity folders or unlicensed assets must be avoided.

## Audit conclusion

The project is correctly targeted to Unity 6.5 (`6000.5.3f1`), has completed editor import and Codex hardening, and passes all 67 Edit Mode cases. Earlier Unity editor baselines have been superseded.
