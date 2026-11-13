import { useMemo } from "react";
import { Controller, useFieldArray, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { FieldDescription, FieldError } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { SelectField } from "@/components/form/form-fields";
import { DamageDiagram, DamageLegend } from "@/components/damage-diagram/damage-diagram";
import { STATE_COUNT } from "@/components/damage-diagram/parts";
import { vehiclesService } from "@/api/services/vehicles.service";
import { BodyPartState } from "@/api/types";
import { useLookupStore } from "@/store/lookup-store";
import { bodyPartsSchema, type BodyPartsValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";

const toRecord = (options: { value: number; label: string }[] | undefined) =>
  Object.fromEntries((options ?? []).map((o) => [o.value, o.label])) as Record<number, string>;

export function StepBodyParts({ vehicle, onSaved, registerSave }: StepProps) {
  const lookups = useLookupStore((state) => state.lookups);
  const partLabels = useMemo(() => toRecord(lookups?.bodyPart), [lookups]);
  const stateLabels = useMemo(() => toRecord(lookups?.bodyPartState), [lookups]);
  const form = useForm<BodyPartsValues>({
    resolver: zodResolver(bodyPartsSchema),
    defaultValues: {
      parts: vehicle.bodyParts.map((p) => ({ part: p.part, state: p.state, note: p.note ?? "" })),
    },
  });
  const { fields } = useFieldArray({ control: form.control, name: "parts" });
  const parts = useWatch({ control: form.control, name: "parts" });
  const states = Object.fromEntries(parts.map((p) => [p.part, p.state])) as Record<number, number>;
  const changedCount = parts.filter((p) => p.state !== BodyPartState.Original).length;

  useStepSave({
    form,
    registerSave,
    onSaved,
    submit: (v) =>
      vehiclesService.updateBodyParts(
        vehicle.id,
        v.parts.map((p) => ({ part: p.part, state: p.state, note: p.note.trim() || null })),
      ),
  });

  // Tıklandıkça durum sırayla değişir: Orijinal → Lokal Boyalı → Boyalı → Değişen → Orijinal
  const cyclePart = (part: number) => {
    const index = parts.findIndex((p) => p.part === part);
    if (index < 0) return;
    form.setValue(`parts.${index}.state`, (parts[index].state + 1) % STATE_COUNT, { shouldDirty: true });
  };

  const setAllOriginal = () => {
    parts.forEach((_, index) => form.setValue(`parts.${index}.state`, BodyPartState.Original, { shouldDirty: true }));
  };

  const stateOptions = lookups?.bodyPartState ?? [];

  return (
    <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()} className="grid grid-cols-1 gap-6 lg:grid-cols-[280px_1fr]">
      <div className="flex flex-col gap-4">
        <DamageDiagram states={states} partLabels={partLabels} stateLabels={stateLabels} onPartClick={cyclePart} />
        <DamageLegend stateLabels={stateLabels} />
        <FieldDescription>
          Parçaya tıkladıkça durumu sırayla değişir. Değişen parça sayısı: {changedCount}
        </FieldDescription>
        <Button type="button" variant="outline" onClick={setAllOriginal}>
          <CheckCheck className="w-4 h-4 mr-2" />
          Tümü orijinal
        </Button>
      </div>
      <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
        {fields.map((field, index) => (
          <div key={field.id} className="flex flex-col gap-2 rounded-lg border p-3">
            <Controller
              control={form.control}
              name={`parts.${index}.state`}
              render={({ field: stateField }) => (
                <SelectField
                  id={`part-${field.part}`}
                  label={partLabels[field.part] ?? `Parça ${field.part}`}
                  options={stateOptions}
                  value={stateField.value}
                  onChange={(value) => stateField.onChange(value ?? BodyPartState.Original)}
                />
              )}
            />
            <Input
              placeholder="Not (isteğe bağlı), örn. sağ kenarda çizik"
              aria-label={`${partLabels[field.part] ?? field.part} notu`}
              maxLength={200}
              autoComplete="off"
              {...form.register(`parts.${index}.note`)}
            />
            <FieldError>{form.formState.errors.parts?.[index]?.note?.message}</FieldError>
          </div>
        ))}
      </div>
    </form>
  );
}
