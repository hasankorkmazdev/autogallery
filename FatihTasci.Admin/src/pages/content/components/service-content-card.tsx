import type { UseFormReturn } from "react-hook-form";
import { ExternalLink } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { TextareaField } from "@/components/form/form-fields";
import { publicSiteUrl } from "@/lib/format";
import { MAX_SERVICE_BODY, MAX_SERVICE_SUMMARY, type ContentOutput, type ContentValues } from "./content-schema";

interface ServiceContentCardProps {
  form: UseFormReturn<ContentValues, unknown, ContentOutput>;
  index: number;
}

// Bir hizmetin kısa yazısı (kartlar) ve sayfa metni. Başlık ve adres hizmet türünden gelir, değişmez.
export function ServiceContentCard({ form, index }: ServiceContentCardProps) {
  const service = form.getValues(`services.${index}`);
  const errors = form.formState.errors.services?.[index];

  return (
    <Card>
      <CardHeader className="flex flex-row flex-wrap items-start justify-between gap-2">
        <div className="flex flex-col gap-1">
          <CardTitle>{service.title}</CardTitle>
          <CardDescription>Sitede {service.path}</CardDescription>
        </div>
        <Button variant="outline" size="sm" asChild>
          <a href={publicSiteUrl(service.path)} target="_blank" rel="noreferrer">
            <ExternalLink className="w-4 h-4 mr-2" />
            Sayfayı aç
          </a>
        </Button>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <TextareaField
          id={`service-summary-${index}`}
          label="Kısa yazı"
          required
          rows={3}
          description={`Ana sayfadaki ve Hizmetler sayfasındaki kartta, hizmet sayfasının girişinde görünür. En fazla ${MAX_SERVICE_SUMMARY} karakter.`}
          error={errors?.summary?.message}
          {...form.register(`services.${index}.summary`)}
        />
        <TextareaField
          id={`service-body-${index}`}
          label="Sayfa metni"
          rows={9}
          description={`Hizmet sayfasının metni. Boş satır yeni paragraf başlatır. En fazla ${MAX_SERVICE_BODY} karakter.`}
          error={errors?.body?.message}
          {...form.register(`services.${index}.body`)}
        />
      </CardContent>
    </Card>
  );
}
