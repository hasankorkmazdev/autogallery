import { useEffect, useRef } from "react";
import { Button } from "@/components/ui/button";
import { WIZARD_STEPS } from "../types";

interface WizardStepsNavProps {
  current: number;
  enabled: boolean;
  onSelect: (step: number) => void;
}

// Adımlar arasında serbest geçiş. Yeni araçta taslak oluşana kadar yalnız ilk adım açıktır.
// Her genişlikte tek satır: yer varsa butonlar satırı doldurur, yoksa şerit yatay kayar
// (açık adım ortalanır). Açıklama geniş ekranda görünür, diğerlerinde ipucudur.
export function WizardStepsNav({ current, enabled, onSelect }: WizardStepsNavProps) {
  const navRef = useRef<HTMLElement>(null);

  useEffect(() => {
    const nav = navRef.current;
    const active = nav?.querySelector<HTMLElement>('[aria-current="step"]');
    if (!nav || !active || nav.scrollWidth <= nav.clientWidth) return;
    nav.scrollLeft = active.offsetLeft - (nav.clientWidth - active.offsetWidth) / 2;
  }, [current]);

  // Şerit taşıyorken fare tekerleği yatay kaydırır; şeridin ucunda sayfa kaydırması sürer.
  useEffect(() => {
    const nav = navRef.current;
    if (!nav) return;
    const onWheel = (e: WheelEvent) => {
      if (Math.abs(e.deltaY) <= Math.abs(e.deltaX)) return;
      const max = nav.scrollWidth - nav.clientWidth;
      const next = Math.min(max, Math.max(0, nav.scrollLeft + e.deltaY));
      if (next === nav.scrollLeft) return;
      e.preventDefault();
      nav.scrollLeft = next;
    };
    nav.addEventListener("wheel", onWheel, { passive: false });
    return () => nav.removeEventListener("wheel", onWheel);
  }, []);

  return (
    <nav
      ref={navRef}
      aria-label="Sihirbaz adımları"
      className="no-scrollbar relative -mx-3 flex gap-2 overflow-x-auto px-3 md:-mx-4 md:px-4">
      {WIZARD_STEPS.map((item) => (
        <Button
          key={item.step}
          type="button"
          variant={item.step === current ? "default" : "outline"}
          size="row"
          className="w-auto shrink-0 grow"
          title={item.description}
          disabled={!enabled && item.step !== 1}
          aria-current={item.step === current ? "step" : undefined}
          onClick={() => onSelect(item.step)}>
          <span className="flex flex-col items-start">
            <span>
              {item.step}. {item.title}
            </span>
            <span className="text-xs opacity-70 max-2xl:hidden">{item.description}</span>
          </span>
        </Button>
      ))}
    </nav>
  );
}
