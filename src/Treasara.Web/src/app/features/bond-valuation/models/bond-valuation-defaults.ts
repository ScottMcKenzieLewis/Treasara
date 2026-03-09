import { BondValuationRequest } from './bond-valuation-request.model';

export const DEFAULT_BOND_VALUATION_REQUEST: BondValuationRequest = {
  notional: 1_000_000,
  currency: 'USD',
  couponRate: 0.05,
  issueDate: '2024-01-01',
  maturityDate: '2034-01-01',
  valuationDate: '2026-03-07',
  frequency: 'SEMIANNUAL',
  dayCount: 'THIRTY360',
  rollConvention: 'NOROLL',
  curveRate: 0.045
};