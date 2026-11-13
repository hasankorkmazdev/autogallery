import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
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
import { InputField } from "@/components/ui/input-field";

const schema = z.object({
  name: z.string().trim().min(1, "Ad zorunludur.").max(120, "Ad en fazla 120 karakter olabilir."),
});
type Values = z.infer<typeof schema>;

interface CatalogQuickAddDialogProps {
  open: boolean;
  title: string;
  description: string;
  fieldLabel: string;
  fieldDescription: string;
  onOpenChange: (open: boolean) => void;
  onSubmit: (name: string) => Promise<void>;
}

// Araç girerken listede olmayan marka / seri / modeli anında ekleme penceresi.
export function CatalogQuickAddDialog({
  open,
  title,
  description,
  fieldLabel,
  fieldDescription,
  onOpenChange,
  onSubmit,
}: CatalogQuickAddDialogProps) {
  const [saving, setSaving] = useState(false);
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { name: "" } });

  useEffect(() => {
    if (open) form.reset({ name: "" });
  }, [open, form]);

  const submit = async (values: Values) => {
    setSaving(true);
    try {
      await onSubmit(values.name);
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
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <form
          id="catalog-quick-add"
          autoComplete="off"
          noValidate
          onSubmit={(e) => {
            e.stopPropagation();
            form.handleSubmit(submit)(e);
          }}>
          <InputField
            id="quick-add-name"
            label={fieldLabel}
            description={fieldDescription}
            error={form.formState.errors.name?.message}
            autoComplete="off"
            autoFocus
            {...form.register("name")}
          />
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="catalog-quick-add" disabled={saving}>
            {saving ? "Ekleniyor..." : "Ekle"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
