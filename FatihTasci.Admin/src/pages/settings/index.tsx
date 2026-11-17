import { useEffect, useState } from "react";
import { Settings } from "lucide-react";
import { toast } from "sonner";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { settingsService } from "@/api/services/settings.service";
import type { SiteSettings } from "@/api/types";
import { useSiteSettingsStore } from "@/store/site-settings-store";
import { SettingsTabs } from "./components/settings-tabs";

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
    return <SettingsTabs settings={settings} onChanged={handleChanged} />;
  };

  return (
    <PageLayout className="overflow-y-auto">
      <PageHeader title="Ayarlar" description="Firma ve iletişim bilgileri, konum, sosyal medya bağlantıları, logo ve filigran." icon={<Settings />} />
      {renderContent()}
    </PageLayout>
  );
}
