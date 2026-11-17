import { useState } from "react";
import { Controller, useForm, type FieldErrors } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { InputField } from "@/components/ui/input-field";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { SwitchField, TextareaField } from "@/components/form/form-fields";
import { settingsService } from "@/api/services/settings.service";
import type { SiteSettings } from "@/api/types";
import { LogoCard } from "./logo-card";
import { SocialLinksCard } from "./social-links-card";
import { WatermarkCard } from "./watermark-card";
import {
  FIELD_TABS,
  settingsSchema,
  toSettingsValues,
  toSiteSettings,
  type SettingsOutput,
  type SettingsTab,
  type SettingsValues,
} from "./settings-schema";

const TABS: { value: SettingsTab; label: string }[] = [
  { value: "company", label: "Firma ve iletişim" },
  { value: "location", label: "Konum" },
  { value: "social", label: "Sosyal medya" },
  { value: "media", label: "Logo ve görsel" },
];

interface SettingsTabsProps {
  settings: SiteSettings;
  onChanged: (settings: SiteSettings) => void;
}

export function SettingsTabs({ settings, onChanged }: SettingsTabsProps) {
  const [tab, setTab] = useState<SettingsTab>("company");
  const [saving, setSaving] = useState(false);
  const form = useForm<SettingsValues, unknown, SettingsOutput>({ resolver: zodResolver(settingsSchema), defaultValues: toSettingsValues(settings) });
  const { errors } = form.formState;

  const submit = async (values: SettingsOutput) => {
    setSaving(true);
    try {
      const result = await settingsService.update(toSiteSettings(values));
      form.reset(toSettingsValues(result.data));
      onChanged(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  // Tüm sekmeler tek formdur; hata başka sekmedeyse o sekmeye geçilir.
  const invalid = (fieldErrors: FieldErrors<SettingsValues>) => {
    const first = (Object.keys(fieldErrors) as (keyof SettingsValues)[])[0];
    if (!first) return;
    const target = FIELD_TABS[first];
    if (target !== tab) {
      setTab(target);
      toast.error(`"${TABS.find((x) => x.value === target)?.label}" sekmesinde düzeltilmesi gereken alan var.`);
    }
  };

  const saveButton = (
    <div className="flex justify-end">
      <Button type="submit" disabled={saving}>
        {saving ? "Kaydediliyor..." : "Ayarları kaydet"}
      </Button>
    </div>
  );

  const onSubmit = form.handleSubmit(submit, invalid);

  return (
    <Tabs value={tab} onValueChange={(v) => setTab(v as SettingsTab)}>
      {/* Telefonda dört sekme 2×2 ızgarada, sm'den itibaren tek satır. */}
      <TabsList className="grid h-auto w-full grid-cols-2 sm:inline-flex sm:h-9 sm:w-auto">
        {TABS.map((x) => (
          <TabsTrigger key={x.value} value={x.value}>
            {x.label}
          </TabsTrigger>
        ))}
      </TabsList>

      <TabsContent value="company">
        <form autoComplete="off" noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
          <Card>
            <CardHeader>
              <CardTitle>Firma ve iletişim</CardTitle>
              <CardDescription>Sitenin başlığında, alt bilgisinde ve araç sayfalarındaki butonlarda kullanılır.</CardDescription>
            </CardHeader>
            <CardContent className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <InputField id="galleryName" label="Firma (galeri) adı" required description="Site başlığı, sekme adı ve panel başlığı." error={errors.galleryName?.message} {...form.register("galleryName")} />
              <InputField id="phone" label="Telefon" description="Ara butonunda kullanılır, örn. 0212 123 45 67." error={errors.phone?.message} {...form.register("phone")} />
              <InputField id="whatsApp" label="WhatsApp" description="Ülke koduyla, yalnız rakam: 905321234567." error={errors.whatsApp?.message} {...form.register("whatsApp")} />
              <InputField id="email" label="E-posta" description="Sitede iletişim adresi olarak görünür." error={errors.email?.message} {...form.register("email")} />
              <TextareaField id="workingHours" label="Çalışma saatleri" rows={3} description="Örn. Pazartesi - Cumartesi 09:00 - 19:00." error={errors.workingHours?.message} {...form.register("workingHours")} />
            </CardContent>
          </Card>
          {saveButton}
        </form>
      </TabsContent>

      <TabsContent value="location">
        <form autoComplete="off" noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
          <Card>
            <CardHeader>
              <CardTitle>Konum</CardTitle>
              <CardDescription>Ana sayfa ve iletişim sayfasındaki harita ile yol tarifi bağlantıları için. Enlem ve boylam boşsa harita gösterilmez.</CardDescription>
            </CardHeader>
            <CardContent className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <TextareaField id="address" label="Adres" rows={3} description="Alt bilgide ve iletişim sayfasında görünür." error={errors.address?.message} {...form.register("address")} />
              <div className="flex flex-col gap-4">
                <InputField id="latitude" label="Enlem" description="Google Haritalar'da konuma sağ tıklayınca çıkan ilk sayı, örn. 41.0082." error={errors.latitude?.message} {...form.register("latitude")} />
                <InputField id="longitude" label="Boylam" description="İkinci sayı, örn. 28.9784." error={errors.longitude?.message} {...form.register("longitude")} />
              </div>
            </CardContent>
          </Card>
          {saveButton}
        </form>
      </TabsContent>

      <TabsContent value="social">
        <form autoComplete="off" noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
          <SocialLinksCard form={form} />
          {saveButton}
        </form>
      </TabsContent>

      <TabsContent value="media">
        <div className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <LogoCard settings={settings} onChanged={onChanged} />
            <WatermarkCard />
          </div>
          <form autoComplete="off" noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
            <Card>
              <CardHeader>
                <CardTitle>Görsel ayarları</CardTitle>
                <CardDescription>Filigran. Ana sayfa videosu Site İçeriği → Ana Sayfa'dan değişir.</CardDescription>
              </CardHeader>
              <CardContent className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <Controller control={form.control} name="watermarkEnabled" render={({ field }) => (
                  <SwitchField id="watermarkEnabled" label="Fotoğraflara filigran bas"
                    description="Logo, araç fotoğraflarının sağ alt köşesine yarı saydam basılır (liste küçük resimleri hariç). Logo yoksa basılmaz."
                    checked={field.value} onChange={field.onChange} />
                )} />
              </CardContent>
            </Card>
            {saveButton}
          </form>
        </div>
      </TabsContent>
    </Tabs>
  );
}
