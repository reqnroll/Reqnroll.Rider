Feature: Code generation

Scenario: Should generate step with a title cased method name
	Given the following step text
		"""
		Given the first number
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[Given(@"the first number")]
		public void GivenTheFirstNumber()
		{
		    throw new PendingStepException();
		}
		"""

Scenario: Should generate step with the corrent step attribute
	Given the following step text
		"""
		When the first number
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number")]
		public void WhenTheFirstNumber()
		{
		    throw new PendingStepException();
		}
		"""

Scenario: Should generate steps with integer argument
	Given the following step text
		"""
		When the first number 12
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number (.*)")]
		public void WhenTheFirstNumber(int p0)
		{
		    throw new PendingStepException();
		}
		"""
	
Scenario: Should generate steps with decimal argument
	Given the following step text
		"""
		When the first number 12.12
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number (.*)")]
		public void WhenTheFirstNumber(decimal p0)
		{
		    throw new PendingStepException();
		}
		"""
	
Scenario: Should generate steps with datetime argument
	Given the following step text
		"""
		When the first number 2021-03-30
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number (.*)")]
		public void WhenTheFirstNumber(DateTime p0)
		{
		    throw new PendingStepException();
		}
		"""
	
Scenario: Should generate steps with Table argument
	Given the following step text
		"""
		When this step has a table
		        | header1 |
		        | value   |
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"this step has a table")]
		public void WhenThisStepHasATable(Table table)
		{
		    throw new PendingStepException();
		}
		"""
	
Scenario: Should generate steps with multiline arguments
	Given the following step text
		```
		When this step has a multiline
		"""
		multiline text
		"""
		```
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"this step has a multiline")]
		public void WhenThisStepHasAMultiline(string multilineText)
		{ 
		   throw new PendingStepException();
		}
		"""

Scenario: Should generate steps with integer argument in double quotes as strings
	Given the following step text
		"""
		When the first number "12"
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number ""(.*)""")]
		public void WhenTheFirstNumber(string p0)
		{
		    throw new PendingStepException();
		}
		"""

Scenario: Should generate steps with integer argument in sigle quotes as strings
	Given the following step text
		"""
		When the first number '12'
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number '(.*)'")]
		public void WhenTheFirstNumber(string p0)
		{
		    throw new PendingStepException();
		}
		"""

Scenario: Should generate steps with string argument in double quotes
	Given the following step text
		"""
		When the first number "twelve"
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number ""(.*)""")]
		public void WhenTheFirstNumber(string twelve)
		{
		    throw new PendingStepException();
		}
		"""

Scenario: Should generate steps with string argument in single quotes
	Given the following step text
		"""
		When the first number 'twelve'
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number '(.*)'")]
		public void WhenTheFirstNumber(string twelve)
		{
		    throw new PendingStepException();
		}
		"""

Scenario: Should generate steps with multiple argument
	Given the following step text
		"""
		When the first number "twelve" or "eleven"
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number ""(.*)"" or ""(.*)""")]
		public void WhenTheFirstNumberOr(string twelve, string eleven)
		{
		    throw new PendingStepException();
		}
		"""

Scenario Outline: Should generate steps with outline parameters inlined from the first example
	Given the following step text
		"""
		Scenario Outline: addtion
		When the first number <number>
		Examples: 
		| number |
		| ten    |
		| eleven |
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number ten")]
		public void WhenTheFirstNumberTen()
		{
		    throw new PendingStepException();
		}
		"""

Scenario Outline: Should generate steps with outline parameters inlined from the first example - integer
	Given the following step text
		"""
		Scenario Outline: addtion
		When the first number <number>
		Examples: 
		| number |
		| 10     |
		| ten    |
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number (.*)")]
		public void WhenTheFirstNumber(int p0)
		{
		    throw new PendingStepException();
		}
		"""

Scenario Outline: Should generate steps with missing outline parameters as string
	Given the following step text
		"""
		Scenario Outline: addtion
		When the first number <other>
		Examples: 
		| number |
		| 10     |
		| ten    |
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number (.*)")]
		public void WhenTheFirstNumber(string other)
		{
		    throw new PendingStepException();
		}
		"""

Scenario Outline: Should generate steps with outline parameters in single quoutes
	Given the following step text
		"""
		Scenario Outline: addtion
		When the first number '<number>'
		Examples: 
		| number |
		| twelve |
		| ten    |
		"""
	When the create step action is executed
	Then the following C# snippet should be generated
		"""
		[When(@"the first number '(.*)'")]
		public void WhenTheFirstNumber(string twelve)
		{
		    throw new PendingStepException();
		}
		"""
