import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

export default function NotFoundPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-6 text-center">
      <h1 className="text-3xl font-bold tracking-tight">Sayfa bulunamadı</h1>
      <p className="text-muted-foreground text-sm">Aradığınız sayfa panelde yok.</p>
      <Button asChild>
        <Link to="/vehicles">Araçlara dön</Link>
      </Button>
    </div>
  );
}
