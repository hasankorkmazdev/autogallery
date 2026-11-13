import { Car, Inbox, Library, ListChecks, Settings } from "lucide-react";

export interface NavItem {
  title: string;
  url: string;
  icon?: React.ElementType;
}

export const navigation: NavItem[] = [
  { title: "Araçlar", url: "/vehicles", icon: Car },
  { title: "Talepler", url: "/leads", icon: Inbox },
  { title: "Katalog", url: "/catalog", icon: Library },
  { title: "Donanımlar", url: "/features", icon: ListChecks },
  { title: "Ayarlar", url: "/settings", icon: Settings },
];

export function getPageTitle(pathname: string) {
  if (pathname === "/vehicles/new") return "Yeni Araç";
  if (pathname.startsWith("/vehicles/")) return "Araç Düzenle";
  return navigation.find((item) => pathname.startsWith(item.url))?.title ?? "";
}
