import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { UserRole, type User } from "@/api/types";

interface AuthState {
  user: User | null;
  token: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
  login: (user: User, token: string, expiresAt: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      expiresAt: null,
      isAuthenticated: false,
      login: (user, token, expiresAt) => set({ user, token, expiresAt, isAuthenticated: true }),
      logout: () => set({ user: null, token: null, expiresAt: null, isAuthenticated: false }),
    }),
    {
      name: "auth-storage",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

/**
 * Oturumdaki kullanıcı Yönetici mi? Menü ve butonları gizlemek içindir; asıl kontrol sunucudadır.
 */
export function useIsAdmin() {
  return useAuthStore((state) => state.user?.role === UserRole.Admin);
}

/**
 * Token süresi dolmuşsa oturum kapalı sayılır.
 */
export function isSessionValid(expiresAt: string | null) {
  return !!expiresAt && new Date(expiresAt).getTime() > Date.now();
}
