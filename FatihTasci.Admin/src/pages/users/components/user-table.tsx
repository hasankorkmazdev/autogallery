import { KeyRound, Pencil } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { UserRole, type UserListItem } from "@/api/types";
import { formatDateTime } from "@/lib/format";
import { labelOf, useLookupStore } from "@/store/lookup-store";

interface UserTableProps {
  items: UserListItem[] | null;
  currentUserId: string | undefined;
  onEdit: (user: UserListItem) => void;
  onResetPassword: (user: UserListItem) => void;
}

export function UserTable({ items, currentUserId, onEdit, onResetPassword }: UserTableProps) {
  const lookups = useLookupStore((state) => state.lookups);
  const roleBadge = (user: UserListItem) => (
    <Badge variant={user.role === UserRole.Admin ? "default" : "secondary"}>{labelOf(lookups?.userRole, user.role)}</Badge>
  );
  const activeBadge = (user: UserListItem) =>
    user.isActive ? <Badge variant="outline">Aktif</Badge> : <Badge variant="secondary">Pasif</Badge>;

  const renderBody = () => {
    if (!items || items.length === 0) {
      return (
        <TableRow>
          <TableCell colSpan={5} className="h-24 text-center">
            <span className="text-muted-foreground">{items ? "Kullanıcı bulunamadı." : "Yükleniyor..."}</span>
          </TableCell>
        </TableRow>
      );
    }
    return items.map((user) => (
      <TableRow key={user.id}>
        <TableCell className="text-left">
          <div className="flex flex-col gap-0.5">
            <span className="font-medium">
              {user.name} {user.surname}
              {user.id === currentUserId && <span className="ml-2 text-xs text-muted-foreground">(siz)</span>}
            </span>
            <span className="text-xs text-muted-foreground break-all">{user.eMail}</span>
            {user.phone && <span className="text-xs text-muted-foreground">{user.phone}</span>}
            {/* Dar ekranda rol ve durum sütunları gizlenir, burada görünür. */}
            <span className="flex flex-wrap gap-1 pt-1 sm:hidden">
              {roleBadge(user)}
              {activeBadge(user)}
            </span>
          </div>
        </TableCell>
        <TableCell className="max-sm:hidden">{roleBadge(user)}</TableCell>
        <TableCell className="max-sm:hidden">{activeBadge(user)}</TableCell>
        <TableCell className="max-lg:hidden">
          <span className="text-xs text-muted-foreground">{formatDateTime(user.lastLoginDate)}</span>
        </TableCell>
        <TableCell className="text-right">
          <div className="flex justify-end gap-1">
            <Button variant="ghost" size="icon-sm" title="Şifre değiştir" onClick={() => onResetPassword(user)}>
              <KeyRound className="w-4 h-4" />
            </Button>
            <Button variant="ghost" size="icon-sm" title="Düzenle" onClick={() => onEdit(user)}>
              <Pencil className="w-4 h-4" />
            </Button>
          </div>
        </TableCell>
      </TableRow>
    ));
  };

  return (
    <div className="overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="text-left">Kullanıcı</TableHead>
            <TableHead className="max-sm:hidden">Rol</TableHead>
            <TableHead className="max-sm:hidden">Durum</TableHead>
            <TableHead className="max-lg:hidden">Son giriş</TableHead>
            <TableHead className="text-right">İşlemler</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>{renderBody()}</TableBody>
      </Table>
    </div>
  );
}
