```text
## 1. Test Results

- Total tests: 23
- Passed: 23
- Failed: 0
- Skipped: 0



2-2. Code Coverage

Based on coverage-report/Summary.txt:

Simulation.Checkout.Application: 95.8% line coverage

Simulation.Checkout.Domain: 100% line coverage



Line coverage
90%
Covered lines:	106
Uncovered lines:	11
Coverable lines:	117
Total lines:	228
Line coverage:	90.5%



3
Moq: Used to mock the inventory, order repository and notification services and verify their interactions

AutoFixture: Used to generate test orders with customized positive quantities and non-negative prices

Bogus: Used to generate realistic customer names and email addresses using a fixed seed


4
No, it is unfair that a customer with a subtotal of 999.99 pays more than a customer with a subtotal of 1,000 because the discount threshold creates a sudden 100 EGP price reduction

but the E-commerce websites i dealed with before do the same


5- No bugs

but Potential issue If the order is saved successfully but sending the confirmation fails
 CheckoutAsync throws an exception even though the order has already been saved which conflicts with BR-09
```

