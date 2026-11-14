import { useEffect, useState } from "react";
import { Settings } from "lucide-react";
import { toast } from "sonner";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { settingsService } from "@/api/services/settings.service";
import type { SiteSettings } from "@/api/types";
import { useSiteSettingsStore } from "@/store/site-settings-store";
import { SettingsForm } from "./components/settings-form";
import { LogoCard } from "./components/logo-card";
import { WatermarkCard } from "./components/watermark-card";

export default function SettingsPage() {
  const [settings, setSettings] = useState<SiteSettings | null>(null);
  const setStoreSettings = useSiteSettingsStore((state) => state.set);

  useEffect(() => {
    settingsService
      .get()
      .then(setSettings)
      .catch((error: Error) => toast.error(error.message));
  }, []);

  // Panel başlığındaki firma adı ve logo da güncellensin.
  const handleChanged = (next: SiteSettings) => {
    setSettings(next);
    setStoreSettings(next);
  };

  const renderContent = () => {
    if (!settings) return <PageLoader />;
    return (
      <div className="flex flex-col gap-4">
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <LogoCard settings={settings} onChanged={handleChanged} />
          <WatermarkCard />
        </div>
        <SettingsForm settings={settings} onSaved={handleChanged} />
      </div>
    );
  };

  return (
    <PageLayout className="overflow-y-auto">
      <PageHeader title="Ayarlar" description="Firma adı ve logosu, filigran, iletişim bilgileri ve bağlantılar." icon={<Settings className="w-8 h-8" />} />
      {renderContent()}
    </PageLayout>
  );
}
