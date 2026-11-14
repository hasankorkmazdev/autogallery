import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { FieldGroup } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { SwitchField } from "@/components/form/form-fields";
import { showroomService } from "@/api/services/showroom.service";
import type { ShowroomScene } from "@/api/types";

// Sunucudaki SaveShowroomSceneRequestModelValidator ile aynı kurallar.
const schema = z.object({
  tag: z.string().trim().min(1, "Etiket zorunludur.").max(40, "Etiket en fazla 40 karakter olabilir."),
  title: z.string().trim().min(1, "Başlık zorunludur.").max(80, "Başlık en fazla 80 karakter olabilir."),
  isActive: z.boolean(),
});
type Values = z.input<typeof schema>;
type Output = z.output<typeof schema>;

export interface SceneDialogState {
  scene: ShowroomScene | null;
}

interface SceneDialogProps {
  state: SceneDialogState | null;
  onOpenChange: (open: boolean) => void;
  onSaved: (scenes: ShowroomScene[]) => void;
}

export function SceneDialog({ state, onOpenChange, onSaved }: SceneDialogProps) {
  const [saving, setSaving] = useState(false);
  const form = useForm<Values, unknown, Output>({
    resolver: zodResolver(schema),
    defaultValues: { tag: "", title: "", isActive: true },
  });
  const { errors } = form.formState;

  useEffect(() => {
    if (!state) return;
    form.reset({
      tag: state.scene?.tag ?? "",
      title: state.scene?.title ?? "",
      isActive: state.scene?.isActive ?? true,
    });
  }, [state, form]);

  const submit = async (values: Output) => {
    setSaving(true);
    try {
      const request = { tag: values.tag, title: values.title, isActive: values.isActive };
      const result = state?.scene
        ? await showroomService.update(state.scene.id, request)
        : await showroomService.create(request);
      toast.success(result.message);
      onSaved(result.data);
      onOpenChange(false);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={state !== null} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{state?.scene ? "Sahneyi düzenle" : "Sahne ekle"}</DialogTitle>
          <DialogDescription>Fotoğrafı sahneyi kaydettikten sonra karttan yükleyebilirsiniz.</DialogDescription>
        </DialogHeader>
        <form id="scene-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <InputField id="scene-tag" label="Etiket" required
              description="Kısa ve büyük harfle gösterilir; dar panelde dikey yazılır. Örn. Sergi salonu."
              error={errors.tag?.message} autoComplete="off" {...form.register("tag")} />
            <InputField id="scene-title" label="Başlık" required
              description="Sahne açıkken fotoğrafın altında görünür. Örn. Işıklandırılmış Sergi Salonu."
              error={errors.title?.message} autoComplete="off" {...form.register("title")} />
            <Controller control={form.control} name="isActive" render={({ field }) => (
              <SwitchField id="scene-active" label="Sitede göster"
                description="Kapalıysa sahne fotoğrafı olsa da sitede görünmez."
                checked={field.value} onChange={field.onChange} />
            )} />
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="scene-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
