import { useState } from "react";
import { Library } from "lucide-react";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { catalogService } from "@/api/services/catalog.service";
import { useQuery } from "@/hooks/use-query";
import type { Brand, CarModel, Series } from "@/api/types";
import { CatalogListPanel } from "./components/catalog-list-panel";
import { CatalogItemDialog, type CatalogItemValues } from "./components/catalog-item-dialog";

type Level = "brand" | "series" | "model";

interface DialogState {
  level: Level;
  id: number | null;
  initial: CatalogItemValues | null;
}

const LEVEL_TITLES: Record<Level, string> = { brand: "Marka", series: "Model", model: "Paket" };

// Marka → Seri → Model kataloğu
export default function CatalogPage() {
  const [brandId, setBrandId] = useState<number | null>(null);
  const [seriesId, setSeriesId] = useState<number | null>(null);
  const [dialog, setDialog] = useState<DialogState | null>(null);

  const [version, setVersion] = useState(0);
  const brandsQuery = useQuery(`brands-${version}`, () => catalogService.getBrands(true));
  const seriesQuery = useQuery(`series-${brandId}-${version}`, () => catalogService.getSeries(brandId!, true), brandId !== null);
  const modelsQuery = useQuery(`models-${seriesId}-${version}`, () => catalogService.getModels(seriesId!, true), seriesId !== null);
  const brands: Brand[] = brandsQuery.data ?? [];
  const series: Series[] = seriesQuery.data ?? [];
  const models: CarModel[] = modelsQuery.data ?? [];
  const reload = () => setVersion((v) => v + 1);

  const initialValues = (level: Level, id: number): CatalogItemValues | null => {
    if (level === "brand") {
      const x = brands.find((b) => b.id === id);
      return x ? { name: x.name, sortOrder: x.sortOrder, isActive: x.isActive } : null;
    }
    if (level === "series") {
      const x = series.find((s) => s.id === id);
      return x ? { name: x.name, sortOrder: x.sortOrder, isActive: x.isActive } : null;
    }
    const x = models.find((m) => m.id === id);
    return x ? { name: x.name, sortOrder: 0, isActive: x.isActive } : null;
  };

  const handleSubmit = async (values: CatalogItemValues) => {
    if (!dialog) return "";
    const { level, id } = dialog;

    if (level === "brand") {
      const result = id ? await catalogService.updateBrand(id, values) : await catalogService.createBrand(values);
      reload();
      return result.message;
    }
    if (level === "series" && brandId) {
      const request = { ...values, brandId };
      const result = id ? await catalogService.updateSeries(id, request) : await catalogService.createSeries(request);
      reload();
      return result.message;
    }
    if (level === "model" && seriesId) {
      const request = { seriesId, name: values.name, isActive: values.isActive };
      const result = id ? await catalogService.updateModel(id, request) : await catalogService.createModel(request);
      reload();
      return result.message;
    }
    return "";
  };

  const dialogTitle = dialog ? `${LEVEL_TITLES[dialog.level]} ${dialog.id ? "düzenle" : "ekle"}` : "";

  return (
    <PageLayout>
      <PageHeader
        title="Araç Katalog Yapısı"
        description="Marka, model ve paket listesi. Araç girerken eksik kayıt sihirbazdan da eklenebilir."
        icon={<Library />}
      />
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <CatalogListPanel
          title="Markalar"
          description="Model listesi için bir marka seçin."
          emptyText="Marka yok."
          items={brands.map((b) => ({ id: b.id, name: b.name, slug: b.slug, isActive: b.isActive, count: b.seriesCount, logoUrl: b.logoUrl }))}
          selectedId={brandId}
          onSelect={(id) => {
            setBrandId(id);
            setSeriesId(null);
          }}
          onAdd={() => setDialog({ level: "brand", id: null, initial: null })}
          onEdit={(id) => setDialog({ level: "brand", id, initial: initialValues("brand", id) })}
        />
        <CatalogListPanel
          title="Modeller"
          description="Paket listesi için bir model seçin."
          emptyText={brandId ? "Bu markada model yok." : "Önce marka seçin."}
          items={series.map((s) => ({ id: s.id, name: s.name, slug: s.slug, isActive: s.isActive, count: s.modelCount }))}
          selectedId={seriesId}
          disabled={!brandId}
          onSelect={setSeriesId}
          onAdd={() => setDialog({ level: "series", id: null, initial: null })}
          onEdit={(id) => setDialog({ level: "series", id, initial: initialValues("series", id) })}
        />
        <CatalogListPanel
          title="Paketler"
          description="Motor ve donanım paketi, örn. 320i M Sport."
          emptyText={seriesId ? "Bu modelde paket yok." : "Önce model seçin."}
          items={models.map((m) => ({ id: m.id, name: m.name, slug: m.slug, isActive: m.isActive }))}
          disabled={!seriesId}
          onAdd={() => setDialog({ level: "model", id: null, initial: null })}
          onEdit={(id) => setDialog({ level: "model", id, initial: initialValues("model", id) })}
        />
      </div>
      <CatalogItemDialog
        open={dialog !== null}
        title={dialogTitle}
        initial={dialog?.initial ?? null}
        showSortOrder={dialog?.level !== "model"}
        onOpenChange={(open) => !open && setDialog(null)}
        onSubmit={handleSubmit}
      />
    </PageLayout>
  );
}
