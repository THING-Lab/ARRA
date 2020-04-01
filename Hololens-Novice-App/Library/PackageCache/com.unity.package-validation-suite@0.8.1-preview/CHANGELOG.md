# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.8.1] - 2020-02-20
- Whitelisted bee.dll, pram.exe, tundra2.exe. Required for incremental build pipeline used in com.unity.platforms
- Added information line to API Validation to know which version was used for comparison
- Fixed validate button not appearing when a package was added through a git URL or a local tarball path

## [0.8.0] - 2020-02-04
- Added error to fail validation if a package has unity field 2020.2 until the new Package Lifecycle is ready
- Added error when UNRELEASED section is present in a package CHANGELOG.md during promotion
- Added warning when UNRELEASED section is present in a package CHANGELOG.md during CI/publish to candidates
- Changed display name validation to allow up to 50 characters instead of 25
- Changed path length validation to ignore hidden directories
- Fixed documentation generation errors (missing index.md file)

## [0.7.15] - 2020-01-22
- Added Validation Suite version to the validation suite text report
- Added support of <inheritdoc/> tag in missing docs validation.
- Fixed issue with API Validation not finding some compiled assemblies inside a package

## [0.7.14] - 2020-01-03
- Whitelisting ILSpy.exe

## [0.7.13] - 2019-12-16
- Whitelisting Unity.ProcessServer.exe

## [0.7.12] - 2019-12-09
- Fix Assembly Validation to better distinguish test assemblies.

## [0.7.11] - 2019-11-28
- Made changes to allow running Validation Tests from other packages.
- Bug fixes in License Validation.
- Bug fixes in UpdaterConfiguration Validation.

## [0.7.10] - 2019-11-01
- Fix an issue with the restricted file validation

## [0.7.9] - 2019-10-31
- Happy Halloween!!
- Relaxed the API validation rules in preview
- Added a more restrictive forbidden files list.

## [0.7.8] - 2019-10-17
- Removed Dependency Validation check
- Added "com.ptc" as a supported package name domain.

## [0.7.7] - 2019-09-20
- Added whitelist for HavokVisualDebugger.exe

## [0.7.6] - 2019-09-19
- Fix bug preventing the Validation Suite from properly running against latest version of Unity.

## [0.7.5] - 2019-09-18
- Fixed issue causing built-in packages validation to fail in Unity versions < 2019.2

## [0.7.4] - 2019-09-16
- Disable semver validation upon api breaking changes on Unity 2018.X
- Allow console error whitelisting for API Updater Validation.

## [0.7.3] - 2019-09-10
- Removed Dependency Validation test to prevent asking for major version changes when adding or removing dependencies
- Fixed issues with scope of references used in APIValidation Assembly

## [0.7.2] - 2019-08-27
- Add support for 2018.3 (without the package manager UI integration).

## [0.7.1] - 2019-08-23
- Modified the test output structure to differentiate info, warning and error output.
- Added validation test to check for the existing of the "Resources" directory in packages, which is not recommended.
- Modified Packman UI integration to turn yellow upon warnings in a run.
- Fixed preview package fetch, to allow API evaluation testing, as well as diff generation.

## [0.6.2] - 2019-07-15
- Allows validation suite to be used by develop package
- Moved validation suite output to Library path

## [0.6.1] - 2019-07-15
- Changed maximum file path length validation to be 140 characters instead of 100.
- Changed Dependency Validation to issue a Warning instead of an error when package's Major version conflicts with verified set.
- Added exception handling in BuildTestSuite when calling assembly.GetTypes()
- Fixed path length validation to handle absolute/relative paths correctly

## [0.6.0] - 2019-07-11
- Added Maximum Path Length validation to raise an error if file paths in a package are becoming too long, risking Windows long path issues to appear.
- Fixed another issue in UpdateConfiguration validation causing some false-positives in DOTS packages.

## [0.5.2] - 2019-05-17
- removing validations involving where tests should be found.  They can now be anywhere.

## [0.5.1] - 2019-05-17
- Patched an issue in the UpdateConfiguration validation

## [0.5.0] - 2019-05-15
- Added XML Documentation validation
- Added ApiScraper exception to RestrictedFilesValidation
- Changed outdated documentation

## [0.4.0] - 2019-04-03
- Properly handle dependencies on built-in packages, which aren't in the production registry.
- Fix unit tests
- Added support for local validation of packages with unpublished dependencies.
- Add new public API to test all embedded packages.
- Validate that package dependencies won't cause major conflicts
- Validate that package has a minimum set of tests.
- Fix the fact that validation suite will pollute the project.
- Add project template support
- Hide npm pop-ups on Windows.
- Fix validation suite freeze issues when used offline
- Add validation to check repository information in `package.json`
- Validate that both preview and verified packages have their required documentation.
- Refactor unit tests to use parametrized arguments.
- Support UI Element out of experimental
- Added support for validating packages' local dependencies during Local Development
- Removed ProjectTemplateValidation test
- Add validation to check that API Updater configurations are not added outside major releases.
- Add unit tests to Unity Version Validation
- Fixing bug PAI-637 : searches for word "test" in path and finds it in file name rather than searching only folder names.

## [0.3.0] - 2018-06-05
- Hide validation suite when packages are not available
- Accept versions with and without  pre-release tag in changelog
- Fix 'View Results' button to show up after validation
- Shorten assembly definition log by shortening the path
- Fix validation of Assembly Definition file to accept 'Editor' platform type.
- Fix npm launch in paths with spaces
- Fix validation suite UI to show up after new installation.
- Fix validation suite to support `documentation` folder containing the special characters `.` or `~`
- Fix validation suite display in built-in packages
- Add tests for SemVer rules defined in [Semantic Versioning in Packages](https://confluence.hq.unity3d.com/display/PAK/Semantic+Versioning+in+Packages)
- Add minimal documentation.
- Enable API Validation
- Clarify the log message created when the old binaries are not present on Artifactory
- Fix build on 2018.1

## [0.1.0] - 2017-12-20
### This is the first release of *Unity Package Validation Suite*.
