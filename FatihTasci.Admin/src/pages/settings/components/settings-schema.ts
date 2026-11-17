import { z } from "zod";
import type { SiteSettings } from "@/api/types";

// Kurallar sunucudaki SiteSettingsModelValidator / SocialLinkModelValidator ile aynıdır.
export const MAX_SOCIAL_LINKS = 12;

const isHttpsUrl = (v: string) => {
  try {
    return new URL(v).protocol === "https:";
  } catch {
    return false;
  }
};
const coordinate = (max: number, message: string) =>
  z.string().trim().refine((v) => v === "" || (/^-?\d+(\.\d+)?$/.test(v) && Math.abs(Number(v)) <= max), { message });

const socialLinkSchema = z.object({
  platform: z.number({ message: "Platform seçin." }).nullable().refine((v) => v !== null, { message: "Platform seçin." }),
  url: z
    .string()
    .trim()
    .min(1, "Bağlantı zorunludur.")
    .max(300, "Bağlantı en fazla 300 karakter olabilir.")
    .refine(isHttpsUrl, { message: "Geçerli bir https bağlantısı girin." }),
});

export const settingsSchema = z.object({
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
  socialLinks: z.array(socialLinkSchema).max(MAX_SOCIAL_LINKS, `En fazla ${MAX_SOCIAL_LINKS} bağlantı eklenebilir.`),
  watermarkEnabled: z.boolean(),
});
export type SettingsValues = z.input<typeof settingsSchema>;
export type SettingsOutput = z.output<typeof settingsSchema>;

export type SettingsTab = "company" | "location" | "social" | "media";

/** Hatalı alanın hangi sekmede olduğu: kaydederken hata başka sekmedeyse oraya geçilir. */
export const FIELD_TABS: Record<keyof SettingsValues, SettingsTab> = {
  galleryName: "company",
  phone: "company",
  whatsApp: "company",
  email: "company",
  workingHours: "company",
  address: "location",
  latitude: "location",
  longitude: "location",
  socialLinks: "social",
  watermarkEnabled: "media",
};

export const toSettingsValues = (s: SiteSettings): SettingsValues => ({
  galleryName: s.galleryName ?? "",
  phone: s.phone ?? "",
  whatsApp: s.whatsApp ?? "",
  email: s.email ?? "",
  address: s.address ?? "",
  latitude: s.latitude ?? "",
  longitude: s.longitude ?? "",
  workingHours: s.workingHours ?? "",
  socialLinks: (s.socialLinks ?? []).map((x) => ({ platform: x.platform, url: x.url })),
  watermarkEnabled: s.watermarkEnabled,
});

const textOrNull = (v: string) => v.trim() || null;

export const toSiteSettings = (v: SettingsOutput): SiteSettings => ({
  galleryName: textOrNull(v.galleryName),
  phone: textOrNull(v.phone),
  whatsApp: textOrNull(v.whatsApp),
  email: textOrNull(v.email),
  address: textOrNull(v.address),
  latitude: textOrNull(v.latitude),
  longitude: textOrNull(v.longitude),
  workingHours: textOrNull(v.workingHours),
  socialLinks: v.socialLinks.map((x) => ({ platform: x.platform, url: x.url.trim() })),
  watermarkEnabled: v.watermarkEnabled,
});
