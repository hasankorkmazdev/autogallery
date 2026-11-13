import { PART_SHAPES, STATE_COLORS } from "./parts";

interface DamageDiagramProps {
  states: Record<number, number>;
  partLabels: Record<number, string>;
  stateLabels: Record<number, string>;
  onPartClick?: (part: number) => void;
}

// Üstten görünüm araç şeması. Parçaya tıklandıkça durum sırayla değişir (sihirbaz adım 4).
export function DamageDiagram({ states, partLabels, stateLabels, onPartClick }: DamageDiagramProps) {
  return (
    <svg viewBox="0 0 240 432" role="group" aria-label="Boya ve değişen şeması" className="w-full max-w-[240px] mx-auto">
      {Object.entries(PART_SHAPES).map(([key, shape]) => {
        const part = Number(key);
        const state = states[part] ?? 0;
        const label = `${partLabels[part] ?? part}: ${stateLabels[state] ?? state}`;
        return (
          <rect
            key={part}
            x={shape.x}
            y={shape.y}
            width={shape.w}
            height={shape.h}
            rx={6}
            fill={STATE_COLORS[state]}
            stroke="currentColor"
            strokeOpacity={0.25}
            strokeWidth={2}
            role="button"
            tabIndex={0}
            aria-label={label}
            className="cursor-pointer outline-none focus-visible:opacity-80 hover:opacity-80"
            onClick={() => onPartClick?.(part)}
            onKeyDown={(e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                onPartClick?.(part);
              }
            }}>
            <title>{label}</title>
          </rect>
        );
      })}
      <text x={120} y={22} textAnchor="middle" fontSize={10} className="fill-muted-foreground pointer-events-none">
        ÖN
      </text>
      <text x={120} y={415} textAnchor="middle" fontSize={10} className="fill-muted-foreground pointer-events-none">
        ARKA
      </text>
    </svg>
  );
}

export function DamageLegend({ stateLabels }: { stateLabels: Record<number, string> }) {
  return (
    <ul className="grid grid-cols-2 gap-2 text-sm">
      {Object.entries(STATE_COLORS).map(([state, color]) => (
        <li key={state} className="flex items-center gap-2">
          <span className="inline-block size-3 rounded-sm" style={{ backgroundColor: color }} />
          {stateLabels[Number(state)]}
        </li>
      ))}
    </ul>
  );
}
