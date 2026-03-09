export interface ApiErrorDetail {
  field?: string;
  message: string;
}

export interface ApiError {
  statusCode?: number;
  message: string;
  details?: ApiErrorDetail[];
  traceId?: string;
}
