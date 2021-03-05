#The default colors are based on the "Rider Light" color scheme
Feature: Syntax highlighting
  This is the description of the feature

  Scenario: Feature keyword should be highlighted as keyword (default: blue)

  Scenario: Scenarios should be highlighted as keyword (default: blue)

  Scenario Outline: Scenario outlines should be highlighted as keyword (default: blue)
    
#this is a comment
  Scenario: Comments are highlighted as comments (default:green)

  Rule: This is a rule
    Scenario: Rule keyword should be highlighted as keyword (default: blue)

    Scenario: Step keywords should be highlighted as keyword (default: blue)
      Given this is a given step
      When this is a when step
      Then this is a then step
      And this is an and step
      But this is a but step
      * this a step

    @ThisIsATag
    Scenario: Tags should be highlighted as tag (default: purple)

    Scenario Outline: Scenario outline parameters should be highlighted
      Given this is a step with <parameter>

    Scenario: Example keyword should be highlighted as keyword (default: blue)
      Examples: This is an example

    Scenario: Table header should be highlighted as header (default: light blue)
      Examples:
        | Header1 | Header2 | Header3 |
        | 1       | 2       | 3       |

    Scenario: Table pipe should be highlighted as keyword (default: blue)
      Examples:
        | Header1 | Header2 | Header3 |
        | 1       | 2       | 3       |

    Scenario: Table content should be highlighted as parameter (default: grey)
      Examples:
        | Header1 | Header2 | Header3 |
        | 1       | 2       | 3       |

    Scenario: Parameter should be highlighted based on the regexp pattern
      Given this a step with a string "param1"

    Scenario: Parameter should not be highlighted if there is no matching regexp pattern
      Given this another step with a string "param1"

    Scenario: Only the keywords that match the specified language (English) should be highlighed
    The default language is the English.
    The language can be specified in 2 ways:
    - In the gherkin file with the language directive
    - In the specflow.jon file. e.g:
    {
    "language": {
    "feature": "de-AT"
    }
    }
      Given this is a given step
      When this is a when step
      Then this is a then step
      And this is an and step
      But this is a but step

    Szenariogrundriss: German keywords should not be highlighed (default language: English)
    Angenommen this is a given step
    Wenn this is a when step
    Dann this is a then step
    Und this is an and step
    Aber this is a but step      