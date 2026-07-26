# Vertical Slice Asset Register

## Purpose

List every non-code asset needed for Milestones M1–M5. Each item has an identifier so placeholders can be replaced without changing scene logic.

## Naming rules

- Prefix by category: `spr_`, `tex_`, `ico_`, `vfx_`, `anim_`, `sfx_`, `mus_`, `font_`, `mat_`, `prefab_`.
- Use lowercase snake case.
- Variants end with a descriptive suffix, not a number where meaning is known.
- Placeholder assets use the final identifier with `_placeholder` in metadata, not in runtime references.

## Dish and environment visuals

| ID | Type | Description | Priority |
|---|---|---|---|
| `spr_dish_standard_base` | Sprite | Transparent round dish base and rim | M1 |
| `spr_dish_standard_glass` | Sprite | Glass highlight and edge reflection | M1 |
| `tex_medium_nutrient_agar_base` | Texture | Soft translucent agar surface | M1 |
| `tex_medium_nutrient_agar_dry` | Texture | Fine dry or cracked detail | M3 |
| `tex_medium_nutrient_agar_wet` | Texture | Saturated glossy detail | M4 |
| `tex_condensation_mask` | Texture | Reusable droplet mask | M4 |
| `mat_dish_glass` | Material | Mobile-safe transparent dish material | M1 |
| `mat_agar` | Material | Tintable agar material | M1 |
| `mat_environment_overlay` | Material | Moisture, temperature, nutrient overlays | M2 |

## Rapid Bacterium colony visuals

| ID | Type | Description | Priority |
|---|---|---|---|
| `tex_rapid_bacterium_density` | Texture/atlas | Density field lookup or atlas | M3 |
| `tex_rapid_bacterium_edge` | Texture | Active colony edge texture | M3 |
| `tex_rapid_bacterium_stress` | Texture | Roughened stressed surface | M3 |
| `tex_rapid_bacterium_dormant` | Texture | Compact low-activity state | Later |
| `tex_rapid_bacterium_dying` | Texture | Breakdown texture without gore | M3 |
| `anim_colony_growth_pulse` | Animation | Subtle healthy internal motion | M3 |
| `anim_colony_stress_slow` | Animation | Reduced movement and irregular edge | M3 |
| `anim_colony_recovery` | Animation | Gradual return of edge activity | M4 |

## Visual effects

| ID | Description | Trigger |
|---|---|---|
| `vfx_inoculation_drop` | Initial droplet entering dish | Experiment start |
| `vfx_growth_micro_pulse` | Small local growth feedback | Healthy growth milestone |
| `vfx_heat_shimmer_subtle` | Low-intensity heat indication | Excess temperature |
| `vfx_moisture_dose` | Droplet and spreading ripple | Moisture intervention |
| `vfx_condensation` | Sparse droplets near dish edge | High moisture or heat cycle |
| `vfx_discovery_spark` | Minimal celebratory highlight | Discovery |
| `vfx_success_ring` | Dish-edge completion effect | Successful outcome |

All effects must have reduced-motion alternatives.

## UI icons

- `ico_temperature`
- `ico_moisture`
- `ico_light`
- `ico_airflow`
- `ico_nutrients`
- `ico_inspect`
- `ico_pause`
- `ico_play`
- `ico_speed_1x`
- `ico_speed_2x`
- `ico_speed_4x`
- `ico_growth_up`
- `ico_growth_flat`
- `ico_growth_down`
- `ico_health`
- `ico_coverage`
- `ico_warning`
- `ico_discovery`
- `ico_star`
- `ico_knowledge`
- `ico_lock`
- `ico_accuracy_observed`
- `ico_accuracy_simplified`
- `ico_accuracy_gameplay`

Icons require readable silhouettes at 24 logical pixels and text labels where critical.

## UI components and backgrounds

- `spr_panel_bottom_sheet`
- `spr_panel_tutorial_prompt`
- `spr_panel_discovery`
- `spr_panel_outcome`
- `spr_button_primary`
- `spr_button_secondary`
- `spr_slider_track`
- `spr_slider_handle`
- `spr_progress_coverage`
- `spr_progress_health`
- `spr_status_chip`
- `spr_objective_chip`
- `spr_inspection_marker`

Prefer Unity UI shapes or nine-sliced assets where possible.

## Audio

### Music

| ID | Description | Length target |
|---|---|---:|
| `mus_menu_ambient` | Curious, calm menu loop | 60–90 s |
| `mus_dish_observation` | Minimal active experiment loop | 90–150 s |

### Sound effects

- `sfx_ui_select`
- `sfx_ui_back`
- `sfx_ui_panel_open`
- `sfx_simulation_pause`
- `sfx_simulation_resume`
- `sfx_temperature_step`
- `sfx_moisture_dose`
- `sfx_growth_positive`
- `sfx_warning_heat`
- `sfx_warning_dry`
- `sfx_recovery`
- `sfx_discovery`
- `sfx_success`
- `sfx_soft_failure`

Target sounds are short, non-harsh, and understandable at low phone-speaker volume.

## Typography

Use a redistributable font family with:

- Regular
- Medium or semibold
- Clear numerals
- Broad language coverage if feasible
- Strong readability at mobile sizes

Do not commit unlicensed font files.

## Content illustrations

- Rapid Bacterium collection card
- Nutrient Agar collection card
- Standard Dish collection card
- The Comfortable Range experiment thumbnail
- A Comfortable Range discovery illustration
- Heat Can Dry a Dish discovery illustration

These may begin as stylised placeholder diagrams.

## Placeholder policy

M1–M3 may use geometric placeholders when:

- Final identifier and expected dimensions are preserved.
- Placeholder status is logged.
- The placeholder does not conceal a readability or performance problem.

## Technical budgets

Initial mobile guidance:

- Prefer sprite atlases.
- Avoid excessive transparent overdraw across the whole screen.
- Keep particle counts low and pooled.
- Use compressed audio appropriate to clip length.
- Prepare visual assets at practical mobile resolutions, not arbitrary 4K.
- Verify transparent materials on target Android devices.

## Asset completion checklist

For every asset record:

- Identifier
- Owner
- Status
- Source or licence
- Import settings
- Target size
- Pivot or slicing information
- Accessibility alternative where needed
- Scene or prefab consumers
