import { useMemo } from "react";
import { Link } from "react-router-dom";
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from "@tanstack/react-table";
import { ImageOff, Star } from "lucide-react";
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

function TitleCell({ vehicle }: { vehicle: VehicleListItem }) {
  return (
    <div className="flex flex-col">
      <Link to={`/vehicles/${vehicle.id}`} className="font-medium hover:underline">
        {vehicle.title}
      </Link>
      <span className="text-xs text-muted-foreground flex items-center gap-1">
        İlan No {vehicle.listingNo} · {vehicle.photoCount} fotoğraf
        {vehicle.isFeatured && <Star className="size-3" aria-label="Vitrinde" />}
      </span>
    </div>
  );
}

export function VehicleTable({ items, loading, onStatusChange, onCopy }: VehicleTableProps) {
  const columns = useMemo<ColumnDef<VehicleListItem>[]>(
    () => [
      { id: "cover", header: "", cell: ({ row }) => <CoverCell vehicle={row.original} /> },
      { id: "title", header: "Araç", cell: ({ row }) => <TitleCell vehicle={row.original} /> },
      { accessorKey: "year", header: "Yıl", cell: ({ row }) => row.original.year ?? "-" },
      { accessorKey: "kilometer", header: "Kilometre", cell: ({ row }) => formatKm(row.original.kilometer) },
      { accessorKey: "price", header: "Fiyat", cell: ({ row }) => formatPrice(row.original.price) },
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
          <TableCell key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>
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
                <TableHead key={header.id}>
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
