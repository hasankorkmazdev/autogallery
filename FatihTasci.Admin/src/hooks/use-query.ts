import { useEffect, useState } from "react";
import { toast } from "sonner";

interface QueryState<T> {
  key: string;
  data: T | null;
}

/**
 * Parametreleri değiştikçe veriyi yeniden yükler. Yükleme durumu, son tamamlanan isteğin
 * anahtarı ile güncel anahtar karşılaştırılarak hesaplanır (effect içinde senkron setState yok).
 * Hata durumunda mesaj toast ile gösterilir.
 */
export function useQuery<T>(key: string, fetcher: () => Promise<T>, enabled = true) {
  const [state, setState] = useState<QueryState<T>>({ key: "", data: null });

  useEffect(() => {
    if (!enabled) return;
    let cancelled = false;
    fetcher()
      .then((data) => {
        if (!cancelled) setState({ key, data });
      })
      .catch((error: Error) => {
        if (cancelled) return;
        toast.error(error.message);
        setState((prev) => ({ key, data: prev.data }));
      });
    return () => {
      cancelled = true;
    };
    // fetcher her render'da yeniden oluşur; yeniden yükleme yalnızca anahtara bağlıdır.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key, enabled]);

  return {
    data: enabled ? state.data : null,
    loading: enabled && state.key !== key,
    isCurrent: state.key === key,
  };
}
