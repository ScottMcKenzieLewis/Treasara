import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface BuildInfo {
  readonly version: string;
  readonly label: string;
  readonly production: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class BuildInfoService {
  get build(): BuildInfo {
    return {
      version: environment.build.version,
      label: environment.build.label,
      production: environment.production
    };
  }

  get displayLabel(): string {
    return `v${this.build.version} (${this.build.label})`;
  }
}