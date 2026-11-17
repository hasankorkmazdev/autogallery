import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type {
  Brand,
  CarModel,
  Color,
  Feature,
  FeatureGroup,
  Lookups,
  Series,
  ServiceResult,
} from "@/api/types";

export interface SaveBrandRequest {
  name: string;
  sortOrder: number;
  isActive: boolean;
}

export interface SaveSeriesRequest extends SaveBrandRequest {
  brandId: number;
}

export interface SaveModelRequest {
  seriesId: number;
  name: string;
  isActive: boolean;
}

export interface SaveFeatureRequest {
  category: number;
  name: string;
  isActive: boolean;
}

export const catalogService = {
  getBrands: (includeInactive = false) =>
    unwrap(api.get<ServiceResult<Brand[]>>("/brands", { params: { includeInactive } })),
  createBrand: (request: SaveBrandRequest) =>
    unwrapWithMessage(api.post<ServiceResult<Brand>>("/brands", request)),
  updateBrand: (id: number, request: SaveBrandRequest) =>
    unwrapWithMessage(api.put<ServiceResult<Brand>>(`/brands/${id}`, request)),

  getSeries: (brandId: number, includeInactive = false) =>
    unwrap(api.get<ServiceResult<Series[]>>(`/brands/${brandId}/series`, { params: { includeInactive } })),
  createSeries: (request: SaveSeriesRequest) =>
    unwrapWithMessage(api.post<ServiceResult<Series>>("/series", request)),
  updateSeries: (id: number, request: SaveSeriesRequest) =>
    unwrapWithMessage(api.put<ServiceResult<Series>>(`/series/${id}`, request)),

  getModels: (seriesId: number, includeInactive = false) =>
    unwrap(api.get<ServiceResult<CarModel[]>>(`/series/${seriesId}/models`, { params: { includeInactive } })),
  createModel: (request: SaveModelRequest) =>
    unwrapWithMessage(api.post<ServiceResult<CarModel>>("/models", request)),
  updateModel: (id: number, request: SaveModelRequest) =>
    unwrapWithMessage(api.put<ServiceResult<CarModel>>(`/models/${id}`, request)),

  getColors: () => unwrap(api.get<ServiceResult<Color[]>>("/colors")),
  getLookups: () => unwrap(api.get<ServiceResult<Lookups>>("/lookups")),

  getFeatures: (includeInactive = false) =>
    unwrap(api.get<ServiceResult<FeatureGroup[]>>("/features", { params: { includeInactive } })),
  createFeature: (request: SaveFeatureRequest) =>
    unwrapWithMessage(api.post<ServiceResult<Feature>>("/features", request)),
  updateFeature: (id: number, request: SaveFeatureRequest) =>
    unwrapWithMessage(api.put<ServiceResult<Feature>>(`/features/${id}`, request)),
  /** Kategorideki tüm donanımların (pasifler dahil) yeni sırası; güncel listeyi döner. */
  reorderFeatures: (category: number, featureIds: number[]) =>
    unwrapWithMessage(api.put<ServiceResult<FeatureGroup[]>>("/features/order", { category, featureIds })),
};
