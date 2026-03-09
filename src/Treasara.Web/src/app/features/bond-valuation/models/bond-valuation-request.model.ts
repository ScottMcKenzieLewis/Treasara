export interface BondValuationRequest {
  notional: number;
  currency: string;
  couponRate: number;
  issueDate: string;
  maturityDate: string;
  valuationDate: string;
  frequency: string;
  dayCount: string;
  rollConvention: string;
  curveRate: number;
}