// Boya / değişen şeması parça yerleşimi (üstten görünüm, ön üstte, viewBox 0 0 240 432).
// Vitrindeki FatihTasci.Server/Views/Shared/_DamageDiagram.cshtml ile aynı olmalıdır.
// Anahtarlar sunucudaki BodyPart enum değerleridir.

export interface PartShape {
  x: number;
  y: number;
  w: number;
  h: number;
}

export const PART_SHAPES: Record<number, PartShape> = {
  1: { x: 60, y: 8, w: 120, h: 26 }, // Ön Tampon
  2: { x: 60, y: 40, w: 120, h: 80 }, // Motor Kaputu
  3: { x: 60, y: 126, w: 120, h: 180 }, // Tavan
  4: { x: 60, y: 312, w: 120, h: 80 }, // Bagaj Kapağı
  5: { x: 60, y: 398, w: 120, h: 26 }, // Arka Tampon
  6: { x: 14, y: 40, w: 40, h: 80 }, // Sol Ön Çamurluk
  7: { x: 14, y: 126, w: 40, h: 87 }, // Sol Ön Kapı
  8: { x: 14, y: 219, w: 40, h: 87 }, // Sol Arka Kapı
  9: { x: 14, y: 312, w: 40, h: 80 }, // Sol Arka Çamurluk
  10: { x: 186, y: 40, w: 40, h: 80 }, // Sağ Ön Çamurluk
  11: { x: 186, y: 126, w: 40, h: 87 }, // Sağ Ön Kapı
  12: { x: 186, y: 219, w: 40, h: 87 }, // Sağ Arka Kapı
  13: { x: 186, y: 312, w: 40, h: 80 }, // Sağ Arka Çamurluk
};

// Hasar şeması renkleri (PROJE_DOKUMANI §6): Orijinal gri, Lokal Boyalı sarı, Boyalı turuncu, Değişen kırmızı.
// Bu renkler yalnız şemada kullanılır.
export const STATE_COLORS: Record<number, string> = {
  0: "#9CA3AF",
  1: "#EAB308",
  2: "#F97316",
  3: "#DC2626",
};

export const STATE_COUNT = 4;
