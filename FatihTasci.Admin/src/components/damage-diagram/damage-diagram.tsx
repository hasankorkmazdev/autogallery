import {
  BODY_PATHS,
  GLASS_PATHS,
  LAMP_PATHS,
  PART_SHAPES,
  STATE_COLORS,
  STATE_TEXT_COLORS,
  VIEW_BOX,
  WHEEL_RADIUS,
  WHEELS,
} from "./parts";

interface DamageDiagramProps {
  states: Record<number, number>;
  partLabels: Record<number, string>;
  stateLabels: Record<number, string>;
  onPartClick?: (part: number) => void;
}

// Açılmış araç şeması (ekspertiz raporlarındaki gibi). Parçaya tıklandıkça durum sırayla değişir (sihirbaz adım 4).
// Orijinal olmayan parçaların üstünde durum yazar (Lokal Boyalı, Boyalı, Değişen).
export function DamageDiagram({ states, partLabels, stateLabels, onPartClick }: DamageDiagramProps) {
  const parts = Object.entries(PART_SHAPES).map(([key, shape]) => ({ part: Number(key), shape }));

  return (
    <svg viewBox={VIEW_BOX} role="group" aria-label="Boya ve değişen şeması" className="w-full max-w-[320px] mx-auto">
      <g className="pointer-events-none" fill="currentColor">
        {BODY_PATHS.map((d) => (
          <path key={d} d={d} opacity={0.08} />
        ))}
        {GLASS_PATHS.map((d) => (
          <path key={d} d={d} opacity={0.3} />
        ))}
      </g>

      {parts.map(({ part, shape }) => {
        const state = states[part] ?? 0;
        const label = `${partLabels[part] ?? part}: ${stateLabels[state] ?? state}`;
        return (
          <path
            key={part}
            d={shape.d}
            fill={STATE_COLORS[state]}
            stroke="currentColor"
            strokeOpacity={0.35}
            strokeWidth={1.5}
            strokeLinejoin="round"
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
          </path>
        );
      })}

      <g className="pointer-events-none" fill="currentColor">
        {LAMP_PATHS.map((d) => (
          <path key={d} d={d} opacity={0.35} />
        ))}
        {WHEELS.map((wheel) => (
          <g key={`${wheel.cx}-${wheel.cy}`}>
            <circle cx={wheel.cx} cy={wheel.cy} r={WHEEL_RADIUS} opacity={0.6} className="stroke-background" strokeWidth={3} />
            <circle cx={wheel.cx} cy={wheel.cy} r={WHEEL_RADIUS / 2.5} className="fill-background" opacity={0.5} />
          </g>
        ))}
      </g>

      <g className="pointer-events-none select-none" fontSize={10} fontWeight={600} textAnchor="middle" dominantBaseline="central">
        {parts
          .filter(({ part }) => (states[part] ?? 0) !== 0)
          .map(({ part, shape }) => {
            const state = states[part];
            return (
              <text
                key={part}
                x={shape.labelX}
                y={shape.labelY}
                fill={STATE_TEXT_COLORS[state]}
                transform={shape.vertical ? `rotate(-90 ${shape.labelX} ${shape.labelY})` : undefined}>
                {stateLabels[state] ?? state}
              </text>
            );
          })}
      </g>

      <g className="fill-muted-foreground pointer-events-none" fontSize={10} textAnchor="end" dominantBaseline="central" letterSpacing="0.15em">
        <text x={106} y={37}>
          ÖN
        </text>
        <text x={106} y={375}>
          ARKA
        </text>
      </g>
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
