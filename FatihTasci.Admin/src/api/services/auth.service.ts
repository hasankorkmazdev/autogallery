import { api, unwrap } from "@/api/axios";
import type { LoginResponse, ServiceResult, User } from "@/api/types";

export const authService = {
  login: (eMail: string, password: string) =>
    unwrap(api.post<ServiceResult<LoginResponse>>("/auth/login", { eMail, password })),
  me: () => unwrap(api.get<ServiceResult<User>>("/auth/me")),
  /** HttpOnly çerezi yalnız sunucu silebilir. */
  logout: () => unwrap(api.post<ServiceResult<null>>("/auth/logout")),
};
