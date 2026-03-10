import { ApplicationConfig, provideBrowserGlobalErrorListeners,  provideAppInitializer, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ThemeService } from './core/services/theme.service';
import { correlationIdInterceptor } from './core/http/correlation-id.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([correlationIdInterceptor])
    ),
    provideAppInitializer(() => {
      inject(ThemeService).initialize();
    })
  ]
};
