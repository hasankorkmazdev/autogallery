import { Combobox } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import { useLookupStore } from "@/store/lookup-store";

const ALL = "all";

interface VehicleFiltersProps {
  search: string;
  status: number | undefined;
  onSearchChange: (value: string) => void;
  onStatusChange: (value: number | undefined) => void;
}

export function VehicleFilters({ search, status, onSearchChange, onStatusChange }: VehicleFiltersProps) {
  // Selector'da `?? []` kullanılmaz: her çağrıda yeni dizi döner ve sonsuz render'a yol açar.
  const statusOptions = useLookupStore((state) => state.lookups?.vehicleStatus) ?? [];

  return (
    <div className="flex flex-col gap-2 md:flex-row md:items-center">
      <Input
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        placeholder="Başlık, marka, model, paket ya da ilan no ile ara"
        aria-label="Araç ara"
        autoComplete="off"
        className="md:w-80"
      />
      <Combobox
        value={status === undefined ? ALL : String(status)}
        onValueChange={(v) => onStatusChange(v === ALL ? undefined : Number(v))}
        options={[
          { value: ALL, label: "Tüm durumlar (arşiv hariç)" },
          ...statusOptions.map((option) => ({ value: String(option.value), label: option.label })),
        ]}
        aria-label="Durum filtresi"
        className="md:w-56"
      />
    </div>
  );
}
