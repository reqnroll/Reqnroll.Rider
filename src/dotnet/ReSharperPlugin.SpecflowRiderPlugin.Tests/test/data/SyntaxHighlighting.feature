
Feature: Syntax highlighting

  Description of the syntax highlighting feature. The default colors are based on the JetBrains Rider "Classic Light" color scheme.

  Scenario: Feature keyword should be highlighted as keyword (default: blue)

  Scenario: Scenario keyword should be highlighted as keyword (default: blue)

  Scenario Outline: Scenario Outline keyword should be highlighted as keyword (default: blue)
    
  Scenario: Comments should be highlighted as comments (default:green)
    #this is a comment
 
  Scenario: Rule keyword should be highlighted as keyword (default: blue)
    Rule: This is a rule

  Scenario: Gherkin step keywords should be highlighted as keyword (default: blue)
    Given this is a Given step
    When this is a When step
    Then this is a Then step
    And this is an And step
    But this is a But step
      * this a step

  Scenario: Tags should be highlighted as tag (default: purple)
    @ThisIsATag

  Scenario Outline: Scenario Outline parameters should be highlighted (default: light blue)
    Given this is a step with <parameter>

  Scenario: Example keyword should be highlighted as keyword (default: blue)
    Examples: This is an example

  Scenario: Table pipes should be highlighted as keyword (default: blue)
    Examples:
      | Header1 | Header2 | Header3 |
      | 1       | 2       | 3       |

  Scenario: Table headers should be highlighted as header (default: light blue)
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

  Scenario: Only the keywords that match the specified feature file language should be highlighted (default: English)
    The feature file language can be specified in 2 ways:
    - In the Gherkin feature file with the language directive
    - In the specflow.jon file. e.g:
         {
          "language": {
          "feature": "de-AT"
         }
         }
      
  Szenariogrundriss: German keywords should not be highlighed (default: English)
    Angenommen this is a given step
    Wenn this is a when step
    Dann this is a then step
    Und this is an and step
    Aber this is a but step      
