import { Navigate, Outlet } from "react-router-dom";
import { useIsAdmin } from "@/store/auth-store";

// Yalnız Yönetici'nin açabileceği sayfalar. Personel araç listesine yönlendirilir.
export function AdminRoute() {
  const isAdmin = useIsAdmin();
  return isAdmin ? <Outlet /> : <Navigate to="/vehicles" replace />;
}
