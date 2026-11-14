import * as React from "react";
import { Check, ChevronDown } from "lucide-react";

import { cn } from "@/lib/utils";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

export interface ComboboxOption {
  value: string;
  label: string;
  /** Seçeneğin solunda gösterilir (örn. marka logosu) */
  icon?: React.ReactNode;
}

interface ComboboxProps {
  id?: string;
  options: ComboboxOption[];
  value: string;
  onValueChange: (value: string) => void;
  placeholder?: string;
  searchPlaceholder?: string;
  emptyText?: string;
  disabled?: boolean;
  className?: string;
  "aria-label"?: string;
  "aria-invalid"?: boolean;
}

// Türkçe büyük/küçük harf ve aksan farkını yok sayar: "skoda" → "Škoda", "IST" → "ist", "citroen" → "Citroën".
const FOLD: Record<string, string> = { ı: "i", ş: "s", ğ: "g", ü: "u", ö: "o", ç: "c" };
function fold(text: string) {
  return text
    .toLocaleLowerCase("tr")
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .replace(/[ışğüöç]/g, (c) => FOLD[c]);
}

// Başta eşleşen üstte, içinde geçen altta; eşleşmeyen gizlenir.
function filter(_value: string, search: string, keywords?: string[]) {
  const query = fold(search.trim());
  if (!query) return 1;
  const label = fold(keywords?.[0] ?? "");
  if (label.startsWith(query)) return 1;
  if (label.split(/[\s-]+/).some((word) => word.startsWith(query))) return 0.8;
  return label.includes(query) ? 0.5 : 0;
}

// Aranabilir seçim kutusu: Select ile aynı görünüm, açılınca üstte arama kutusu.
const Combobox = React.forwardRef<HTMLButtonElement, ComboboxProps>(
  (
    {
      id,
      options,
      value,
      onValueChange,
      placeholder = "Seçin",
      searchPlaceholder = "Ara...",
      emptyText = "Sonuç yok.",
      disabled,
      className,
      ...aria
    },
    ref,
  ) => {
    const [open, setOpen] = React.useState(false);
    const selected = options.find((option) => option.value === value);

    return (
      // modal: Dialog içinde açıldığında listenin fare tekerleğiyle kaydırılabilmesi için gerekli
      <Popover open={open} onOpenChange={setOpen} modal>
        <PopoverTrigger asChild disabled={disabled}>
          <button
            ref={ref}
            id={id}
            type="button"
            role="combobox"
            aria-expanded={open}
            aria-label={aria["aria-label"]}
            aria-invalid={aria["aria-invalid"]}
            className={cn(
              "flex h-8 w-full items-center justify-between gap-2 whitespace-nowrap rounded-md border border-input bg-transparent px-3 py-2 text-left text-sm shadow-sm ring-offset-background focus:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 aria-[invalid=true]:border-destructive",
              className,
            )}>
            <span className={cn("flex min-w-0 items-center gap-2", !selected && "text-muted-foreground")}>
              {selected?.icon}
              <span className="truncate">{selected ? selected.label : placeholder}</span>
            </span>
            <ChevronDown className="h-4 w-4 shrink-0 opacity-50" />
          </button>
        </PopoverTrigger>
        <PopoverContent className="w-[var(--radix-popover-trigger-width)] min-w-[12rem] p-0" align="start">
          <Command filter={filter}>
            <CommandInput placeholder={searchPlaceholder} autoComplete="off" />
            <CommandList>
              <CommandEmpty>{emptyText}</CommandEmpty>
              <CommandGroup>
                {options.map((option) => (
                  <CommandItem
                    key={option.value}
                    value={option.value}
                    keywords={[option.label]}
                    onSelect={() => {
                      onValueChange(option.value);
                      setOpen(false);
                    }}>
                    <span className="flex min-w-0 flex-1 items-center gap-2">
                      {option.icon}
                      <span className="truncate">{option.label}</span>
                    </span>
                    <Check className={cn("ml-2 h-4 w-4 shrink-0", option.value === value ? "opacity-100" : "opacity-0")} />
                  </CommandItem>
                ))}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    );
  },
);
Combobox.displayName = "Combobox";

export { Combobox };
