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
        <TableCell>
          <div className="flex flex-col">
            <span className="font-medium">
              {user.name} {user.surname}
              {user.id === currentUserId && <span className="ml-2 text-xs text-muted-foreground">(siz)</span>}
            </span>
            <span className="text-xs text-muted-foreground">{user.eMail}</span>
          </div>
        </TableCell>
        <TableCell>
          <Badge variant={user.role === UserRole.Admin ? "default" : "secondary"}>{labelOf(lookups?.userRole, user.role)}</Badge>
        </TableCell>
        <TableCell>
          {user.isActive ? <Badge variant="outline">Aktif</Badge> : <Badge variant="secondary">Pasif</Badge>}
        </TableCell>
        <TableCell>
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
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kullanıcı</TableHead>
            <TableHead>Rol</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Son giriş</TableHead>
            <TableHead className="text-right">İşlemler</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>{renderBody()}</TableBody>
      </Table>
    </div>
  );
}
