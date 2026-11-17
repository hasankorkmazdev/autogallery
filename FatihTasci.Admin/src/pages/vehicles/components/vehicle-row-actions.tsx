import { useNavigate } from "react-router-dom";
import { Copy, ExternalLink, MoreHorizontal, Pencil } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { VehicleStatus, type VehicleListItem, type VehicleStatusValue } from "@/api/types";
import { publicSiteUrl } from "@/lib/format";

interface VehicleRowActionsProps {
  vehicle: VehicleListItem;
  onStatusChange: (vehicle: VehicleListItem, status: VehicleStatusValue) => void;
  onCopy: (vehicle: VehicleListItem) => void;
}

const isVisibleOnSite = (status: VehicleStatusValue) =>
  status === VehicleStatus.Published || status === VehicleStatus.Reserved || status === VehicleStatus.Sold;

// Listeden tek tıkla Rezerve / Satıldı.
export function VehicleRowActions({ vehicle, onStatusChange, onCopy }: VehicleRowActionsProps) {
  const navigate = useNavigate();
  const canReserve = vehicle.status === VehicleStatus.Published;
  const canSell = vehicle.status === VehicleStatus.Published || vehicle.status === VehicleStatus.Reserved;

  return (
    <div className="flex items-center justify-end gap-1">
      {canReserve && (
        <Button variant="outline" size="sm" className="max-xl:hidden" onClick={() => onStatusChange(vehicle, VehicleStatus.Reserved)}>
          Rezerve
        </Button>
      )}
      {canSell && (
        <Button variant="outline" size="sm" className="max-xl:hidden" onClick={() => onStatusChange(vehicle, VehicleStatus.Sold)}>
          Satıldı
        </Button>
      )}
      <Button variant="ghost" size="icon-sm" title="Düzenle" onClick={() => navigate(`/vehicles/${vehicle.id}`)}>
        <Pencil className="w-4 h-4" />
      </Button>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="icon-sm" title="Diğer işlemler">
            <MoreHorizontal className="w-4 h-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          {isVisibleOnSite(vehicle.status) && (
            <DropdownMenuItem asChild>
              <a href={publicSiteUrl(vehicle.publicUrl)} target="_blank" rel="noreferrer">
                <ExternalLink /> Sitede gör
              </a>
            </DropdownMenuItem>
          )}
          <DropdownMenuItem onClick={() => onCopy(vehicle)}>
            <Copy /> Benzer ilan olarak kopyala
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          {/* Dar ekranda tek tık butonları yer kaplamasın diye burada */}
          {canReserve && (
            <DropdownMenuItem className="xl:hidden" onClick={() => onStatusChange(vehicle, VehicleStatus.Reserved)}>
              Rezerve
            </DropdownMenuItem>
          )}
          {canSell && (
            <DropdownMenuItem className="xl:hidden" onClick={() => onStatusChange(vehicle, VehicleStatus.Sold)}>
              Satıldı
            </DropdownMenuItem>
          )}
          {vehicle.status === VehicleStatus.Reserved && (
            <DropdownMenuItem onClick={() => onStatusChange(vehicle, VehicleStatus.Published)}>
              Rezervasyonu kaldır
            </DropdownMenuItem>
          )}
          {vehicle.status === VehicleStatus.Sold && (
            <DropdownMenuItem onClick={() => onStatusChange(vehicle, VehicleStatus.Published)}>
              Yeniden yayınla
            </DropdownMenuItem>
          )}
          {vehicle.status !== VehicleStatus.Archived && (
            <DropdownMenuItem onClick={() => onStatusChange(vehicle, VehicleStatus.Archived)}>
              Arşive kaldır
            </DropdownMenuItem>
          )}
          {vehicle.status === VehicleStatus.Archived && (
            <DropdownMenuItem onClick={() => onStatusChange(vehicle, VehicleStatus.Draft)}>
              Taslağa geri al
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
