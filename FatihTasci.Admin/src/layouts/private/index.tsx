import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useEffect } from "react";
import { isSessionValid, useAuthStore } from "@/store/auth-store";
import { useLookupStore } from "@/store/lookup-store";
import { AppSidebar } from "./components/app-sidebar";
import { getPageTitle } from "./components/sidebar-config";
import { Separator } from "@/components/ui/separator";
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { ModeToggle } from "@/components/mode-toggle";
import { APP_NAME } from "@/lib/app";

export function PrivateLayout() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const expiresAt = useAuthStore((state) => state.expiresAt);
  const logout = useAuthStore((state) => state.logout);
  const loadLookups = useLookupStore((state) => state.load);
  const { pathname } = useLocation();
  const pageTitle = getPageTitle(pathname);
  const sessionValid = isAuthenticated && isSessionValid(expiresAt);

  useEffect(() => {
    document.title = pageTitle ? `${pageTitle} | ${APP_NAME}` : APP_NAME;
  }, [pageTitle]);

  useEffect(() => {
    if (isAuthenticated && !sessionValid) logout();
  }, [isAuthenticated, sessionValid, logout]);

  useEffect(() => {
    if (sessionValid) loadLookups();
  }, [sessionValid, loadLookups]);

  if (!sessionValid) {
    return <Navigate to="/login" replace />;
  }

  return (
    <SidebarProvider>
      <AppSidebar />
      {/* min-w-0: geniş içerik (tablo, adım şeridi) ana alanı kenar çubuğunun yanında ekrandan taşırmasın */}
      <SidebarInset className="min-w-0">
        <header className="flex h-16 shrink-0 items-center justify-between gap-2 px-4 border-b">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator orientation="vertical" className="mr-2" />
            <span className="text-sm font-medium">{pageTitle}</span>
          </div>
          <ModeToggle />
        </header>
        <div className="flex flex-1 flex-col min-h-0">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
