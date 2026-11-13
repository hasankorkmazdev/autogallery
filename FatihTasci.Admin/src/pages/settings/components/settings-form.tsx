import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { InputField } from "@/components/ui/input-field";
import { TextareaField } from "@/components/form/form-fields";
import { settingsService } from "@/api/services/settings.service";
import type { SiteSettings } from "@/api/types";

// Kurallar sunucudaki SiteSettingsModelValidator ile aynıdır.
const httpsUrl = (v: string) => {
  if (v === "") return true;
  try {
    return new URL(v).protocol === "https:";
  } catch {
    return false;
  }
};
const optionalUrl = z.string().trim().refine(httpsUrl, { message: "Geçerli bir https bağlantısı girin." });
const coordinate = (max: number, message: string) =>
  z.string().trim().refine((v) => v === "" || (/^-?\d+(\.\d+)?$/.test(v) && Math.abs(Number(v)) <= max), { message });

const schema = z.object({
  galleryName: z.string().trim().min(1, "Galeri adı zorunludur.").max(80, "Galeri adı en fazla 80 karakter olabilir."),
  phone: z.string().trim().refine((v) => v === "" || /^[0-9 +()-]{10,20}$/.test(v), { message: "Geçerli bir telefon numarası girin." }),
  whatsApp: z.string().trim().refine((v) => v === "" || /^[0-9]{10,15}$/.test(v), {
    message: "WhatsApp numarasını ülke koduyla ve yalnızca rakamla girin (örn. 905321234567).",
  }),
  email: z.string().trim().refine((v) => v === "" || z.email().safeParse(v).success, { message: "Geçerli bir e-posta adresi girin." }),
  address: z.string().max(300, "Adres en fazla 300 karakter olabilir."),
  latitude: coordinate(90, "Enlem -90 ile 90 arasında, nokta ile yazılmalıdır."),
  longitude: coordinate(180, "Boylam -180 ile 180 arasında, nokta ile yazılmalıdır."),
  workingHours: z.string().max(200, "Çalışma saatleri en fazla 200 karakter olabilir."),
  instagram: optionalUrl,
  facebook: optionalUrl,
  youtube: optionalUrl,
  heroVideoUrl: optionalUrl,
});
type Values = z.infer<typeof schema>;

const toValues = (s: SiteSettings): Values => ({
  galleryName: s.galleryName ?? "",
  phone: s.phone ?? "",
  whatsApp: s.whatsApp ?? "",
  email: s.email ?? "",
  address: s.address ?? "",
  latitude: s.latitude ?? "",
  longitude: s.longitude ?? "",
  workingHours: s.workingHours ?? "",
  instagram: s.instagram ?? "",
  facebook: s.facebook ?? "",
  youtube: s.youtube ?? "",
  heroVideoUrl: s.heroVideoUrl ?? "",
});

const toSettings = (v: Values): SiteSettings =>
  Object.fromEntries(Object.entries(v).map(([key, value]) => [key, value.trim() || null])) as unknown as SiteSettings;

export function SettingsForm({ settings }: { settings: SiteSettings }) {
  const [saving, setSaving] = useState(false);
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: toValues(settings) });
  const { errors } = form.formState;

  const submit = async (values: Values) => {
    setSaving(true);
    try {
      const result = await settingsService.update(toSettings(values));
      form.reset(toValues(result.data));
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <form autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)} className="flex flex-col gap-4">
      <Card>
        <CardHeader>
          <CardTitle>Galeri ve iletişim</CardTitle>
          <CardDescription>Sitenin başlığında, alt bilgisinde ve araç sayfalarındaki butonlarda kullanılır.</CardDescription>
        </CardHeader>
        <CardContent className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <InputField id="galleryName" label="Galeri adı" required description="Site başlığı ve sekme adı." error={errors.galleryName?.message} {...form.register("galleryName")} />
          <InputField id="phone" label="Telefon" description="Ara butonunda kullanılır, örn. 0212 123 45 67." error={errors.phone?.message} {...form.register("phone")} />
          <InputField id="whatsApp" label="WhatsApp" description="Ülke koduyla, yalnız rakam: 905321234567." error={errors.whatsApp?.message} {...form.register("whatsApp")} />
          <InputField id="email" label="E-posta" description="Sitede iletişim adresi olarak görünür." error={errors.email?.message} {...form.register("email")} />
          <TextareaField id="address" label="Adres" rows={3} description="Alt bilgide ve iletişim sayfasında görünür." error={errors.address?.message} {...form.register("address")} />
          <TextareaField id="workingHours" label="Çalışma saatleri" rows={3} description="Örn. Pazartesi - Cumartesi 09:00 - 19:00." error={errors.workingHours?.message} {...form.register("workingHours")} />
          <InputField id="latitude" label="Enlem" description="Harita için, örn. 41.0082." error={errors.latitude?.message} {...form.register("latitude")} />
          <InputField id="longitude" label="Boylam" description="Harita için, örn. 28.9784." error={errors.longitude?.message} {...form.register("longitude")} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Sosyal medya ve görsel</CardTitle>
          <CardDescription>Bağlantılar https ile başlamalıdır.</CardDescription>
        </CardHeader>
        <CardContent className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <InputField id="instagram" label="Instagram" description="Profil bağlantısı." error={errors.instagram?.message} {...form.register("instagram")} />
          <InputField id="facebook" label="Facebook" description="Sayfa bağlantısı." error={errors.facebook?.message} {...form.register("facebook")} />
          <InputField id="youtube" label="YouTube" description="Kanal bağlantısı." error={errors.youtube?.message} {...form.register("youtube")} />
          <InputField id="heroVideoUrl" label="Ana sayfa videosu" description="Faz 2'deki sinematik ana sayfa için." error={errors.heroVideoUrl?.message} {...form.register("heroVideoUrl")} />
        </CardContent>
      </Card>
      <div className="flex justify-end">
        <Button type="submit" disabled={saving}>
          {saving ? "Kaydediliyor..." : "Ayarları kaydet"}
        </Button>
      </div>
    </form>
  );
}
