import type { BudgetSavings } from "../types/analytics";
import { Card } from "./Card";

interface BudgetSavingsCardProps {
  data: BudgetSavings | null;
}

function formatMoney(
  value: number,
  currency: string,
): string {
  return new Intl.NumberFormat("uk-UA", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(value);
}

export function BudgetSavingsCard({
  data,
}: BudgetSavingsCardProps) {
  return (
    <Card title="Budget savings" className="savings-card">
      {!data ? (
        <p className="muted">No data available</p>
      ) : (
        <>
          <div className="savings-value">
            {formatMoney(
              data.savingsAmount,
              data.currency,
            )}
          </div>

          <div className="summary-grid">
            <div>
              <span>Expected budget</span>
              <strong>
                {formatMoney(
                  data.expectedAmount,
                  data.currency,
                )}
              </strong>
            </div>

            <div>
              <span>Contract amount</span>
              <strong>
                {formatMoney(
                  data.contractAmount,
                  data.currency,
                )}
              </strong>
            </div>
          </div>
        </>
      )}
    </Card>
  );
}