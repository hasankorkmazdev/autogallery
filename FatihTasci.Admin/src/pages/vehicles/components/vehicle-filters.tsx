import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useLookupStore } from "@/store/lookup-store";

const ALL = "all";

interface VehicleFiltersProps {
  search: string;
  status: number | undefined;
  onSearchChange: (value: string) => void;
  onStatusChange: (value: number | undefined) => void;
}

export function VehicleFilters({ search, status, onSearchChange, onStatusChange }: VehicleFiltersProps) {
  const statusOptions = useLookupStore((state) => state.lookups?.vehicleStatus ?? []);

  return (
    <div className="flex flex-col gap-2 md:flex-row md:items-center">
      <Input
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        placeholder="Başlık, marka, model ya da ilan no ile ara"
        aria-label="Araç ara"
        autoComplete="off"
        className="md:w-80"
      />
      <Select
        value={status === undefined ? ALL : String(status)}
        onValueChange={(v) => onStatusChange(v === ALL ? undefined : Number(v))}>
        <SelectTrigger className="md:w-48" aria-label="Durum filtresi">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>Tüm durumlar (arşiv hariç)</SelectItem>
          {statusOptions.map((option) => (
            <SelectItem key={option.value} value={String(option.value)}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
