import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { SelectField } from "@/components/form/form-fields";
import { BrandLogo } from "@/components/brand-logo";

interface CatalogSelectProps {
  id: string;
  label: string;
  description: string;
  addTitle: string;
  items: { id: number; name: string; logoUrl?: string | null }[];
  value: number | null;
  error?: string;
  disabled?: boolean;
  onChange: (value: number | null) => void;
  onAdd: () => void;
}

// Seçim kutusu + "listede yoksa ekle" butonu
export function CatalogSelect({
  id,
  label,
  description,
  addTitle,
  items,
  value,
  error,
  disabled,
  onChange,
  onAdd,
}: CatalogSelectProps) {
  return (
    <div className="flex items-start gap-2">
      <div className="flex-1">
        <SelectField
          id={id}
          label={label}
          description={description}
          error={error}
          required
          disabled={disabled}
          options={items.map((x) => ({
            value: x.id,
            label: x.name,
            icon: x.logoUrl !== undefined ? <BrandLogo src={x.logoUrl} /> : undefined,
          }))}
          value={value}
          onChange={onChange}
        />
      </div>
      <Button type="button" variant="outline" size="icon" className="mt-6" disabled={disabled} title={addTitle} onClick={onAdd}>
        <Plus className="w-4 h-4" />
      </Button>
    </div>
  );
}
