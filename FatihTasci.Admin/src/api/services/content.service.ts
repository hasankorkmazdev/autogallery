import type { AxiosProgressEvent } from "axios";
import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { ServiceResult, SiteContent } from "@/api/types";

// Vitrin içeriği: ana sayfa videosu, Hizmetler, Kurumsal, Sayılarla Biz (yalnız Yönetici).
export const contentService = {
  get: () => unwrap(api.get<ServiceResult<SiteContent>>("/content")),
  update: (content: SiteContent) => unwrapWithMessage(api.put<ServiceResult<SiteContent>>("/content", content)),
  /** Video (MP4) ve tarayıcıda videodan alınan ilk kare (afiş) birlikte gönderilir */
  uploadHeroVideo: (video: File, poster: Blob, onProgress: (percent: number) => void) => {
    const form = new FormData();
    form.append("video", video);
    form.append("poster", poster, "poster.webp");
    return unwrapWithMessage(
      api.post<ServiceResult<SiteContent>>("/content/hero-video", form, {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress: (event: AxiosProgressEvent) => {
          if (event.total) onProgress(Math.round((event.loaded / event.total) * 100));
        },
      }),
    );
  },
  deleteHeroVideo: () => unwrapWithMessage(api.delete<ServiceResult<SiteContent>>("/content/hero-video")),
};
