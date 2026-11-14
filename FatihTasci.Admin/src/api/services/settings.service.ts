import type { AxiosProgressEvent } from "axios";
import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { ServiceResult, SiteSettings, WatermarkJobStatus } from "@/api/types";

export const settingsService = {
  get: () => unwrap(api.get<ServiceResult<SiteSettings>>("/settings")),
  update: (settings: SiteSettings) =>
    unwrapWithMessage(api.put<ServiceResult<SiteSettings>>("/settings", settings)),
  uploadLogo: (file: File, onProgress: (percent: number) => void) => {
    const form = new FormData();
    form.append("file", file);
    return unwrapWithMessage(
      api.post<ServiceResult<SiteSettings>>("/settings/logo", form, {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress: (event: AxiosProgressEvent) => {
          if (event.total) onProgress(Math.round((event.loaded / event.total) * 100));
        },
      }),
    );
  },
  deleteLogo: () => unwrapWithMessage(api.delete<ServiceResult<SiteSettings>>("/settings/logo")),
  getWatermarkStatus: () => unwrap(api.get<ServiceResult<WatermarkJobStatus>>("/settings/watermark/status")),
  applyWatermark: () => unwrapWithMessage(api.post<ServiceResult<WatermarkJobStatus>>("/settings/watermark/apply")),
};
