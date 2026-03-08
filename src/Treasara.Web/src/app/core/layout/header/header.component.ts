import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeToggleComponent } from '../../../shared/theme-toggle/theme-toggle.component';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, ThemeToggleComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

  readonly themeService = inject(ThemeService);

  get logoPath(): string {
    return this.themeService.isDark()
      ? 'assets/logo/logo-dark.svg'
      : 'assets/logo/logo-light.svg';
  }

}