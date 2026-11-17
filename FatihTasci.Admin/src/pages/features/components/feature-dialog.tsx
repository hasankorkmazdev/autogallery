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
import { SelectField, SwitchField } from "@/components/form/form-fields";
import { catalogService } from "@/api/services/catalog.service";
import type { Feature } from "@/api/types";
import { useLookupStore } from "@/store/lookup-store";

const schema = z.object({
  category: z.number().nullable().refine((v) => v !== null, { message: "Kategori seçin." }),
  name: z.string().trim().min(1, "Donanım adı zorunludur.").max(80, "Donanım adı en fazla 80 karakter olabilir."),
  isActive: z.boolean(),
});
type Values = z.input<typeof schema>;
type Output = z.output<typeof schema>;

export interface FeatureDialogState {
  feature: Feature | null;
  category: number | null;
}

interface FeatureDialogProps {
  state: FeatureDialogState | null;
  onOpenChange: (open: boolean) => void;
  onSaved: () => void;
}

export function FeatureDialog({ state, onOpenChange, onSaved }: FeatureDialogProps) {
  // Selector'da `?? []` kullanılmaz: her çağrıda yeni dizi döner ve sonsuz render'a yol açar.
  const categories = useLookupStore((s) => s.lookups?.featureCategory) ?? [];
  const [saving, setSaving] = useState(false);
  const form = useForm<Values, unknown, Output>({
    resolver: zodResolver(schema),
    defaultValues: { category: null, name: "", isActive: true },
  });
  const { errors } = form.formState;

  useEffect(() => {
    if (!state) return;
    const f = state.feature;
    form.reset({
      category: f?.category ?? state.category,
      name: f?.name ?? "",
      isActive: f?.isActive ?? true,
    });
  }, [state, form]);

  const submit = async (values: Output) => {
    setSaving(true);
    try {
      const request = { category: values.category!, name: values.name.trim(), isActive: values.isActive };
      const result = state?.feature
        ? await catalogService.updateFeature(state.feature.id, request)
        : await catalogService.createFeature(request);
      toast.success(result.message);
      onSaved();
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
          <DialogTitle>{state?.feature ? "Donanım düzenle" : "Donanım ekle"}</DialogTitle>
          <DialogDescription>
            Donanımlar araç sihirbazının 5. adımında onay kutusu olarak listelenir. Yeni donanım kategorinin sonuna eklenir;
            sırasını listede sürükleyerek değiştirin.
          </DialogDescription>
        </DialogHeader>
        <form id="feature-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <Controller control={form.control} name="category" render={({ field }) => (
              <SelectField id="feature-category" label="Kategori" required options={categories}
                description="Sitede donanım bu başlık altında görünür. Kategori değişirse yeni kategorinin sonuna geçer." error={errors.category?.message}
                value={field.value} onChange={field.onChange} />
            )} />
            <InputField id="feature-name" label="Ad" description="Örn. Adaptif hız sabitleyici."
              error={errors.name?.message} autoComplete="off" {...form.register("name")} />
            <Controller control={form.control} name="isActive" render={({ field }) => (
              <SwitchField id="feature-active" label="Aktif" description="Pasif donanım sihirbazda ve sitede görünmez."
                checked={field.value} onChange={field.onChange} />
            )} />
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="feature-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
