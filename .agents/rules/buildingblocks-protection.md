---
paths:
  - "src/BuildingBlocks/**/*"
---

# ⚠️ BuildingBlocks Protection

**STOP. You are modifying BuildingBlocks.**

Changes to BuildingBlocks affect ALL modules across the entire framework. These are core abstractions that many projects depend on.

## Before Proceeding

1. **Consider alternatives** - Can this be done in the module instead?
2. **Assess impact** - What modules will this affect?

- Make minimal, focused changes
- Ensure backward compatibility
- Update all affected modules
- Document the change

## Alternatives to Consider

| Instead of... | Consider... |
|---------------|-------------|
| Modifying Core | Extension method in module |
| Changing Persistence | Custom repository in module |
| Updating Web | Module-specific middleware |
