import { CarFront } from "lucide-react";

// Kart başlıklarında (giriş) kullanılan logo. Galeri logosu netleşince görselle değiştirilecek.
export function AppLogo() {
  return (
    <div className="flex items-center gap-2 justify-center mb-2">
      <div className="flex size-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
        <CarFront className="size-5" />
      </div>
    </div>
  );
}
