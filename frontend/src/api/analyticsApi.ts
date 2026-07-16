import type {
  BudgetSavings,
  TopProcurer,
  TopSupplier,
} from "../types/analytics";

const API_URL =
  import.meta.env.VITE_API_URL ?? "http://localhost:5297";

async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(`${API_URL}${path}`);

  if (!response.ok) {
    throw new Error(
      `Request failed with status ${response.status}`,
    );
  }

  return response.json() as Promise<T>;
}

export function getBudgetSavings(): Promise<BudgetSavings> {
  return getJson<BudgetSavings>(
    "/api/analytics/savings",
  );
}

export function getTopProcurers(
  limit = 5,
): Promise<TopProcurer[]> {
  return getJson<TopProcurer[]>(
    `/api/analytics/top-procurers?limit=${limit}`,
  );
}

export function getTopSuppliers(
  limit = 5,
): Promise<TopSupplier[]> {
  return getJson<TopSupplier[]>(
    `/api/analytics/top-suppliers?limit=${limit}`,
  );
}