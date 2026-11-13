import { useRef, useState } from "react";
import { ImagePlus } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { FieldDescription } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import { vehiclesService } from "@/api/services/vehicles.service";
import type { VehicleMedia } from "@/api/types";
import { cn } from "@/lib/utils";

const ACCEPTED = ["image/jpeg", "image/png", "image/webp"];
const MAX_BYTES = 25 * 1024 * 1024;

interface MediaUploaderProps {
  vehicleId: string;
  onUploaded: (media: VehicleMedia[]) => void;
}

// Toplu sürükle-bırak yükleme ve ilerleme göstergesi. WebP varyantları sunucuda üretilir.
export function MediaUploader({ vehicleId, onUploaded }: MediaUploaderProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const [progress, setProgress] = useState<number | null>(null);

  const upload = async (fileList: FileList | null) => {
    const files = Array.from(fileList ?? []);
    if (files.length === 0) return;

    const invalid = files.find((f) => !ACCEPTED.includes(f.type) || f.size > MAX_BYTES);
    if (invalid) {
      toast.error(`${invalid.name}: yalnızca 25 MB'a kadar JPG, PNG ve WebP yüklenebilir.`);
      return;
    }

    setProgress(0);
    try {
      const result = await vehiclesService.uploadMedia(vehicleId, files, setProgress);
      onUploaded(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setProgress(null);
      if (inputRef.current) inputRef.current.value = "";
    }
  };

  const uploading = progress !== null;

  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center gap-3 rounded-lg border-2 border-dashed p-8 text-center",
        dragging && "border-primary bg-muted",
      )}
      onDragOver={(e) => {
        e.preventDefault();
        setDragging(true);
      }}
      onDragLeave={() => setDragging(false)}
      onDrop={(e) => {
        e.preventDefault();
        setDragging(false);
        if (!uploading) upload(e.dataTransfer.files);
      }}>
      <ImagePlus className="size-8 text-muted-foreground" />
      <div className="text-sm">Fotoğrafları buraya sürükleyin ya da seçin</div>
      <FieldDescription>
        JPG, PNG ya da WebP, en fazla 25 MB. Önerilen: 3:2 oran, en az 12 fotoğraf, ilk fotoğraf ön 3/4 açı.
      </FieldDescription>
      <Input
        ref={inputRef}
        type="file"
        accept={ACCEPTED.join(",")}
        multiple
        className="hidden"
        aria-label="Fotoğraf seç"
        onChange={(e) => upload(e.target.files)}
      />
      <Button type="button" variant="outline" disabled={uploading} onClick={() => inputRef.current?.click()}>
        Fotoğraf seç
      </Button>
      {uploading && (
        <div className="flex w-full max-w-sm flex-col gap-1">
          <Progress value={progress} />
          <FieldDescription>
            {progress < 100 ? `Yükleniyor... %${progress}` : "Fotoğraflar işleniyor (WebP varyantları üretiliyor)..."}
          </FieldDescription>
        </div>
      )}
    </div>
  );
}
