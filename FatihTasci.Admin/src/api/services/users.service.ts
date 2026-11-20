import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { Advisor, SaveUserRequest, ServiceResult, UserListItem } from "@/api/types";

// Yalnız Yönetici rolü kullanabilir (sunucu 403 döner); getAdvisors tüm rollere açıktır.
export const usersService = {
  getAdvisors: () => unwrap(api.get<ServiceResult<Advisor[]>>("/advisors")),
  getAll: () => unwrap(api.get<ServiceResult<UserListItem[]>>("/users")),
  create: (request: SaveUserRequest) => unwrapWithMessage(api.post<ServiceResult<UserListItem>>("/users", request)),
  update: (id: string, request: SaveUserRequest) =>
    unwrapWithMessage(api.put<ServiceResult<UserListItem>>(`/users/${id}`, request)),
  resetPassword: (id: string, password: string) =>
    unwrapWithMessage(api.put<ServiceResult<null>>(`/users/${id}/password`, { password })),
};
