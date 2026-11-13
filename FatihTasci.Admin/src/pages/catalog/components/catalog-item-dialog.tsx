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
import { NumberField, SwitchField } from "@/components/form/form-fields";

const schema = z.object({
  name: z.string().trim().min(1, "Ad zorunludur.").max(120, "Ad en fazla 120 karakter olabilir."),
  sortOrder: z.number().min(0, "Sıra 0 ya da daha büyük olmalıdır."),
  isActive: z.boolean(),
});
export type CatalogItemValues = z.infer<typeof schema>;

interface CatalogItemDialogProps {
  open: boolean;
  title: string;
  initial: CatalogItemValues | null;
  showSortOrder: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (values: CatalogItemValues) => Promise<string>;
}

export function CatalogItemDialog({ open, title, initial, showSortOrder, onOpenChange, onSubmit }: CatalogItemDialogProps) {
  const [saving, setSaving] = useState(false);
  const form = useForm<CatalogItemValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", sortOrder: 0, isActive: true },
  });

  useEffect(() => {
    if (open) form.reset(initial ?? { name: "", sortOrder: 0, isActive: true });
  }, [open, initial, form]);

  const submit = async (values: CatalogItemValues) => {
    setSaving(true);
    try {
      toast.success(await onSubmit({ ...values, name: values.name.trim() }));
      onOpenChange(false);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>Adres (slug) addan otomatik üretilir. Ad değişirse eski adres yeni adrese yönlendirilir.</DialogDescription>
        </DialogHeader>
        <form id="catalog-item-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <InputField id="catalog-name" label="Ad" description="Sitede ve panelde görünen ad."
              error={form.formState.errors.name?.message} autoComplete="off" {...form.register("name")} />
            {showSortOrder && (
              <Controller control={form.control} name="sortOrder" render={({ field }) => (
                <NumberField id="catalog-sort" label="Sıra" min={0} max={1000} thousandSeparator={false}
                  description="Küçük sayı listede önce gelir." error={form.formState.errors.sortOrder?.message}
                  value={field.value} onChange={(v) => field.onChange(v ?? 0)} />
              )} />
            )}
            <Controller control={form.control} name="isActive" render={({ field }) => (
              <SwitchField id="catalog-active" label="Aktif" description="Pasif kayıtlar araç girerken listede görünmez."
                checked={field.value} onChange={field.onChange} />
            )} />
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="catalog-item-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
