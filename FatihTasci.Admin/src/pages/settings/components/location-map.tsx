import { useEffect, useRef } from "react";
import { useWatch, type UseFormReturn } from "react-hook-form";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import "./location-map.css";
import { Field, FieldDescription, FieldLabel } from "@/components/ui/field";
import { toCoordinates, type SettingsOutput, type SettingsValues } from "./settings-schema";

// Leaflet (BSD-2-Clause) + OpenStreetMap karoları. OSMF karo kullanım politikası: atıf haritanın üstünde görünür,
// karo adresi yalnız tile.openstreetmap.org, önden / toplu karo indirme yok; tarayıcı Referer'ı gönderir
// (Referrer-Policy strict-origin-when-cross-origin). Panel CSP'sinde img-src'de bu adres izinlidir (SecurityMiddleware).
const TILE_URL = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
const ATTRIBUTION = '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> katkıda bulunanlar';
const PREFIX = '<a href="https://leafletjs.com" target="_blank" rel="noopener">Leaflet</a>';

const TURKEY: L.LatLngTuple = [39.0, 35.2];
const TURKEY_ZOOM = 6;
const POINT_ZOOM = 17;
/** Elle yazılan nokta bu yakınlıktan uzaktaysa harita noktaya yaklaşır. */
const DETAIL_ZOOM = 14;

const pinIcon = L.divIcon({ className: "location-map-pin", html: "<span></span>", iconSize: [28, 34], iconAnchor: [14, 34] });

const format = (point: L.LatLng) => {
  const p = point.wrap();
  return [p.lat.toFixed(6), p.lng.toFixed(6)] as const;
};

interface LocationMapProps {
  form: UseFormReturn<SettingsValues, unknown, SettingsOutput>;
}

/**
 * Galeri konumunu haritadan seçme: tıklamak ya da işaretçiyi sürüklemek enlem / boylam alanlarını doldurur,
 * alanlara yazılan geçerli değer işaretçiyi taşır. Doğruluk kaynağı formdaki değerlerdir; işaretçi onlardan çizilir.
 */
export function LocationMap({ form }: LocationMapProps) {
  const { control, getValues, setValue } = form;
  const containerRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<L.Map | null>(null);
  const markerRef = useRef<L.Marker | null>(null);
  /** Haritadan yazılan son değer: işaretçi zaten oradadır, görünüm kaydırılmaz. */
  const fromMapRef = useRef<string | null>(null);
  const [latitude, longitude] = useWatch({ control, name: ["latitude", "longitude"] });

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const choose = (point: L.LatLng) => {
      const [lat, lng] = format(point);
      fromMapRef.current = `${lat},${lng}`;
      setValue("latitude", lat, { shouldDirty: true, shouldValidate: true });
      setValue("longitude", lng, { shouldDirty: true, shouldValidate: true });
    };

    const saved = toCoordinates(getValues("latitude"), getValues("longitude"));
    const map = L.map(container, { center: saved ?? TURKEY, zoom: saved ? POINT_ZOOM : TURKEY_ZOOM });
    map.attributionControl.setPrefix(PREFIX);
    L.tileLayer(TILE_URL, { maxZoom: 19, attribution: ATTRIBUTION }).addTo(map);
    map.on("click", (e: L.LeafletMouseEvent) => choose(e.latlng));

    const marker = L.marker(saved ?? TURKEY, { icon: pinIcon, draggable: true, autoPan: true, title: "Galeri konumu", alt: "Galeri konumu" });
    marker.on("dragend", () => choose(marker.getLatLng()));
    mapRef.current = map;
    markerRef.current = marker;

    // Kenar çubuğu açılıp kapanınca ya da ekran döndüğünde karolar yeni boyuta göre yeniden yerleşir.
    const observer = new ResizeObserver(() => map.invalidateSize());
    observer.observe(container);

    return () => {
      observer.disconnect();
      map.remove();
      mapRef.current = null;
      markerRef.current = null;
    };
  }, [getValues, setValue]);

  useEffect(() => {
    const map = mapRef.current;
    const marker = markerRef.current;
    if (!map || !marker) return;

    // Alanlar boşaltılınca işaretçi kalkar; yazılırken geçersiz kalan değer (örn. "37.") işaretçiyi oynatmaz.
    if (latitude.trim() === "" && longitude.trim() === "") {
      marker.remove();
      return;
    }
    const point = toCoordinates(latitude, longitude);
    if (!point) return;

    marker.setLatLng(point);
    if (!map.hasLayer(marker)) marker.addTo(map);

    const typed = fromMapRef.current !== `${latitude},${longitude}`;
    if (typed &&(map.getZoom() < DETAIL_ZOOM || !map.getBounds().contains(point))) {
      map.setView(point, Math.max(map.getZoom(), POINT_ZOOM));
    }
  }, [latitude, longitude]);

  return (
    <Field className="h-full">
      <FieldLabel>Haritada konum</FieldLabel>
      <div
        ref={containerRef}
        aria-label="Galeri konumunu seçme haritası"
        className="location-map isolate min-h-72 flex-1 overflow-hidden rounded-md border bg-muted lg:min-h-80"
      />
      <FieldDescription>
        Haritaya tıklayın ya da işaretçiyi sürükleyin; enlem ve boylam dolar. Alanlara yazınca işaretçi taşınır. Kaydedince sitedeki harita bu noktayı gösterir.
      </FieldDescription>
    </Field>
  );
}
