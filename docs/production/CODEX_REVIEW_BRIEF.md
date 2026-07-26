# Codex Review Brief

Review priorities: simulation correctness/determinism, save integrity, separation of simulation/presentation, milestone criteria, mobile performance, validation, UX/accessibility, and maintainability.

Required questions:

- Does behaviour depend on frame rate?
- Is simulation randomness seeded?
- Can visuals change authoritative state?
- Can saves resume deterministically?
- Are IDs stable and validated?
- Are growth, resources, stress, dormancy, and death distinct?
- Is the limiting factor understandable?
- Are failures informative?
- Is mobile lifecycle safe?
- Are simplifications labelled?
- Are unsafe cultivation details absent?
- Are golden scenarios tested?

For each finding provide severity, file/system, observed problem, impact, correction, and verification. Avoid unrelated large refactors.