import {
  closestCenter,
  DndContext,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from "@dnd-kit/core";
import {
  arrayMove,
  rectSortingStrategy,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical, Star, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { VehicleMedia } from "@/api/types";

interface MediaGridProps {
  media: VehicleMedia[];
  onReorder: (ordered: VehicleMedia[]) => void;
  onSetCover: (media: VehicleMedia) => void;
  onDelete: (media: VehicleMedia) => void;
}

interface MediaTileProps {
  item: VehicleMedia;
  index: number;
  onSetCover: (media: VehicleMedia) => void;
  onDelete: (media: VehicleMedia) => void;
}

function MediaTile({ item, index, onSetCover, onDelete }: MediaTileProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: item.id });
  const style = { transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.6 : 1 };

  return (
    <div ref={setNodeRef} style={style} className="relative overflow-hidden rounded-lg border bg-muted">
      <img src={item.smallUrl} alt={`Fotoğraf ${index + 1}`} className="aspect-[3/2] w-full object-cover" loading="lazy" />
      <div className="absolute left-2 top-2 flex gap-1">
        <Badge variant="secondary">{index + 1}</Badge>
        {item.isCover && <Badge>Kapak</Badge>}
      </div>
      <div className="flex items-center justify-between gap-1 bg-card p-1">
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          title="Sürükleyerek sırala"
          {...attributes}
          {...listeners}>
          <GripVertical className="w-4 h-4" />
        </Button>
        <div className="flex gap-1">
          {!item.isCover && (
            <Button type="button" variant="ghost" size="icon-sm" title="Kapak yap" onClick={() => onSetCover(item)}>
              <Star className="w-4 h-4" />
            </Button>
          )}
          <Button type="button" variant="ghost-destructive" size="icon-sm" title="Sil" onClick={() => onDelete(item)}>
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}

// dnd-kit ile sürükleyerek sıralama ve kapak seçimi
export function MediaGrid({ media, onReorder, onSetCover, onDelete }: MediaGridProps) {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 4 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;
    const from = media.findIndex((x) => x.id === active.id);
    const to = media.findIndex((x) => x.id === over.id);
    onReorder(arrayMove(media, from, to));
  };

  return (
    <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
      <SortableContext items={media.map((x) => x.id)} strategy={rectSortingStrategy}>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6">
          {media.map((item, index) => (
            <MediaTile key={item.id} item={item} index={index} onSetCover={onSetCover} onDelete={onDelete} />
          ))}
        </div>
      </SortableContext>
    </DndContext>
  );
}
