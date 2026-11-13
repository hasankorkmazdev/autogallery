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
        <TableCell>
          <div className="flex flex-col">
            <span className="font-medium">{lead.fullName}</span>
            <span className="text-xs text-muted-foreground">{lead.phone}</span>
          </div>
        </TableCell>
        <TableCell>{labelOf(lookups?.leadType, lead.type)}</TableCell>
        <TableCell>{lead.vehicleTitle ? `${lead.vehicleTitle} (${lead.vehicleListingNo})` : "-"}</TableCell>
        <TableCell>
          <Badge variant={STATUS_VARIANTS[lead.status]}>{labelOf(lookups?.leadStatus, lead.status)}</Badge>
        </TableCell>
        <TableCell>
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
            <TableHead>Kişi</TableHead>
            <TableHead>Tür</TableHead>
            <TableHead>Araç</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Tarih</TableHead>
            <TableHead className="text-right">İşlem</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>{renderBody()}</TableBody>
      </Table>
    </div>
  );
}
