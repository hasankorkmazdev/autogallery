import { useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { InputField } from "@/components/ui/input-field";
import { NumberField, SelectField, SwitchField } from "@/components/form/form-fields";
import { vehiclesService } from "@/api/services/vehicles.service";
import { useLookupStore } from "@/store/lookup-store";
import { createConditionSchema, type ConditionOutput, type ConditionValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";

export function StepCondition({ vehicle, onSaved, registerSave }: StepProps) {
  const lookups = useLookupStore((state) => state.lookups);
  const schema = useMemo(() => createConditionSchema(vehicle.kilometer), [vehicle.kilometer]);
  const form = useForm<ConditionValues, unknown, ConditionOutput>({
    resolver: zodResolver(schema),
    defaultValues: {
      condition: vehicle.condition,
      hasWarranty: vehicle.hasWarranty,
      warrantyEndDate: vehicle.warrantyEndDate?.slice(0, 10) ?? "",
      heavyDamageRecord: vehicle.heavyDamageRecord,
      plateOrigin: vehicle.plateOrigin,
      acceptsTrade: vehicle.acceptsTrade,
      tramerAmount: vehicle.tramerAmount,
    },
  });
  const { errors } = form.formState;
  const hasWarranty = useWatch({ control: form.control, name: "hasWarranty" });

  useStepSave({
    form,
    registerSave,
    onSaved,
    submit: (v) =>
      vehiclesService.updateCondition(vehicle.id, {
        condition: v.condition!,
        hasWarranty: v.hasWarranty,
        warrantyEndDate: v.hasWarranty && v.warrantyEndDate ? v.warrantyEndDate : null,
        heavyDamageRecord: v.heavyDamageRecord,
        plateOrigin: v.plateOrigin!,
        acceptsTrade: v.acceptsTrade,
        tramerAmount: v.tramerAmount,
      }),
  });

  return (
    <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()} className="grid grid-cols-1 gap-4 md:grid-cols-2">
      <Controller control={form.control} name="condition" render={({ field }) => (
        <SelectField id="condition" label="Durumu" required options={lookups?.condition ?? []}
          description="Sıfır araçta kilometre en fazla 100 olabilir." error={errors.condition?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="plateOrigin" render={({ field }) => (
        <SelectField id="plateOrigin" label="Plaka / uyruk" required options={lookups?.plateOrigin ?? []}
          description="Aracın plaka durumu." error={errors.plateOrigin?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="tramerAmount" render={({ field }) => (
        <NumberField id="tramerAmount" label="Tramer tutarı" min={0} step={1000} suffix=" TL"
          description="Boş bırakılırsa sitede &quot;Kayıt yok&quot; yazar." error={errors.tramerAmount?.message} value={field.value} onChange={field.onChange} />
      )} />
      <div className="flex flex-col gap-4">
        <Controller control={form.control} name="hasWarranty" render={({ field }) => (
          <SwitchField id="hasWarranty" label="Garantisi var" description="Açıksa bitiş tarihi zorunludur."
            checked={field.value} onChange={field.onChange} />
        )} />
        {hasWarranty && (
          <InputField id="warrantyEndDate" type="date" label="Garanti bitiş tarihi" required
            description="Bugünden sonraki bir tarih olmalıdır." error={errors.warrantyEndDate?.message} {...form.register("warrantyEndDate")} />
        )}
      </div>
      <Controller control={form.control} name="heavyDamageRecord" render={({ field }) => (
        <SwitchField id="heavyDamageRecord" label="Ağır hasar kayıtlı" description="Ruhsatta ağır hasar kaydı varsa açın."
          checked={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="acceptsTrade" render={({ field }) => (
        <SwitchField id="acceptsTrade" label="Takasa uygun" description="Sitede &quot;Takasa uygun&quot; olarak görünür."
          checked={field.value} onChange={field.onChange} />
      )} />
    </form>
  );
}
