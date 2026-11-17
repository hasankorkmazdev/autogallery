import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { UserRole, type User } from "@/api/types";

/**
 * Yalnız ekranda gösterilecek oturum bilgisi. Token HttpOnly çerezdedir, burada tutulmaz.
 */
interface AuthState {
  user: User | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
  login: (user: User, expiresAt: string) => void;
  logout: () => void;
}

const loggedOut = { user: null, expiresAt: null, isAuthenticated: false };

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      ...loggedOut,
      login: (user, expiresAt) => set({ user, expiresAt, isAuthenticated: true }),
      logout: () => set(loggedOut),
    }),
    {
      name: "auth-storage",
      storage: createJSONStorage(() => localStorage),
      // Sürüm 0 token'ı localStorage'da tutuyordu; eski kayıt silinir, yeniden giriş istenir.
      version: 1,
      migrate: () => loggedOut,
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
