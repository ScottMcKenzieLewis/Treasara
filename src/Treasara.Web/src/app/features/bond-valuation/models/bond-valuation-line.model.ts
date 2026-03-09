export interface BondValuationLine {
  paymentDate: string;
  cashflowAmount: number;
  discountFactor: number;
  presentValue: number;
  currency: string;
}