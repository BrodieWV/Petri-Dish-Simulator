# Saccharomyces cerevisiae Candidate Review

Status: decision support only. This species is **not approved**, no production definition exists, and no simulation values are authorized by this document.

## Why it is a plausible M6 candidate

The Content Bible already includes a Yeast-like Culture archetype. *Saccharomyces cerevisiae* would give the first two-organism comparison one bacterium and one budding yeast rather than two visually similar bacteria. It is also well represented in primary research, which makes educational claims and their limits easier to review.

This is a product-fit argument, not a scientific conclusion. The product owner must still approve the species, identity scope, gameplay contrast, and visual direction.

## Source-backed statements suitable for review

- *S. cerevisiae* is a budding yeast. Bud formation is a cell-cycle-linked process, although the game would not simulate individual cells or the molecular control system.
- Experimental studies commonly observe strong temperature effects. A multi-species study reported a species-level optimum near 32.3 degrees C and maximum near 45.4 degrees C across its tested strains, while other strain-specific work found different optima and stress responses. Any game tolerance must therefore be explicitly educationalised and must not imply a universal strain value.
- Nutrient conditions and strain background can substantially alter colony morphology. A survey across strains and conditions found that carbon limitation combined with rich nitrogen was a major trigger for complex colony morphology. The present simulation does not separate carbon and nitrogen pools, so it cannot represent that mechanism faithfully.
- ATCC assigns BSL-1 to specific deposited *S. cerevisiae* material under its own risk assessment. This does not establish that every strain, preparation, or use of the species is universally risk-free.

## Safe player-facing direction if approved

- Describe it as an educationalised budding yeast culture.
- Teach that organisms can respond differently to temperature and nutrient availability.
- Use rounded clustered colony cues without claiming microscopic, strain-specific, or diagnostic accuracy.
- State that the game compresses cell division, metabolism, nutrient composition, colony age, and strain variation into normalized gameplay values.
- Keep fermentation as optional background context, not a simulated laboratory or food-production process.

## Claims and shortcuts to avoid

- Do not call a normalized temperature value the universal optimum or lethal boundary for the species.
- Do not infer species or strain identity from colony appearance.
- Do not equate one BSL-1 deposit with a blanket safety claim for every *S. cerevisiae* strain.
- Do not describe the current single nutrient pool as glucose, nitrogen, or a real medium recipe.
- Do not represent budding, pseudohyphae, sporulation, ethanol production, or fermentation as simulated unless those systems are separately approved and implemented.
- Do not reuse the existing `rapid-bacterium` stable ID or visual profile.

## Proposed educational contrast, pending approval

The candidate could be tuned to form denser, more compact clusters, expand more slowly across the agar, prefer a somewhat warmer range than the current *B. subtilis* gameplay profile, and show a stronger visible response to nutrient limitation. These are proposed design goals only. Each numeric mapping would remain a low-confidence gameplay abstraction and would need same-seed distinct-outcome tests.

## Product decisions required

1. Approve or reject *Saccharomyces cerevisiae* as organism two.
2. If approved, decide whether the content identity is species-level or explicitly a non-strain-specific educational culture.
3. Approve the compact-cluster/slower-spread/warmer-preference/nutrient-sensitive gameplay contrast, or provide a different contrast.
4. Approve a rounded clustered visual direction that is clearly stylised and non-diagnostic.
5. Confirm that fermentation, sporulation, pseudohyphae, and strain-specific traits remain outside M6.

## Sources

- Granek, J. A., and Magwene, P. M. (2010). "Environmental and Genetic Determinants of Colony Morphology in Yeast." *PLoS Genetics* 6(1):e1000823. https://doi.org/10.1371/journal.pgen.1000823
- Salvado, Z. et al. (2011). "Temperature Adaptation Markedly Determines Evolution within the Genus Saccharomyces." *Applied and Environmental Microbiology* 77(7):2292-2302. https://doi.org/10.1128/AEM.01861-10
- Singer, R. A., Bedard, D. P., and Johnston, G. C. (1984). "Bud formation by the yeast Saccharomyces cerevisiae is directly dependent on start." *Journal of Cell Biology* 98(2):678-684. https://doi.org/10.1083/jcb.98.2.678
- ATCC material page for *S. cerevisiae* MSV-1, including its material-specific biosafety assessment: https://www.atcc.org/products/msv-1

## Required implementation evidence after approval

- a new stable ID and definition version that do not alter `rapid-bacterium`;
- explicit sources, confidence, and simplification metadata in the asset;
- validation and exact-value tests;
- same-seed tests proving meaningfully distinct behaviour on both production media;
- schema-v3 save/load, restart, missing-definition, and exact-continuation coverage;
- selection-panel readability and non-diagnostic visual review;
- qualified microbiology review before release.
