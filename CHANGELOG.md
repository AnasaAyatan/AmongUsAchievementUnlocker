# Changelog

## [Unreleased] — v0.1.0 release candidate

### Added
- Windows Forms GUI with searchable achievement table and a confirmation-gated **Unlock remaining** action.
- Display of generated friendly labels, Steam achievement IDs, and unlocked/locked status.
- All / Unlocked / Locked status filters, overall progress bar, and unlocked/remaining counts.
- Collapsible raw diagnostics panel.
- Windows packaging workflow including GUI and CLI, excluding proprietary native Steamworks binaries.
- Windows setup, developer guide, and version-specific release notes.

### Fixed
- Root CLI project incorrectly compiling `gui/*.cs`.
- GUI parsing failed to recognize CLI achievement lines.
- Docking order and table text contrast.
- Improved diagnostics for missing, incompatible, and wrong-architecture native libraries.

### Validation
- **Confirmed locally:** CLI successfully retrieved all 33 achievements, subsequent listing showed 33 unlocked, Windows Forms GUI displayed the results, and the dark table styling was visible.
- **Not yet confirmed:** latest progress/filter/diagnostics UI revision, fresh extracted release package, installer (not provided).
- No public/stable release is claimed by this changelog.

## Early prototype
- Initial .NET 8 Steamworks.NET CLI, Steam App ID verification, achievement listing and confirmation-protected writing.
