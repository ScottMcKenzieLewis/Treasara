export const BOND_VALUATION_TOOLTIPS = {

  notional:
`Notional

Principal amount of the bond.

Coupon payments and the final repayment
at maturity are calculated from this value.`,

  currency:
`Currency

The currency used for the valuation request.

All calculated present values and
cash flows will be returned in this currency.`,

  couponRate:
`Coupon Rate

Annual interest rate paid by the bond.

Example:
0.05 = 5% annual coupon.`,

  curveRate:
`Curve Rate

Flat discount rate used to convert future
cash flows into present value.

Higher rates reduce present value.`,

  issueDate:
`Issue Date

Start date of the bond.

The coupon schedule is generated
beginning from this date.`,

  maturityDate:
`Maturity Date

Final payment date of the bond.

The last coupon and principal
repayment occur on this date.`,

  valuationDate:
`Valuation Date

Date the bond is priced.

Cash flows before this date
are excluded from valuation.`,

  frequency:
`Frequency

How often coupons are paid.

Examples:
Annual
Semiannual
Quarterly`,

  dayCount:
`Day Count Convention

Rule used to convert time between dates
into year fractions for interest accrual
and discounting.`,

  rollConvention:
`Roll Convention

Determines how payment dates are adjusted
when they fall on weekends or holidays.`

} as const;