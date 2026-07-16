import { useCallback, useEffect, useState } from "react";
import {
  getBudgetSavings,
  getTopProcurers,
  getTopSuppliers,
} from "../api/analyticsApi";
import { BudgetSavingsCard } from "../components/BudgetSavingsCard";
import { ImportButton } from "../components/ImportButton";
import { TopProcurersCard } from "../components/TopProcurersCard";
import { TopSuppliersCard } from "../components/TopSuppliersCard";
import type {
  BudgetSavings,
  ImportResult,
  TopProcurer,
  TopSupplier,
} from "../types/analytics";

export function Dashboard() {
  const [savings, setSavings] =
    useState<BudgetSavings | null>(null);

  const [procurers, setProcurers] =
    useState<TopProcurer[]>([]);

  const [suppliers, setSuppliers] =
    useState<TopSupplier[]>([]);

  const [isLoading, setIsLoading] = useState(true);
  const [isImporting, setIsImporting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [importResult, setImportResult] =
    useState<ImportResult | null>(null);

  const loadAnalytics = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const [
        savingsResult,
        procurersResult,
        suppliersResult,
      ] = await Promise.all([
        getBudgetSavings(),
        getTopProcurers(),
        getTopSuppliers(),
      ]);

      setSavings(savingsResult);
      setProcurers(procurersResult);
      setSuppliers(suppliersResult);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load analytics.",
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;
    async function loadInitialAnalytics() {
        try {
        const [
            savingsResult,
            procurersResult,
            suppliersResult,
        ] = await Promise.all([
            getBudgetSavings(),
            getTopProcurers(),
            getTopSuppliers(),
        ]);

        if (isCancelled) {
            return;
        }

        setSavings(savingsResult);
        setProcurers(procurersResult);
        setSuppliers(suppliersResult);
        } catch (loadError) {
        if (isCancelled) {
            return;
        }

        setError(
            loadError instanceof Error
            ? loadError.message
            : "Unable to load analytics.",
        );
        } finally {
        if (!isCancelled) {
            setIsLoading(false);
        }
        }
    }

    void loadInitialAnalytics();

    return () => {
        isCancelled = true;
    };
    }, []);

  async function handleImportSuccess(
    result: ImportResult,
  ) {
    setImportResult(result);
    setIsImporting(false);
    await loadAnalytics();
  }

  return (
    <main className="dashboard">
      <header className="dashboard-header">
        <div>
          <p className="eyebrow">
            Procurement intelligence
          </p>
          <h1>Tender Analytics Dashboard</h1>
          <p className="header-description">
            Import tender data and explore budget,
            procurer, and supplier analytics.
          </p>
        </div>

        <div className="header-actions">
          <button
            className="button button-secondary"
            type="button"
            onClick={() => void loadAnalytics()}
            disabled={isLoading}
          >
            {isLoading ? "Refreshing..." : "Refresh"}
          </button>

          <ImportButton
            isImporting={isImporting}
            onImportStart={() => {
              setIsImporting(true);
              setError(null);
              setImportResult(null);
            }}
            onImportSuccess={(result) => {
              void handleImportSuccess(result);
            }}
            onImportError={(message) => {
              setError(message);
              setIsImporting(false);
            }}
          />
        </div>
      </header>

      {error && (
        <div className="alert alert-error">
          {error}
        </div>
      )}

      {importResult && (
        <div className="alert alert-success">
          Import completed: {importResult.importedCount} imported,
          {" "}
          {importResult.skippedCount} skipped,
          {" "}
          {importResult.failedCount} failed.
        </div>
      )}

      {isLoading ? (
        <div className="loading-panel">
          Loading analytics...
        </div>
      ) : (
        <>
          <BudgetSavingsCard data={savings} />

          <div className="dashboard-grid">
            <TopProcurersCard items={procurers} />
            <TopSuppliersCard items={suppliers} />
          </div>
        </>
      )}
    </main>
  );
}