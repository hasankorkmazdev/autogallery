import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

interface PageLayoutProps {
  children: ReactNode;
  className?: string;
}

interface PageHeaderProps {
  title: string;
  description?: string;
  icon?: ReactNode;
  actions?: ReactNode;
  className?: string;
}

interface PageContentProps {
  children: ReactNode;
  className?: string;
  isFilled?: boolean; // If true, removes padding and allows content to fill the card
}

export function PageLayout({ children, className }: PageLayoutProps) {
  return (
    <div
      className={cn(
        "flex flex-col gap-6 h-full p-6 animate-in fade-in duration-500",
        className,
      )}>
      {children}
    </div>
  );
}

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
        "flex items-center justify-between gap-4 shrink-0",
        className,
      )}>
      <div className="flex flex-col gap-1">
        <h1 className="text-3xl font-bold tracking-tight text-foreground flex items-center gap-2">
          {icon && <span className="text-primary">{icon}</span>}
          {title}
        </h1>
        {description && (
          <p className="text-muted-foreground text-sm">{description}</p>
        )}
      </div>
      {actions && <div className="flex items-center gap-2">{actions}</div>}
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
        "flex-1 min-h-0 bg-card border rounded-xl shadow-sm overflow-hidden flex flex-col",
        !isFilled && "p-4",
        className,
      )}>
      {children}
    </div>
  );
}
