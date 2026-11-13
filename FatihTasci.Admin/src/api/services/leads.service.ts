import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { Lead, PagedList, ServiceResult } from "@/api/types";

export interface LeadListRequest {
  pageIndex: number;
  pageSize: number;
  status?: number;
  type?: number;
  search?: string;
}

export const leadsService = {
  getList: (request: LeadListRequest) =>
    unwrap(api.get<ServiceResult<PagedList<Lead>>>("/leads", { params: request })),
  getNewCount: () => unwrap(api.get<ServiceResult<number>>("/leads/new-count")),
  update: (id: string, status: number, internalNote: string | null) =>
    unwrapWithMessage(api.put<ServiceResult<Lead>>(`/leads/${id}`, { status, internalNote })),
};
