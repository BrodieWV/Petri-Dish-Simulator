# Starting Balance Values

These values are provisional gameplay units for the first vertical slice. They are not laboratory measurements. All values must remain externally tunable and carry the label `Gameplay abstraction` until reviewed.

## Normalised scales

Most internal environmental and biological quantities use a 0–1 range.

- 0: absent or minimum
- 1: maximum supported value
- Percentages shown to players are display conversions.

Temperature remains in degrees Celsius for player readability.

## Rapid Bacterium

| Property | Starting value | Notes |
|---|---:|---|
| Preferred temperature minimum | 26 °C | Full growth begins |
| Preferred temperature optimum | 28 °C | Tutorial target |
| Preferred temperature maximum | 30 °C | Full growth ends |
| Growth temperature minimum | 12 °C | Below this, no positive growth |
| Growth temperature maximum | 36 °C | Above this, no positive growth |
| Survival temperature minimum | 5 °C | Sustained exposure causes damage |
| Survival temperature maximum | 40 °C | Sustained exposure causes damage |
| Preferred moisture minimum | 0.65 | 65% display value |
| Preferred moisture maximum | 0.80 | 80% display value |
| Growth moisture minimum | 0.38 | Below this, growth stops |
| Survival moisture minimum | 0.18 | Below this, damage accumulates |
| Base biomass growth per tick | 0.020 | Before suitability and crowding |
| Nutrient consumed per biomass gained | 0.70 | Normalised gameplay ratio |
| Moisture consumed per biomass gained | 0.08 | Small direct use |
| Waste produced per biomass gained | 0.25 | Used after vertical slice if enabled |
| Natural stress recovery per tick | 0.025 | Under suitable conditions |
| Stress gained under limiting conditions | 0.018 | Scaled by severity |
| Damage gained outside survival limits | 0.035 | Scaled by exposure |
| Local carrying capacity | 1.00 | Maximum density per cell |
| Spread threshold | 0.32 | Density before outward spread |
| Base spread fraction per tick | 0.10 | Before suitability |
| Dormancy entry stress | 0.62 | Later milestone feature |
| Death threshold | 1.00 damage | Population begins loss |

## Nutrient Agar

| Property | Starting value | Notes |
|---|---:|---|
| Initial nutrient level | 0.80 | Tutorial start |
| Maximum nutrient capacity | 1.00 | |
| Initial moisture | 0.72 | Tutorial start |
| Maximum moisture capacity | 1.00 | |
| Moisture diffusion | 0.12 | Fraction of local difference per tick |
| Nutrient diffusion | 0.06 | Slower than moisture |
| Base evaporation per tick | 0.0015 | At 22 °C and low airflow |
| Edge evaporation multiplier | 1.50 | Creates visible drying edge |
| Structural spread resistance | 0.05 | Low resistance |
| Visual opacity | 0.28 | Rendering guidance only |

## Temperature response

Use a smooth response curve rather than hard bands.

Suggested anchor points:

| Temperature | Growth suitability |
|---:|---:|
| 5 °C | 0.00 plus damage |
| 12 °C | 0.00 |
| 18 °C | 0.30 |
| 22 °C | 0.65 |
| 26 °C | 1.00 |
| 28 °C | 1.00 |
| 30 °C | 1.00 |
| 33 °C | 0.55 |
| 36 °C | 0.00 |
| 40 °C | 0.00 plus damage |

Interpolate smoothly between anchors. Do not use a single linear range across the entire curve.

## Moisture response

| Moisture | Growth suitability |
|---:|---:|
| 0.18 | 0.00 plus damage |
| 0.38 | 0.00 |
| 0.50 | 0.45 |
| 0.65 | 1.00 |
| 0.80 | 1.00 |
| 0.90 | 0.75 |
| 1.00 | 0.40 |

Excess moisture should limit growth mildly in this archetype, not kill the culture during the first tutorial.

## Combined suitability

For the first slice:

`combined suitability = minimum(temperature suitability, moisture suitability, nutrient suitability)`

This minimum-factor approach makes the limiting condition easy to explain. More advanced editions may use multiplicative or metabolic models.

Nutrient suitability:

- 0.20 or higher: 1.00
- 0.05–0.20: interpolate from 0.20 to 1.00
- Below 0.05: interpolate from 0.00 to 0.20

## Growth rule

Conceptual rule:

`growth = base growth × combined suitability × available space × health modifier`

Where:

- Available space falls from 1 to 0 as local density approaches carrying capacity.
- Health modifier falls as stress and damage rise.
- Growth is applied only after resource cost is confirmed.

## Stress and recovery

- Suitability above 0.80: recover stress.
- Suitability 0.35–0.80: stress remains stable or changes slowly.
- Suitability below 0.35: stress accumulates.
- Outside a survival limit: damage accumulates in addition to stress.
- Recovery has a five-tick delay after returning to suitable conditions.

## Temperature intervention

- Player range: 5–42 °C
- Control increment: 1 °C
- Environmental movement: maximum 1.5 °C per simulation tick
- Display target and actual temperature separately only if testing shows the lag is understandable.

## Moisture intervention

- One moisture dose adds 0.08 to the targeted or global field.
- Maximum one dose every five ticks in the guided experiment.
- Moisture is capped at medium capacity.
- Applying moisture creates a short local or global diffusion delay.

## Simulation timing

Prototype recommendation:

- One simulation tick: 0.5 real seconds at normal speed
- Fast speed: 4 ticks per real second
- Very fast development speed: 20 ticks per real second
- Visual interpolation occurs independently

## Tuning targets

Under ideal conditions:

- First visible expansion: within 10 real seconds
- 10% coverage: 35–50 real seconds
- 40% coverage: 2–3 minutes
- 55% coverage: 3–5 minutes

At 18 °C:

- Growth should be clearly slower, but still visible within 20 seconds.

At 36 °C:

- New growth stops within 3–5 ticks.
- Stress becomes visible within 8–12 ticks.

## Validation scenarios

- Ideal: 28 °C, moisture 0.72, nutrients 0.80
- Cool: 18 °C, moisture 0.72
- Hot: 36 °C, moisture 0.72
- Dry: 28 °C, moisture 0.30
- Starved: 28 °C, moisture 0.72, nutrients 0.03
- Recovery: ten hot ticks followed by ideal conditions

These values are a starting calibration set, not final balance.
