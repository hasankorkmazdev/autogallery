import { useState } from "react";
import { Plus, Users } from "lucide-react";
import { Button } from "@/components/ui/button";
import { PageContent, PageHeader, PageLayout } from "@/components/layout/page-layout";
import { useQuery } from "@/hooks/use-query";
import { usersService } from "@/api/services/users.service";
import type { UserListItem } from "@/api/types";
import { useAuthStore } from "@/store/auth-store";
import { UserTable } from "./components/user-table";
import { UserDialog, type UserDialogState } from "./components/user-dialog";
import { PasswordDialog } from "./components/password-dialog";

export default function UsersPage() {
  const currentUserId = useAuthStore((state) => state.user?.id);
  const [version, setVersion] = useState(0);
  const [dialog, setDialog] = useState<UserDialogState | null>(null);
  const [passwordUser, setPasswordUser] = useState<UserListItem | null>(null);
  const { data: users } = useQuery(`users-${version}`, usersService.getAll);

  return (
    <PageLayout>
      <PageHeader
        title="Kullanıcılar"
        description="Panele giriş yapabilen kişiler. Kullanıcı silinmez, pasife alınır; en az bir aktif yönetici kalır."
        icon={<Users />}
        actions={
          <Button onClick={() => setDialog({ user: null })}>
            <Plus className="w-4 h-4 mr-2" />
            Kullanıcı ekle
          </Button>
        }
      />
      <PageContent>
        <UserTable
          items={users}
          currentUserId={currentUserId}
          onEdit={(user) => setDialog({ user })}
          onResetPassword={setPasswordUser}
        />
      </PageContent>
      <UserDialog
        state={dialog}
        currentUserId={currentUserId}
        onOpenChange={(open) => !open && setDialog(null)}
        onSaved={() => setVersion((v) => v + 1)}
      />
      <PasswordDialog user={passwordUser} onOpenChange={(open) => !open && setPasswordUser(null)} />
    </PageLayout>
  );
}
