import type { AxiosProgressEvent } from "axios";
import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { SaveShowroomScene, ServiceResult, ShowroomScene } from "@/api/types";

// Her işlem güncel sahne listesini döner.
type ScenesResult = ServiceResult<ShowroomScene[]>;

export const showroomService = {
  getAll: () => unwrap(api.get<ScenesResult>("/showroom-scenes")),
  create: (request: SaveShowroomScene) => unwrapWithMessage(api.post<ScenesResult>("/showroom-scenes", request)),
  update: (id: number, request: SaveShowroomScene) =>
    unwrapWithMessage(api.put<ScenesResult>(`/showroom-scenes/${id}`, request)),
  reorder: (sceneIds: number[]) => unwrapWithMessage(api.put<ScenesResult>("/showroom-scenes/order", { sceneIds })),
  remove: (id: number) => unwrapWithMessage(api.delete<ScenesResult>(`/showroom-scenes/${id}`)),
  uploadImage: (id: number, file: File, onProgress: (percent: number) => void) => {
    const form = new FormData();
    form.append("file", file);
    return unwrapWithMessage(
      api.post<ScenesResult>(`/showroom-scenes/${id}/image`, form, {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress: (event: AxiosProgressEvent) => {
          if (event.total) onProgress(Math.round((event.loaded / event.total) * 100));
        },
      }),
    );
  },
  deleteImage: (id: number) => unwrapWithMessage(api.delete<ScenesResult>(`/showroom-scenes/${id}/image`)),
};
