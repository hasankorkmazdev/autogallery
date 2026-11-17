import {
  closestCenter,
  DndContext,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from "@dnd-kit/core";
import { restrictToParentElement, restrictToVerticalAxis } from "@dnd-kit/modifiers";
import { arrayMove, SortableContext, sortableKeyboardCoordinates, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { FieldDescription } from "@/components/ui/field";
import type { Feature, FeatureGroup } from "@/api/types";
import { FeatureRow } from "./feature-row";

interface FeatureGroupCardProps {
  group: FeatureGroup;
  onReorder: (category: number, ordered: Feature[]) => void;
  onAdd: (category: number) => void;
  onEdit: (feature: Feature) => void;
}

// Bir kategorinin donanımları; sürükleyerek sıralanır (yalnız kategori içinde). Sihirbaz ve site bu sırayı kullanır.
export function FeatureGroupCard({ group, onReorder, onAdd, onEdit }: FeatureGroupCardProps) {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 4 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;
    const from = group.features.findIndex((x) => x.id === active.id);
    const to = group.features.findIndex((x) => x.id === over.id);
    onReorder(group.category, arrayMove(group.features, from, to));
  };

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between gap-2">
        <CardTitle>{group.categoryName}</CardTitle>
        <Button size="icon-sm" variant="outline" title="Bu kategoriye ekle" onClick={() => onAdd(group.category)}>
          <Plus className="w-4 h-4" />
        </Button>
      </CardHeader>
      <CardContent className="flex flex-col gap-0.5">
        {group.features.length === 0 && <FieldDescription>Bu kategoride donanım yok.</FieldDescription>}
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          modifiers={[restrictToVerticalAxis, restrictToParentElement]}
          onDragEnd={handleDragEnd}>
          <SortableContext items={group.features.map((x) => x.id)} strategy={verticalListSortingStrategy}>
            {group.features.map((feature, index) => (
              <FeatureRow key={feature.id} feature={feature} index={index} onEdit={onEdit} />
            ))}
          </SortableContext>
        </DndContext>
      </CardContent>
    </Card>
  );
}
