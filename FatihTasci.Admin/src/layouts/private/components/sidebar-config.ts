import { Car, Images, Inbox, Library, ListChecks, Settings, Users } from "lucide-react";

export interface NavItem {
  title: string;
  url: string;
  icon?: React.ElementType;
  /** Yalnız Yönetici görür; sayfa da AdminRoute ile korunur. */
  adminOnly?: boolean;
}

export const navigation: NavItem[] = [
  { title: "Araçlar", url: "/vehicles", icon: Car },
  { title: "Talepler", url: "/leads", icon: Inbox },
  { title: "Katalog", url: "/catalog", icon: Library, adminOnly: true },
  { title: "Donanımlar", url: "/features", icon: ListChecks, adminOnly: true },
  { title: "Showroom", url: "/showroom", icon: Images, adminOnly: true },
  { title: "Kullanıcılar", url: "/users", icon: Users, adminOnly: true },
  { title: "Ayarlar", url: "/settings", icon: Settings, adminOnly: true },
];

export function getPageTitle(pathname: string) {
  if (pathname === "/vehicles/new") return "Yeni Araç";
  if (pathname.startsWith("/vehicles/")) return "Araç Düzenle";
  return navigation.find((item) => pathname.startsWith(item.url))?.title ?? "";
}
