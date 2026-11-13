import { Badge, type BadgeProps } from "@/components/ui/badge";
import { VehicleStatus, type VehicleStatusValue } from "@/api/types";
import { labelOf, useLookupStore } from "@/store/lookup-store";

const VARIANTS: Record<VehicleStatusValue, BadgeProps["variant"]> = {
  [VehicleStatus.Draft]: "secondary",
  [VehicleStatus.Published]: "success",
  [VehicleStatus.Reserved]: "warning",
  [VehicleStatus.Sold]: "default",
  [VehicleStatus.Archived]: "outline",
};

export function VehicleStatusBadge({ status }: { status: VehicleStatusValue }) {
  const options = useLookupStore((state) => state.lookups?.vehicleStatus);
  return <Badge variant={VARIANTS[status]}>{labelOf(options, status)}</Badge>;
}
