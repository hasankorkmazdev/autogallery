import { create } from "zustand";
import type { Color, Lookups } from "@/api/types";
import { catalogService } from "@/api/services/catalog.service";

/**
 * Enum etiketleri ve renkler oturum boyunca bir kez yüklenir (persist yok).
 */
interface LookupState {
  lookups: Lookups | null;
  colors: Color[];
  loading: boolean;
  load: () => Promise<void>;
}

export const useLookupStore = create<LookupState>()((set, get) => ({
  lookups: null,
  colors: [],
  loading: false,
  load: async () => {
    if (get().lookups || get().loading) return;
    set({ loading: true });
    try {
      const [lookups, colors] = await Promise.all([catalogService.getLookups(), catalogService.getColors()]);
      set({ lookups, colors });
    } finally {
      set({ loading: false });
    }
  },
}));

export function labelOf(options: { value: number; label: string }[] | undefined, value: number | null | undefined) {
  if (value === null || value === undefined) return "-";
  return options?.find((x) => x.value === value)?.label ?? String(value);
}
