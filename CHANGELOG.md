# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## 1.23.7
- Fix matching feature with tests

## 1.23.6
- Fix matching feature with tests when path contains '.' or project is inside a solution folder

## 1.23.5
- Fix matching feature with tests when path contains '+'
- Fix error when loading steps from external assembly (#208)

## 1.23.4
- Fix issue when resolving test associated to a scenario

## 1.23.3
- Fix issue when resolving step. The cache was depending on another cache not ready yet.

## 1.23.2
- Update to 2023.1 RC1

## 1.23.1
- Fix parsing of step text that can lead to unrecognized steps

## 1.23.0
- Fix invalid association between scenario and unit test when 2 feature file are name the same in different folders (#191)
- Fix comment indentation (#185)

## 1.22.0
### Fixed
- Fix matching step in Scenario Outline when the last part is a parameter and the value of the parameter is empty Fix (#194)
- Fix icon and text in Show usage popup (#105)

## 1.21.0
### Added
- Add a new option to format tags. One tag / line or all the tag on the same line.
- Add support for scope
### Fixed
- Invalid step resolution when 2 steps had the same name
- Fix code preview in Code Style configuration

## 1.20.0
### Added
- Support for language injection on pystring (Json / XML syntax color)
- Rework completion logic to not only use the last word of the step

## 1.19.1
### Fixed
- Fix spellcheck

## 1.19.0
### Added
- Support for Rider 2023.3-EAP7-SNAPSHOT

## 1.18.0
### Added
- Support for Rider 2023.2.3

## 1.17.0
### Added
- Support for Rider 2023.2-EAP6-SNAPSHOT

## 1.16.0
### Added
- Support for Rider 2023.1

## 1.15.2
### Added
- Support for Rider 2022.3

## 1.15.1
### Added
- Support for Rider 2022.3 eap 7

## 1.15.0
### Added
- Support for Rider 2022.3

## 1.14.2
### Added
- Hide live template and other completion items added by other plugin when completing steps
- Add support for backticks in Doc strings (multiline string) [#75](https://github.com/reqnroll/Reqnroll.Rider/issues/75)
(- When reqnroll.json is not available, use config from App.config  [#162](https://github.com/reqnroll/Reqnroll.Rider/issues/162)
- Disable code completion in case of adding spaces before a step. [#147](https://github.com/reqnroll/Reqnroll.Rider/issues/147)
- Do not show unresolved step warning for ignored scenarios. [#137](https://github.com/reqnroll/Reqnroll.Rider/issues/137)

### Fixed
- Fix invalid step resolution when using method name and parameter is a integer

## 1.14.1
### Added
- Enable spell check in .feature [#153](https://github.com/reqnroll/Reqnroll.Rider/issues/153)

### Fixed
- Fix  `"""` in multiline text
- Fix parsing step starting by `@` [#176](https://github.com/reqnroll/Reqnroll.Rider/issues/176)
- Fix null reference [#155](https://github.com/reqnroll/Reqnroll.Rider/issues/155)

## 1.14
### Added
- Support for Rider 2022.2

## 1.13
### Fixed
- fixes https://github.com/reqnroll/Reqnroll.Rider/issues/174

## 1.12
### Added
- Support for Rider 2022.1

## 1.11
### Added
- Support for Rider 2022.1 eap 6

## 1.10.2
### Added
- Support for Rider 2021.3.2
- Add code completion for tags

## 1.10.1
### Added
- Support for Rider 2021.3 eap 7

## 1.10.0
### Added
- Support for Rider 2021.3 eap

## 1.9.4
### Added
- Support for Rider 2021.2 final

## 1.9.3
### Added
- Support for Rider 2021.9 EAP build 212.4746.97
- Update the project template to the latest Reqnroll version

### Fixed
- "Where to create the step?" UI isn't consistent [#89](https://github.com/reqnroll/Reqnroll.Rider/issues/89)

## 1.9.2
### Added
- Support for Rider 2021.7 EAP build 212.4746.33
- Support the new "Reformat and Clenaup" feature
- Use the native notification popup for the on boarding messages

### Fixed
- Keyword completion is case sensitive by default [#148](https://github.com/reqnroll/Reqnroll.Rider/issues/148)

## 1.9.1
### Added
- Support for Rider 2021.2 EAP build 212.4416.21

## 1.9.0
### Added
- Support for Rider 2021.2 EAP build 212.4037.61

## 1.8.0

### Added
- Further enhance the gherkin code inspections [#98](https://github.com/reqnroll/Reqnroll.Rider/issues/98)

### Fixed
- Fix smart indentation when adding newline after a table [#87](https://github.com/reqnroll/Reqnroll.Rider/issues/87)
- Сode completion shows irrelevant results in feature files [#86](https://github.com/reqnroll/Reqnroll.Rider/issues/86)
- Don't show the test gutter marks when the scenario does not have a title [#136](https://github.com/reqnroll/Reqnroll.Rider/pull/138)
- Fix exceptions when no or an invalid language is provided [#138](https://github.com/reqnroll/Reqnroll.Rider/pull/138)
- Fix NullReferenceException when the createNewFileTarget cannot create the file [#135](https://github.com/reqnroll/Reqnroll.Rider/pull/135)
- Make the create step quick fix bulb yellow [#90](https://github.com/reqnroll/Reqnroll.Rider/pull/90)
- Fix exception when adding tag after the last row in the feature file [#126](https://github.com/reqnroll/Reqnroll.Rider/pull/126)
- Fix folding when adding tags before a Background [#125](https://github.com/reqnroll/Reqnroll.Rider/pull/125)

## 1.7.0

### Added
- After running tests, if a step failed it will be marked with an icon in the gutter. For this to works, you may need to enable `traceTimings`
- Allow to create a new binding class from the Create Step quickfix
- Format table while typing `|` character
- Syntax analysis, highlight some syntax errors
- Add run tests action in the feature file context menu [#122](https://github.com/reqnroll/Reqnroll.Rider/pull/122)

### Fixed
- Fix smart indentation when adding newline after a table [#87](https://github.com/reqnroll/Reqnroll.Rider/issues/87)
- Fix crash in unresolved step daemon [#100](https://github.com/reqnroll/Reqnroll.Rider/issues/100)
- `.feature.cs` is not nested after creation [#99](https://github.com/reqnroll/Reqnroll.Rider/issues/99)
- Arguments are not noticeable [#118](https://github.com/reqnroll/Reqnroll.Rider/pull/118)
- Сode completion shows irrelevant results in feature files [#86](https://github.com/reqnroll/Reqnroll.Rider/issues/86)
- Fix the "phantom" errors in Xunit projects [#115](https://github.com/reqnroll/Reqnroll.Rider/pull/115)
- Fix the test gutter mark for the old style csprojects [#116](https://github.com/reqnroll/Reqnroll.Rider/pull/116)
- Make Reqnroll project template consistent with other templates [#121](https://github.com/reqnroll/Reqnroll.Rider/pull/121)

## 1.6.0

### Added
- Add project template for Reqnroll
- Add item templates for Reqnroll artifacts (feature files, step definition files, etc.)

## 1.5.1

### Fixed
- Fix "Extend Selection" in 2021.1

## 1.5.0

### Added
- Support for Rider 2021.1

### Fixed
- Create step quickfix should generate same code as Reqnroll or the Visual Studio extension.
- Show onboarding information after opening a Reqnroll solution [#66](https://github.com/reqnroll/Reqnroll.Rider/issues/66)


## 1.4.0
### Added 
- Add autocompletion for Gherkin keywords [#46](https://github.com/reqnroll/Reqnroll.Rider/issues/49)
- Add support for Extend Selection in feature files [#56](https://github.com/reqnroll/Reqnroll.Rider/issues/56)
- Ability to fold Feature/Scenario/Background [#15](https://github.com/reqnroll/Reqnroll.Rider/issues/15)
- Add support for placing breakpoints in feature files [#2](https://github.com/reqnroll/Reqnroll.Rider/issues/2)

### Fixed
- Should be able to Configure Inspection of "Undefined step" [#62](https://github.com/reqnroll/Reqnroll.Rider/issues/62)
- Add separate indentation rule for 'And' steps [#52](https://github.com/reqnroll/Reqnroll.Rider/issues/52)

## 1.3.7
### Added
- "Comment Line" action now works [#20](https://github.com/reqnroll/Reqnroll.Rider/issues/20)
- Add syntax highlighting for step arguments [#30](https://github.com/reqnroll/Reqnroll.Rider/issues/30)
- Support for formatting in feature files [#10](https://github.com/reqnroll/Reqnroll.Rider/issues/10) and [#31](https://github.com/reqnroll/Reqnroll.Rider/issues/31)
- Add autocompletion for steps [#36](https://github.com/reqnroll/Reqnroll.Rider/pull/36)
### Fixed
- Fix step navigation for non english feature files [#27](https://github.com/reqnroll/Reqnroll.Rider/issues/27)
- Remove unused actions in 'Go to' menu [#5](https://github.com/reqnroll/Reqnroll.Rider/issues/5)

## 1.3.6
### Fixed
- Fix: Create steps when pattern contains a $ with a number like `Given someone pays $50`
- Support `But` [#26](https://github.com/reqnroll/Reqnroll.Rider/issues/26) and `*`[#25](https://github.com/reqnroll/Reqnroll.Rider/issues/25) step keyword
- When resolving reference from a step to its definition, and the step use example parameters, replace the text with the first example [#24](https://github.com/reqnroll/Reqnroll.Rider/issues/24)

## 1.3.5
### Fixed
- Fix: Check reference for steps inside `Rule:`
- Fix: Escape step before using them as pattern when creating a step with the quick fix.
- Fix: Keep special character in pattern when creating a step with the quick fix

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
- Add buttons to run tests in gutter of reqnroll files

## 1.2.3
### Fixed
- Step definitions were not found when `using` were inside namsepace
- Step definitions were not found when they were inside a `partial class`

## 1.2.2
### Fixed
- Cache was not built in some scenario the first time the project is open with the plugin.
- The cache process was crashing when using reqnroll with other locales
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
