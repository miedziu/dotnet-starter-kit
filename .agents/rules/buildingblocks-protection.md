---
paths:
  - "src/BuildingBlocks/**/*"
---

# ⚠️ BuildingBlocks Protection

**STOP. You are modifying BuildingBlocks.** Changes affect ALL modules.

## Before Proceeding

1. **Consider alternatives** — Can this be done in the module instead?
2. **Assess impact** — What modules will this affect?

- Make minimal, focused changes
- Update all affected modules
- Document the change

## Alternatives

| Instead of... | Consider... |
|---|---|
| Modifying Core | Extension method in module |
| Changing Persistence | Custom repository in module |
| Updating Web | Module-specific middleware |