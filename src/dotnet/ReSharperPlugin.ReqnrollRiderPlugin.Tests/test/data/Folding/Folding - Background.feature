Feature: Folding - Background
Simple calculator for adding two numbers
  
Background:
Given I have entered 50 into the calculator
  
@mytag
Scenario: Add two numbers
Given I have entered 70 into the calculator
When I press add
Then the result should be 120 on the screen