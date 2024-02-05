Feature: Folding - Scenario Outline with examples
Simple calculator for adding two numbers
  
@mytag
Scenario Outline: Add two numbers
Given I have entered <First> in the calculator
And I have entered <Second> into the calculator
When I press add
Then the result should be <Result> on the screen

Examples:
  | First | Second | Result |
  | 50    | 70     | 120    |
  | 30    | 40     | 70     |
  | 60    | 30     | 90     |