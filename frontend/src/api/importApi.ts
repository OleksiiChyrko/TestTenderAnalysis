import type {
  ImportRequest,
  ImportResult,
} from "../types/analytics";

const API_URL =
  import.meta.env.VITE_API_URL ?? "http://localhost:5297";

export async function importFeed(
  request: ImportRequest,
): Promise<ImportResult> {
  const response = await fetch(
    `${API_URL}/api/import/feed`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    },
  );

  if (!response.ok) {
    const message = await response.text();

    throw new Error(
      message || `Import failed with status ${response.status}`,
    );
  }

  return response.json() as Promise<ImportResult>;
}