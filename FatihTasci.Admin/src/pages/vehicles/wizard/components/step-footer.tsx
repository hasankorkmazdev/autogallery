import { ChevronLeft, ChevronRight, Save } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

interface StepFooterProps {
  step: number;
  lastStep: number;
  isNew: boolean;
  saving: boolean;
  onBack: () => void;
  onNext: () => void;
  onSave: () => void;
}

export function StepFooter({ step, lastStep, isNew, saving, onBack, onNext, onSave }: StepFooterProps) {
  const nextLabel = isNew ? "Taslağı oluştur ve devam et" : "Kaydet ve devam et";

  return (
    <div className="flex flex-col gap-4">
      <Separator />
      <div className="flex flex-col-reverse gap-2 sm:flex-row sm:justify-between">
        <Button type="button" variant="outline" disabled={step === 1 || saving} onClick={onBack}>
          <ChevronLeft className="w-4 h-4 mr-1" />
          Geri
        </Button>
        <div className="flex flex-col gap-2 sm:flex-row">
          {!isNew && (
            <Button type="button" variant="outline" disabled={saving} onClick={onSave}>
              <Save className="w-4 h-4 mr-2" />
              Kaydet
            </Button>
          )}
          {step < lastStep && (
            <Button type="button" disabled={saving} onClick={onNext}>
              {saving ? "Kaydediliyor..." : nextLabel}
              <ChevronRight className="w-4 h-4 ml-1" />
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
