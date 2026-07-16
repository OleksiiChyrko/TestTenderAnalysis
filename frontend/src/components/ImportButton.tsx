import type {
  ImportRequest,
  ImportResult,
} from "../types/analytics";
import { importFeed } from "../api/importApi";

interface ImportButtonProps {
  isImporting: boolean;
  onImportStart: () => void;
  onImportSuccess: (result: ImportResult) => void;
  onImportError: (message: string) => void;
}

const importRequest: ImportRequest = {
  dateFrom: "2025-12-01T00:00:00Z",
  dateTo: "2026-01-01T00:00:00Z",
  maxPages: 3,
  maxConcurrency: 8,
};

export function ImportButton({
  isImporting,
  onImportStart,
  onImportSuccess,
  onImportError,
}: ImportButtonProps) {
  async function handleImport() {
    onImportStart();

    try {
      const result = await importFeed(importRequest);
      onImportSuccess(result);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Import failed.";

      onImportError(message);
    }
  }

  return (
    <button
      className="button button-primary"
      type="button"
      onClick={handleImport}
      disabled={isImporting}
    >
      {isImporting ? "Importing..." : "Import feed"}
    </button>
  );
}