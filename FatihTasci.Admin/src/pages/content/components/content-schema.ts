import { z } from "zod";
import type { SiteContent, SiteServiceTypeValue } from "@/api/types";

// Kurallar sunucudaki SiteContentModelValidator / ServiceContentModelValidator / StatItemModelValidator ile aynıdır.
export const MAX_STATS = 6;
export const MAX_SERVICE_SUMMARY = 300;
export const MAX_SERVICE_BODY = 3000;

const serviceSchema = z.object({
  type: z.custom<SiteServiceTypeValue>(),
  title: z.string(),
  path: z.string(),
  summary: z
    .string()
    .trim()
    .min(1, "Kısa yazı zorunludur.")
    .max(MAX_SERVICE_SUMMARY, `Kısa yazı en fazla ${MAX_SERVICE_SUMMARY} karakter olabilir.`),
  body: z.string().max(MAX_SERVICE_BODY, `Sayfa metni en fazla ${MAX_SERVICE_BODY} karakter olabilir.`),
});

const statSchema = z.object({
  value: z.string().trim().min(1, "Rakam zorunludur.").max(12, "Rakam en fazla 12 karakter olabilir."),
  label: z.string().trim().min(1, "Açıklama zorunludur.").max(40, "Açıklama en fazla 40 karakter olabilir."),
});

export const contentSchema = z.object({
  services: z.array(serviceSchema),
  corporateIntro: z.string().max(500, "Giriş yazısı en fazla 500 karakter olabilir."),
  corporateAbout: z.string().max(3000, "Hakkımızda metni en fazla 3000 karakter olabilir."),
  corporateVision: z.string().max(1000, "Vizyon metni en fazla 1000 karakter olabilir."),
  corporateMission: z.string().max(1000, "Misyon metni en fazla 1000 karakter olabilir."),
  stats: z.array(statSchema).max(MAX_STATS, `En fazla ${MAX_STATS} rakam eklenebilir.`),
});
export type ContentValues = z.input<typeof contentSchema>;
export type ContentOutput = z.output<typeof contentSchema>;

/** "home" formun parçası değildir: ana sayfa videosu anında yüklenir. */
export type ContentTab = "home" | "services" | "corporate" | "stats";

/** Hatalı alanın hangi sekmede olduğu: kaydederken hata başka sekmedeyse oraya geçilir. */
export const FIELD_TABS: Record<keyof ContentValues, ContentTab> = {
  services: "services",
  corporateIntro: "corporate",
  corporateAbout: "corporate",
  corporateVision: "corporate",
  corporateMission: "corporate",
  stats: "stats",
};

export const toContentValues = (c: SiteContent): ContentValues => ({
  services: c.services.map((x) => ({
    type: x.type,
    title: x.title ?? "",
    path: x.path ?? "",
    summary: x.summary ?? "",
    body: x.body ?? "",
  })),
  corporateIntro: c.corporateIntro ?? "",
  corporateAbout: c.corporateAbout ?? "",
  corporateVision: c.corporateVision ?? "",
  corporateMission: c.corporateMission ?? "",
  stats: c.stats.map((x) => ({ value: x.value, label: x.label })),
});

export const toSiteContent = (v: ContentOutput): SiteContent => ({
  services: v.services.map((x) => ({ type: x.type, summary: x.summary, body: x.body.trim() })),
  corporateIntro: v.corporateIntro.trim(),
  corporateAbout: v.corporateAbout.trim(),
  corporateVision: v.corporateVision.trim(),
  corporateMission: v.corporateMission.trim(),
  stats: v.stats.map((x) => ({ value: x.value, label: x.label })),
});
