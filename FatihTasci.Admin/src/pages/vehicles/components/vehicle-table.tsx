import { useMemo } from "react";
import { Link } from "react-router-dom";
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from "@tanstack/react-table";
import { ImageOff, Star, Tag } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { VehicleListItem, VehicleStatusValue } from "@/api/types";
import { formatDateTime, formatKm, formatPrice } from "@/lib/format";
import { VehicleStatusBadge } from "./vehicle-status-badge";
import { VehicleRowActions } from "./vehicle-row-actions";

interface VehicleTableProps {
  items: VehicleListItem[];
  loading: boolean;
  onStatusChange: (vehicle: VehicleListItem, status: VehicleStatusValue) => void;
  onCopy: (vehicle: VehicleListItem) => void;
}

function CoverCell({ vehicle }: { vehicle: VehicleListItem }) {
  if (!vehicle.coverUrl) {
    return (
      <div className="flex h-12 w-[72px] items-center justify-center rounded-md bg-muted text-muted-foreground">
        <ImageOff className="size-4" />
      </div>
    );
  }
  return <img src={vehicle.coverUrl} alt="" className="h-12 w-[72px] rounded-md object-cover" loading="lazy" />;
}

// Kampanya varsa eski fiyat üstü çizili, altında kampanyalı fiyat.
function PriceCell({ vehicle }: { vehicle: VehicleListItem }) {
  const { price, campaignPrice } = vehicle;
  if (!price || !campaignPrice || campaignPrice >= price) return formatPrice(price);
  return (
    <div className="flex flex-col">
      <span className="text-xs text-muted-foreground line-through">{formatPrice(price)}</span>
      <span className="font-medium">{formatPrice(campaignPrice)}</span>
    </div>
  );
}

// Dar ekranda gizlenen sütunların (yıl, km, fiyat, durum) özeti başlığın altında görünür.
function TitleCell({ vehicle }: { vehicle: VehicleListItem }) {
  return (
    <div className="flex flex-col gap-0.5">
      <Link to={`/vehicles/${vehicle.id}`} className="font-medium hover:underline">
        {vehicle.title}
      </Link>
      <span className="text-xs text-muted-foreground flex flex-wrap items-center gap-1">
        İlan No {vehicle.listingNo} · {vehicle.photoCount} fotoğraf
        {vehicle.isFeatured && <Star className="size-3" aria-label="Vitrinde" />}
        {vehicle.isOpportunity && <Tag className="size-3" aria-label="Fırsat aracı" />}
      </span>
      <span className="text-xs text-muted-foreground">
        {vehicle.advisorName ? `Danışman: ${vehicle.advisorName}` : "Danışman atanmamış"}
      </span>
      <span className="text-xs text-muted-foreground xl:hidden">
        {vehicle.year ?? "-"} · {formatKm(vehicle.kilometer)}
      </span>
      <div className="flex flex-wrap items-center gap-2 pt-1 lg:hidden">
        <span className="text-sm">
          <PriceCell vehicle={vehicle} />
        </span>
        <span className="sm:hidden">
          <VehicleStatusBadge status={vehicle.status} />
        </span>
      </div>
    </div>
  );
}

// Sütun id'sine göre hizalama ve hangi genişlikte gizleneceği (başlık ve hücrede aynı).
// Eşikler md'den itibaren açık kenar çubuğunun (16rem) kapladığı yeri hesaba katar.
const COLUMN_CLASSES: Record<string, string> = {
  cover: "max-sm:hidden",
  title: "text-left",
  year: "max-xl:hidden whitespace-nowrap",
  kilometer: "max-xl:hidden whitespace-nowrap",
  price: "max-lg:hidden whitespace-nowrap",
  status: "max-sm:hidden",
  updated: "max-2xl:hidden",
};

export function VehicleTable({ items, loading, onStatusChange, onCopy }: VehicleTableProps) {
  const columns = useMemo<ColumnDef<VehicleListItem>[]>(
    () => [
      { id: "cover", header: "", cell: ({ row }) => <CoverCell vehicle={row.original} /> },
      { id: "title", header: "Araç", cell: ({ row }) => <TitleCell vehicle={row.original} /> },
      { accessorKey: "year", header: "Yıl", cell: ({ row }) => row.original.year ?? "-" },
      { accessorKey: "kilometer", header: "Kilometre", cell: ({ row }) => formatKm(row.original.kilometer) },
      { accessorKey: "price", header: "Fiyat", cell: ({ row }) => <PriceCell vehicle={row.original} /> },
      { accessorKey: "status", header: "Durum", cell: ({ row }) => <VehicleStatusBadge status={row.original.status} /> },
      {
        id: "updated",
        header: "Son güncelleme",
        cell: ({ row }) => (
          <span className="text-muted-foreground text-xs">
            {formatDateTime(row.original.lastUpdateDate ?? row.original.createDate)}
          </span>
        ),
      },
      {
        id: "actions",
        header: () => <div className="text-right">İşlemler</div>,
        cell: ({ row }) => (
          <VehicleRowActions vehicle={row.original} onStatusChange={onStatusChange} onCopy={onCopy} />
        ),
      },
    ],
    [onStatusChange, onCopy],
  );

  // TanStack Table bilinçli olarak memo dışı fonksiyon döndürür; React Compiler uyarısı bu kullanımda geçersiz.
  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({ data: items, columns, getCoreRowModel: getCoreRowModel() });

  const renderBody = () => {
    if (items.length === 0) {
      return (
        <TableRow>
          <TableCell colSpan={columns.length} className="h-24 text-center">
            <span className="text-muted-foreground">{loading ? "Yükleniyor..." : "Araç bulunamadı."}</span>
          </TableCell>
        </TableRow>
      );
    }
    return table.getRowModel().rows.map((row) => (
      <TableRow key={row.id}>
        {row.getVisibleCells().map((cell) => (
          <TableCell key={cell.id} className={COLUMN_CLASSES[cell.column.id]}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>
        ))}
      </TableRow>
    ));
  };

  return (
    <div className="overflow-x-auto">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((group) => (
            <TableRow key={group.id}>
              {group.headers.map((header) => (
                <TableHead key={header.id} className={COLUMN_CLASSES[header.column.id]}>
                  {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>{renderBody()}</TableBody>
      </Table>
    </div>
  );
}
