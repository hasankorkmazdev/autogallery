import { Badge, type BadgeProps } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { Lead } from "@/api/types";
import { formatDateTime } from "@/lib/format";
import { labelOf, useLookupStore } from "@/store/lookup-store";

const STATUS_VARIANTS: Record<number, BadgeProps["variant"]> = {
  0: "warning",
  1: "secondary",
  2: "default",
  3: "outline",
};

interface LeadTableProps {
  items: Lead[];
  loading: boolean;
  onOpen: (lead: Lead) => void;
}

export function LeadTable({ items, loading, onOpen }: LeadTableProps) {
  const lookups = useLookupStore((state) => state.lookups);
  // Satış / takas talebinde ziyaretçinin aracı ve fotoğraf sayısı, diğerlerinde talebin geldiği ilan.
  const vehicleText = (lead: Lead) => {
    if (lead.sellBrand) {
      const year = lead.sellYear ? ` (${lead.sellYear})` : "";
      const photos = lead.media.length > 0 ? ` · ${lead.media.length} fotoğraf` : "";
      return `${lead.sellBrand} ${lead.sellModel ?? ""}${year}${photos}`;
    }
    return lead.vehicleTitle ? `${lead.vehicleTitle} (${lead.vehicleListingNo})` : null;
  };
  const statusBadge = (lead: Lead) => (
    <Badge variant={STATUS_VARIANTS[lead.status]}>{labelOf(lookups?.leadStatus, lead.status)}</Badge>
  );

  const renderBody = () => {
    if (items.length === 0) {
      return (
        <TableRow>
          <TableCell colSpan={6} className="h-24 text-center">
            <span className="text-muted-foreground">{loading ? "Yükleniyor..." : "Talep bulunamadı."}</span>
          </TableCell>
        </TableRow>
      );
    }
    return items.map((lead) => (
      <TableRow key={lead.id}>
        <TableCell className="text-left">
          {/* Dar ekranda gizlenen sütunların (tür, tarih, araç, durum) özeti burada görünür. */}
          <div className="flex flex-col gap-0.5">
            <span className="font-medium">{lead.fullName}</span>
            <span className="text-xs text-muted-foreground">{lead.phone}</span>
            <span className="text-xs text-muted-foreground lg:hidden">
              {labelOf(lookups?.leadType, lead.type)} · {formatDateTime(lead.createDate)}
            </span>
            {vehicleText(lead) && <span className="text-xs text-muted-foreground xl:hidden">{vehicleText(lead)}</span>}
            <span className="pt-1 sm:hidden">{statusBadge(lead)}</span>
          </div>
        </TableCell>
        <TableCell className="max-lg:hidden">{labelOf(lookups?.leadType, lead.type)}</TableCell>
        <TableCell className="max-xl:hidden">{vehicleText(lead) ?? "-"}</TableCell>
        <TableCell className="max-sm:hidden">{statusBadge(lead)}</TableCell>
        <TableCell className="max-lg:hidden">
          <span className="text-xs text-muted-foreground">{formatDateTime(lead.createDate)}</span>
        </TableCell>
        <TableCell className="text-right">
          <Button variant="outline" size="sm" onClick={() => onOpen(lead)}>
            Aç
          </Button>
        </TableCell>
      </TableRow>
    ));
  };

  return (
    <div className="overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="text-left">Kişi</TableHead>
            <TableHead className="max-lg:hidden">Tür</TableHead>
            <TableHead className="max-xl:hidden">Araç</TableHead>
            <TableHead className="max-sm:hidden">Durum</TableHead>
            <TableHead className="max-lg:hidden">Tarih</TableHead>
            <TableHead className="text-right">İşlem</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>{renderBody()}</TableBody>
      </Table>
    </div>
  );
}
