import { useEffect, useState } from "react";
import { Settings } from "lucide-react";
import { toast } from "sonner";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { settingsService } from "@/api/services/settings.service";
import type { SiteSettings } from "@/api/types";
import { SettingsForm } from "./components/settings-form";

export default function SettingsPage() {
  const [settings, setSettings] = useState<SiteSettings | null>(null);

  useEffect(() => {
    settingsService
      .get()
      .then(setSettings)
      .catch((error: Error) => toast.error(error.message));
  }, []);

  return (
    <PageLayout className="overflow-y-auto">
      <PageHeader title="Ayarlar" description="Site genelindeki iletişim bilgileri ve bağlantılar." icon={<Settings className="w-8 h-8" />} />
      {settings ? <SettingsForm settings={settings} /> : <PageLoader />}
    </PageLayout>
  );
}
