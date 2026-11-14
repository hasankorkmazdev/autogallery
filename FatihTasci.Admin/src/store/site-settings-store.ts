import { create } from "zustand";
import type { SiteSettings } from "@/api/types";
import { settingsService } from "@/api/services/settings.service";

/**
 * Panel başlığındaki firma adı ve logosu için site ayarları. Ayarlar sayfası kaydettikçe günceller.
 */
interface SiteSettingsState {
  settings: SiteSettings | null;
  load: () => Promise<void>;
  set: (settings: SiteSettings) => void;
}

export const useSiteSettingsStore = create<SiteSettingsState>()((set, get) => ({
  settings: null,
  load: async () => {
    if (get().settings) return;
    set({ settings: await settingsService.get() });
  },
  set: (settings) => set({ settings }),
}));
