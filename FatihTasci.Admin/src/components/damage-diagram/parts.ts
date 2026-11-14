// Boya / değişen şeması: açılmış araç görünümü (ekspertiz raporlarındaki gibi), ön üstte.
// Ortada üstten görünüm (kaput, tavan, bagaj), iki yanda açılmış yan görünüm (çamurluk, kapılar),
// üstte ön tampon, altta arka tampon. Sağ yan sol yanın aynasıdır (x' = 360 - x).
// Vitrindeki FatihTasci.Server/Views/Shared/_DamageDiagram.cshtml ile aynı olmalıdır.
// Anahtarlar sunucudaki BodyPart enum değerleridir.

export const VIEW_BOX = "0 14 360 386";

export interface PartShape {
  /** SVG path */
  d: string;
  /** Durum etiketinin ortası */
  labelX: number;
  labelY: number;
  /** Yan görünümdeki parçalarda etiket dikey yazılır */
  vertical: boolean;
}

export const PART_SHAPES: Record<number, PartShape> = {
  1: { d: "M124 24 H236 Q244 24 244 32 V42 Q244 50 236 50 H124 Q116 50 116 42 V32 Q116 24 124 24 Z", labelX: 180, labelY: 37, vertical: false }, // Ön Tampon
  2: { d: "M128 64 Q180 56 232 64 L240 132 Q180 122 120 132 Z", labelX: 180, labelY: 97, vertical: false }, // Motor Kaputu
  3: { d: "M130 206 Q180 200 230 206 V272 Q180 278 130 272 Z", labelX: 180, labelY: 239, vertical: false }, // Tavan
  4: { d: "M124 316 Q180 324 236 316 L232 350 Q180 360 128 350 Z", labelX: 180, labelY: 336, vertical: false }, // Bagaj Kapağı
  5: { d: "M124 362 H236 Q244 362 244 370 V380 Q244 388 236 388 H124 Q116 388 116 380 V370 Q116 362 124 362 Z", labelX: 180, labelY: 375, vertical: false }, // Arka Tampon
  6: { d: "M38 60 H62 Q74 80 80 108 V138 H26 V72 Q26 60 38 60 Z", labelX: 64, labelY: 99, vertical: true }, // Sol Ön Çamurluk
  7: { d: "M26 142 H80 V220 H26 Z", labelX: 53, labelY: 181, vertical: true }, // Sol Ön Kapı
  8: { d: "M26 224 H80 V290 H26 Z", labelX: 53, labelY: 257, vertical: true }, // Sol Arka Kapı
  9: { d: "M26 294 H80 V316 Q74 342 62 358 H38 Q26 358 26 346 Z", labelX: 64, labelY: 326, vertical: true }, // Sol Arka Çamurluk
  10: { d: "M322 60 H298 Q286 80 280 108 V138 H334 V72 Q334 60 322 60 Z", labelX: 296, labelY: 99, vertical: true }, // Sağ Ön Çamurluk
  11: { d: "M334 142 H280 V220 H334 Z", labelX: 307, labelY: 181, vertical: true }, // Sağ Ön Kapı
  12: { d: "M334 224 H280 V290 H334 Z", labelX: 307, labelY: 257, vertical: true }, // Sağ Arka Kapı
  13: { d: "M334 294 H280 V316 Q286 342 298 358 H322 Q334 358 334 346 Z", labelX: 296, labelY: 326, vertical: true }, // Sağ Arka Çamurluk
};

// Tıklanmayan süs çizimleri. Gövde ve camlar parçaların altında, farlar ve tekerlekler üstünde çizilir.
export const BODY_PATHS = [
  "M180 56 Q236 56 244 70 L248 140 V300 L244 352 Q236 362 180 360 Q124 362 116 352 L112 300 V140 L116 70 Q124 56 180 56 Z", // üstten görünüm
  "M38 56 H66 Q100 112 107 222 Q103 300 68 362 H38 Q22 362 22 346 V72 Q22 56 38 56 Z", // sol yan
  "M322 56 H294 Q260 112 253 222 Q257 300 292 362 H322 Q338 362 338 346 V72 Q338 56 322 56 Z", // sağ yan
];
export const GLASS_PATHS = [
  "M120 138 Q180 126 240 138 L230 200 Q180 194 130 200 Z", // ön cam
  "M130 278 Q180 284 230 278 L236 310 Q180 318 124 310 Z", // arka cam
  "M84 142 Q100 162 105 220 H84 Z", // sol ön kapı camı
  "M84 224 H105 Q103 262 84 290 Z", // sol arka kapı camı
  "M276 142 Q260 162 255 220 H276 Z", // sağ ön kapı camı
  "M276 224 H255 Q257 262 276 290 Z", // sağ arka kapı camı
];
export const LAMP_PATHS = [
  "M124 31 H146 L142 43 H126 Z",
  "M236 31 H214 L218 43 H234 Z",
  "M124 369 H146 L142 381 H126 Z",
  "M236 369 H214 L218 381 H234 Z",
];
export const WHEELS = [
  { cx: 26, cy: 112 },
  { cx: 26, cy: 322 },
  { cx: 334, cy: 112 },
  { cx: 334, cy: 322 },
];
export const WHEEL_RADIUS = 18;

// Hasar şeması renkleri: Orijinal gri, Lokal Boyalı sarı, Boyalı turuncu, Değişen kırmızı.
// Bu renkler yalnız şemada kullanılır.
export const STATE_COLORS: Record<number, string> = {
  0: "#9CA3AF",
  1: "#EAB308",
  2: "#F97316",
  3: "#DC2626",
};

// Parça üstündeki durum yazısının rengi (zemine göre okunur olsun diye)
export const STATE_TEXT_COLORS: Record<number, string> = {
  1: "#1F2937",
  2: "#1F2937",
  3: "#FFFFFF",
};

export const STATE_COUNT = 4;
