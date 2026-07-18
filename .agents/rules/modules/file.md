# Module: File

Presigned-URL file lifecycle (upload → finalize → serve → delete) shared by Chat attachments, avatars. Module `Order = 350` (loads before consumer modules).

**Entities:** `FileAsset` (soft-deletable): status `PendingUpload → Available | Quarantined`, `Visibility` (Public/Private), `ScanStatus`. `FileDbContext`. Publishes `FileFinalizedIntegrationEvent`.

**Areas:** RequestUploadUrl, FinalizeUpload, GetFileDownloadUrl/Metadata, ChangeVisibility, Delete/Restore, ListMy/Shared/Trashed. Purge jobs (orphaned hourly, deleted daily).

## Gotchas

- **Presigned flow** — never stream uploads through API. RequestUploadUrl validates category/extension/size pre-check, persists `PendingUpload`; client uploads directly; **FinalizeUpload debits** (not at request time) and flips to Available/Quarantined.
- **`FileAccessPolicyRegistry`** resolves `IFileAccessPolicy` by **OwnerType** — case-insensitive, **closed by default**, **last-write-wins** on duplicates. Each module registers its policy in `ConfigureServices`. Files ships `DefaultUploaderOnlyPolicy` for `"MyFiles"`/`"User"`.
- `CanChangeVisibilityAsync` defaults to delete rule (uploader-only); domain-bound files may override to forbid visibility flips.

To support new owner type: implement `IFileAccessPolicy`, register in owning module, use OwnerType in RequestUploadUrl.
