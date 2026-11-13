import { Loader2 } from "lucide-react";

export function PageLoader({ text = "Yükleniyor..." }: { text?: string }) {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-2 p-10 text-muted-foreground">
      <Loader2 className="size-6 animate-spin" />
      <span className="text-sm">{text}</span>
    </div>
  );
}
