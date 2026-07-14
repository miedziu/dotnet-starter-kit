# Module: Catalog

Product catalog — products, categories (tree), brands — with soft-delete/restore/trash and search. Module `Order = 600`. **Reference module** for soft-delete + image patterns.

**Entities:** `Product` (soft-deletable) + `ProductImage`, `Brand`, `Category` (self-referencing tree), `Money` (owned VO). `CatalogDbContext`. Domain events (`ProductCreated`/`PriceChanged`/`StockAdjusted`) are **internal**, not integration events.

**Areas:** Products (+ price/stock/images), Categories (+ tree), Brands — each with Create/Update/Delete/Search/ListTrashed/Restore.

## Gotchas / patterns to copy

- **Soft-delete + restore + trashed-listing** — filtered unique indexes on `"IsDeleted" = FALSE` so SKU/Slug stay unique among live rows only.
- **EF value-generation for nav children** — `ProductImageConfiguration` sets `Id.ValueGeneratedNever()` (same nav-child footgun as Chat).
- **Single-thumbnail invariant** enforced by aggregate, not partial unique index (Postgres non-deferrable partial unique indexes can't handle demote/promote ordering in one transaction).
- Registers `ProductFileAccessPolicy` (OwnerType `"Product"`) for product images via Files module.
- Route ordering: literal segments (`/trash`, `/tree`, `/restore`) registered **before** `/{id:guid}` catch-alls.