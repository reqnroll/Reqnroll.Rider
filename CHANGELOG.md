# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## 1.3.4
### Fixed
- Correctly detect steps from assemblies (#14)
- Improve how tests are detected for NUnit / XUnit to display run test icon in the gutter

## 1.3.3
### Fixed
- Fix reference / find usage for step inside `Scenario Outline`
- Fix _Create Step_ for step inside `Scenario Outline`

## 1.3.2
### Fixed
- Detect step definition using `[StepDefinition]`
- Detect step definitions from NuGet / Assemblies

## 1.3.1
### Fixed
- "Run test" button was not displayed in gutter for XUnit test
- "Create step" quick fix now handle partial class correctly and file with multiple classes.

## 1.3.0
### Added
- Add buttons to run tests in gutter of specflow files

## 1.2.3
### Fixed
- Step definitions were not found when `using` were inside namsepace
- Step definitions were not found when they were inside a `partial class`

## 1.2.2
### Fixed
- Cache was not built in some scenario the first time the project is open with the plugin.
- The cache process was crashing when using specflow with other locales
- Fix a bug when specialized locales was used and not found in locales list

## 1.2.0
### Added
- Add highlighting: Warning when method name does not match with the pattern in the attribute.
- Add quickfix to rename method when name does not match the pattern. 

## 1.1.1
### Fixed
- Errors were disappearing from _Error in solution_ window

## 1.1.0
### Added
- Find usage on a step definition now list all usage of a step and let you navigate to it.
### Fixed
- Fix bug: The quick fix to create step was proposing to create the new step in some files not accessible by the current project

## 1.0.0
### Added
- Initial version
- Syntax highlight
- Go to declaration
- Quick-fix create missing steps
