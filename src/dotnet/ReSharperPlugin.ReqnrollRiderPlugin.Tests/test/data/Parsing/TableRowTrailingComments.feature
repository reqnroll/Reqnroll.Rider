Feature: Data Table row with trailing comments

Scenario: Data Table example
    Then the data should be
    | Id | Value   |
    | 1  | Charlie |
    | 2  | Bob     |

Scenario: Data Table can have a comment trailing a row
    Then the data should be
    | Id | Value   |
    | 1  | Charlie | # Initial Value
    | 2  | Bob     |

Scenario: Data Table can have a comment trailing its definition with a Note header
    Then the data should be
    | Id | Value   | Notes
    | 1  | Charlie | Initial Value
    | 2  | Bob     |

Scenario: Data Table can have a comment in the header
    Then the data should be
    | Id | Value   | # Notes
    | 1  | Charlie | # Initial Value
    | 2  | Bob     |
    
Scenario: Data Table can have a comment between rows
    Then the data should be
      | Id | Value   | # Notes
      | 1  | Charlie | # Initial Value
      # Here's another row
      | 2  | Bob     |