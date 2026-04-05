Feature: Cucumber stock keeping

  Scenario: eating
    Given something

  Scenario Outline: eating
    Given there are <start> cucumbers

    Examples: Good
      | start |
      | 12 23 |
      | 20    |

  Rule:

    Scenario:
      Given something