import type { TopSupplier } from "../types/analytics";
import { Card } from "./Card";

interface TopSuppliersCardProps {
  items: TopSupplier[];
}

function formatMoney(value: number): string {
  return new Intl.NumberFormat("uk-UA", {
    style: "currency",
    currency: "UAH",
    maximumFractionDigits: 2,
  }).format(value);
}

export function TopSuppliersCard({
  items,
}: TopSuppliersCardProps) {
  return (
    <Card title="Top suppliers">
      {items.length === 0 ? (
        <p className="muted">No suppliers found</p>
      ) : (
        <div className="ranking-list">
          {items.map((item, index) => (
            <div
              className="ranking-item"
              key={item.identifier ?? item.name}
            >
              <div className="ranking-position">
                {index + 1}
              </div>

              <div className="ranking-content">
                <strong>{item.name}</strong>
                <span>
                  {item.contractsCount} contract
                  {item.contractsCount === 1 ? "" : "s"}
                </span>
              </div>

              <div className="ranking-value">
                {formatMoney(
                  item.totalContractAmount,
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}