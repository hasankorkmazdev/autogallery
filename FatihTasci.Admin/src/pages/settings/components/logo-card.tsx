import { useRef, useState } from "react";
import { ImageOff, ImagePlus, ImageUp } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FieldDescription } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import { settingsService } from "@/api/services/settings.service";
import type { SiteSettings } from "@/api/types";

const ACCEPTED_IMAGES = ["image/png", "image/webp", "image/jpeg"];
const MAX_LOGO_BYTES = 5 * 1024 * 1024;

interface LogoCardProps {
  settings: SiteSettings;
  onChanged: (settings: SiteSettings) => void;
}

// Firma logosu: sitenin başlığında, panelde ve fotoğraf filigranında kullanılır.
export function LogoCard({ settings, onChanged }: LogoCardProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [progress, setProgress] = useState<number | null>(null);
  const [removing, setRemoving] = useState(false);
  const busy = progress !== null || removing;

  const upload = async (fileList: FileList | null) => {
    const file = fileList?.[0];
    if (inputRef.current) inputRef.current.value = "";
    if (!file) return;
    if (!ACCEPTED_IMAGES.includes(file.type) || file.size > MAX_LOGO_BYTES) {
      toast.error(`${file.name}: yalnızca 5 MB'a kadar PNG, WebP ve JPG yüklenebilir.`);
      return;
    }
    setProgress(0);
    try {
      const result = await settingsService.uploadLogo(file, setProgress);
      onChanged(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setProgress(null);
    }
  };

  const remove = async () => {
    setRemoving(true);
    try {
      const result = await settingsService.deleteLogo();
      onChanged(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setRemoving(false);
    }
  };

  const renderPreview = () => {
    if (progress !== null) {
      return (
        <div className="flex w-full flex-col gap-2">
          <Progress value={progress} />
          <FieldDescription>{progress < 100 ? `Yükleniyor... %${progress}` : "Logo işleniyor..."}</FieldDescription>
        </div>
      );
    }
    if (settings.logoUrl) {
      return <img src={settings.logoUrl} alt="Firma logosu" className="max-h-28 max-w-full object-contain" />;
    }
    return (
      <div className="flex flex-col items-center gap-2 text-center">
        <ImagePlus className="size-8 text-muted-foreground" />
        <FieldDescription>Logo yüklenmedi. Filigran basılmaz, sitede yalnız firma adı görünür.</FieldDescription>
      </div>
    );
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Firma logosu</CardTitle>
        <CardDescription>
          Sitenin başlığında ve fotoğraf filigranında kullanılır. Saydam zeminli PNG önerilir; en fazla 5 MB.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div
          className="flex min-h-36 items-center justify-center rounded-lg border border-dashed bg-muted p-4"
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => {
            e.preventDefault();
            upload(e.dataTransfer.files);
          }}>
          {renderPreview()}
        </div>
        <div className="flex flex-wrap gap-2">
          <Button type="button" variant="outline" disabled={busy} onClick={() => inputRef.current?.click()}>
            <ImageUp className="w-4 h-4 mr-2" />
            {settings.logoUrl ? "Logoyu değiştir" : "Logo yükle"}
          </Button>
          {settings.logoUrl && (
            <Button type="button" variant="ghost-destructive" disabled={busy} onClick={remove}>
              <ImageOff className="w-4 h-4 mr-2" />
              Logoyu kaldır
            </Button>
          )}
        </div>
        <Input
          ref={inputRef}
          type="file"
          accept={ACCEPTED_IMAGES.join(",")}
          className="hidden"
          aria-label="Firma logosu"
          onChange={(e) => upload(e.target.files)}
        />
      </CardContent>
    </Card>
  );
}
