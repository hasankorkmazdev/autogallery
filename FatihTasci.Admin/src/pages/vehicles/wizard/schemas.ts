import { z } from "zod";
import { FuelType, VehicleCondition } from "@/api/types";

// Kurallar sunucudaki FluentValidation validator'larıyla aynıdır
// (FatihTasci.Core/Models/Domain/Vehicle/Vehicle*RequestModel.cs). Biri değişirse diğeri de güncellenir.

export const MIN_YEAR = 1950;
export const MAX_YEAR = new Date().getFullYear() + 1;
export const MAX_KILOMETER = 2_000_000;
export const MAX_NEW_VEHICLE_KILOMETER = 100;

const requiredNumber = (message: string) =>
  z
    .number()
    .nullable()
    .refine((v) => v !== null, { message });

const numberInRange = (requiredMessage: string, min: number, max: number, rangeMessage: string) =>
  requiredNumber(requiredMessage).refine((v) => v === null || (v >= min && v <= max), { message: rangeMessage });

// --- Adım 1: Kimlik

export const identitySchema = z.object({
  brandId: requiredNumber("Marka seçin."),
  seriesId: requiredNumber("Model seçin."),
  carModelId: requiredNumber("Paket seçin."),
  title: z.string().trim().min(1, "İlan başlığı zorunludur.").max(150, "İlan başlığı en fazla 150 karakter olabilir."),
});
export type IdentityValues = z.input<typeof identitySchema>;
export type IdentityOutput = z.output<typeof identitySchema>;

// --- Adım 2: Teknik

const isElectric = (fuel: number | null) => fuel === FuelType.Electric;
const hasBattery = (fuel: number | null) => fuel === FuelType.Electric || fuel === FuelType.PlugInHybrid;

export const createTechnicalSchema = (condition: number) =>
  z
    .object({
      year: numberInRange("Model yılı zorunludur.", MIN_YEAR, MAX_YEAR, `Model yılı ${MIN_YEAR} ile ${MAX_YEAR} arasında olmalıdır.`),
      kilometer: numberInRange("Kilometre zorunludur.", 0, MAX_KILOMETER, "Kilometre 0 ile 2.000.000 arasında olmalıdır."),
      fuelType: requiredNumber("Yakıt tipi seçin."),
      transmission: requiredNumber("Vites tipi seçin."),
      bodyType: requiredNumber("Kasa tipi seçin."),
      drivetrain: requiredNumber("Çekiş tipi seçin."),
      engineCc: z.number().nullable(),
      powerHp: numberInRange("Motor gücü zorunludur.", 1, 2000, "Motor gücü 1 ile 2000 hp arasında olmalıdır."),
      batteryKwh: z.number().nullable(),
      rangeKm: z.number().nullable(),
      exteriorColorId: requiredNumber("Dış renk seçin."),
      interiorColor: z.string().max(50, "İç renk en fazla 50 karakter olabilir."),
    })
    .superRefine((v, ctx) => {
      if (isElectric(v.fuelType)) {
        if (v.engineCc !== null) {
          ctx.addIssue({ code: "custom", path: ["engineCc"], message: "Elektrikli araçta motor hacmi boş bırakılmalıdır." });
        }
      } else if (v.engineCc === null) {
        ctx.addIssue({ code: "custom", path: ["engineCc"], message: "Motor hacmi zorunludur." });
      } else if (v.engineCc < 50 || v.engineCc > 10000) {
        ctx.addIssue({ code: "custom", path: ["engineCc"], message: "Motor hacmi 50 ile 10.000 cc arasında olmalıdır." });
      }

      if (!hasBattery(v.fuelType)) {
        if (v.batteryKwh !== null) {
          ctx.addIssue({ code: "custom", path: ["batteryKwh"], message: "Batarya kapasitesi yalnızca elektrikli ve plug-in hibrit araçta girilir." });
        }
        if (v.rangeKm !== null) {
          ctx.addIssue({ code: "custom", path: ["rangeKm"], message: "Menzil yalnızca elektrikli ve plug-in hibrit araçta girilir." });
        }
      } else {
        if (v.batteryKwh !== null && (v.batteryKwh < 1 || v.batteryKwh > 300)) {
          ctx.addIssue({ code: "custom", path: ["batteryKwh"], message: "Batarya kapasitesi 1 ile 300 kWh arasında olmalıdır." });
        }
        if (v.rangeKm !== null && (v.rangeKm < 1 || v.rangeKm > 2000)) {
          ctx.addIssue({ code: "custom", path: ["rangeKm"], message: "Menzil 1 ile 2000 km arasında olmalıdır." });
        }
      }

      if (condition === VehicleCondition.New && v.kilometer !== null && v.kilometer > MAX_NEW_VEHICLE_KILOMETER) {
        ctx.addIssue({ code: "custom", path: ["kilometer"], message: "Sıfır araçta kilometre 0 ile 100 arasında olmalıdır." });
      }
    });
export type TechnicalValues = z.input<ReturnType<typeof createTechnicalSchema>>;
export type TechnicalOutput = z.output<ReturnType<typeof createTechnicalSchema>>;

// --- Adım 3: Durum

const todayIso = () => new Date().toISOString().slice(0, 10);

export const createConditionSchema = (kilometer: number | null) =>
  z
    .object({
      condition: requiredNumber("Araç durumu seçin."),
      hasWarranty: z.boolean(),
      warrantyEndDate: z.string(),
      heavyDamageRecord: z.boolean(),
      plateOrigin: requiredNumber("Plaka / uyruk seçin."),
      acceptsTrade: z.boolean(),
      tramerAmount: z.number().nullable(),
    })
    .superRefine((v, ctx) => {
      if (v.hasWarranty) {
        if (!v.warrantyEndDate) {
          ctx.addIssue({ code: "custom", path: ["warrantyEndDate"], message: "Garanti bitiş tarihi zorunludur." });
        } else if (v.warrantyEndDate <= todayIso()) {
          ctx.addIssue({ code: "custom", path: ["warrantyEndDate"], message: "Garanti bitiş tarihi bugünden sonra olmalıdır." });
        }
      }
      if (v.tramerAmount !== null && v.tramerAmount < 0) {
        ctx.addIssue({ code: "custom", path: ["tramerAmount"], message: "Tramer tutarı 0 ya da daha büyük olmalıdır." });
      }
      if (v.condition === VehicleCondition.New && kilometer !== null && kilometer > MAX_NEW_VEHICLE_KILOMETER) {
        ctx.addIssue({
          code: "custom",
          path: ["condition"],
          message: "Sıfır araçta kilometre 0 ile 100 arasında olmalıdır. Önce Teknik adımında kilometreyi düzeltin.",
        });
      }
    });
export type ConditionValues = z.input<ReturnType<typeof createConditionSchema>>;
export type ConditionOutput = z.output<ReturnType<typeof createConditionSchema>>;

// --- Adım 4: Boya / değişen

export const bodyPartsSchema = z.object({
  parts: z.array(
    z.object({
      part: z.number(),
      state: z.number().min(0).max(3),
      note: z.string().max(200, "Not en fazla 200 karakter olabilir."),
    }),
  ),
});
export type BodyPartsValues = z.infer<typeof bodyPartsSchema>;

// --- Adım 5: Donanım

export const featuresSchema = z.object({
  featureIds: z.array(z.number()),
});
export type FeaturesValues = z.infer<typeof featuresSchema>;

// --- Adım 6: Video

const httpsUrl = (value: string) => {
  try {
    return new URL(value).protocol === "https:";
  } catch {
    return false;
  }
};

export const videoSchema = z.object({
  videoUrl: z
    .string()
    .trim()
    .max(300, "Video bağlantısı en fazla 300 karakter olabilir.")
    .refine((v) => v === "" || httpsUrl(v), { message: "Geçerli bir https bağlantısı girin." }),
});
export type VideoValues = z.infer<typeof videoSchema>;

// --- Adım 7: Fiyat ve yayın

export const pricingSchema = z
  .object({
    price: z
      .number()
      .nullable()
      .refine((v) => v === null || (v >= 0 && v <= 1_000_000_000), { message: "Fiyat 0 ile 1.000.000.000 TL arasında olmalıdır." }),
    // Boşsa kampanya yok; doluysa sitede fiyatın üstü çizilir.
    campaignPrice: z.number().nullable(),
    showPrice: z.boolean(),
    // HTML uzunluğu (biçimlendirme dahil); sunucudaki sınırla aynı.
    description: z.string().max(20000, "Açıklama biçimlendirmesiyle birlikte en fazla 20.000 karakter olabilir."),
    // Danışman zorunlu; aktif olduğunu sunucu kontrol eder. Sitede "Ara" ve WhatsApp danışmanın telefonuna gider.
    advisorId: z.string().nullable().refine((v) => v !== null && v !== "", { message: "Danışman seçin." }),
    isFeatured: z.boolean(),
    featuredOrder: z.number().min(0, "Vitrin sırası 0 ya da daha büyük olmalıdır."),
    isOpportunity: z.boolean(),
  })
  .superRefine((v, ctx) => {
    if (v.campaignPrice === null) return;
    if (v.campaignPrice <= 0) {
      ctx.addIssue({ code: "custom", path: ["campaignPrice"], message: "Kampanyalı fiyat 0'dan büyük olmalıdır." });
    } else if (!(v.price !== null && v.price > 0 && v.campaignPrice < v.price)) {
      ctx.addIssue({ code: "custom", path: ["campaignPrice"], message: "Kampanyalı fiyat, normal fiyattan düşük olmalıdır." });
    }
  });
export type PricingValues = z.infer<typeof pricingSchema>;
