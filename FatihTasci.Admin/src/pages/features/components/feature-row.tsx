import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical, Pencil } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { Feature } from "@/api/types";

interface FeatureRowProps {
  feature: Feature;
  index: number;
  onEdit: (feature: Feature) => void;
}

// Donanım satırı: sıra numarası, sürükleme tutamacı, ad ve düzenle.
export function FeatureRow({ feature, index, onEdit }: FeatureRowProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: feature.id });
  const style = { transform: CSS.Transform.toString(transform), transition };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`relative flex items-center gap-1 rounded-md bg-card text-sm ${isDragging ? "z-10 shadow-md ring-1 ring-border" : ""}`}>
      <Button type="button" variant="ghost" size="icon-sm" title="Sürükleyerek sırala" {...attributes} {...listeners}>
        <GripVertical className="w-4 h-4" />
      </Button>
      <span className="w-6 shrink-0 text-right text-xs tabular-nums text-muted-foreground">{index + 1}</span>
      <span className="flex min-w-0 flex-1 items-center gap-2 py-1">
        <span className="min-w-0 break-words">{feature.name}</span>
        {!feature.isActive && <Badge variant="secondary">Pasif</Badge>}
      </span>
      <Button type="button" variant="ghost" size="icon-sm" title="Düzenle" onClick={() => onEdit(feature)}>
        <Pencil className="w-4 h-4" />
      </Button>
    </div>
  );
}
