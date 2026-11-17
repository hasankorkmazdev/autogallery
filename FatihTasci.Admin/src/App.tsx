import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { PrivateLayout } from "@/layouts/private/index";
import { PublicLayout } from "@/layouts/public/index";
import LoginPage from "@/pages/login/index";
import VehiclesPage from "@/pages/vehicles/index";
import VehicleWizardPage from "@/pages/vehicles/wizard/index";
import LeadsPage from "@/pages/leads/index";
import DeliveriesPage from "@/pages/deliveries/index";
import CatalogPage from "@/pages/catalog/index";
import FeaturesPage from "@/pages/features/index";
import SettingsPage from "@/pages/settings/index";
import ShowroomPage from "@/pages/showroom/index";
import ContentPage from "@/pages/content/index";
import UsersPage from "@/pages/users/index";
import { AdminRoute } from "@/layouts/private/components/admin-route";
import NotFoundPage from "@/pages/not-found";

export default function App() {
  return (
    <BrowserRouter basename={import.meta.env.BASE_URL}>
      <Routes>
        <Route element={<PublicLayout />}>
          <Route path="/login" element={<LoginPage />} />
        </Route>
        <Route element={<PrivateLayout />}>
          <Route index element={<Navigate to="/vehicles" replace />} />
          <Route path="/vehicles" element={<VehiclesPage />} />
          <Route path="/vehicles/new" element={<VehicleWizardPage />} />
          <Route path="/vehicles/:id" element={<VehicleWizardPage />} />
          <Route path="/leads" element={<LeadsPage />} />
          <Route path="/deliveries" element={<DeliveriesPage />} />
          <Route element={<AdminRoute />}>
            <Route path="/catalog" element={<CatalogPage />} />
            <Route path="/features" element={<FeaturesPage />} />
            <Route path="/showroom" element={<ShowroomPage />} />
            <Route path="/content" element={<ContentPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
}
