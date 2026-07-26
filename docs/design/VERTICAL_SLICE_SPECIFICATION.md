# Vertical Slice Specification

## Purpose

Define the first complete playable experiment so implementation agents do not need to invent game rules, feedback, or success conditions.

## Slice identity

- **Experiment ID:** `guide.01.comfortable_range`
- **Title:** The Comfortable Range
- **Organism:** Rapid Bacterium
- **Medium:** Nutrient Agar
- **Dish:** Standard Round Dish
- **Primary lesson:** Organisms grow fastest within a preferred temperature range and become stressed outside it.
- **Secondary lesson:** Growth also consumes nutrients and moisture.
- **Target play time:** 4–7 minutes
- **Target audience:** First-time player

## Player objective

Grow the culture to 55% dish coverage while keeping average colony health above 70%.

## Starting state

- Temperature: 18 °C
- Moisture: 72%
- Nutrients: 80%
- Airflow: Low and fixed
- Light: Low and fixed
- Initial inoculation: one central colony occupying approximately 2% of dish area
- Simulation seed: fixed tutorial seed
- Simulation speed: normal
- Available interventions: temperature only for the first stage; moisture unlocks during the rescue stage

## Intended sequence

### Stage 1 — Observe slow growth

The culture begins below its preferred temperature. Growth is visible but slow.

Trigger after either 20 simulation ticks or 8% coverage:

- Condition panel: `Growing slowly`
- Limiting factor: `Temperature is below the preferred range`
- Tutor prompt: raise temperature gradually rather than immediately setting it to maximum.

### Stage 2 — Find the preferred range

The player adjusts temperature. The preferred band is 26–30 °C, with 28 °C as the tutorial target.

When the culture remains within the preferred band for 15 consecutive simulation ticks:

- Growth-front animation becomes more active.
- Healthy colony texture becomes brighter and smoother.
- Discovery unlocked: `A Comfortable Range`.
- Objective updates to reach 40% coverage.

### Stage 3 — Demonstrate overheating

At 40% coverage, the tutorial introduces a temporary heat fault that raises temperature to 36 °C. The player is told this is a deliberate demonstration, not a punishment.

Expected effects:

- Growth slows within several ticks.
- Stress increases.
- Colony edge loses activity.
- Condition panel changes to `Too hot`.
- Primary limiting factor becomes temperature.

The player must return temperature to 26–30 °C.

### Stage 4 — Moisture rescue

Once temperature is corrected, moisture has fallen to approximately 48% due to accelerated evaporation during the heat fault. Moisture control becomes available.

The player adds moisture until the dish returns to the preferred band of 65–80%.

Expected feedback:

- Dry visual texture softens.
- Growth resumes after a short recovery delay.
- Discovery unlocked: `Heat Can Dry a Dish`.

### Stage 5 — Complete culture

The player reaches 55% coverage with average health above 70%.

Outcome:

- Success screen explains temperature tolerance, delayed recovery, moisture loss, and nutrient consumption.
- Player earns two Knowledge points and one Culture Star.
- Nutrient Agar and Rapid Bacterium remain unlocked for sandbox play.

## Failure and recovery

This tutorial does not hard-fail from one mistake.

Soft failure states:

- Temperature above 38 °C for 20 ticks
- Temperature below 8 °C for 30 ticks
- Moisture below 25% for 20 ticks
- Average health below 25%

On soft failure:

1. Pause simulation.
2. Show the strongest causal factor.
3. Offer `Retry from checkpoint` and `Restart experiment`.
4. Preserve any discovery already earned.

## Condition labels

Priority order:

1. Dying
2. Too hot
3. Too cold
4. Too dry
5. Nutrient limited
6. Recovering
7. Growing slowly
8. Growing well
9. Stable

## Player-visible metrics

Visible:

- Temperature
- Moisture
- Coverage
- Condition
- Growth trend
- Main limiting factor

Hidden during tutorial:

- Exact biomass
- Exact waste concentration
- Cell-level suitability
- Random stream state

## Visual requirements

- Dish remains readable at phone width.
- Colony growth front must visibly advance at least once every 2–4 seconds at normal speed under ideal conditions.
- Heat stress changes motion and texture, not colour alone.
- Dryness appears first near exposed edges.
- Recovery is gradual rather than instantaneous.

## Audio requirements

- Soft loop during incubation
- Positive growth pulse when ideal range is sustained
- Distinct but non-alarming heat warning
- Moisture application sound
- Discovery cue
- Success cue

## Acceptance criteria

- A first-time player can complete the experiment without external explanation.
- Three temperature bands produce visibly different outcomes.
- Overheating creates delayed stress and moisture loss.
- Correcting temperature alone does not instantly remove stress.
- Moisture intervention restores growth after a recovery delay.
- Restarting with the tutorial seed reproduces the same initial dish.
- Outcome explanation correctly identifies the main causes.
