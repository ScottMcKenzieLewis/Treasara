import { HttpInterceptorFn } from '@angular/common/http';
import { ulid } from 'ulid';

const CORRELATION_HEADER = 'X-Correlation-Id';

export const correlationIdInterceptor: HttpInterceptorFn = (
  req,
  next
) => {
  const correlationId = req.headers.get(CORRELATION_HEADER) ?? ulid();

  const cloned = req.clone({
    setHeaders: {
      [CORRELATION_HEADER]: correlationId
    }
  });

  return next(cloned);
};