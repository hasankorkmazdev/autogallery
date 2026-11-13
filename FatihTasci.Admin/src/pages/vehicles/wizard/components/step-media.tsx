import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { FieldDescription } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { Separator } from "@/components/ui/separator";
import { vehiclesService } from "@/api/services/vehicles.service";
import type { VehicleMedia } from "@/api/types";
import { videoSchema, type VideoValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";
import { MediaUploader } from "./media-uploader";
import { MediaGrid } from "./media-grid";
import { ExpertiseUpload } from "./expertise-upload";

const RECOMMENDED_PHOTOS = 8;

export function StepMedia({ vehicle, onSaved, registerSave }: StepProps) {
  const [media, setMedia] = useState<VehicleMedia[]>(vehicle.media);
  const form = useForm<VideoValues>({
    resolver: zodResolver(videoSchema),
    defaultValues: { videoUrl: vehicle.videoUrl ?? "" },
  });

  // Fotoğraf işlemleri anında kaydedilir; adım değişirken yalnız video bağlantısı kaydedilir.
  useStepSave({
    form,
    registerSave,
    onSaved,
    submit: (v) => vehiclesService.updateVideo(vehicle.id, v.videoUrl.trim() || null),
  });

  const applyMedia = (next: VehicleMedia[]) => {
    setMedia(next);
    onSaved({ ...vehicle, media: next });
  };

  const run = async (action: () => Promise<{ data: VehicleMedia[]; message: string }>, quiet = false) => {
    try {
      const result = await action();
      applyMedia(result.data);
      if (!quiet) toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
      setMedia(vehicle.media);
    }
  };

  const handleReorder = (ordered: VehicleMedia[]) => {
    setMedia(ordered);
    run(() => vehiclesService.reorderMedia(vehicle.id, ordered.map((x) => x.id)), true);
  };

  return (
    <div className="flex flex-col gap-6">
      <MediaUploader vehicleId={vehicle.id} onUploaded={applyMedia} />
      {media.length > 0 && (
        <div className="flex flex-col gap-2">
          <FieldDescription>
            {media.length} fotoğraf. Sürükleyerek sıralayın, yıldız ile kapak seçin.
            {media.length < RECOMMENDED_PHOTOS && ` Yayın için en az ${RECOMMENDED_PHOTOS} fotoğraf önerilir.`}
          </FieldDescription>
          <MediaGrid
            media={media}
            onReorder={handleReorder}
            onSetCover={(item) => run(() => vehiclesService.setCover(vehicle.id, item.id))}
            onDelete={(item) => run(() => vehiclesService.deleteMedia(vehicle.id, item.id))}
          />
        </div>
      )}
      <Separator />
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()}>
          <InputField
            id="videoUrl"
            label="Video bağlantısı"
            description="İsteğe bağlı. YouTube bağlantısı araç sayfasına gömülür."
            placeholder="https://www.youtube.com/watch?v=..."
            error={form.formState.errors.videoUrl?.message}
            autoComplete="off"
            {...form.register("videoUrl")}
          />
        </form>
        <ExpertiseUpload
          vehicleId={vehicle.id}
          url={vehicle.expertiseReportUrl}
          onChange={(url) => onSaved({ ...vehicle, expertiseReportUrl: url })}
        />
      </div>
    </div>
  );
}
