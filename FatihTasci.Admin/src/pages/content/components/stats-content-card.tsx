import { useFieldArray, type UseFormReturn } from "react-hook-form";
import { ArrowDown, ArrowUp, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FieldDescription } from "@/components/ui/field";
import { InputField } from "@/components/ui/input-field";
import { MAX_STATS, type ContentOutput, type ContentValues } from "./content-schema";

interface StatsContentCardProps {
  form: UseFormReturn<ContentValues, unknown, ContentOutput>;
}

// "Sayılarla Biz" rakamları: ana sayfada ve Kurumsal sayfada bu sırayla görünür. Liste boşsa bölüm gizlenir.
export function StatsContentCard({ form }: StatsContentCardProps) {
  const { fields, append, remove, move } = useFieldArray({ control: form.control, name: "stats" });
  const errors = form.formState.errors.stats;
  const full = fields.length >= MAX_STATS;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Sayılarla Biz</CardTitle>
        <CardDescription>
          Ana sayfada ve Kurumsal sayfada bu sırayla görünür. Rakam yazı olarak gösterilir, örn. &quot;40&quot;, &quot;5.000+&quot;. Tüm
          satırları kaldırırsanız bölüm sitede görünmez.
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        {fields.length === 0 && <FieldDescription>Rakam eklenmedi; bölüm sitede görünmüyor.</FieldDescription>}
        {fields.map((field, index) => (
          <div key={field.id} className="grid grid-cols-1 items-start gap-3 rounded-lg border p-3 sm:grid-cols-[160px_1fr_auto]">
            <InputField
              id={`stat-value-${index}`}
              label="Rakam"
              required
              placeholder="5.000+"
              description="En fazla 12 karakter."
              error={errors?.[index]?.value?.message}
              {...form.register(`stats.${index}.value`)}
            />
            <InputField
              id={`stat-label-${index}`}
              label="Açıklama"
              required
              placeholder="Araç Satışı"
              description="Rakamın altında görünür, en fazla 40 karakter."
              error={errors?.[index]?.label?.message}
              {...form.register(`stats.${index}.label`)}
            />
            <div className="flex gap-1 sm:pt-6">
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
          <Button type="button" variant="outline" disabled={full} onClick={() => append({ value: "", label: "" })}>
            <Plus /> Rakam ekle
          </Button>
          {full && <FieldDescription>En fazla {MAX_STATS} rakam eklenebilir.</FieldDescription>}
        </div>
      </CardContent>
    </Card>
  );
}
