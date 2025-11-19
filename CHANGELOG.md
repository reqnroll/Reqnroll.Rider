# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## 2025.3.1
- Fix step completion not always working as expected after a space or at the end of a line

## 2025.3.0
- Support for Rider 2025.3

## 2025.1.2
- Fix step completion only completing one word instead of the full sentence

## 2025.1.0
- Support for Rider 2025.1

## 2024.3.3

- Fix invalid error being reported on `Rule:` preceded by a tag. Fix #31
- Fix Typing Assists was not initialized. Fix #40
- Fix invalid error being reported when no text was present after `Background:`. Fix #43

## 2024.3.2

- Fix errors when parsing steps during the cache step
- Fix code injection. Do not include the indentation in the injected context. This fix error when injecting XML.


## 2024.3.1

- Add support to cucumber expressions
- Fix missing translation for Rule in Dutch
- Fix error when using Specflow

## 2024.3.0

- Support of Rider 2024.3

## 2024.2.2

- Fix error related to default setting provider

## 2024.2.1

- Fix completion not showing up in `.feature`

## 2024.1.4

- Fix gutter mark in `.feature` when using SpecFlow

## 2024.1.3

- Also read config from `specflow.json` 

## 2024.1.2

- Fix broken file templates #6
- Mark Specflow as incompatible to fix potential issue at startup
- Add reference of nuget that should trigger this plugin suggestion in plugin.xml

## 2024.1.0

- Support for Reqnroll projects.
- Support for SpecFlow code snippet generation is dropped. (Can be re-enabled later if necessary.)
- Initial release based on v1.23.6 of the [SpecFlow for Rider](https://github.com/SpecFlowOSS/SpecFlow.Rider) extension.
