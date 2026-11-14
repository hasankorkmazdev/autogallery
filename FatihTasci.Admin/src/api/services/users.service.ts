import { api, unwrap, unwrapWithMessage } from "@/api/axios";
import type { SaveUserRequest, ServiceResult, UserListItem } from "@/api/types";

// Yalnız Yönetici rolü kullanabilir (sunucu 403 döner).
export const usersService = {
  getAll: () => unwrap(api.get<ServiceResult<UserListItem[]>>("/users")),
  create: (request: SaveUserRequest) => unwrapWithMessage(api.post<ServiceResult<UserListItem>>("/users", request)),
  update: (id: string, request: SaveUserRequest) =>
    unwrapWithMessage(api.put<ServiceResult<UserListItem>>(`/users/${id}`, request)),
  resetPassword: (id: string, password: string) =>
    unwrapWithMessage(api.put<ServiceResult<null>>(`/users/${id}/password`, { password })),
};
