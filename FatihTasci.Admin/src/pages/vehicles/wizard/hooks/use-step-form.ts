import { useCallback, useEffect, useRef } from "react";
import type { FieldValues, UseFormReturn } from "react-hook-form";
import { toast } from "sonner";
import type { VehicleDetail } from "@/api/types";
import type { StepSaveHandler } from "../types";

interface UseStepSaveOptions<TInput extends FieldValues, TOutput extends FieldValues> {
  form: UseFormReturn<TInput, unknown, TOutput>;
  registerSave: (handler: StepSaveHandler | null) => void;
  submit: (values: TOutput) => Promise<{ data: VehicleDetail; message: string }>;
  onSaved: (vehicle: VehicleDetail) => void;
  /** Form değişmemiş olsa da kaydet (örn. yeni araç taslağı) */
  alwaysSave?: boolean;
}

/**
 * Sihirbaz adımlarının ortak kaydetme davranışı (otomatik taslak kaydı):
 * adım değişirken ya da "Kaydet" ile form doğrulanır ve değiştiyse sunucuya yazılır.
 */
export function useStepSave<TInput extends FieldValues, TOutput extends FieldValues = TInput>({
  form,
  registerSave,
  submit,
  onSaved,
  alwaysSave = false,
}: UseStepSaveOptions<TInput, TOutput>) {
  const submitRef = useRef(submit);
  const onSavedRef = useRef(onSaved);
  // formState bir proxy'dir: isDirty render sırasında okunmazsa güncellenmez.
  const { isDirty } = form.formState;
  const isDirtyRef = useRef(isDirty);

  useEffect(() => {
    submitRef.current = submit;
    onSavedRef.current = onSaved;
    isDirtyRef.current = isDirty;
  });

  const save = useCallback<StepSaveHandler>(async () => {
    if (!alwaysSave && !isDirtyRef.current) return true;

    let ok = false;
    await form.handleSubmit(
      async (values) => {
        try {
          const result = await submitRef.current(values);
          toast.success(result.message);
          onSavedRef.current(result.data);
          form.reset(form.getValues());
          ok = true;
        } catch (error) {
          toast.error((error as Error).message);
        }
      },
      () => toast.error("Formdaki hataları düzeltin."),
    )();
    return ok;
  }, [form, alwaysSave]);

  useEffect(() => {
    registerSave(save);
    return () => registerSave(null);
  }, [registerSave, save]);

  return save;
}
