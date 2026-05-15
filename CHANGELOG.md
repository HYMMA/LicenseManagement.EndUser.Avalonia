# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2026-05-15

### Changed
- Bumped `LicenseManagement.EndUser` dependency from 2.0.0 to 3.0.1 to pick up the
  tamper-detection fix in `LicenseSignatureValidationHandler` and the uninstall disk-read fix.
- Excluded `fix-audit/` and `LicenseManagement.EndUser.Avalonia.Tests/` subdirectories from the
  main SDK glob so stale branch snapshots and test files don't pollute the library build.

### Added
- Unit test project (`LicenseManagement.EndUser.Avalonia.Tests`) with 30 tests covering
  `LicenseCredentials` constructor validation and `WithProductId` immutability, `LicenseErrorPresenter`
  message coverage for all `LicenseErrorKind` values, `LicenseOperationResult` factory methods, and
  `LicenseViewModel` property binding and command guard behaviour using stub service implementations.

## [2.0.0] - 2026-05-15

Initial public release. Avalonia 11 UI components for license-management.com end-user SDK.
Provides `LicenseControl` (embeddable UserControl), `LicenseWindow` (standalone modal), view
models, converters, and a `ILicenseService` abstraction. Wraps `LicenseHandlingLaunch`,
`LicenseHandlingInstall`, and `LicenseHandlingUninstall` from the `LicenseManagement.EndUser` SDK.
