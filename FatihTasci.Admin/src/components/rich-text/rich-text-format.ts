// Açıklama editörünün biçimleri. Renk ve hizalama satır içi style değil sınıf olarak saklanır: vitrinin
// CSP'si (style-src 'self') satır içi style'ı engeller. Sınıflar üç yerde aynıdır (CLAUDE.md "Açıklama
// editörü"): burası ↔ Core VehicleService.SanitizeDescription (izinli sınıflar) ↔ vitrin site.css (renkler).

export type TextColorKey = "gold" | "red" | "blue" | "green" | "muted";
export type TextAlignKey = "center" | "right";

export interface TextColorOption {
  key: TextColorKey;
  label: string;
}

/** Renkler vitrinin beyaz zeminine göre seçildi (değerleri rich-text.css / site.css); hasar şemasının renkleri kullanılmaz. */
export const TEXT_COLORS: TextColorOption[] = [
  { key: "gold", label: "Altın" },
  { key: "red", label: "Kırmızı" },
  { key: "blue", label: "Mavi" },
  { key: "green", label: "Yeşil" },
  { key: "muted", label: "Gri" },
];

const COLOR_KEYS = new Set<string>(TEXT_COLORS.map((x) => x.key));
const ALIGN_KEYS = new Set<string>(["center", "right"]);

export const textColorClass = (key: TextColorKey) => `tc-${key}`;
export const textAlignClass = (key: TextAlignKey) => `ta-${key}`;

function keyFromClass(element: HTMLElement, prefix: string, allowed: Set<string>) {
  for (const name of Array.from(element.classList)) {
    if (name.startsWith(prefix) && allowed.has(name.slice(prefix.length))) return name.slice(prefix.length);
  }
  return null;
}

export const colorFromElement = (element: HTMLElement) => keyFromClass(element, "tc-", COLOR_KEYS) as TextColorKey | null;
export const alignFromElement = (element: HTMLElement) => keyFromClass(element, "ta-", ALIGN_KEYS) as TextAlignKey | null;

const BLOCK_TAG = /<(p|ul|ol|li|h[1-6])[\s>]/i;

/**
 * Editör açılmadan önceki açıklamalar düz metindi (satır sonları vitrinde white-space: pre-line ile
 * korunuyordu). Böyle bir metnin her satırı paragraf olur; metin zaten sunucuda temizlenmiş HTML'dir.
 */
export function toEditorHtml(value: string | null | undefined) {
  if (!value?.trim()) return "";
  if (BLOCK_TAG.test(value)) return value;
  return value
    .split(/\r?\n/)
    .map((line) => `<p>${line}</p>`)
    .join("");
}
