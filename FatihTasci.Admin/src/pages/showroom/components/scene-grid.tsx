import {
  closestCenter,
  DndContext,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from "@dnd-kit/core";
import { arrayMove, rectSortingStrategy, SortableContext, sortableKeyboardCoordinates } from "@dnd-kit/sortable";
import type { ShowroomScene } from "@/api/types";
import { SceneCard } from "./scene-card";

interface SceneGridProps {
  scenes: ShowroomScene[];
  uploads: Record<number, number>;
  onReorder: (ordered: ShowroomScene[]) => void;
  onEdit: (scene: ShowroomScene) => void;
  onUpload: (scene: ShowroomScene, file: File) => void;
  onRemoveImage: (scene: ShowroomScene) => void;
  onDelete: (scene: ShowroomScene) => void;
}

// dnd-kit ile sürükleyerek sıralama; sıra sitedeki sahne sırasıdır.
export function SceneGrid({ scenes, uploads, onReorder, ...actions }: SceneGridProps) {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 4 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;
    const from = scenes.findIndex((x) => x.id === active.id);
    const to = scenes.findIndex((x) => x.id === over.id);
    onReorder(arrayMove(scenes, from, to));
  };

  return (
    <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
      <SortableContext items={scenes.map((x) => x.id)} strategy={rectSortingStrategy}>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4">
          {scenes.map((scene, index) => (
            <SceneCard key={scene.id} scene={scene} index={index} progress={uploads[scene.id] ?? null} {...actions} />
          ))}
        </div>
      </SortableContext>
    </DndContext>
  );
}
