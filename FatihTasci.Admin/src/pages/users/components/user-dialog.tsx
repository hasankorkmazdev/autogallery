import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
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
import { SelectField, SwitchField } from "@/components/form/form-fields";
import { usersService } from "@/api/services/users.service";
import { UserRole, type UserListItem } from "@/api/types";
import { useLookupStore } from "@/store/lookup-store";

// Kurallar sunucudaki SaveUserRequestModelValidator ile aynıdır; şifre yalnız yeni kullanıcıda istenir.
const schema = z
  .object({
    isNew: z.boolean(),
    name: z.string().trim().min(1, "Ad zorunludur.").max(50, "Ad en fazla 50 karakter olabilir."),
    surname: z.string().trim().min(1, "Soyad zorunludur.").max(50, "Soyad en fazla 50 karakter olabilir."),
    eMail: z
      .string()
      .trim()
      .min(1, "E-posta zorunludur.")
      .max(150, "E-posta en fazla 150 karakter olabilir.")
      .refine((v) => z.email().safeParse(v).success, { message: "Geçerli bir e-posta adresi girin." }),
    role: z.number().nullable().refine((v) => v !== null, { message: "Rol seçin." }),
    isActive: z.boolean(),
    password: z.string(),
  })
  .superRefine((values, ctx) => {
    if (!values.isNew) return;
    if (values.password.length < 8) {
      ctx.addIssue({ code: "custom", path: ["password"], message: "Şifre en az 8 karakter olmalıdır." });
    } else if (values.password.length > 100) {
      ctx.addIssue({ code: "custom", path: ["password"], message: "Şifre en fazla 100 karakter olabilir." });
    }
  });
type Values = z.input<typeof schema>;
type Output = z.output<typeof schema>;

export interface UserDialogState {
  user: UserListItem | null;
}

interface UserDialogProps {
  state: UserDialogState | null;
  currentUserId: string | undefined;
  onOpenChange: (open: boolean) => void;
  onSaved: () => void;
}

export function UserDialog({ state, currentUserId, onOpenChange, onSaved }: UserDialogProps) {
  // Selector'da `?? []` kullanılmaz: her çağrıda yeni dizi döner ve sonsuz render'a yol açar.
  const roles = useLookupStore((s) => s.lookups?.userRole) ?? [];
  const [saving, setSaving] = useState(false);
  const form = useForm<Values, unknown, Output>({
    resolver: zodResolver(schema),
    defaultValues: { isNew: true, name: "", surname: "", eMail: "", role: UserRole.Staff, isActive: true, password: "" },
  });
  const { errors } = form.formState;
  const isNew = !state?.user;
  // Kişi kendi rolünü değiştiremez, kendini pasife alamaz (sunucu da reddeder).
  const isSelf = !!state?.user && state.user.id === currentUserId;

  useEffect(() => {
    if (!state) return;
    const u = state.user;
    form.reset({
      isNew: !u,
      name: u?.name ?? "",
      surname: u?.surname ?? "",
      eMail: u?.eMail ?? "",
      role: u?.role ?? UserRole.Staff,
      isActive: u?.isActive ?? true,
      password: "",
    });
  }, [state, form]);

  const submit = async (values: Output) => {
    setSaving(true);
    try {
      const request = {
        name: values.name.trim(),
        surname: values.surname.trim(),
        eMail: values.eMail.trim(),
        role: values.role!,
        isActive: values.isActive,
        password: values.isNew ? values.password : undefined,
      };
      const result = state?.user ? await usersService.update(state.user.id, request) : await usersService.create(request);
      toast.success(result.message);
      onSaved();
      onOpenChange(false);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={state !== null} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isNew ? "Kullanıcı ekle" : "Kullanıcı düzenle"}</DialogTitle>
          <DialogDescription>
            Yönetici her şeyi yönetir. Personel araç ve talepleri yönetir; ayarlar, showroom, katalog düzenleme ve kullanıcılar kapalıdır.
          </DialogDescription>
        </DialogHeader>
        <form id="user-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <InputField id="user-name" label="Ad" required description="Panelde görünen ad."
                error={errors.name?.message} autoComplete="off" {...form.register("name")} />
              <InputField id="user-surname" label="Soyad" required description="Panelde görünen soyad."
                error={errors.surname?.message} autoComplete="off" {...form.register("surname")} />
            </div>
            <InputField id="user-email" label="E-posta" required description="Giriş için kullanılır."
              error={errors.eMail?.message} autoComplete="off" {...form.register("eMail")} />
            {isNew && (
              <InputField id="user-password" label="Şifre" required password description="En az 8 karakter. Kullanıcıya siz iletin."
                error={errors.password?.message} autoComplete="new-password" {...form.register("password")} />
            )}
            <Controller control={form.control} name="role" render={({ field }) => (
              <SelectField id="user-role" label="Rol" required options={roles} disabled={isSelf}
                description={isSelf ? "Kendi rolünüzü değiştiremezsiniz." : "Yetkileri belirler."}
                error={errors.role?.message} value={field.value} onChange={field.onChange} />
            )} />
            <Controller control={form.control} name="isActive" render={({ field }) => (
              <SwitchField id="user-active" label="Aktif" disabled={isSelf}
                description={isSelf ? "Kendinizi pasife alamazsınız." : "Pasif kullanıcı giriş yapamaz, açık oturumu kapanır."}
                checked={field.value} onChange={field.onChange} />
            )} />
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Vazgeç
          </Button>
          <Button type="submit" form="user-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
