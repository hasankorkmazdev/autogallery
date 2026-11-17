import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

interface PageLayoutProps {
  children: ReactNode;
  className?: string;
}

interface PageHeaderProps {
  title: string;
  description?: string;
  /** Simge boyutu burada verilir; sayfa yalnız simgeyi geçer (`<Car />`). */
  icon?: ReactNode;
  actions?: ReactNode;
  className?: string;
}

interface PageContentProps {
  children: ReactNode;
  className?: string;
  isFilled?: boolean; // If true, removes padding and allows content to fill the card
}

// Açılış animasyonu yalnız yayında: geliştirmede test ve ekran görüntüleri yarı saydam yakalanmasın.
const ENTER_ANIMATION = import.meta.env.DEV ? "" : "animate-in fade-in duration-500";

// Panel genelinde tek responsive düzen: telefonda dar kenar boşluğu, md'den itibaren geniş.
export function PageLayout({ children, className }: PageLayoutProps) {
  return (
    <div
      className={cn(
        "flex min-w-0 flex-col gap-4 h-full p-4 md:gap-6 md:p-6",
        ENTER_ANIMATION,
        className,
      )}>
      {children}
    </div>
  );
}

// Butonlar sığdığı sürece başlığın sağında, sığmayınca (telefon, uzun başlık) alt satırda.
export function PageHeader({
  title,
  description,
  icon,
  actions,
  className,
}: PageHeaderProps) {
  return (
    <div
      className={cn(
        "flex flex-wrap items-center justify-between gap-x-4 gap-y-3 shrink-0",
        className,
      )}>
      <div className="flex min-w-0 flex-1 basis-72 flex-col gap-1">
        <h1 className="flex items-center gap-2 text-2xl font-bold tracking-tight text-foreground break-words md:text-3xl">
          {icon && <span className="shrink-0 text-primary [&>svg]:size-6 md:[&>svg]:size-8">{icon}</span>}
          <span className="min-w-0">{title}</span>
        </h1>
        {description && (
          <p className="text-muted-foreground text-sm">{description}</p>
        )}
      </div>
      {actions && <div className="flex max-w-full flex-wrap items-center gap-2">{actions}</div>}
    </div>
  );
}

export function PageContent({
  children,
  className,
  isFilled = false,
}: PageContentProps) {
  return (
    <div
      className={cn(
        "flex-1 min-h-0 min-w-0 bg-card border rounded-xl shadow-sm overflow-hidden flex flex-col",
        !isFilled && "p-3 md:p-4",
        className,
      )}>
      {children}
    </div>
  );
}
