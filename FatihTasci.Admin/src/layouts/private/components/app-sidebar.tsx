import * as React from "react";
import { CarFront, ExternalLink } from "lucide-react";
import { useAuthStore, useIsAdmin } from "@/store/auth-store";
import { useSiteSettingsStore } from "@/store/site-settings-store";
import { navigation } from "./sidebar-config";
import { NavMain } from "./nav-main";
import { NavUser } from "./nav-user";
import { publicSiteUrl } from "@/lib/format";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/components/ui/sidebar";

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const user = useAuthStore((state) => state.user);
  const isAdmin = useIsAdmin();
  // Firma adı ve logosu site ayarlarından gelir (kodda sabit yazılmaz).
  const settings = useSiteSettingsStore((state) => state.settings);
  const loadSettings = useSiteSettingsStore((state) => state.load);

  React.useEffect(() => {
    loadSettings().catch(() => undefined);
  }, [loadSettings]);

  const items = navigation.filter((item) => isAdmin || !item.adminOnly);

  const userData = {
    name: user ? `${user.name} ${user.surname}` : "",
    email: user?.eMail ?? "",
  };

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <a href={publicSiteUrl("/")} target="_blank" rel="noreferrer">
                {/* Logo geniş bir kelime logosu (~6:1), kare kutuya sığmaz; burada simge ve firma adı kullanılır. */}
                <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
                  <CarFront className="size-4" />
                </div>
                <div className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-semibold text-sidebar-foreground">{settings?.galleryName || "Oto Galeri"}</span>
                  <span className="truncate text-xs text-muted-foreground flex items-center gap-1">
                    Siteyi aç <ExternalLink className="size-3" />
                  </span>
                </div>
              </a>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <NavMain items={items} groupLabel="Yönetim" />
      </SidebarContent>

      <SidebarFooter>
        <NavUser user={userData} />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  );
}
