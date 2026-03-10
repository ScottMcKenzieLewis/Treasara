import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  ChartConfiguration,
  LinearScale,
  Tooltip,
  Legend
} from 'chart.js';

Chart.register(
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend
);

export type CashFlowChartPoint = {
  label: string;
  cashFlowAmount: number;
  presentValue?: number;
  discountFactor?: number;
  isFinal?: boolean;
};

@Component({
  selector: 'app-cash-flow-chart',
  standalone: true,
  templateUrl: './cash-flow-chart.component.html',
  styleUrl: './cash-flow-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CashFlowChartComponent
  implements AfterViewInit, OnChanges, OnDestroy
{
  @Input() title = 'Cash Flow Profile';
  @Input() subtitle = 'Remaining coupon and redemption flows.';
  @Input() currency = 'USD';
  @Input() points: CashFlowChartPoint[] = [];

  @ViewChild('chartCanvas')
  private chartCanvas?: ElementRef<HTMLCanvasElement>;

  private chart?: Chart<'bar'>;

  ngAfterViewInit(): void {
    this.renderChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['points'] || changes['currency']) {
      this.renderChart();
    }
  }

  ngOnDestroy(): void {
    this.destroyChart();
  }

  private renderChart(): void {
    const canvas = this.chartCanvas?.nativeElement;
    if (!canvas) {
      return;
    }

    this.destroyChart();

    const labels = this.points.map(p => p.label);
    const values = this.points.map(p => p.cashFlowAmount);
    const backgroundColors = this.points.map(p =>
      p.isFinal
        ? 'rgba(37, 99, 235, 0.82)'
        : 'rgba(79, 182, 172, 0.78)'
    );
    const borderColors = this.points.map(p =>
      p.isFinal
        ? 'rgba(37, 99, 235, 1)'
        : 'rgba(79, 182, 172, 1)'
    );

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Cash Flow',
            data: values,
            backgroundColor: backgroundColors,
            borderColor: borderColors,
            borderWidth: 1,
            borderRadius: 8,
            borderSkipped: false,
            maxBarThickness: 32
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: {
          duration: 250
        },
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            displayColors: false,
            callbacks: {
              title: (items) => items[0]?.label ?? '',
              label: (context) => {
                const point = this.points[context.dataIndex];
                const lines: string[] = [
                  `Cash Flow: ${this.formatCurrency(point.cashFlowAmount)}`
                ];

                if (point.presentValue !== undefined) {
                  lines.push(`PV: ${this.formatCurrency(point.presentValue)}`);
                }

                if (point.discountFactor !== undefined) {
                  lines.push(`DF: ${point.discountFactor.toFixed(6)}`);
                }

                return lines;
              }
            }
          }
        },
        scales: {
          x: {
            grid: {
              display: false
            },
            ticks: {
              color: '#607089',
              maxRotation: 0,
              autoSkip: true
            },
            border: {
              display: false
            }
          },
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(20, 32, 51, 0.08)'
            },
            ticks: {
              color: '#607089',
              callback: (value) => {
                if (typeof value !== 'number') {
                  return value;
                }

                return this.formatCompactCurrency(value);
              }
            },
            border: {
              display: false
            }
          }
        }
      }
    };

    this.chart = new Chart(canvas, config);
  }

  private destroyChart(): void {
    this.chart?.destroy();
    this.chart = undefined;
  }

  private formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this.currency,
      maximumFractionDigits: 0
    }).format(value);
  }

  private formatCompactCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this.currency,
      notation: 'compact',
      maximumFractionDigits: 1
    }).format(value);
  }
}