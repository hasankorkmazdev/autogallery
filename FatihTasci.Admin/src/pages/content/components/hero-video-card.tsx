import { useRef, useState } from "react";
import { RotateCcw, Upload } from "lucide-react";
import { toast } from "sonner";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FieldDescription } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import { contentService } from "@/api/services/content.service";
import type { HeroVideo } from "@/api/types";
import { capturePoster, checkVideoFile, MAX_VIDEO_MB, VIDEO_ACCEPT } from "@/lib/video";

interface HeroVideoCardProps {
  video: HeroVideo;
  onChanged: (video: HeroVideo) => void;
}

// Ana sayfa girişinin videosu: sessiz döngüde oynar, üstünde firma adı ve slogan durur.
export function HeroVideoCard({ video, onChanged }: HeroVideoCardProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [confirmReset, setConfirmReset] = useState(false);
  const busy = status !== null;

  const upload = async (fileList: FileList | null) => {
    const file = fileList?.[0];
    if (inputRef.current) inputRef.current.value = "";
    if (!file) return;
    const invalid = checkVideoFile(file);
    if (invalid) {
      toast.error(invalid);
      return;
    }

    setStatus("Video inceleniyor...");
    setProgress(0);
    try {
      const captured = await capturePoster(file);
      setStatus("Yükleniyor...");
      const result = await contentService.uploadHeroVideo(file, captured.poster, setProgress);
      if (result.data.heroVideo) onChanged(result.data.heroVideo);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setStatus(null);
    }
  };

  const reset = async () => {
    setConfirmReset(false);
    setStatus("Varsayılan videoya dönülüyor...");
    try {
      const result = await contentService.deleteHeroVideo();
      if (result.data.heroVideo) onChanged(result.data.heroVideo);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setStatus(null);
    }
  };

  const renderPreview = () => {
    if (status !== null) {
      return (
        <div className="flex w-full flex-col gap-2">
          <Progress value={progress} />
          <FieldDescription>{progress > 0 && progress < 100 ? `${status} %${progress}` : status}</FieldDescription>
        </div>
      );
    }
    return (
      <video
        key={video.videoHdUrl}
        className="aspect-video w-full rounded-md bg-black object-cover"
        src={video.videoHdUrl}
        poster={video.posterSmallUrl}
        controls
        muted
        loop
        playsInline
        preload="none"
        aria-label="Ana sayfa videosu"
      />
    );
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Ana sayfa videosu</CardTitle>
        <CardDescription>
          {video.isCustom ? "Yüklediğiniz video kullanılıyor." : "Galerinin drone videosu kullanılıyor (varsayılan)."} Video sessiz
          ve döngüde oynar; ilk karesi video yüklenene kadar görünür.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div
          className="flex min-h-36 items-center justify-center rounded-lg border border-dashed bg-muted p-2"
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => {
            e.preventDefault();
            if (!busy) upload(e.dataTransfer.files);
          }}>
          {renderPreview()}
        </div>
        <FieldDescription>
          MP4 (H.264), yatay 1920×1080, sessiz, 10–30 saniye ve en fazla {MAX_VIDEO_MB} MB. Sunucu videoyu sıkıştırmaz; telefondan
          izleyenler için 10–15 MB altı önerilir. Başı ve sonu birbirine bağlanan bir çekim döngüde kesintisiz görünür.
        </FieldDescription>
        <div className="flex flex-wrap gap-2">
          <Button type="button" variant="outline" disabled={busy} onClick={() => inputRef.current?.click()}>
            <Upload className="w-4 h-4 mr-2" />
            Video yükle
          </Button>
          {video.isCustom && (
            <Button type="button" variant="ghost-destructive" disabled={busy} onClick={() => setConfirmReset(true)}>
              <RotateCcw className="w-4 h-4 mr-2" />
              Varsayılan videoya dön
            </Button>
          )}
        </div>
        <Input
          ref={inputRef}
          type="file"
          accept={VIDEO_ACCEPT}
          className="hidden"
          aria-label="Ana sayfa videosu"
          onChange={(e) => upload(e.target.files)}
        />
      </CardContent>

      <AlertDialog open={confirmReset} onOpenChange={setConfirmReset}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Varsayılan videoya dönülsün mü?</AlertDialogTitle>
            <AlertDialogDescription>
              Yüklediğiniz video silinir, ana sayfada galerinin drone videosu oynar. Aynı videoyu kullanmak için yeniden
              yüklemeniz gerekir.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Vazgeç</AlertDialogCancel>
            <AlertDialogAction onClick={reset}>Varsayılana dön</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </Card>
  );
}
