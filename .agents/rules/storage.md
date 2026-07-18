# Storage & file uploads

`s/Lib/Storage/`. Use `IStorageService` for files/blobs.

## `IStorageService`

Methods: `UploadAsync<T>`, `RemoveAsync`, `DownloadAsync`, `ExistsAsync`, `GetSizeAsync`, `GenerateUploadUrlAsync`/`GenerateDownloadUrlAsync` (presigned), `HeadObjectAsync`, `BuildPublicUrl(key)` → string (server-relative path for local storage).

`FileType`: `Image` (5MB), `Document`, `Pdf` (10MB). `FileTypeMetadata.GetRules` enforces extension + size. **Always propagate `CancellationToken`.**

## Providers

`AddHeroStorage(config)` reads `Storage:Provider` eagerly: `"s3"` → `S3StorageService` (supports MinIO via `ServiceUrl` + `ForcePathStyle`); else `LocalStorageService`.

## Presigned upload flow (preferred)

Don't stream large files through API:

1. `RequestUploadUrl` — validates category/extension/size, returns presigned PUT URL, persists `PendingUpload`
2. Client uploads **directly** to storage
3. `FinalizeUpload` — flips to `Available`, debits here (not at request time), publishes `FileFinalizedIntegrationEvent`

Local/dev without MinIO uses `LocalPresignTokenStore` (in-memory one-shot tokens).
