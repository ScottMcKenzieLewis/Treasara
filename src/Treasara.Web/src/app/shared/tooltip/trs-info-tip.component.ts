import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule, TooltipPosition } from '@angular/material/tooltip';
import { TrsTooltipDirective } from './trs-tooltip.directive';

@Component({
  selector: 'trs-info-tip',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, TrsTooltipDirective],
  templateUrl: './trs-info-tip.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TrsInfoTipComponent {
  @Input() text = '';
  @Input() position: TooltipPosition = 'above';
  @Input() showDelay = 250;
  @Input() hideDelay = 0;
  @Input() disabled = false;
  @Input() ariaLabel = 'More information';
  @Input() trsTooltipClass: string | string[] = 'trs-tooltip-panel';
}