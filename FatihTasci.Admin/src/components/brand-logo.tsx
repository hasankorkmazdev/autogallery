import { CarFront } from "lucide-react";
import { cn } from "@/lib/utils";

interface BrandLogoProps {
  src: string | null | undefined;
  alt?: string;
  className?: string;
}

// Marka logosu (/images/brands/{slug}.png). Logolar çoğunlukla koyu renkli olduğu için koyu temada da
// okunsun diye beyaz bir kutuda gösterilir. Logo yoksa genel araç ikonu görünür.
export function BrandLogo({ src, alt = "", className }: BrandLogoProps) {
  return (
    <span className={cn("inline-flex size-6 shrink-0 items-center justify-center overflow-hidden rounded bg-white", className)}>
      {src ? (
        <img src={src} alt={alt} className="size-full object-contain p-0.5" loading="lazy" />
      ) : (
        <CarFront className="size-3.5 text-neutral-500" />
      )}
    </span>
  );
}
