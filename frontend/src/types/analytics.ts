export interface BudgetSavings {
  expectedAmount: number;
  contractAmount: number;
  savingsAmount: number;
  currency: string;
}

export interface TopProcurer {
  identifier: string | null;
  name: string;
  totalContractAmount: number;
  tendersCount: number;
}

export interface TopSupplier {
  identifier: string | null;
  name: string;
  totalContractAmount: number;
  contractsCount: number;
}

export interface ImportRequest {
  dateFrom: string;
  dateTo: string;
  maxPages: number;
  maxConcurrency: number;
}

export interface ImportResult {
  pagesProcessed: number;
  feedItemsProcessed: number;
  candidatesCount: number;
  importedCount: number;
  skippedCount: number;
  failedCount: number;
  errors: string[];
}