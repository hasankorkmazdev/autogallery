import axios, { isAxiosError } from "axios";
import type { ServiceResult } from "@/api/types";
import { useAuthStore } from "@/store/auth-store";

// Panel ve API aynı origin'den sunulur (geliştirmede Vite proxy'si ile).
// Oturum token'ı HttpOnly çerezdedir; tarayıcı her istekte kendisi gönderir, JS okuyamaz.
// X-Requested-With: sunucu durum değiştiren isteklerde CSRF kontrolü için bu başlığı ister.
export const api = axios.create({
  baseURL: "/api",
  headers: {
    "Content-Type": "application/json",
    "X-Requested-With": "XMLHttpRequest",
  },
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = `${import.meta.env.BASE_URL}login`;
    }
    return Promise.reject(error);
  },
);

/**
 * Sunucu yanıtındaki ServiceResultModel'i döner; hata kodunda (400, 404...) mesajla Error fırlatır.
 */
export async function unwrap<T>(request: Promise<{ data: ServiceResult<T> }>): Promise<T> {
  try {
    const response = await request;
    return response.data.data;
  } catch (error) {
    throw new Error(getErrorMessage(error));
  }
}

/**
 * Başarılı yanıtın mesajını da döner (toast'larda kullanılır).
 */
export async function unwrapWithMessage<T>(
  request: Promise<{ data: ServiceResult<T> }>,
): Promise<{ data: T; message: string }> {
  try {
    const response = await request;
    return { data: response.data.data, message: response.data.message };
  } catch (error) {
    throw new Error(getErrorMessage(error));
  }
}

export function getErrorMessage(error: unknown): string {
  if (isAxiosError(error)) {
    if (!error.response) {
      return "Sunucuya ulaşılamadı. Bağlantınızı kontrol edin.";
    }
    const message = error.response.data?.message;
    if (typeof message === "string" && message.length > 0) {
      return message;
    }
    if (error.response.status === 429) {
      return "Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin.";
    }
    return "İşlem sırasında bir hata oluştu.";
  }
  if (error instanceof Error) {
    return error.message;
  }
  return "İşlem sırasında bir hata oluştu.";
}
