import * as React from "react";
import { Search } from "lucide-react";

import { Input } from "@/components/ui/input";

// Solunda arama ikonu olan Input. className (genişlik, flex vb.) sarmalayıcıya verilir.
const SearchInput = React.forwardRef<
  HTMLInputElement,
  React.ComponentProps<typeof Input>
>(({ className, ...props }, ref) => (
  <div className={["relative", className].filter(Boolean).join(" ")}>
    <Search className="pointer-events-none absolute left-2.5 top-2 size-4 text-muted-foreground" />
    <Input ref={ref} className="pl-9" {...props} />
  </div>
));
SearchInput.displayName = "SearchInput";

export { SearchInput };
