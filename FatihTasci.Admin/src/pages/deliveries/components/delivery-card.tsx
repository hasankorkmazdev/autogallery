import { ImageOff, Pencil, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { formatDate } from "@/lib/format";
import type { Delivery } from "@/api/types";

interface DeliveryCardProps {
  delivery: Delivery;
  onEdit: (delivery: Delivery) => void;
  onDelete: (delivery: Delivery) => void;
}

function DeliveryStatus({ delivery }: { delivery: Delivery }) {
  if (!delivery.isActive) return <Badge variant="secondary">Gizli</Badge>;
  if (!delivery.mediumUrl) return <Badge variant="outline">Fotoğraf yok</Badge>;
  return <Badge>Sitede</Badge>;
}

// Teslimat kartı: fotoğraf sitedeki gibi 4:5 kırpılır; araç, müşteri, tarih, durum, düzenle / sil.
export function DeliveryCard({ delivery, onEdit, onDelete }: DeliveryCardProps) {
  const meta = [delivery.customerName, delivery.city, formatDate(delivery.deliveredAt)].filter(Boolean).join(" · ");

  return (
    <div className="relative flex flex-col overflow-hidden rounded-lg border bg-card">
      {delivery.mediumUrl ? (
        <img src={delivery.mediumUrl} alt={delivery.vehicleTitle} className="aspect-[4/5] w-full object-cover" loading="lazy" />
      ) : (
        <div className="flex aspect-[4/5] w-full items-center justify-center bg-muted">
          <ImageOff className="size-8 text-muted-foreground" />
        </div>
      )}
      <div className="absolute left-2 top-2">
        <DeliveryStatus delivery={delivery} />
      </div>
      <div className="flex flex-col gap-1 p-3">
        <div className="font-medium leading-snug break-words">{delivery.vehicleTitle}</div>
        <div className="text-sm text-muted-foreground">{meta}</div>
      </div>
      <div className="mt-auto flex justify-end gap-1 border-t p-1">
        <Button type="button" variant="ghost" size="icon-sm" title="Düzenle" onClick={() => onEdit(delivery)}>
          <Pencil className="w-4 h-4" />
        </Button>
        <Button type="button" variant="ghost-destructive" size="icon-sm" title="Sil" onClick={() => onDelete(delivery)}>
          <Trash2 className="w-4 h-4" />
        </Button>
      </div>
    </div>
  );
}
