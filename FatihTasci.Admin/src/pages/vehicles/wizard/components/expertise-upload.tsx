import { useRef, useState } from "react";
import { FileText, Trash2, Upload } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Field, FieldDescription, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { vehiclesService } from "@/api/services/vehicles.service";

const MAX_BYTES = 20 * 1024 * 1024;

interface ExpertiseUploadProps {
  vehicleId: string;
  url: string | null;
  onChange: (url: string | null) => void;
}

export function ExpertiseUpload({ vehicleId, url, onChange }: ExpertiseUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);

  const upload = async (file: File | undefined) => {
    if (!file) return;
    if (file.type !== "application/pdf" || file.size > MAX_BYTES) {
      toast.error("Ekspertiz raporu en fazla 20 MB'lık bir PDF olmalıdır.");
      return;
    }
    setBusy(true);
    try {
      const result = await vehiclesService.uploadExpertise(vehicleId, file);
      onChange(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setBusy(false);
      if (inputRef.current) inputRef.current.value = "";
    }
  };

  const remove = async () => {
    setBusy(true);
    try {
      const result = await vehiclesService.deleteExpertise(vehicleId);
      onChange(null);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Field>
      <FieldLabel htmlFor="expertise-file">Ekspertiz raporu</FieldLabel>
      <div className="flex flex-wrap items-center gap-2">
        {url && (
          <Button type="button" variant="link" asChild>
            <a href={url} target="_blank" rel="noreferrer">
              <FileText className="w-4 h-4 mr-1" /> Raporu aç
            </a>
          </Button>
        )}
        <Input
          ref={inputRef}
          id="expertise-file"
          type="file"
          accept="application/pdf"
          className="hidden"
          onChange={(e) => upload(e.target.files?.[0])}
        />
        <Button type="button" variant="outline" disabled={busy} onClick={() => inputRef.current?.click()}>
          <Upload className="w-4 h-4 mr-2" />
          {url ? "Raporu değiştir" : "PDF yükle"}
        </Button>
        {url && (
          <Button type="button" variant="ghost-destructive" size="icon" disabled={busy} title="Raporu sil" onClick={remove}>
            <Trash2 className="w-4 h-4" />
          </Button>
        )}
      </div>
      <FieldDescription>İsteğe bağlı. Sitede araç sayfasından indirilebilir. PDF, en fazla 20 MB.</FieldDescription>
    </Field>
  );
}
