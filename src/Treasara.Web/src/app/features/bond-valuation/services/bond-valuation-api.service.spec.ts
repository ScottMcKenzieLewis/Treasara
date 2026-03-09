import { TestBed } from '@angular/core/testing';

import { BondValuationApiService } from './bond-valuation-api.service';

describe('BondValuationApiService', () => {
  let service: BondValuationApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BondValuationApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
