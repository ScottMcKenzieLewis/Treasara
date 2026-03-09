import { BondValuationLine } from './bond-valuation-line.model';

export interface BondValuationResponse {
  instrumentType: string;
  totalPresentValue: number;
  currency: string;
  lines: BondValuationLine[];
}