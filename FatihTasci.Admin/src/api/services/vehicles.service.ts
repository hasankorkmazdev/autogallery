import type { AxiosProgressEvent } from "axios";
import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type {
  BodyPartItem,
  PagedList,
  PublishCheck,
  ServiceResult,
  VehicleDetail,
  VehicleListItem,
  VehicleMedia,
  VehicleStatusValue,
} from "@/api/types";

export interface VehicleListRequest {
  pageIndex: number;
  pageSize: number;
  status?: number;
  brandId?: number;
  search?: string;
}

export interface VehicleIdentityRequest {
  brandId: number;
  seriesId: number;
  carModelId: number;
  title: string;
}

export interface VehicleTechnicalRequest {
  year: number;
  kilometer: number;
  fuelType: number;
  transmission: number;
  bodyType: number;
  engineCc: number | null;
  powerHp: number;
  drivetrain: number;
  batteryKwh: number | null;
  rangeKm: number | null;
  exteriorColorId: number;
  interiorColor: string | null;
}

export interface VehicleConditionRequest {
  condition: number;
  hasWarranty: boolean;
  warrantyEndDate: string | null;
  heavyDamageRecord: boolean;
  plateOrigin: number;
  acceptsTrade: boolean;
  tramerAmount: number | null;
}

export interface VehiclePricingRequest {
  price: number | null;
  campaignPrice: number | null;
  showPrice: boolean;
  description: string | null;
  isFeatured: boolean;
  featuredOrder: number;
}

type DetailResult = ServiceResult<VehicleDetail>;
type MediaResult = ServiceResult<VehicleMedia[]>;

export const vehiclesService = {
  getList: (request: VehicleListRequest) =>
    unwrap(api.get<ServiceResult<PagedList<VehicleListItem>>>("/vehicles", { params: request })),
  get: (id: string) => unwrap(api.get<DetailResult>(`/vehicles/${id}`)),
  create: (request: VehicleIdentityRequest) =>
    unwrapWithMessage(api.post<DetailResult>("/vehicles", request)),
  updateIdentity: (id: string, request: VehicleIdentityRequest) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/identity`, request)),
  updateTechnical: (id: string, request: VehicleTechnicalRequest) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/technical`, request)),
  updateCondition: (id: string, request: VehicleConditionRequest) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/condition`, request)),
  updateBodyParts: (id: string, parts: BodyPartItem[]) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/body-parts`, { parts })),
  updateFeatures: (id: string, featureIds: number[]) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/features`, { featureIds })),
  getFeatureSuggestion: (id: string) =>
    unwrapWithMessage(api.get<ServiceResult<number[]>>(`/vehicles/${id}/features/suggestion`)),
  updateVideo: (id: string, videoUrl: string | null) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/video`, { videoUrl })),
  updatePricing: (id: string, request: VehiclePricingRequest) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/pricing`, request)),
  getPublishCheck: (id: string) => unwrap(api.get<ServiceResult<PublishCheck>>(`/vehicles/${id}/publish-check`)),
  updateStatus: (id: string, status: VehicleStatusValue) =>
    unwrapWithMessage(api.put<DetailResult>(`/vehicles/${id}/status`, { status })),
  copy: (id: string) => unwrapWithMessage(api.post<DetailResult>(`/vehicles/${id}/copy`)),

  uploadMedia: (id: string, files: File[], onProgress: (percent: number) => void) => {
    const form = new FormData();
    files.forEach((file) => form.append("files", file));
    return unwrapWithMessage(
      api.post<MediaResult>(`/vehicles/${id}/media`, form, {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress: (event: AxiosProgressEvent) => {
          if (event.total) onProgress(Math.round((event.loaded / event.total) * 100));
        },
      }),
    );
  },
  reorderMedia: (id: string, mediaIds: string[]) =>
    unwrapWithMessage(api.put<MediaResult>(`/vehicles/${id}/media/order`, { mediaIds })),
  setCover: (id: string, mediaId: string) =>
    unwrapWithMessage(api.put<MediaResult>(`/vehicles/${id}/media/${mediaId}/cover`)),
  deleteMedia: (id: string, mediaId: string) =>
    unwrapWithMessage(api.delete<MediaResult>(`/vehicles/${id}/media/${mediaId}`)),
  uploadExpertise: (id: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return unwrapWithMessage(
      api.post<ServiceResult<string>>(`/vehicles/${id}/expertise`, form, {
        headers: { "Content-Type": "multipart/form-data" },
      }),
    );
  },
  deleteExpertise: (id: string) =>
    unwrapWithMessage(api.delete<ServiceResult<null>>(`/vehicles/${id}/expertise`)),
};
