import { useEffect, useState } from "react";
import { Stamp } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FieldDescription } from "@/components/ui/field";
import { Progress } from "@/components/ui/progress";
import { settingsService } from "@/api/services/settings.service";
import type { WatermarkJobStatus } from "@/api/types";
import { formatDateTime, formatNumber } from "@/lib/format";

const POLL_INTERVAL_MS = 2000;

function StatusText({ status }: { status: WatermarkJobStatus | null }) {
  if (!status) return null;
  if (status.isRunning) {
    const percent = status.total > 0 ? Math.round((status.processed / status.total) * 100) : 0;
    return (
      <div className="flex flex-col gap-2">
        <Progress value={percent} />
        <FieldDescription>
          {formatNumber(status.processed)} / {formatNumber(status.total)} fotoğraf işlendi. Sayfadan ayrılabilirsiniz, iş arka planda sürer.
        </FieldDescription>
      </div>
    );
  }
  if (!status.finishedAt) return null;
  const failed = status.failed > 0 ? `, ${formatNumber(status.failed)} fotoğraf işlenemedi (orijinali eksik olabilir)` : "";
  return (
    <FieldDescription>
      Son çalışma {formatDateTime(status.finishedAt)}: {formatNumber(status.processed)} / {formatNumber(status.total)} fotoğraf{failed}.
    </FieldDescription>
  );
}

// Logo ya da filigran ayarı değişince mevcut fotoğrafların varyantlarını orijinallerden yeniden üretir.
export function WatermarkCard() {
  const [status, setStatus] = useState<WatermarkJobStatus | null>(null);
  const [starting, setStarting] = useState(false);
  const isRunning = status?.isRunning ?? false;

  useEffect(() => {
    let cancelled = false;
    const refresh = () =>
      settingsService
        .getWatermarkStatus()
        .then((next) => {
          if (!cancelled) setStatus(next);
        })
        .catch(() => undefined);

    refresh();
    if (!isRunning) {
      return () => {
        cancelled = true;
      };
    }
    const timer = window.setInterval(refresh, POLL_INTERVAL_MS);
    return () => {
      cancelled = true;
      window.clearInterval(timer);
    };
  }, [isRunning]);

  const start = async () => {
    setStarting(true);
    try {
      const result = await settingsService.applyWatermark();
      setStatus(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setStarting(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Filigranı yeniden uygula</CardTitle>
        <CardDescription>
          Yeni yüklenen fotoğraflara filigran otomatik basılır. Logoyu değiştirdikten ya da filigranı açıp kapattıktan sonra
          mevcut tüm fotoğrafları güncellemek için kullanın. Orijinaller filigransız saklandığı için kalite kaybı olmaz.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <StatusText status={status} />
        <div>
          <Button type="button" variant="outline" disabled={isRunning || starting} onClick={start}>
            <Stamp className="w-4 h-4 mr-2" />
            {isRunning ? "İşleniyor..." : "Tüm fotoğraflara uygula"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
