import { Controller, useFieldArray, type UseFormReturn } from "react-hook-form";
import { ArrowDown, ArrowUp, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FieldDescription } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { SelectField } from "@/components/form/form-fields";
import { useLookupStore } from "@/store/lookup-store";
import { MAX_SOCIAL_LINKS, type SettingsOutput, type SettingsValues } from "./settings-schema";

interface SocialLinksCardProps {
  form: UseFormReturn<SettingsValues, unknown, SettingsOutput>;
}

export function SocialLinksCard({ form }: SocialLinksCardProps) {
  const platforms = useLookupStore((s) => s.lookups?.socialPlatform) ?? [];
  const { fields, append, remove, move } = useFieldArray({ control: form.control, name: "socialLinks" });
  const errors = form.formState.errors.socialLinks;
  const full = fields.length >= MAX_SOCIAL_LINKS;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Sosyal medya ve mağazalar</CardTitle>
        <CardDescription>
          Sitenin alt bilgisinde ve iletişim sayfasında bu sırayla görünür. Instagram, Facebook, X, YouTube, TikTok ya da Sahibinden ve arabam.com
          mağazanız gibi bağlantıları ekleyebilirsiniz.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {fields.length === 0 && <FieldDescription>Henüz bağlantı eklenmedi.</FieldDescription>}
        {fields.map((field, index) => (
          <div key={field.id} className="grid grid-cols-1 items-start gap-3 rounded-lg border p-3 md:grid-cols-[220px_1fr_auto]">
            <Controller
              control={form.control}
              name={`socialLinks.${index}.platform`}
              render={({ field: platform }) => (
                <SelectField
                  id={`social-platform-${index}`}
                  label="Platform"
                  required
                  options={platforms}
                  value={platform.value}
                  onChange={platform.onChange}
                  error={errors?.[index]?.platform?.message}
                />
              )}
            />
            <InputField
              id={`social-url-${index}`}
              label="Bağlantı"
              required
              placeholder="https://"
              description="Profil ya da mağaza sayfasının tam adresi."
              error={errors?.[index]?.url?.message}
              {...form.register(`socialLinks.${index}.url`)}
            />
            <div className="flex gap-1 md:pt-6">
              <Button type="button" variant="ghost" size="icon" title="Yukarı taşı" disabled={index === 0} onClick={() => move(index, index - 1)}>
                <ArrowUp />
              </Button>
              <Button type="button" variant="ghost" size="icon" title="Aşağı taşı" disabled={index === fields.length - 1} onClick={() => move(index, index + 1)}>
                <ArrowDown />
              </Button>
              <Button type="button" variant="ghost-destructive" size="icon" title="Kaldır" onClick={() => remove(index)}>
                <Trash2 />
              </Button>
            </div>
          </div>
        ))}
        <div>
          <Button type="button" variant="outline" disabled={full} onClick={() => append({ platform: null, url: "" })}>
            <Plus /> Bağlantı ekle
          </Button>
          {full && <FieldDescription>En fazla {MAX_SOCIAL_LINKS} bağlantı eklenebilir.</FieldDescription>}
        </div>
      </CardContent>
    </Card>
  );
}
