import { useState } from "react";
import { catalogService } from "@/api/services/catalog.service";
import { useQuery } from "@/hooks/use-query";

/**
 * Marka → Seri → Model zincirleme listeleri. Üst seçim değişince alt liste yeniden yüklenir.
 */
export function useCatalogOptions(brandId: number | null, seriesId: number | null) {
  const [version, setVersion] = useState({ brands: 0, series: 0, models: 0 });

  const brands = useQuery(`brands-${version.brands}`, () => catalogService.getBrands());
  const series = useQuery(`series-${brandId}-${version.series}`, () => catalogService.getSeries(brandId!), brandId !== null);
  const models = useQuery(`models-${seriesId}-${version.models}`, () => catalogService.getModels(seriesId!), seriesId !== null);

  return {
    brands: brands.data ?? [],
    series: (series.isCurrent && series.data) || [],
    models: (models.isCurrent && models.data) || [],
    reloadBrands: () => setVersion((v) => ({ ...v, brands: v.brands + 1 })),
    reloadSeries: () => setVersion((v) => ({ ...v, series: v.series + 1 })),
    reloadModels: () => setVersion((v) => ({ ...v, models: v.models + 1 })),
  };
}
