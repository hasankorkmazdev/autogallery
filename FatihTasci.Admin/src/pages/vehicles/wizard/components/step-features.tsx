import { useEffect, useMemo, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { History } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Field, FieldDescription, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { PageLoader } from "@/components/page-loader";
import { catalogService } from "@/api/services/catalog.service";
import { vehiclesService } from "@/api/services/vehicles.service";
import type { FeatureGroup } from "@/api/types";
import { featuresSchema, type FeaturesValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";

const normalize = (value: string) => value.toLocaleLowerCase("tr-TR");

export function StepFeatures({ vehicle, onSaved, registerSave }: StepProps) {
  const [groups, setGroups] = useState<FeatureGroup[] | null>(null);
  const [search, setSearch] = useState("");
  const form = useForm<FeaturesValues>({
    resolver: zodResolver(featuresSchema),
    defaultValues: { featureIds: vehicle.featureIds },
  });
  const selected = useWatch({ control: form.control, name: "featureIds" });
  const selectedSet = useMemo(() => new Set(selected), [selected]);

  useEffect(() => {
    catalogService
      .getFeatures()
      .then(setGroups)
      .catch((error: Error) => toast.error(error.message));
  }, []);

  useStepSave({
    form,
    registerSave,
    onSaved,
    submit: (v) => vehiclesService.updateFeatures(vehicle.id, v.featureIds),
  });

  const toggle = (featureId: number, checked: boolean) => {
    const next = checked ? [...selected, featureId] : selected.filter((x) => x !== featureId);
    form.setValue("featureIds", next, { shouldDirty: true });
  };

  // "Aynı modelin önceki ilanındaki donanımı getir" kısayolu
  const loadFromPrevious = async () => {
    try {
      const result = await vehiclesService.getFeatureSuggestion(vehicle.id);
      form.setValue("featureIds", result.data, { shouldDirty: true });
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    }
  };

  if (!groups) return <PageLoader />;

  const term = normalize(search.trim());
  const filtered = groups.map((group) => ({
    ...group,
    features: group.features.filter((f) => term === "" || normalize(f.name).includes(term)),
  }));

  return (
    <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()} className="flex flex-col gap-6">
      <div className="flex flex-col gap-2 md:flex-row md:items-end md:justify-between">
        <Field className="md:w-80">
          <FieldLabel htmlFor="feature-search">Donanım ara</FieldLabel>
          <Input id="feature-search" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Örn. sunroof" autoComplete="off" />
          <FieldDescription>{selected.length} donanım seçili.</FieldDescription>
        </Field>
        <Button type="button" variant="outline" onClick={loadFromPrevious}>
          <History className="w-4 h-4 mr-2" />
          Aynı modelin önceki ilanından getir
        </Button>
      </div>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4">
        {filtered.map((group) => (
          <fieldset key={group.category} className="flex flex-col gap-3">
            <legend className="mb-2 text-sm font-semibold">{group.categoryName}</legend>
            {group.features.length === 0 && <FieldDescription>Eşleşen donanım yok.</FieldDescription>}
            {group.features.map((feature) => (
              <label key={feature.id} className="flex items-center gap-2 text-sm cursor-pointer">
                <Checkbox
                  checked={selectedSet.has(feature.id)}
                  onCheckedChange={(checked) => toggle(feature.id, checked === true)}
                />
                {feature.name}
              </label>
            ))}
          </fieldset>
        ))}
      </div>
    </form>
  );
}
