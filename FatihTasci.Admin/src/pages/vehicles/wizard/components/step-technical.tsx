import { useEffect, useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { InputField } from "@/components/ui/input-field";
import { NumberField, SelectField } from "@/components/form/form-fields";
import { vehiclesService } from "@/api/services/vehicles.service";
import { FuelType } from "@/api/types";
import { useLookupStore } from "@/store/lookup-store";
import { createTechnicalSchema, MAX_KILOMETER, MAX_YEAR, MIN_YEAR, type TechnicalOutput, type TechnicalValues } from "../schemas";
import { useStepSave } from "../hooks/use-step-form";
import type { StepProps } from "../types";

export function StepTechnical({ vehicle, onSaved, registerSave }: StepProps) {
  const lookups = useLookupStore((state) => state.lookups);
  const colors = useLookupStore((state) => state.colors);
  const schema = useMemo(() => createTechnicalSchema(vehicle.condition), [vehicle.condition]);
  const form = useForm<TechnicalValues, unknown, TechnicalOutput>({
    resolver: zodResolver(schema),
    defaultValues: {
      year: vehicle.year,
      kilometer: vehicle.kilometer,
      fuelType: vehicle.fuelType,
      transmission: vehicle.transmission,
      bodyType: vehicle.bodyType,
      drivetrain: vehicle.drivetrain,
      engineCc: vehicle.engineCc,
      powerHp: vehicle.powerHp,
      batteryKwh: vehicle.batteryKwh,
      rangeKm: vehicle.rangeKm,
      exteriorColorId: vehicle.exteriorColorId,
      interiorColor: vehicle.interiorColor ?? "",
    },
  });
  const { errors } = form.formState;
  const fuelType = useWatch({ control: form.control, name: "fuelType" });
  const isElectric = fuelType === FuelType.Electric;
  const hasBattery = isElectric || fuelType === FuelType.PlugInHybrid;

  // Yakıt tipi değişince geçersiz kalan alanlar temizlenir (elektrikte motor hacmi, diğerlerinde batarya).
  useEffect(() => {
    if (isElectric && form.getValues("engineCc") !== null) form.setValue("engineCc", null, { shouldDirty: true });
    if (!hasBattery) {
      if (form.getValues("batteryKwh") !== null) form.setValue("batteryKwh", null, { shouldDirty: true });
      if (form.getValues("rangeKm") !== null) form.setValue("rangeKm", null, { shouldDirty: true });
    }
  }, [isElectric, hasBattery, form]);

  useStepSave({
    form,
    registerSave,
    onSaved,
    submit: (v) =>
      vehiclesService.updateTechnical(vehicle.id, {
        year: v.year!,
        kilometer: v.kilometer!,
        fuelType: v.fuelType!,
        transmission: v.transmission!,
        bodyType: v.bodyType!,
        drivetrain: v.drivetrain!,
        engineCc: v.engineCc,
        powerHp: v.powerHp!,
        batteryKwh: v.batteryKwh,
        rangeKm: v.rangeKm,
        exteriorColorId: v.exteriorColorId!,
        interiorColor: v.interiorColor.trim() || null,
      }),
  });

  const colorOptions = colors.map((c) => ({ value: c.id, label: c.name }));

  return (
    <form autoComplete="off" noValidate onSubmit={(e) => e.preventDefault()} className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      <Controller control={form.control} name="year" render={({ field }) => (
        <NumberField id="year" label="Model yılı" required thousandSeparator={false} min={MIN_YEAR} max={MAX_YEAR}
          description={`${MIN_YEAR} ile ${MAX_YEAR} arası.`} error={errors.year?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="kilometer" render={({ field }) => (
        <NumberField id="kilometer" label="Kilometre" required min={0} max={MAX_KILOMETER} step={1000} suffix=" km"
          description="Göstergedeki değer. Sıfır araçta en fazla 100." error={errors.kilometer?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="fuelType" render={({ field }) => (
        <SelectField id="fuelType" label="Yakıt" required options={lookups?.fuelType ?? []}
          description="Elektrikte motor hacmi kapanır, batarya ve menzil açılır." error={errors.fuelType?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="transmission" render={({ field }) => (
        <SelectField id="transmission" label="Vites" required options={lookups?.transmission ?? []}
          description="Vites tipi." error={errors.transmission?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="bodyType" render={({ field }) => (
        <SelectField id="bodyType" label="Kasa tipi" required options={lookups?.bodyType ?? []}
          description="Ruhsattaki kasa tipi." error={errors.bodyType?.message} value={field.value} onChange={field.onChange} />
      )} />
      <Controller control={form.control} name="drivetrain" render={({ field }) => (
        <SelectField id="drivetrain" label="Çekiş" required options={lookups?.drivetrain ?? []}
          description="Önden, arkadan ya da dört çeker." error={errors.drivetrain?.message} value={field.value} onChange={field.onChange} />
      )} />
      {!isElectric && (
        <Controller control={form.control} name="engineCc" render={({ field }) => (
          <NumberField id="engineCc" label="Motor hacmi" required min={50} max={10000} step={100} suffix=" cc"
            description="50 ile 10.000 cc arası." error={errors.engineCc?.message} value={field.value} onChange={field.onChange} />
        )} />
      )}
      <Controller control={form.control} name="powerHp" render={({ field }) => (
        <NumberField id="powerHp" label="Motor gücü" required min={1} max={2000} step={10} suffix=" hp"
          description="Beygir gücü (hp)." error={errors.powerHp?.message} value={field.value} onChange={field.onChange} />
      )} />
      {hasBattery && (
        <Controller control={form.control} name="batteryKwh" render={({ field }) => (
          <NumberField id="batteryKwh" label="Batarya kapasitesi" min={1} max={300} suffix=" kWh"
            description="İsteğe bağlı. 1 ile 300 kWh arası." error={errors.batteryKwh?.message} value={field.value} onChange={field.onChange} />
        )} />
      )}
      {hasBattery && (
        <Controller control={form.control} name="rangeKm" render={({ field }) => (
          <NumberField id="rangeKm" label="Menzil" min={1} max={2000} step={10} suffix=" km"
            description="İsteğe bağlı. WLTP menzili." error={errors.rangeKm?.message} value={field.value} onChange={field.onChange} />
        )} />
      )}
      <Controller control={form.control} name="exteriorColorId" render={({ field }) => (
        <SelectField id="exteriorColorId" label="Dış renk" required options={colorOptions}
          description="Sitede renge göre filtrelemede kullanılır." error={errors.exteriorColorId?.message} value={field.value} onChange={field.onChange} />
      )} />
      <InputField id="interiorColor" label="İç döşeme" maxLength={50} autoComplete="off"
        description="İsteğe bağlı, örn. Siyah deri." error={errors.interiorColor?.message} {...form.register("interiorColor")} />
    </form>
  );
}
