import { useEffect, useMemo, useRef, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ImagePlus } from "lucide-react";
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
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { InputField } from "@/components/ui/input-field";
import { Progress } from "@/components/ui/progress";
import { SwitchField } from "@/components/form/form-fields";
import { deliveriesService } from "@/api/services/deliveries.service";
import type { Delivery } from "@/api/types";

// Sunucudaki DeliveryService.MaxImageBytes ve AllowedImageExtensions ile aynı.
const ACCEPTED_IMAGES = ["image/jpeg", "image/png", "image/webp"];
const MAX_IMAGE_MB = 25;

/** Bugün, yerel saatle yyyy-MM-dd (tarih alanının biçimi). */
const today = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
};

// Sunucudaki SaveDeliveryRequestModelValidator ile aynı kurallar; fotoğraf eklemede zorunlu.
const schema = z
  .object({
    photo: z.custom<File | null>(),
    /** Düzenlenen teslimatın mevcut fotoğrafı; yeni fotoğraf seçilmezse o kalır. */
    currentPhotoUrl: z.string().nullable(),
    vehicleTitle: z.string().trim().min(1, "Araç adı zorunludur.").max(80, "Araç adı en fazla 80 karakter olabilir."),
    customerName: z.string().trim().max(60, "Müşteri adı en fazla 60 karakter olabilir."),
    city: z.string().trim().max(40, "Şehir en fazla 40 karakter olabilir."),
    deliveredAt: z
      .string()
      .min(1, "Teslim tarihi zorunludur.")
      .refine((value) => value <= today(), "Teslim tarihi bugünden sonra olamaz."),
    isActive: z.boolean(),
  })
  .superRefine((values, ctx) => {
    if (!values.photo && !values.currentPhotoUrl) {
      ctx.addIssue({ code: "custom", path: ["photo"], message: "Teslimat fotoğrafını seçin." });
    }
  });
type Values = z.input<typeof schema>;
type Output = z.output<typeof schema>;

export interface DeliveryDialogState {
  delivery: Delivery | null;
}

interface DeliveryDialogProps {
  state: DeliveryDialogState | null;
  onOpenChange: (open: boolean) => void;
  onSaved: (deliveries: Delivery[]) => void;
}

export function DeliveryDialog({ state, onOpenChange, onSaved }: DeliveryDialogProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [saving, setSaving] = useState(false);
  const [progress, setProgress] = useState(0);
  const form = useForm<Values, unknown, Output>({
    resolver: zodResolver(schema),
    defaultValues: { photo: null, currentPhotoUrl: null, vehicleTitle: "", customerName: "", city: "", deliveredAt: today(), isActive: true },
  });
  const { errors } = form.formState;
  const photo = form.watch("photo");
  const currentPhotoUrl = form.watch("currentPhotoUrl");

  // Seçilen fotoğrafın önizlemesi; başka fotoğraf seçilince ya da pencere kapanınca adres bırakılır.
  const selectedUrl = useMemo(() => (photo ? URL.createObjectURL(photo) : null), [photo]);
  useEffect(() => () => {
    if (selectedUrl) URL.revokeObjectURL(selectedUrl);
  }, [selectedUrl]);
  const previewUrl = selectedUrl ?? currentPhotoUrl;

  useEffect(() => {
    if (!state) return;
    const delivery = state.delivery;
    form.reset({
      photo: null,
      currentPhotoUrl: delivery?.mediumUrl ?? null,
      vehicleTitle: delivery?.vehicleTitle ?? "",
      customerName: delivery?.customerName ?? "",
      city: delivery?.city ?? "",
      deliveredAt: delivery?.deliveredAt.slice(0, 10) ?? today(),
      isActive: delivery?.isActive ?? true,
    });
  }, [state, form]);

  const pick = (fileList: FileList | null) => {
    const file = fileList?.[0];
    if (inputRef.current) inputRef.current.value = "";
    if (!file) return;
    if (!ACCEPTED_IMAGES.includes(file.type) || file.size > MAX_IMAGE_MB * 1024 * 1024) {
      form.setError("photo", { message: `Yalnızca ${MAX_IMAGE_MB} MB'a kadar JPG, PNG ve WebP yüklenebilir.` });
      return;
    }
    form.setValue("photo", file, { shouldValidate: true, shouldDirty: true });
  };

  const submit = async (values: Output) => {
    setSaving(true);
    setProgress(0);
    try {
      const request = {
        vehicleTitle: values.vehicleTitle,
        customerName: values.customerName,
        city: values.city,
        deliveredAt: values.deliveredAt,
        isActive: values.isActive,
      };
      const result = state?.delivery
        ? await deliveriesService.update(state.delivery.id, request, values.photo, setProgress)
        : await deliveriesService.create(request, values.photo, setProgress);
      toast.success(result.message);
      onSaved(result.data);
      onOpenChange(false);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  const renderSaving = () => {
    if (!saving) return null;
    const text = progress > 0 && progress < 100 ? `Yükleniyor... %${progress}` : "Kaydediliyor...";
    return (
      <div className="flex flex-col gap-2">
        <Progress value={progress} />
        <FieldDescription>{text}</FieldDescription>
      </div>
    );
  };

  return (
    <Dialog open={state !== null} onOpenChange={(open) => !saving && onOpenChange(open)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{state?.delivery ? "Teslimatı düzenle" : "Teslimat ekle"}</DialogTitle>
          <DialogDescription>
            Fotoğraf sitede ana sayfada ve Teslimatlar sayfasında yayınlanır; yüklemeden önce müşterinin iznini alın.
          </DialogDescription>
        </DialogHeader>
        <form id="delivery-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="delivery-photo">Fotoğraf</FieldLabel>
              <div
                className="flex items-start gap-4"
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => {
                  e.preventDefault();
                  if (!saving) pick(e.dataTransfer.files);
                }}>
                <div className="flex aspect-[4/5] w-28 shrink-0 items-center justify-center overflow-hidden rounded-md border border-dashed bg-muted">
                  {previewUrl ? (
                    <img src={previewUrl} alt="Teslimat fotoğrafı" className="size-full object-cover" />
                  ) : (
                    <ImagePlus className="size-6 text-muted-foreground" />
                  )}
                </div>
                <div className="flex flex-col items-start gap-2">
                  <Button type="button" variant="outline" size="sm" disabled={saving} onClick={() => inputRef.current?.click()}>
                    {previewUrl ? "Fotoğrafı değiştir" : "Fotoğraf seç"}
                  </Button>
                  <FieldDescription>
                    Sitede dikey (4:5) kırpılır, büyütünce tamamı görünür. JPG, PNG, WebP; en fazla {MAX_IMAGE_MB} MB. Sürükleyip
                    bırakabilirsiniz.
                  </FieldDescription>
                </div>
              </div>
              {errors.photo && <FieldError>{errors.photo.message}</FieldError>}
              <Input
                ref={inputRef}
                id="delivery-photo"
                type="file"
                accept={ACCEPTED_IMAGES.join(",")}
                className="hidden"
                onChange={(e) => pick(e.target.files)}
              />
            </Field>
            <InputField id="delivery-vehicle" label="Araç" required
              description="Sitede fotoğrafın altında yazar. Örn. 2021 BMW 320i M Sport."
              error={errors.vehicleTitle?.message} autoComplete="off" {...form.register("vehicleTitle")} />
            <InputField id="delivery-customer" label="Müşteri adı"
              description="İsteğe bağlı. Ad ve soyadın baş harfi önerilir: Ahmet B."
              error={errors.customerName?.message} autoComplete="off" {...form.register("customerName")} />
            <InputField id="delivery-city" label="Şehir"
              description="İsteğe bağlı. Müşterinin şehri, örn. İstanbul."
              error={errors.city?.message} autoComplete="off" {...form.register("city")} />
            <InputField id="delivery-date" type="date" label="Teslim tarihi" required
              description="Teslimatlar sitede bu tarihe göre sıralanır (en yeni önce)."
              error={errors.deliveredAt?.message} autoComplete="off" {...form.register("deliveredAt")} />
            <Controller control={form.control} name="isActive" render={({ field }) => (
              <SwitchField id="delivery-active" label="Sitede göster"
                description="Kapalıysa teslimat sitede görünmez."
                checked={field.value} onChange={field.onChange} />
            )} />
          </FieldGroup>
        </form>
        {renderSaving()}
        <DialogFooter>
          <Button type="button" variant="outline" disabled={saving} onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="delivery-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
