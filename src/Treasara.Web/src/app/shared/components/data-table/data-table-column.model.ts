export interface DataTableColumn<T> {
  key: keyof T | string;
  header: string;
  formatter?: (row: T) => string;
}