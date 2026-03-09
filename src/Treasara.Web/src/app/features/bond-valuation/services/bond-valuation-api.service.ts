import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { BondValuationRequest } from '../models/bond-valuation-request.model';
import { BondValuationResponse } from '../models/bond-valuation-response.model';

@Injectable({
  providedIn: 'root'
})
export class BondValuationApiService {
  private readonly api = inject(ApiService);

  valueBond(
    request: BondValuationRequest
  ): Observable<BondValuationResponse> {
    return this.api.post<BondValuationRequest, BondValuationResponse>(
      '/api/v1/bonds/value',
      request
    );
  }
}