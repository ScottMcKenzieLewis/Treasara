import { Component, HostListener, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeToggleComponent } from '../../../shared/theme-toggle/theme-toggle.component';
import { ThemeService } from '../../services/theme.service';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, 
    ThemeToggleComponent,
    MatMenuModule,
    MatButtonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

  readonly themeService = inject(ThemeService);

  readonly bondsMenuOpen = signal(false);

  get logoPath(): string {
    return this.themeService.isDark()
      ? 'assets/logo/logo-dark.svg'
      : 'assets/logo/logo-light.svg';
  }

}