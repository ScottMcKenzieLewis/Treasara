import { ApiError } from "../models/api-error.model";

export function mapApiError(error: unknown): ApiError {
  const err = error as any;

  if (!err) {
    return { message: 'Unknown error occurred.' };
  }

  if (err.status === 0) {
    return {
      statusCode: 0,
      message: 'Unable to reach the server. Please check your connection.'
    };
  }

  const payload = err.error;

  return {
    statusCode: err.status,
    message:
      payload?.message ??
      payload?.Message ??
      payload?.title ??
      payload?.Title ??
      (typeof payload === 'string' ? payload : null) ??
      'An unexpected server error occurred.',
    details: payload?.details ?? payload?.Details,
    traceId: payload?.traceId ?? payload?.TraceId
  };
}