import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useSearchParams } from "react-router-dom";
import { FieldGroup } from "@/components/ui/field";
import { NumberField, RichTextField, SwitchField, TextSelectField } from "@/components/form/form-fields";
import { vehiclesService } from "@/api/services/vehicles.service";
import { usersService } from "@/api/services/users.service";
import type { Advisor } from "@/api/types";
import { useQuery } from "@/hooks/use-query";
import { pricingSchema, type PricingValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";
import { PublishPanel } from "./publish-panel";
import { formatPrice } from "@/lib/format";

interface StepPricingProps extends StepProps {
  beforeStatusChange: () => Promise<boolean>;
}

// Seçili danışmana göre sitede hangi numaranın kullanılacağını anlatır.
function advisorDescription(advisor: Advisor | undefined) {
  if (!advisor) return "Sitede ilanın iletişim kutusunda adı görünür; \"Ara\" ve WhatsApp danışmanın telefonuna gider.";
  if (!advisor.isActive) return "Bu kullanıcı pasif, sitede gösterilmez. Aktif bir danışman seçin.";
  if (!advisor.phone) return "Bu kullanıcının iletişim telefonu yok, sitede galeri telefonu kullanılır. Telefon Kullanıcılar sayfasından eklenir.";
  return `Sitede "Ara" ${advisor.phone} numarasını arar; cep telefonuysa WhatsApp da bu numaraya gider.`;
}

export function StepPricing({ vehicle, onSaved, registerSave, beforeStatusChange }: StepPricingProps) {
  const [, setSearchParams] = useSearchParams();
  const advisors = useQuery("advisors", usersService.getAdvisors).data;
  const form = useForm<PricingValues>({
    resolver: zodResolver(pricingSchema),
    defaultValues: {
      price: vehicle.price,
      campaignPrice: vehicle.campaignPrice,
      showPrice: vehicle.showPrice,
      description: vehicle.description ?? "",
      advisorId: vehicle.advisorId,
      isFeatured: vehicle.isFeatured,
      featuredOrder: vehicle.featuredOrder,
      isOpportunity: vehicle.isOpportunity,
    },
  });
  const { errors } = form.formState;
  const isFeatured = useWatch({ control: form.control, name: "isFeatured" });
  const [price, campaignPrice] = useWatch({ control: form.control, name: ["price", "campaignPrice"] });
  const discount = price && campaignPrice && campaignPrice > 0 && campaignPrice < price ? price - campaignPrice : null;
  const advisorId = useWatch({ control: form.control, name: "advisorId" });
  // Seçenekler aktif kullanıcılar; araca önceden atanmış pasif danışman da adıyla görünür.
  const advisorOptions = (advisors ?? [])
    .filter((a) => a.isActive || a.id === vehicle.advisorId)
    .map((a) => ({ value: a.id, label: a.isActive ? a.fullName : `${a.fullName} (pasif)` }));

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
        advisorId: v.advisorId,
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
          <Controller control={form.control} name="description" render={({ field }) => (
            <RichTextField id="description" label="Açıklama" placeholder="Aracın hikâyesi, bakım geçmişi, ekstra bilgiler…"
              description="Sitedeki görünümüyle yazılır: renk, başlık, ortalama ve liste araç çubuğundan."
              error={errors.description?.message} value={field.value} onChange={field.onChange} />
          )} />
          <Controller control={form.control} name="advisorId" render={({ field }) => (
            <TextSelectField id="advisorId" label="Danışman" required options={advisorOptions}
              placeholder={advisors ? "Danışman seçin" : "Yükleniyor..."}
              description={advisorDescription(advisors?.find((a) => a.id === advisorId))}
              error={errors.advisorId?.message} value={field.value} onChange={field.onChange} />
          )} />
          <Controller control={form.control} name="isFeatured" render={({ field }) => (
            <SwitchField id="isFeatured" label="Vitrinde öne çıkar"
              description="Ana sayfadaki araç kaydırıcısının &quot;Vitrindekiler&quot; sekmesinde gösterilir (en çok 8 araç; eksikse en yeniler eklenir)." checked={field.value} onChange={field.onChange} />
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
              description="Ana sayfadaki araç kaydırıcısının &quot;Fırsat Araçları&quot; sekmesinde gösterilir." checked={field.value} onChange={field.onChange} />
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
