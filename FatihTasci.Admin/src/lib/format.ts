const numberFormat = new Intl.NumberFormat("tr-TR");
const dateTimeFormat = new Intl.DateTimeFormat("tr-TR", { dateStyle: "short", timeStyle: "short" });
const dateFormat = new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium" });

export function formatNumber(value: number | null | undefined) {
  return value === null || value === undefined ? "-" : numberFormat.format(value);
}

export function formatPrice(value: number | null | undefined) {
  return value === null || value === undefined ? "-" : `${numberFormat.format(value)} TL`;
}

export function formatKm(value: number | null | undefined) {
  return value === null || value === undefined ? "-" : `${numberFormat.format(value)} km`;
}

export function formatDateTime(value: string | null | undefined) {
  return value ? dateTimeFormat.format(new Date(value)) : "-";
}

export function formatDate(value: string | null | undefined) {
  return value ? dateFormat.format(new Date(value)) : "-";
}

/**
 * Vitrin adresi (panel /panel altında, vitrin kökte sunulur).
 */
export function publicSiteUrl(path: string) {
  // Geliştirmede panel Vite'ta (5173), vitrin Server'da (5080) çalışır.
  const origin = import.meta.env.DEV
    ? `${window.location.protocol}//${window.location.hostname}:5080`
    : window.location.origin;
  return `${origin}${path}`;
}
