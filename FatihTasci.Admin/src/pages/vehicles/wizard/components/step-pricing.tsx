import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useSearchParams } from "react-router-dom";
import { FieldGroup } from "@/components/ui/field";
import { NumberField, SwitchField, TextareaField } from "@/components/form/form-fields";
import { vehiclesService } from "@/api/services/vehicles.service";
import { pricingSchema, type PricingValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";
import { PublishPanel } from "./publish-panel";
import { formatPrice } from "@/lib/format";

interface StepPricingProps extends StepProps {
  beforeStatusChange: () => Promise<boolean>;
}

export function StepPricing({ vehicle, onSaved, registerSave, beforeStatusChange }: StepPricingProps) {
  const [, setSearchParams] = useSearchParams();
  const form = useForm<PricingValues>({
    resolver: zodResolver(pricingSchema),
    defaultValues: {
      price: vehicle.price,
      campaignPrice: vehicle.campaignPrice,
      showPrice: vehicle.showPrice,
      description: vehicle.description ?? "",
      isFeatured: vehicle.isFeatured,
      featuredOrder: vehicle.featuredOrder,
    },
  });
  const { errors } = form.formState;
  const isFeatured = useWatch({ control: form.control, name: "isFeatured" });
  const [price, campaignPrice] = useWatch({ control: form.control, name: ["price", "campaignPrice"] });
  const discount = price && campaignPrice && campaignPrice > 0 && campaignPrice < price ? price - campaignPrice : null;

  useStepSave({
    form,
    registerSave,
    onSaved,
    submit: (v) =>
      vehiclesService.updatePricing(vehicle.id, {
        price: v.price,
        campaignPrice: v.campaignPrice,
        showPrice: v.showPrice,
        description: v.description.trim() || null,
        isFeatured: v.isFeatured,
        featuredOrder: v.featuredOrder,
      }),
  });

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_360px]">
      <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()}>
        <FieldGroup>
          <Controller control={form.control} name="price" render={({ field }) => (
            <NumberField id="price" label="Fiyat" min={0} step={10000} suffix=" TL"
              description="Yayınlamak için zorunlu ve 0'dan büyük olmalı. &quot;Fiyat için arayınız&quot; seçili olsa da panelde tutulur."
              error={errors.price?.message} value={field.value} onChange={field.onChange} />
          )} />
          <Controller control={form.control} name="campaignPrice" render={({ field }) => (
            <NumberField id="campaignPrice" label="Kampanyalı fiyat" min={0} step={10000} suffix=" TL"
              description={discount
                ? `Sitede eski fiyatın üstü çizilir ve ${formatPrice(discount)} indirim yazar.`
                : "Boş bırakılırsa kampanya yok. Doluysa sitede eski fiyatın üstü çizilir; sıralama ve fiyat filtresi bu fiyatı kullanır."}
              error={errors.campaignPrice?.message} value={field.value} onChange={field.onChange} />
          )} />
          <Controller control={form.control} name="showPrice" render={({ field }) => (
            <SwitchField id="showPrice" label="Fiyatı sitede göster"
              description="Kapalıysa sitede &quot;Fiyat için arayınız&quot; yazar." checked={field.value} onChange={field.onChange} />
          )} />
          <TextareaField id="description" label="Açıklama" rows={8} maxLength={10000}
            description="Aracın hikâyesi, bakım geçmişi, ekstra bilgiler. Satır sonları korunur."
            error={errors.description?.message} {...form.register("description")} />
          <Controller control={form.control} name="isFeatured" render={({ field }) => (
            <SwitchField id="isFeatured" label="Vitrinde öne çıkar"
              description="Ana sayfadaki &quot;Öne Çıkanlar&quot; bölümünde gösterilir." checked={field.value} onChange={field.onChange} />
          )} />
          {isFeatured && (
            <Controller control={form.control} name="featuredOrder" render={({ field }) => (
              <NumberField id="featuredOrder" label="Vitrin sırası" min={0} max={100} thousandSeparator={false}
                description="Küçük sayı önce gösterilir." error={errors.featuredOrder?.message}
                value={field.value} onChange={(v) => field.onChange(v ?? 0)} />
            )} />
          )}
        </FieldGroup>
      </form>
      <PublishPanel
        vehicle={vehicle}
        onSaved={onSaved}
        beforeStatusChange={beforeStatusChange}
        onGoToStep={async (step) => {
          if (await beforeStatusChange()) setSearchParams({ step: String(step) });
        }}
      />
    </div>
  );
}
