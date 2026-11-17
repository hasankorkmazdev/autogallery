import { useRef, useState } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { FieldGroup } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { catalogService } from "@/api/services/catalog.service";
import { vehiclesService } from "@/api/services/vehicles.service";
import type { VehicleDetail } from "@/api/types";
import { identitySchema, type IdentityOutput, type IdentityValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import { useCatalogOptions } from "../hooks/use-catalog-options";
import { CatalogQuickAddDialog } from "./catalog-quick-add-dialog";
import { CatalogSelect } from "./catalog-select";
import type { StepSaveHandler } from "../types";

interface StepIdentityProps {
  vehicle: VehicleDetail | null;
  onSaved: (vehicle: VehicleDetail) => void;
  registerSave: (handler: StepSaveHandler | null) => void;
}

type AddTarget = "brand" | "series" | "model" | null;

const ADD_DIALOG = {
  brand: {
    title: "Marka ekle",
    description: "Listede olmayan markayı ekleyin. Adres (slug) otomatik üretilir.",
    label: "Marka adı",
    hint: "Örn. Alfa Romeo",
  },
  series: {
    title: "Model ekle",
    description: "Seçili markaya yeni bir model eklenir.",
    label: "Model adı",
    hint: "Örn. 3 Serisi",
  },
  model: {
    title: "Paket ekle",
    description: "Seçili modele yeni bir paket (motor + donanım paketi) eklenir.",
    label: "Paket adı",
    hint: "Örn. 320i M Sport",
  },
} as const;

export function StepIdentity({ vehicle, onSaved, registerSave }: StepIdentityProps) {
  const [addTarget, setAddTarget] = useState<AddTarget>(null);
  const suggestedTitle = useRef(vehicle?.title ?? "");
  const form = useForm<IdentityValues, unknown, IdentityOutput>({
    resolver: zodResolver(identitySchema),
    defaultValues: {
      brandId: vehicle?.brandId ?? null,
      seriesId: vehicle?.seriesId ?? null,
      carModelId: vehicle?.carModelId ?? null,
      title: vehicle?.title ?? "",
    },
  });
  const { errors } = form.formState;
  const brandId = useWatch({ control: form.control, name: "brandId" });
  const seriesId = useWatch({ control: form.control, name: "seriesId" });
  const catalog = useCatalogOptions(brandId, seriesId);

  useStepSave({
    form,
    registerSave,
    onSaved,
    alwaysSave: !vehicle,
    submit: (values) => {
      const request = {
        brandId: values.brandId!,
        seriesId: values.seriesId!,
        carModelId: values.carModelId!,
        title: values.title.trim(),
      };
      return vehicle ? vehiclesService.updateIdentity(vehicle.id, request) : vehiclesService.create(request);
    },
  });

  // Başlık marka / seri / modelden önerilir; kullanıcı değiştirdiyse dokunulmaz.
  const suggestTitle = (modelId: number | null) => {
    const brand = catalog.brands.find((x) => x.id === form.getValues("brandId"));
    const series = catalog.series.find((x) => x.id === form.getValues("seriesId"));
    const model = catalog.models.find((x) => x.id === modelId);
    if (!brand || !series || !model) return;
    const current = form.getValues("title");
    if (current === "" || current === suggestedTitle.current) {
      const suggestion = `${brand.name} ${series.name} ${model.name}`;
      suggestedTitle.current = suggestion;
      form.setValue("title", suggestion, { shouldDirty: true, shouldValidate: true });
    }
  };

  const handleAdd = async (name: string) => {
    if (addTarget === "brand") {
      const result = await catalogService.createBrand({ name, sortOrder: 0, isActive: true });
      await catalog.reloadBrands();
      form.setValue("brandId", result.data.id, { shouldDirty: true, shouldValidate: true });
      form.setValue("seriesId", null, { shouldDirty: true });
      form.setValue("carModelId", null, { shouldDirty: true });
      toast.success(result.message);
    }
    if (addTarget === "series" && brandId) {
      const result = await catalogService.createSeries({ brandId, name, sortOrder: 0, isActive: true });
      await catalog.reloadSeries();
      form.setValue("seriesId", result.data.id, { shouldDirty: true, shouldValidate: true });
      form.setValue("carModelId", null, { shouldDirty: true });
      toast.success(result.message);
    }
    if (addTarget === "model" && seriesId) {
      const result = await catalogService.createModel({ seriesId, name, isActive: true });
      await catalog.reloadModels();
      form.setValue("carModelId", result.data.id, { shouldDirty: true, shouldValidate: true });
      toast.success(result.message);
    }
  };

  const dialog = addTarget ? ADD_DIALOG[addTarget] : ADD_DIALOG.brand;

  return (
    <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()}>
      <FieldGroup className="max-w-2xl">
        <Controller
          control={form.control}
          name="brandId"
          render={({ field }) => (
            <CatalogSelect
              id="brand"
              label="Marka"
              description="Listede yoksa + ile ekleyin."
              addTitle="Marka ekle"
              items={catalog.brands}
              value={field.value}
              error={errors.brandId?.message}
              onChange={(value) => {
                field.onChange(value);
                form.setValue("seriesId", null, { shouldDirty: true });
                form.setValue("carModelId", null, { shouldDirty: true });
              }}
              onAdd={() => setAddTarget("brand")}
            />
          )}
        />
        <Controller
          control={form.control}
          name="seriesId"
          render={({ field }) => (
            <CatalogSelect
              id="series"
              label="Model"
              description="Önce marka seçin. Listede yoksa + ile ekleyin."
              addTitle="Model ekle"
              items={catalog.series}
              value={field.value}
              error={errors.seriesId?.message}
              disabled={!brandId}
              onChange={(value) => {
                field.onChange(value);
                form.setValue("carModelId", null, { shouldDirty: true });
              }}
              onAdd={() => setAddTarget("series")}
            />
          )}
        />
        <Controller
          control={form.control}
          name="carModelId"
          render={({ field }) => (
            <CatalogSelect
              id="model"
              label="Paket"
              description="Motor ve donanım paketi, örn. 320i M Sport. Listede yoksa + ile ekleyin."
              addTitle="Paket ekle"
              items={catalog.models}
              value={field.value}
              error={errors.carModelId?.message}
              disabled={!seriesId}
              onChange={(value) => {
                field.onChange(value);
                suggestTitle(value);
              }}
              onAdd={() => setAddTarget("model")}
            />
          )}
        />
        <InputField
          id="title"
          label="İlan başlığı"
          description="Marka, model ve paketten otomatik önerilir; dilediğiniz gibi düzenleyebilirsiniz."
          error={errors.title?.message}
          maxLength={150}
          autoComplete="off"
          {...form.register("title")}
        />
      </FieldGroup>

      <CatalogQuickAddDialog
        open={addTarget !== null}
        title={dialog.title}
        description={dialog.description}
        fieldLabel={dialog.label}
        fieldDescription={dialog.hint}
        onOpenChange={(open) => !open && setAddTarget(null)}
        onSubmit={handleAdd}
      />
    </form>
  );
}
