import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { ServiceResult, SiteSettings } from "@/api/types";

export const settingsService = {
  get: () => unwrap(api.get<ServiceResult<SiteSettings>>("/settings")),
  update: (settings: SiteSettings) =>
    unwrapWithMessage(api.put<ServiceResult<SiteSettings>>("/settings", settings)),
};
