# Scientific Accuracy and Safety Guide

## Purpose

Keep the game educationally useful without presenting simplified mechanics as laboratory fact or providing unsafe cultivation instructions.

## Core policy

Petri Dish Simulator models relationships, not real-world culturing procedures.

The game may teach that temperature, moisture, nutrients, airflow, light, waste, competition, and time affect living systems. It should not teach players how to culture hazardous organisms or reproduce laboratory protocols.

## Content labels

Every organism, medium, discovery, and challenge must use one of these labels.

### Observed relationship

Use when the broad relationship is well established and the game representation preserves its direction.

Example: higher temperature can increase evaporation.

### Educational simplification

Use when a real relationship is compressed, combined, accelerated, or represented with broad variables.

Example: one general nutrient meter representing several required resources.

### Gameplay abstraction

Use when a mechanic primarily exists to create understandable decisions.

Example: a single health percentage combining stress and damage.

### Fictional

Use for invented organisms, fantasy traits, or deliberately unrealistic content.

## Organism naming

MVP organisms should use broad archetype names:

- Rapid Bacterium
- Yeast-like Culture
- Filamentous Fungus
- Slime Mould

Avoid naming a real pathogen or implying the game simulates a specific strain unless later content has expert review and a clear educational reason.

## Restricted content

Do not include:

- Step-by-step real cultivation protocols
- Exact incubation recipes for pathogens
- Actionable instructions for collecting, isolating, enriching, or propagating hazardous organisms
- Advice for culturing unknown environmental samples
- Sterility, containment, or disposal instructions framed as sufficient professional guidance
- Real diagnostic claims
- Medical treatment claims
- Claims that gameplay outcomes predict laboratory outcomes

## Acceptable content

The game may include:

- Broad explanations of environmental tolerance
- Simplified nutrient depletion
- Stylised competition and contamination
- General explanations of dormancy, growth, and resource limitation
- Historical or educational facts with sources and review
- Fictionalised dishes and organisms
- Classroom discussion prompts that do not instruct wet-lab culturing

## Contamination language

Contamination should be framed as an unexpected competing culture, not as horror or disease.

Preferred:

- Uninvited culture
- Competing colony
- Contaminant archetype
- Unexpected growth

Avoid sensational infection language in the base game.

## Numeric values

All initial values are gameplay tuning values.

Rules:

- Do not attach units to invented internal rates unless the unit is meaningful.
- Real display units such as degrees Celsius may be used, but tolerance values remain archetype-specific game data.
- Documentation must distinguish measured scientific values from tuning values.
- Do not claim a broad archetype’s preferred range applies to all bacteria, fungi, yeasts, or slime moulds.

## Scientific review workflow

Before publishing an educational claim:

1. Identify the exact claim.
2. Classify its accuracy label.
3. Record at least one credible source in the content research notes.
4. Check whether the wording overgeneralises.
5. Check whether the claim creates actionable unsafe guidance.
6. Have uncertain or species-specific claims reviewed by a qualified subject-matter expert before release.

## Source quality hierarchy

Prefer:

1. Peer-reviewed review papers and textbooks
2. University and government educational resources
3. Museum, botanical garden, and recognised scientific institution resources
4. Expert-authored educational material

Do not rely on unsourced blogs, generated summaries, or social posts for final scientific claims.

## Child-friendly presentation

- Explain one relationship at a time.
- Use observation-first language.
- Avoid implying that a failed dish means the player harmed a real creature.
- Do not use graphic death visuals.
- Make failure reversible or informative.
- Keep advanced terminology optional.

## Classroom and parent-facing statement

Recommended wording:

> Petri Dish Simulator is a science-inspired game. It simplifies biological and environmental relationships for play and learning. It is not a laboratory guide and should not be used to plan real microbial cultivation.

## Research record template

For each claim record:

- Claim ID
- Player-facing wording
- Internal full claim
- Accuracy label
- Organism scope
- Source title
- Source type
- Source date
- Relevant passage summary
- Reviewer
- Review date
- Safety notes

## Release gate

A build is not content-ready when:

- An educational claim lacks an accuracy label.
- A broad archetype is described as representing all organisms in its category.
- A challenge contains procedural real-world cultivation instructions.
- A health or medical inference can reasonably be drawn from gameplay.
- The simplified model is presented as predictive laboratory software.
