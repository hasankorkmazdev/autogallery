import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { FieldGroup } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { usersService } from "@/api/services/users.service";
import type { UserListItem } from "@/api/types";

// Kurallar sunucudaki ResetUserPasswordRequestModelValidator ile aynıdır.
const schema = z
  .object({
    password: z.string().min(8, "Şifre en az 8 karakter olmalıdır.").max(100, "Şifre en fazla 100 karakter olabilir."),
    confirm: z.string(),
  })
  .refine((v) => v.password === v.confirm, { path: ["confirm"], message: "Şifreler aynı değil." });
type Values = z.infer<typeof schema>;

interface PasswordDialogProps {
  user: UserListItem | null;
  onOpenChange: (open: boolean) => void;
}

export function PasswordDialog({ user, onOpenChange }: PasswordDialogProps) {
  const [saving, setSaving] = useState(false);
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { password: "", confirm: "" } });
  const { errors } = form.formState;

  useEffect(() => {
    if (user) form.reset({ password: "", confirm: "" });
  }, [user, form]);

  const submit = async (values: Values) => {
    if (!user) return;
    setSaving(true);
    try {
      const result = await usersService.resetPassword(user.id, values.password);
      toast.success(result.message);
      onOpenChange(false);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={user !== null} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Şifre değiştir</DialogTitle>
          <DialogDescription>
            {user ? `${user.name} ${user.surname} için yeni şifre belirleyin. Yeni şifreyi kullanıcıya siz iletin.` : ""}
          </DialogDescription>
        </DialogHeader>
        <form id="password-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <InputField id="new-password" label="Yeni şifre" required password description="En az 8 karakter."
              error={errors.password?.message} autoComplete="new-password" {...form.register("password")} />
            <InputField id="confirm-password" label="Yeni şifre (tekrar)" required password description="Aynı şifreyi tekrar yazın."
              error={errors.confirm?.message} autoComplete="new-password" {...form.register("confirm")} />
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="password-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Şifreyi değiştir"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
