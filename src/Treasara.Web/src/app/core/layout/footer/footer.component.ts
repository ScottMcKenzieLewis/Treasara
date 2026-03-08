import { Component, inject } from '@angular/core';
import { BuildInfoService } from '../../services/build-info.service';


@Component({
  selector: 'app-footer',
  imports: [],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  private readonly buildInfoService = inject(BuildInfoService);

  get buildLabel(): string {
    return this.buildInfoService.displayLabel;
  }}
