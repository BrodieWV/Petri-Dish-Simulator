# Low-Nutrient Agar Content Review

Status: approved content category; numeric balance remains an educational gameplay abstraction.

## Player-facing purpose

Low-Nutrient Agar is the second Phase 2 medium. It starts with fewer available nutrients and slightly less moisture, dries faster, absorbs less added moisture, and resists colony spread more than the default Nutrient Agar. These differences give the player a controlled comparison without changing organism identity or environmental input.

## Scientific basis and limits

Reasoner and Geldreich introduced R2A as a lower-nutrient medium for recovering heterotrophic bacteria from potable water. This supports the qualitative educational point that nutrient composition and concentration can change observable culture outcomes.

The game medium is deliberately named the generic **Low-Nutrient Agar**, not R2A. It contains no ingredient recipe or culture protocol. Its normalized nutrient, moisture, evaporation, diffusion, absorption, and spread-resistance values are design parameters rather than measurements from the paper. It must not be presented as a laboratory formulation or predictive model.

Confidence is **Low**: the category is source-backed, while the numerical mapping and the response of the game's simplified organism are educationalised.

## Source

- Reasoner, D. J., and Geldreich, E. E. (1985). "A new medium for the enumeration and subculture of bacteria from potable water." *Applied and Environmental Microbiology*, 49(1), 1-7. https://doi.org/10.1128/AEM.49.1.1-7.1985

## Verification

- Definition validation must accept the asset and explicit confidence metadata.
- Catalog resolution must find `low-nutrient-agar` while retaining `nutrient-agar` as the migration default.
- With the same organism, seed, temperature, and number of fixed steps, the two production media must produce measurably different nutrient, moisture, and colony-growth outcomes.
- Manual review should confirm that the medium name and explanation are readable on mobile when the selection UI is implemented.
