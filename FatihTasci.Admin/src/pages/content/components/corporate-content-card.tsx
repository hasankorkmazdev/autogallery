import type { UseFormReturn } from "react-hook-form";
import { ExternalLink } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { TextareaField } from "@/components/form/form-fields";
import { publicSiteUrl } from "@/lib/format";
import type { ContentOutput, ContentValues } from "./content-schema";

interface CorporateContentCardProps {
  form: UseFormReturn<ContentValues, unknown, ContentOutput>;
}

// Kurumsal sayfanın yazıları. Boş bırakılan bölüm sitede görünmez.
export function CorporateContentCard({ form }: CorporateContentCardProps) {
  const { errors } = form.formState;

  return (
    <Card>
      <CardHeader className="flex flex-row flex-wrap items-start justify-between gap-2">
        <div className="flex flex-col gap-1">
          <CardTitle>Kurumsal</CardTitle>
          <CardDescription>Sitede /kurumsal. Boş bırakılan bölüm sitede görünmez; boş satır yeni paragraf başlatır.</CardDescription>
        </div>
        <Button variant="outline" size="sm" asChild>
          <a href={publicSiteUrl("/kurumsal")} target="_blank" rel="noreferrer">
            <ExternalLink className="w-4 h-4 mr-2" />
            Sayfayı aç
          </a>
        </Button>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <TextareaField
          id="corporateIntro"
          label="Giriş"
          rows={3}
          description="Sayfanın başında, firma adının altında görünür. Arama sonuçlarında da açıklama olarak kullanılır."
          error={errors.corporateIntro?.message}
          {...form.register("corporateIntro")}
        />
        <TextareaField
          id="corporateAbout"
          label="Hakkımızda"
          rows={10}
          description="Galerinin hikâyesi ve çalışma biçimi."
          error={errors.corporateAbout?.message}
          {...form.register("corporateAbout")}
        />
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <TextareaField
            id="corporateVision"
            label="Vizyonumuz"
            rows={5}
            description="Galerinin ulaşmak istediği hedef."
            error={errors.corporateVision?.message}
            {...form.register("corporateVision")}
          />
          <TextareaField
            id="corporateMission"
            label="Misyonumuz"
            rows={5}
            description="Galerinin her gün yaptığı iş ve ilkeleri."
            error={errors.corporateMission?.message}
            {...form.register("corporateMission")}
          />
        </div>
      </CardContent>
    </Card>
  );
}
