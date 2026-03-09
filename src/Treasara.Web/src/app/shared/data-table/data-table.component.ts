import { CommonModule } from '@angular/common';
import { Component, computed, effect, input, signal } from '@angular/core';
import { DataTableColumn } from './data-table-column.model';

type PageSizeOption = number | 'all';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss'
})
export class DataTableComponent<T extends object> {
  readonly columns = input.required<DataTableColumn<T>[]>();
  readonly rows = input.required<T[]>();

  readonly emptyMessage = input('No rows to display.');
  readonly scrollHeight = input('360px');
  readonly enablePaging = input(true);

  readonly initialPageSize = input<PageSizeOption>(8);
  readonly pageSizeOptions = input<PageSizeOption[]>([5, 10, 25, 'all']);

  readonly currentPage = signal(1);
  readonly pageSize = signal<PageSizeOption>(5);

  readonly totalRows = computed(() => this.rows().length);

  readonly normalizedPageSizeOptions = computed<PageSizeOption[]>(() => {
    const raw = this.pageSizeOptions();

    const numericOptions = raw
      .filter((value): value is number => typeof value === 'number' && Number.isInteger(value) && value > 0)
      .sort((a, b) => a - b);

    const uniqueNumeric = [...new Set(numericOptions)];
    const hasAll = raw.includes('all');

    const result: PageSizeOption[] = [...uniqueNumeric];
    const initial = this.initialPageSize();

    if (
      typeof initial === 'number' &&
      Number.isInteger(initial) &&
      initial > 0 &&
      !result.includes(initial)
    ) {
      result.push(initial);
      result.sort((a, b) => (a as number) - (b as number));
    }

    if (initial === 'all' || hasAll) {
      result.push('all');
    }

    return result.length > 0 ? result : [8, 'all'];
  });

  readonly effectivePageSize = computed(() => {
    const selected = this.pageSize();
    return selected === 'all' ? Math.max(1, this.totalRows()) : selected;
  });

  readonly totalPages = computed(() => {
    if (!this.enablePaging() || this.pageSize() === 'all') {
      return 1;
    }

    const total = this.totalRows();
    const size = this.effectivePageSize();

    return total === 0 ? 1 : Math.ceil(total / size);
  });

  readonly visibleRows = computed(() => {
    const rows = this.rows();

    if (!this.enablePaging() || this.pageSize() === 'all') {
      return rows;
    }

    const page = this.currentPage();
    const size = this.effectivePageSize();
    const start = (page - 1) * size;

    return rows.slice(start, start + size);
  });

  readonly canGoPrevious = computed(() => this.currentPage() > 1);
  readonly canGoNext = computed(() => this.currentPage() < this.totalPages());

  constructor() {
    effect(() => {
      const options = this.normalizedPageSizeOptions();
      const current = this.pageSize();

      if (!options.includes(current)) {
        const initial = this.initialPageSize();
        const fallback = options.includes(initial) ? initial : options[0];
        this.pageSize.set(fallback);
      }
    });

    effect(() => {
      const totalPages = this.totalPages();
      const currentPage = this.currentPage();

      if (currentPage > totalPages) {
        this.currentPage.set(totalPages);
      } else if (currentPage < 1) {
        this.currentPage.set(1);
      }
    });

    effect(() => {
      this.rows();
      this.currentPage.set(1);
    });

    effect(() => {
      if (!this.enablePaging() || this.pageSize() === 'all') {
        this.currentPage.set(1);
      }
    });
  }

  goToFirstPage(): void {
    this.currentPage.set(1);
  }

  goToPreviousPage(): void {
    this.currentPage.update(page => Math.max(1, page - 1));
  }

  goToNextPage(): void {
    this.currentPage.update(page => Math.min(this.totalPages(), page + 1));
  }

  goToLastPage(): void {
    this.currentPage.set(this.totalPages());
  }

  onPageSizeChange(event: Event): void {
    const rawValue = (event.target as HTMLSelectElement).value;
    const value: PageSizeOption = rawValue === 'all' ? 'all' : Number(rawValue);

    if (value !== 'all' && (!Number.isInteger(value) || value <= 0)) {
      return;
    }

    this.pageSize.set(value);
    this.currentPage.set(1);
  }

  getPageSizeOptionValue(option: PageSizeOption): string {
    return option === 'all' ? 'all' : String(option);
  }

  getPageSizeOptionLabel(option: PageSizeOption): string {
    return option === 'all' ? 'All' : String(option);
  }

  getCellValue(row: T, column: DataTableColumn<T>): string {
    if (column.formatter) {
      return column.formatter(row);
    }

    const key = column.key as keyof T;
    const value = row[key];

    return value == null ? '' : String(value);
  }
}