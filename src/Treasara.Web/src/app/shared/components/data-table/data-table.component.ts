import { CommonModule } from '@angular/common';
import { Component, computed, input, signal, OnInit } from '@angular/core';
import { DataTableColumn } from './data-table-column.model';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss'
})
export class DataTableComponent<T extends object> implements OnInit {
  readonly columns = input.required<DataTableColumn<T>[]>();
  readonly rows = input.required<T[]>();

  readonly emptyMessage = input('No rows to display.');
  readonly scrollHeight = input('360px');
  readonly enablePaging = input(true);
  readonly initialPageSize = input(8);

  readonly currentPage = signal(1);
  readonly pageSize = signal(8);

  readonly totalRows = computed(() => this.rows().length);

  readonly totalPages = computed(() => {
    if (!this.enablePaging()) {
      return 1;
    }

    const total = this.totalRows();
    const size = this.pageSize();
    return total === 0 ? 1 : Math.ceil(total / size);
  });

  readonly visibleRows = computed(() => {
    const rows = this.rows();

    if (!this.enablePaging()) {
      return rows;
    }

    const page = this.currentPage();
    const size = this.pageSize();
    const start = (page - 1) * size;

    return rows.slice(start, start + size);
  });

  ngOnInit(): void {
    this.pageSize.set(this.initialPageSize());
  }

  goToPreviousPage(): void {
    this.currentPage.update(page => Math.max(1, page - 1));
  }

  goToNextPage(): void {
    this.currentPage.update(page => Math.min(this.totalPages(), page + 1));
  }

  onPageSizeChange(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);
    this.pageSize.set(value);
    this.currentPage.set(1);
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