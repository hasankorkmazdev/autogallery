import type { AxiosProgressEvent } from "axios";
import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { Delivery, SaveDelivery, ServiceResult } from "@/api/types";

// Her işlem güncel teslimat listesini döner (teslim tarihine göre en yeni önce).
type DeliveriesResult = ServiceResult<Delivery[]>;

// Ekleme ve düzenleme multipart'tır: alanlar + "photo" (eklemede zorunlu, düzenlemede yalnız değişecekse).
const toFormData = (request: SaveDelivery, photo: File | null) => {
  const form = new FormData();
  form.append("vehicleTitle", request.vehicleTitle);
  form.append("customerName", request.customerName);
  form.append("city", request.city);
  form.append("deliveredAt", request.deliveredAt);
  form.append("isActive", String(request.isActive));
  if (photo) form.append("photo", photo);
  return form;
};

const uploadConfig = (onProgress: (percent: number) => void) => ({
  headers: { "Content-Type": "multipart/form-data" },
  onUploadProgress: (event: AxiosProgressEvent) => {
    if (event.total) onProgress(Math.round((event.loaded / event.total) * 100));
  },
});

export const deliveriesService = {
  getAll: () => unwrap(api.get<DeliveriesResult>("/deliveries")),
  create: (request: SaveDelivery, photo: File | null, onProgress: (percent: number) => void) =>
    unwrapWithMessage(api.post<DeliveriesResult>("/deliveries", toFormData(request, photo), uploadConfig(onProgress))),
  update: (id: number, request: SaveDelivery, photo: File | null, onProgress: (percent: number) => void) =>
    unwrapWithMessage(api.put<DeliveriesResult>(`/deliveries/${id}`, toFormData(request, photo), uploadConfig(onProgress))),
  remove: (id: number) => unwrapWithMessage(api.delete<DeliveriesResult>(`/deliveries/${id}`)),
};
