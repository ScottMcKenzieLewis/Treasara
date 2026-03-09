import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { DataTableColumn } from '../../../../shared/components/data-table/data-table-column.model';
import { BondValuationLine } from '../../models/bond-valuation-line.model';
import { BondValuationApiService } from '../../services/bond-valuation-api.service';
import { BondValuationResponse } from '../../models/bond-valuation-response.model';
import { ApiError } from '../../../../core/models/api-error.model';
import { DEFAULT_BOND_VALUATION_REQUEST } from '../../models/bond-valuation-defaults';
import {
  CURRENCY_OPTIONS,
  DAY_COUNT_OPTIONS,
  FREQUENCY_OPTIONS,
  ROLL_CONVENTION_OPTIONS
} from '../../models/bond-valuation-options';
import { mapApiError } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-bond-valuation-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CurrencyPipe,
    DataTableComponent
  ],
  templateUrl: './bond-valuation-page.component.html',
  styleUrl: './bond-valuation-page.component.scss'
})
export class BondValuationPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly bondValuationApi = inject(BondValuationApiService);

  readonly currencyOptions = CURRENCY_OPTIONS;
  readonly frequencyOptions = FREQUENCY_OPTIONS;
  readonly dayCountOptions = DAY_COUNT_OPTIONS;
  readonly rollConventionOptions = ROLL_CONVENTION_OPTIONS;

  readonly isSubmitting = signal(false);
  readonly valuationResult = signal<BondValuationResponse | null>(null);
  readonly apiError = signal<ApiError | null>(null);
  readonly showCalculatorInfo = signal(false);

  readonly lineCount = computed(() => this.valuationResult()?.lines.length ?? 0);

  readonly form = this.fb.nonNullable.group({
    notional: [
      DEFAULT_BOND_VALUATION_REQUEST.notional,
      [Validators.required, Validators.min(0.01)]
    ],
    currency: [
      DEFAULT_BOND_VALUATION_REQUEST.currency,
      [Validators.required]
    ],
    couponRate: [
      DEFAULT_BOND_VALUATION_REQUEST.couponRate,
      [Validators.required, Validators.min(0)]
    ],
    issueDate: [
      DEFAULT_BOND_VALUATION_REQUEST.issueDate,
      [Validators.required]
    ],
    maturityDate: [
      DEFAULT_BOND_VALUATION_REQUEST.maturityDate,
      [Validators.required]
    ],
    valuationDate: [
      DEFAULT_BOND_VALUATION_REQUEST.valuationDate,
      [Validators.required]
    ],
    frequency: [
      DEFAULT_BOND_VALUATION_REQUEST.frequency,
      [Validators.required]
    ],
    dayCount: [
      DEFAULT_BOND_VALUATION_REQUEST.dayCount,
      [Validators.required]
    ],
    rollConvention: [
      DEFAULT_BOND_VALUATION_REQUEST.rollConvention,
      [Validators.required]
    ],
    curveRate: [
      DEFAULT_BOND_VALUATION_REQUEST.curveRate,
      [Validators.required, Validators.min(0)]
    ]
  });

  readonly cashflowColumns: DataTableColumn<BondValuationLine>[] = [
    {
      key: 'paymentDate',
      header: 'Payment Date',
      formatter: (row) => row.paymentDate
    },
    {
      key: 'cashflowAmount',
      header: 'Cashflow Amount',
      formatter: (row) =>
        `${row.currency} ${row.cashflowAmount.toLocaleString(undefined, {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2
        })}`
    },
    {
      key: 'discountFactor',
      header: 'Discount Factor',
      formatter: (row) =>
        row.discountFactor.toLocaleString(undefined, {
          minimumFractionDigits: 4,
          maximumFractionDigits: 6
        })
    },
    {
      key: 'presentValue',
      header: 'Present Value',
      formatter: (row) =>
        `${row.currency} ${row.presentValue.toLocaleString(undefined, {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2
        })}`
    },
    {
      key: 'currency',
      header: 'Currency'
    }
  ];

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.apiError.set(null);
    this.isSubmitting.set(true);

    this.bondValuationApi
      .valueBond(this.form.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.valuationResult.set(response);
          this.isSubmitting.set(false);
        },
        error: (error) => {
          this.apiError.set(mapApiError(error));
          this.valuationResult.set(null);
          this.isSubmitting.set(false);
        }
      });
  }

  resetToDefaults(): void {
    this.form.reset(DEFAULT_BOND_VALUATION_REQUEST);
    this.form.markAsPristine();
    this.form.markAsUntouched();
    this.apiError.set(null);
    this.valuationResult.set(null);
  }

  toggleCalculatorInfo(): void {
    this.showCalculatorInfo.update(value => !value);
  }

  hasError(controlName: keyof typeof DEFAULT_BOND_VALUATION_REQUEST): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }
}