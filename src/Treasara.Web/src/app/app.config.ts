import { ApplicationConfig, provideBrowserGlobalErrorListeners,  provideAppInitializer, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { ThemeService } from './core/services/theme.service';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAppInitializer(() => {
      inject(ThemeService).initialize();
    })
  ]
};
