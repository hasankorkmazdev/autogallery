import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Field, FieldGroup } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { AppLogo } from "@/components/app-logo";
import { useAuthStore } from "@/store/auth-store";
import { authService } from "@/api/services/auth.service";

const loginSchema = z.object({
  eMail: z.string().min(1, "E-posta zorunludur.").email("Geçerli bir e-posta adresi girin."),
  password: z.string().min(1, "Şifre zorunludur."),
});

type LoginValues = z.infer<typeof loginSchema>;

// Geliştirmede appsettings.Development.json SeedAdmin bilgileri; production derlemesine girmez.
const defaultLoginValues: LoginValues = import.meta.env.DEV
  ? { eMail: "admin@galeri.test", password: "Dev-9b85a3d8f4b4!" }
  : { eMail: "", password: "" };

export function LoginForm() {
  const [loading, setLoading] = useState(false);
  const login = useAuthStore((state) => state.login);
  const navigate = useNavigate();
  const form = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: defaultLoginValues,
  });
  const { errors } = form.formState;

  useEffect(() => {
    document.title = "Giriş | Yönetim Paneli";
  }, []);

  const onSubmit = async (values: LoginValues) => {
    setLoading(true);
    try {
      const result = await authService.login(values.eMail, values.password);
      login(result.user, result.token, result.expiresAt);
      toast.success(`Hoş geldiniz, ${result.user.name}.`);
      navigate("/vehicles");
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-center">
          <AppLogo />
          Yönetim Paneli
        </CardTitle>
        <CardDescription className="text-center">
          Araç ve talepleri yönetmek için giriş yapın.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={form.handleSubmit(onSubmit)} autoComplete="off" noValidate>
          <FieldGroup>
            <InputField
              id="email"
              type="email"
              label="E-posta"
              description="Panel hesabınızın e-posta adresi."
              error={errors.eMail?.message}
              autoComplete="off"
              {...form.register("eMail")}
            />
            <InputField
              id="password"
              password
              label="Şifre"
              description="Şifrenizi unuttuysanız galeri yöneticisine başvurun."
              error={errors.password?.message}
              autoComplete="off"
              {...form.register("password")}
            />
            <Field className="mt-2">
              <Button type="submit" disabled={loading} className="w-full">
                {loading ? "Giriş yapılıyor..." : "Giriş yap"}
              </Button>
            </Field>
          </FieldGroup>
        </form>
      </CardContent>
    </Card>
  );
}
