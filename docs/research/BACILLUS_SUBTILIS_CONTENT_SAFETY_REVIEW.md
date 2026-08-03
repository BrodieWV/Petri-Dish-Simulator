# Bacillus subtilis Content and Safety Review

Status: product-owner implementation approval recorded on 3 August 2026; qualified subject-matter review required before release.

Review date: 3 August 2026.

## Recommendation

Use *Bacillus subtilis* as the first named bacterial organism, subject to the release gates below.

It is a strong fit for the existing Rapid Bacterium role because it is a well-studied Gram-positive model organism and supports player-visible teaching about colony growth, environmental stress, biofilms, and endospore formation. The current simulation does not model a particular strain, laboratory medium, biofilm programme, or sporulation pathway, so those traits must remain contextual educational notes rather than claims about the simulated numbers.

Do not describe *B. subtilis* as universally harmless or safe to culture. EFSA's Qualified Presumption of Safety process still requires strain identity, absence of acquired antimicrobial-resistance genes, and lack of toxigenic potential for *Bacillus* strains. The game must not turn that regulatory assessment into practical cultivation advice.

## Claim records

### ORG-BS-001

- Player-facing wording: "A well-studied bacterium used by scientists to explore how bacterial cells grow and change."
- Internal full claim: *Bacillus subtilis* is a major model organism for Gram-positive bacterial cell biology.
- Accuracy label: Observed relationship.
- Organism scope: Species-level identity; no specific simulated strain.
- Source: Errington and van der Aart, "Microbe Profile: Bacillus subtilis: model organism for cellular development, and industrial workhorse," *Microbiology* 166 (2020), DOI `10.1099/mic.0.000922`.
- Source link: https://pmc.ncbi.nlm.nih.gov/articles/PMC7376258/
- Relevant passage summary: The review identifies *B. subtilis* as the best-studied model organism of the Gram-positive lineage and describes its use in cell biology.
- Confidence: High for the general identity claim.
- Safety notes: This is descriptive context, not a claim that the game predicts real growth.

### ORG-BS-002

- Player-facing wording: "When conditions become stressful, some *B. subtilis* cells can begin a specialised endospore-forming process."
- Internal full claim: Environmental stress can initiate the regulated sporulation programme in *B. subtilis*.
- Accuracy label: Observed relationship.
- Organism scope: Broad species biology; exact triggers and outcomes vary with strain and conditions.
- Source: Higgins and Dworkin, "Recent progress in Bacillus subtilis sporulation," *FEMS Microbiology Reviews* 36 (2012), DOI `10.1111/j.1574-6976.2011.00310.x`; review indexed at https://pubmed.ncbi.nlm.nih.gov/22091839/.
- Supporting source: Tan and Ramamurthi, "Spore formation in Bacillus subtilis," *Environmental Microbiology Reports* 6 (2014), DOI `10.1111/1758-2229.12130`, https://pubmed.ncbi.nlm.nih.gov/24983526/.
- Relevant passage summary: These reviews describe sporulation as a specialised developmental response associated with environmental stress.
- Confidence: High for the relationship; low for any mapping to the present simulation.
- Safety notes: Do not provide trigger thresholds, incubation recipes, or wet-lab steps.

### ORG-BS-003

- Player-facing wording: "Different strains and environments can produce very different colony patterns."
- Internal full claim: *B. subtilis* colony architecture varies substantially between strains and environmental contexts.
- Accuracy label: Observed relationship.
- Organism scope: Strain- and environment-dependent.
- Source: Earl, Losick, and Kolter, "Ecology and genomics of Bacillus subtilis," *Trends in Microbiology* 16 (2008), DOI `10.1016/j.tim.2008.03.004`, https://pubmed.ncbi.nlm.nih.gov/18467096/.
- Relevant passage summary: The review discusses genomic diversity and contrasts colony architecture of a natural isolate with domesticated strain 168.
- Confidence: High for variability; moderate for simplified player wording.
- Safety notes: Avoid presenting one generated texture as the canonical appearance of the species.

### ORG-BS-004

- Player-facing wording: none; internal safety qualification only.
- Internal full claim: A species-level safety framework does not remove the need for strain-level checks for *Bacillus* organisms.
- Accuracy label: Observed relationship.
- Organism scope: Regulatory safety assessment, not gameplay biology.
- Source: European Food Safety Authority, "Qualified presumption of safety (QPS)," reviewed 25 March 2026, https://www.efsa.europa.eu/en/topics/topic/qualified-presumption-safety-qps and https://www.efsa.europa.eu/en/applications/qps-assessment.
- Relevant passage summary: QPS is a pre-assessment framework; *Bacillus* strains still require confirmed identity, absence of acquired antimicrobial-resistance genes, and lack of toxigenic potential.
- Confidence: High for the stated EFSA framework.
- Safety notes: Never convert QPS status into a player-facing claim that unknown or home-collected cultures are safe.

## Proposed content metadata

- Stable ID: keep `rapid-bacterium` because schema-version-3 saves require an exact catalog identity match and schema-version-2 migration explicitly targets this ID.
- Definition version: retain version `1` while only non-authoritative identity and education metadata changes. Schema-version-2 migration explicitly accepts only definition version `1`; any biological tuning change requires a separately approved versioning and migration design.
- Display name: `Bacillus subtilis`.
- Scientific name: `Bacillus subtilis`.
- Short description: `A simplified model bacterium used to explore how temperature, moisture, and nutrients influence colony growth.`
- Scientific label: `Named real organism with educationalised behaviour; not a strain-specific cultivation or prediction model.`
- Confidence: `Moderate` for the combined in-game representation.
- Visual profile: retain the existing profile initially only if visual review confirms it is presented as stylised rather than species-canonical.

## Required simplification notes

- All temperature, moisture, growth, stress, demand, carrying-capacity, and spread values are gameplay tuning values, not measured parameters for a real strain.
- The current health and stress meters combine many biological processes.
- The generated colony texture is stylised and does not identify a species or strain.
- Endospore formation, biofilm differentiation, motility, genetics, and strain variation are not simulated.
- Outcomes must not be used to plan cultivation or predict laboratory results.

## Release gates

1. Product owner approves *B. subtilis* as the first named organism. Completed on 3 August 2026.
2. A qualified microbiology reviewer checks the player-facing claims and records reviewer and review date.
3. The implementation preserves schema-version-3 exact identity matching, schema-version-2 migration, and deterministic replay.
4. Tests assert the reviewed metadata, moderate confidence, retained definition version `1`, and unchanged simulation values.
5. The UI displays the scientific label and simplification context anywhere species-specific results are explained.
6. Manual visual review confirms the colony texture is clearly stylised and not represented as diagnostic or strain-specific.

## Explicitly out of scope

- Cultivation procedures, recipes, incubation parameters, isolation, collection, disposal, or containment advice.
- Claims that a real strain will behave like the simulation.
- Strain-specific safety approval.
- Sporulation or biofilm mechanics before those systems have separate design and simulation review.
