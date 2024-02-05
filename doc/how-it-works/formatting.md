
# Prerequisite

The code formatting will not works well if
- A `WHITE_SPACE` token contains `\n` (newline) and ` `  (space at the same time)
- A node start or end with a `WHITE_SPACE` token (except the File node)

So when modifying the `GherkinParser` be careful to preserve this. It's easily done using the method `DoneBeforeWhitespaces` instead of `Done` for the relevant nodes

# Architecture

All the code related to the formatting can be found in the `Fomatting` folders

## `GherkinCodeFormatter`

This class is the one executing the formatting. Is retrieve the settings and then in `Format` it call `DoDeclarativeFormat` which will effectively apply the formatting.

This class also implement the method `GetMinimalSeparatorByNodeTypes` which allow to define a a token level (the output of the Lexer) when some token should stay split using a space or a newline.

## `GherkinFormatSettingsKey`

This class will contains all the configurable options of the formatter, like indentation type/size and all other options we want to add.

## `GherkinFormattingStylePage`

It describe the configuration panel

For this to works there is also a class in the java class `GherkinStyleSettingsProvider`
and an entry in the `plugin.xml`

```xml
<langCodeStyleSettingsProvider implementation="com.jetbrains.rider.plugins.reqnrollriderplugin.settings.GherkinStyleSettingsProvider"/>
```


## `GherkinFormatterInfoProvider`

This contains all the formatting rules.

A rule is defined using the following syntax

```c#
Describe<IndentingRule>()
    .Name(rule.name + "Indent")
    .Where(
        Parent().HasType(rule.parent),
        Node().HasType(rule.child))
    .Return(IndentType.External)
    .Build();
```

- `Describe<****Rule>` define the kind of rule (see below for the details)
- `Where()` allow to define where the rule apply. There multiple types of conditions available, just check the autocomplete for the list. `Satisfies` allow to write custom conditions. /!\ `Node()` and `Left()` represent the same things here. By conventions:
    - `Node` is meant to be used with `Parent`
    - `Left` is meant to be used with `Right`
- `Return` choose what to do when this rule match. The returns type depends on the rule (see below)
- `Swith` allow to define a condition if the rule should be apply based on the formatting settings from `GherkinFormatSettingsKey`
- `Group` this is used, for example, when 2 rules want to modify the same things, like add a newline. When rules are grouped the one with the highest `.Priority()` will win

### Configuration

It's possible to configure a rule depending on the settings with a code like


`.Switch(key => key.SOME_BOOLEAN,
    When(true).Return(IntervalFormatType.NewLine),
    When(false).Return(IntervalFormatType.None)
)`


Node:
- `.Switch` can be chained if we have a rule that need to be configured using multiple settings.
- `When()` arguments are a list of valid values for the match
- To configure indent size we need multiple `When()` for each size (see `ContinuousIndentOptions`)

### `IndentingRule`

This rules will ident the node

`Returns` should returns a value from the enum `IndentType`. Mostly `External` or `Internal`

- `External` meant that the `Node()` should be indented
- `Internal` meant that the children of the `Node` should be indented

### `BlankLinesRule`

This rules allow to add blank lines between statement

The `Returns` to use here is the following 

```c#
Returns(int minBlankLines, int maxBlankLinesMild, int maxBlankLinesStrict, bool inherit)
```
- `minBlankLines` is the minimum blanks line required. If there is less that this number of blank lines
- `maxBlankLinesMild`  FIXME
- `maxBlankLinesStrict` is the absolute maximum number of lines allowed here
- `inherit` is unused

To configure this rules we can use

`.SwitchBlankLines(key => key.NumberOfBlankLineBetweenScenarios, false, BlankLineLimitKind.LimitMinimum)` 

### `FormatingRule`

This rules allow to configure NewLine (not blank lines) if a line should be inserted or removed.
This rules also allow to configure how a line should be wrap using the values
- `PlaceToWrap`
- `GoodPlaceToWrap`
- `ExcellentPlaceToWrap`
- `NoWrap`

`Returns` should use a value from the enum `IntervalFormatType`

It can also be configured using `Switch` like the following example

```
.Switch(key => key.SOME_BOOLEAN, When(true).Return(IntervalFormatType.NewLine), When(false).Return(IntervalFormatType.None))
```


### `IntAlignRule`

/!\ This rule is not executed if the parameter `shouldDoIntAlign` is not `true` in `DoDeclarativeFormat`

This rules is used to aligned element internally, like the Gherkin tables

the `Calculate` method returns for each node an `IntAlignOptionValue` and the first parameter is an `id`, it will then aligned all the element with the same `id`

```
Describe<IntAlignRule>()
    .Name("AlignTableCells")
    .Where(
        Node().HasType(GherkinNodeTypes.TABLE_CELL))
    .Calculate((o, context) => new IntAlignOptionValue("pipe", 1))
    .Build();
```

### `WrapRule`

This rules is used to define if the line need to be wrapped.

`Returns` should use a value from the enum `WrapType`

# Tests

The tests can be found inside `test/data/Formatting` in the test project.

The `.feature` is the source file and the `.gold` is the expected result after the formatting
Tests are listed inside `GherkinCodeFormatterTest`

To configure the formatter for a given test, it's possible to add a `.jcnf` files following this syntax:

```
{
SomeKey: 2,
SomeOtherKey: true
}
```

`.jcnf` files can also allow to enable debug using `MakeLog:true` so the formatter will write logs inside a file in the same directory.

Another debug setting is `DumpRulesForInterval`. `Left` and `Right` are used to compare with the file context when a Node is ending with `Left` and the next is starting with the `Right` part.

```
{
MakeLog: true,
DumpRulesForInterval: { Left: "...", Right: "..." }
}
```

# Debug

To debug the rules you can also try to attach to the Resharper process and then use the following piece of code and add a breakpoint to find what `Where` to write for a rule.

```c#
.Where(
  Left().Satisfies((node, context) =>
    {
      Console.WriteLine(node);
      return true;
    }),
  Right().Satisfies((node, context) =>
    {
      Console.WriteLine(node);
      return false;
    }))
```
