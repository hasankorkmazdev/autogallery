import type { VehicleDetail } from "@/api/types";

/**
 * Adımın kaydetme fonksiyonu. Form değişmemişse hiçbir şey yapmadan true döner;
 * doğrulama ya da sunucu hatasında false döner ve sihirbaz adımda kalır.
 */
export type StepSaveHandler = () => Promise<boolean>;

export interface StepProps {
  vehicle: VehicleDetail;
  onSaved: (vehicle: VehicleDetail) => void;
  registerSave: (handler: StepSaveHandler | null) => void;
}

export const WIZARD_STEPS = [
  { step: 1, title: "Kimlik", description: "Marka, model, paket" },
  { step: 2, title: "Teknik", description: "Yıl, km, motor" },
  { step: 3, title: "Durum", description: "Garanti, Tramer" },
  { step: 4, title: "Boya / Değişen", description: "13 parça" },
  { step: 5, title: "Donanım", description: "Donanım listesi" },
  { step: 6, title: "Medya", description: "Fotoğraf, video" },
  { step: 7, title: "Fiyat ve Yayın", description: "Fiyat, yayın" },
] as const;
