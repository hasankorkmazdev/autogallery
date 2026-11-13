import { useCallback, useEffect, useState } from "react";
import { AlertTriangle, CheckCircle2, CircleX } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { vehiclesService } from "@/api/services/vehicles.service";
import { VehicleStatus, type PublishCheck, type PublishCheckItem, type VehicleDetail, type VehicleStatusValue } from "@/api/types";

interface PublishPanelProps {
  vehicle: VehicleDetail;
  onSaved: (vehicle: VehicleDetail) => void;
  /** Durum değişmeden önce açık formu (fiyat) kaydeder */
  beforeStatusChange: () => Promise<boolean>;
  onGoToStep: (step: number) => void;
}

function CheckIcon({ item }: { item: PublishCheckItem }) {
  if (item.passed) return <CheckCircle2 className="size-4 shrink-0 text-success" />;
  if (item.blocking) return <CircleX className="size-4 shrink-0 text-destructive" />;
  return <AlertTriangle className="size-4 shrink-0 text-warning" />;
}

// Yayın kontrol listesi ve Yayınla / Taslak / Rezerve butonları
export function PublishPanel({ vehicle, onSaved, beforeStatusChange, onGoToStep }: PublishPanelProps) {
  const [check, setCheck] = useState<PublishCheck | null>(null);
  const [busy, setBusy] = useState(false);

  const loadCheck = useCallback(async () => {
    try {
      setCheck(await vehiclesService.getPublishCheck(vehicle.id));
    } catch (error) {
      toast.error((error as Error).message);
    }
  }, [vehicle.id]);

  useEffect(() => {
    loadCheck();
  }, [loadCheck, vehicle]);

  const changeStatus = async (status: VehicleStatusValue) => {
    setBusy(true);
    try {
      if (!(await beforeStatusChange())) return;
      const result = await vehiclesService.updateStatus(vehicle.id, status);
      onSaved(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setBusy(false);
    }
  };

  const isPublished = vehicle.status === VehicleStatus.Published;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Yayın kontrol listesi</CardTitle>
        <CardDescription>Kırmızı maddeler tamamlanmadan araç yayınlanamaz; sarılar öneridir.</CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <ul className="flex flex-col gap-2 text-sm">
          {check?.items.map((item) => (
            <li key={item.key} className="flex items-center gap-2">
              <CheckIcon item={item} />
              <span className="flex-1">{item.label}</span>
              {!item.passed && (
                <Button type="button" variant="link" size="link" onClick={() => onGoToStep(item.step)}>
                  Adım {item.step}
                </Button>
              )}
            </li>
          ))}
        </ul>
        <div className="flex flex-col gap-2">
          <Button type="button" disabled={busy} onClick={() => changeStatus(VehicleStatus.Published)}>
            {isPublished ? "Kaydet (yayında)" : "Yayınla"}
          </Button>
          <Button type="button" variant="outline" disabled={busy} onClick={() => changeStatus(VehicleStatus.Reserved)}>
            Rezerve et
          </Button>
          {vehicle.status !== VehicleStatus.Draft && (
            <Button type="button" variant="outline" disabled={busy} onClick={() => changeStatus(VehicleStatus.Draft)}>
              Yayından kaldır (taslağa al)
            </Button>
          )}
          {vehicle.status === VehicleStatus.Draft && (
            <Button type="button" variant="outline" disabled={busy} onClick={() => changeStatus(VehicleStatus.Draft)}>
              Taslak olarak bırak
            </Button>
          )}
          {(isPublished || vehicle.status === VehicleStatus.Reserved) && (
            <Button type="button" variant="outline" disabled={busy} onClick={() => changeStatus(VehicleStatus.Sold)}>
              Satıldı olarak işaretle
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
