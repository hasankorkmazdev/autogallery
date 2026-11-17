import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useSearchParams } from "react-router-dom";
import { FieldGroup } from "@/components/ui/field";
import { NumberField, SwitchField, TextareaField } from "@/components/form/form-fields";
import { InputField } from "@/components/ui/input-field";
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
      contactName: vehicle.contactName ?? "",
      contactPhone: vehicle.contactPhone ?? "",
      isFeatured: vehicle.isFeatured,
      featuredOrder: vehicle.featuredOrder,
      isOpportunity: vehicle.isOpportunity,
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
        contactName: v.contactName || null,
        contactPhone: v.contactPhone || null,
        isFeatured: v.isFeatured,
        featuredOrder: v.featuredOrder,
        isOpportunity: v.isOpportunity,
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
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <InputField id="contactName" label="İlan yetkilisi" maxLength={80} autoComplete="off"
              description="Sitede ilanın iletişim kutusunda görünür, örn. Ahmet Yılmaz."
              error={errors.contactName?.message} {...form.register("contactName")} />
            <InputField id="contactPhone" label="Yetkili telefonu" type="tel" maxLength={20} autoComplete="off"
              description="Boşsa galeri telefonu kullanılır. Cep telefonuysa WhatsApp da bu numaraya gider."
              error={errors.contactPhone?.message} {...form.register("contactPhone")} />
          </div>
          <Controller control={form.control} name="isFeatured" render={({ field }) => (
            <SwitchField id="isFeatured" label="Vitrinde öne çıkar"
              description="Ana sayfanın ilk ekranındaki vitrin mozaiğinde gösterilir (en çok 5 araç; eksikse en yeniler eklenir)." checked={field.value} onChange={field.onChange} />
          )} />
          {isFeatured && (
            <Controller control={form.control} name="featuredOrder" render={({ field }) => (
              <NumberField id="featuredOrder" label="Vitrin sırası" min={0} max={100} thousandSeparator={false}
                description="Küçük sayı önce gösterilir." error={errors.featuredOrder?.message}
                value={field.value} onChange={(v) => field.onChange(v ?? 0)} />
            )} />
          )}
          <Controller control={form.control} name="isOpportunity" render={({ field }) => (
            <SwitchField id="isOpportunity" label="Fırsat aracı"
              description="Ana sayfadaki &quot;Fırsat Araçları&quot; şeridinde adı ve fiyatıyla akar." checked={field.value} onChange={field.onChange} />
          )} />
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
