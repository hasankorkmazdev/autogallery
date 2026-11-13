import * as React from "react";

import { cn } from "@/lib/utils";

// Bitişik buton grubu: ilk butonun sol, son butonun sağ köşeleri yuvarlak, aradakiler köşesiz;
// komşu kenarlıklar üst üste biner. Butonlara ayrıca class verilmez.
const ButtonGroup = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    role="group"
    className={cn(
      "inline-flex shadow-sm [&>*]:min-w-9 [&>*]:rounded-none [&>*:first-child]:rounded-l-md [&>*:last-child]:rounded-r-md [&>*:not(:first-child)]:-ml-px",
      className,
    )}
    {...props}
  />
));
ButtonGroup.displayName = "ButtonGroup";

export { ButtonGroup };
