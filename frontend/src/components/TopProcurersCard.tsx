import type { TopProcurer } from "../types/analytics";
import { Card } from "./Card";

interface TopProcurersCardProps {
  items: TopProcurer[];
}

function formatMoney(value: number): string {
  return new Intl.NumberFormat("uk-UA", {
    style: "currency",
    currency: "UAH",
    maximumFractionDigits: 2,
  }).format(value);
}

export function TopProcurersCard({
  items,
}: TopProcurersCardProps) {
  return (
    <Card title="Top procurers">
      {items.length === 0 ? (
        <p className="muted">No procurers found</p>
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
                  {item.tendersCount} tender
                  {item.tendersCount === 1 ? "" : "s"}
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