import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

type QueryParamValue = string | number | boolean | null | undefined;

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly http = inject(HttpClient);

  // Later, move this to environment.apiBaseUrl when the API is deployed.
  private readonly baseUrl = environment.apiBaseUrl;

  get<T>(
    path: string,
    options?: {
      params?: Record<string, QueryParamValue>;
      headers?: HttpHeaders | Record<string, string | string[]>;
    }
  ): Observable<T> {
    return this.http.get<T>(this.buildUrl(path), {
      params: this.buildParams(options?.params),
      headers: options?.headers
    });
  }

  post<TRequest, TResponse>(
    path: string,
    body: TRequest,
    options?: {
      params?: Record<string, QueryParamValue>;
      headers?: HttpHeaders | Record<string, string | string[]>;
    }
  ): Observable<TResponse> {
    return this.http.post<TResponse>(this.buildUrl(path), body, {
      params: this.buildParams(options?.params),
      headers: options?.headers
    });
  }

  private buildUrl(path: string): string {
    const trimmedBase = this.baseUrl.replace(/\/+$/, '');
    const trimmedPath = path.replace(/^\/+/, '');

    return `${trimmedBase}/${trimmedPath}`;
  }

  private buildParams(
    values?: Record<string, QueryParamValue>
  ): HttpParams | undefined {
    if (!values) {
      return undefined;
    }

    let params = new HttpParams();

    for (const [key, value] of Object.entries(values)) {
      if (value !== null && value !== undefined) {
        params = params.set(key, String(value));
      }
    }

    return params;
  }
}
