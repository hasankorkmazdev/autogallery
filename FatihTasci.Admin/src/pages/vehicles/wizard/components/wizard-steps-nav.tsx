import { Button } from "@/components/ui/button";
import { WIZARD_STEPS } from "../types";

interface WizardStepsNavProps {
  current: number;
  enabled: boolean;
  onSelect: (step: number) => void;
}

// Adımlar arasında serbest geçiş. Yeni araçta taslak oluşana kadar yalnız ilk adım açıktır.
export function WizardStepsNav({ current, enabled, onSelect }: WizardStepsNavProps) {
  return (
    <nav aria-label="Sihirbaz adımları" className="grid grid-cols-2 gap-2 sm:grid-cols-4 lg:grid-cols-7">
      {WIZARD_STEPS.map((item) => (
        <Button
          key={item.step}
          type="button"
          variant={item.step === current ? "default" : "outline"}
          size="row"
          disabled={!enabled && item.step !== 1}
          aria-current={item.step === current ? "step" : undefined}
          onClick={() => onSelect(item.step)}>
          <span className="flex flex-col items-start">
            <span>
              {item.step}. {item.title}
            </span>
            <span className="text-xs opacity-70">{item.description}</span>
          </span>
        </Button>
      ))}
    </nav>
  );
}
