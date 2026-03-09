import {
  Directive,
  HostBinding,
  Input,
  inject,
} from '@angular/core';
import { MatTooltip, TooltipPosition } from '@angular/material/tooltip';

@Directive({
  selector: '[trsTooltip]',
  standalone: true,
  hostDirectives: [
    {
      directive: MatTooltip,
      inputs: [
        'matTooltip: trsTooltip',
        'matTooltipPosition: trsTooltipPosition',
        'matTooltipShowDelay: trsTooltipShowDelay',
        'matTooltipHideDelay: trsTooltipHideDelay',
        'matTooltipClass: trsTooltipClass',
        'matTooltipDisabled: trsTooltipDisabled',
        'matTooltipTouchGestures: trsTooltipTouchGestures',
      ],
    },
  ],
})
export class TrsTooltipDirective {
  @Input() trsTooltip = '';
  @Input() trsTooltipPosition: TooltipPosition = 'above';
  @Input() trsTooltipShowDelay = 250;
  @Input() trsTooltipHideDelay = 0;
  @Input() trsTooltipClass: string | string[] = 'trs-tooltip-panel';
  @Input() trsTooltipDisabled = false;
  @Input() trsTooltipTouchGestures: 'auto' | 'on' | 'off' = 'auto';

  @HostBinding('class.trs-tooltip-trigger')
  protected readonly hostClass = true;

  private readonly matTooltip = inject(MatTooltip);

  show(): void {
    this.matTooltip.show();
  }

  hide(): void {
    this.matTooltip.hide();
  }

  toggle(): void {
    this.matTooltip.toggle();
  }
}