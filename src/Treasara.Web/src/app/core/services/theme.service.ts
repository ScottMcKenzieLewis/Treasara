import { Injectable, signal } from '@angular/core';

export type AppTheme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  readonly theme = signal<AppTheme>('light');

  initialize(): void {
    const saved = localStorage.getItem('treasara-theme') as AppTheme | null;

    if (saved === 'light' || saved === 'dark') {
      this.setTheme(saved);
      return;
    }

    const prefersDark = globalThis.matchMedia('(prefers-color-scheme: dark)').matches;
    this.setTheme(prefersDark ? 'dark' : 'light');
  }

  toggle(): void {
    this.setTheme(this.isDark() ? 'light' : 'dark');
  }

  isDark(): boolean {
    return this.theme() === 'dark';
  }

  setTheme(theme: AppTheme): void {
    this.theme.set(theme);
    document.documentElement.classList.remove('light-theme', 'dark-theme');
    document.documentElement.classList.add(`${theme}-theme`);
    localStorage.setItem('treasara-theme', theme);
  }
}