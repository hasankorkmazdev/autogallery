import { useRef } from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical, ImageOff, ImagePlus, ImageUp, Pencil, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { FieldDescription } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import type { ShowroomScene } from "@/api/types";

const ACCEPTED_IMAGES = ["image/jpeg", "image/png", "image/webp"];
const MAX_IMAGE_BYTES = 25 * 1024 * 1024;

interface SceneCardProps {
  scene: ShowroomScene;
  index: number;
  /** Yükleme sürüyorsa yüzde, yoksa null */
  progress: number | null;
  onEdit: (scene: ShowroomScene) => void;
  onUpload: (scene: ShowroomScene, file: File) => void;
  onRemoveImage: (scene: ShowroomScene) => void;
  onDelete: (scene: ShowroomScene) => void;
}

function SceneStatus({ scene }: { scene: ShowroomScene }) {
  if (!scene.isActive) return <Badge variant="secondary">Gizli</Badge>;
  if (!scene.mediumUrl) return <Badge variant="outline">Fotoğraf bekliyor</Badge>;
  return <Badge>Sitede</Badge>;
}

// Sahne kartı: fotoğraf önizlemesi, durum, düzenle / fotoğraf değiştir / kaldır / sil, sürükleyerek sıralama.
export function SceneCard({ scene, index, progress, onEdit, onUpload, onRemoveImage, onDelete }: SceneCardProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: scene.id });
  const style = { transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.6 : 1 };
  const inputRef = useRef<HTMLInputElement>(null);
  const uploading = progress !== null;

  const pick = (fileList: FileList | null) => {
    const file = fileList?.[0];
    if (inputRef.current) inputRef.current.value = "";
    if (!file) return;
    if (!ACCEPTED_IMAGES.includes(file.type) || file.size > MAX_IMAGE_BYTES) {
      toast.error(`${file.name}: yalnızca 25 MB'a kadar JPG, PNG ve WebP yüklenebilir.`);
      return;
    }
    onUpload(scene, file);
  };

  const renderPreview = () => {
    if (uploading) {
      return (
        <div className="flex aspect-[3/2] w-full flex-col items-center justify-center gap-2 bg-muted p-6">
          <Progress value={progress} />
          <FieldDescription>
            {progress < 100 ? `Yükleniyor... %${progress}` : "Fotoğraf işleniyor..."}
          </FieldDescription>
        </div>
      );
    }
    if (scene.mediumUrl) {
      return <img src={scene.mediumUrl} alt={scene.title} className="aspect-[3/2] w-full object-cover" loading="lazy" />;
    }
    return (
      <div
        className="flex aspect-[3/2] w-full flex-col items-center justify-center gap-2 border-b-2 border-dashed bg-muted p-6 text-center"
        onDragOver={(e) => e.preventDefault()}
        onDrop={(e) => {
          e.preventDefault();
          pick(e.dataTransfer.files);
        }}>
        <ImagePlus className="size-8 text-muted-foreground" />
        <FieldDescription>Fotoğrafı sürükleyin ya da seçin</FieldDescription>
        <Button type="button" variant="outline" size="sm" onClick={() => inputRef.current?.click()}>
          Fotoğraf seç
        </Button>
      </div>
    );
  };

  return (
    <div ref={setNodeRef} style={style} className="relative flex flex-col overflow-hidden rounded-lg border bg-card">
      {renderPreview()}
      <div className="absolute left-2 top-2 flex gap-1">
        <Badge variant="secondary">{String(index + 1).padStart(2, "0")}</Badge>
        <SceneStatus scene={scene} />
      </div>
      <div className="flex flex-col gap-1 p-3">
        <div className="text-xs uppercase tracking-widest text-muted-foreground">{scene.tag}</div>
        <div className="font-medium">{scene.title}</div>
      </div>
      <div className="mt-auto flex items-center justify-between gap-1 border-t p-1">
        <Button type="button" variant="ghost" size="icon-sm" title="Sürükleyerek sırala" {...attributes} {...listeners}>
          <GripVertical className="w-4 h-4" />
        </Button>
        <div className="flex gap-1">
          <Button type="button" variant="ghost" size="icon-sm" title="Düzenle" onClick={() => onEdit(scene)}>
            <Pencil className="w-4 h-4" />
          </Button>
          <Button type="button" variant="ghost" size="icon-sm" disabled={uploading}
            title={scene.mediumUrl ? "Fotoğrafı değiştir" : "Fotoğraf yükle"} onClick={() => inputRef.current?.click()}>
            <ImageUp className="w-4 h-4" />
          </Button>
          {scene.mediumUrl && (
            <Button type="button" variant="ghost" size="icon-sm" disabled={uploading} title="Fotoğrafı kaldır" onClick={() => onRemoveImage(scene)}>
              <ImageOff className="w-4 h-4" />
            </Button>
          )}
          <Button type="button" variant="ghost-destructive" size="icon-sm" title="Sahneyi sil" onClick={() => onDelete(scene)}>
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      </div>
      <Input
        ref={inputRef}
        type="file"
        accept={ACCEPTED_IMAGES.join(",")}
        className="hidden"
        aria-label={`${scene.title} fotoğrafı`}
        onChange={(e) => pick(e.target.files)}
      />
    </div>
  );
}
