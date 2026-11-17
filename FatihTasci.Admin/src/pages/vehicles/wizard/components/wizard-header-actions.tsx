import { Copy, ExternalLink } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { VehicleStatus, type VehicleDetail } from "@/api/types";
import { vehiclesService } from "@/api/services/vehicles.service";
import { publicSiteUrl } from "@/lib/format";
import { VehicleStatusBadge } from "../../components/vehicle-status-badge";

interface WizardHeaderActionsProps {
  vehicle: VehicleDetail;
  onCopied: (copy: VehicleDetail) => void;
}

export function WizardHeaderActions({ vehicle, onCopied }: WizardHeaderActionsProps) {
  const visibleOnSite =
    vehicle.status === VehicleStatus.Published ||
    vehicle.status === VehicleStatus.Reserved ||
    vehicle.status === VehicleStatus.Sold;

  const handleCopy = async () => {
    try {
      const result = await vehiclesService.copy(vehicle.id);
      toast.success(result.message);
      onCopied(result.data);
    } catch (error) {
      toast.error((error as Error).message);
    }
  };

  return (
    <div className="flex flex-wrap items-center gap-2">
      <VehicleStatusBadge status={vehicle.status} />
      {visibleOnSite && (
        <Button variant="outline" asChild>
          <a href={publicSiteUrl(vehicle.publicUrl)} target="_blank" rel="noreferrer">
            <ExternalLink className="w-4 h-4 mr-2" />
            Sitede gör
          </a>
        </Button>
      )}
      <Button variant="outline" onClick={handleCopy} title="Aynı paketten yeni bir taslak oluşturur">
        <Copy className="w-4 h-4 mr-2" />
        <span className="sm:hidden">Kopyala</span>
        <span className="max-sm:hidden">Benzer ilan olarak kopyala</span>
      </Button>
    </div>
  );
}
