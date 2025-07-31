export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  type?: 'text' | 'number' | 'date' | 'currency' | 'boolean' | 'actions';
  width?: string;
}

export interface TableAction {
  label: string;
  icon: string;
  color: 'primary' | 'accent' | 'warn';
  action: (item: any) => void;
  disabled?: (item: any) => boolean;
}
