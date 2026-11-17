import { useState } from "react";
import { useForm, type FieldErrors } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { contentService } from "@/api/services/content.service";
import type { HeroVideo, SiteContent } from "@/api/types";
import { CorporateContentCard } from "./corporate-content-card";
import { HeroVideoCard } from "./hero-video-card";
import { ServiceContentCard } from "./service-content-card";
import { StatsContentCard } from "./stats-content-card";
import {
  contentSchema,
  FIELD_TABS,
  toContentValues,
  toSiteContent,
  type ContentOutput,
  type ContentTab,
  type ContentValues,
} from "./content-schema";

const TABS: { value: ContentTab; label: string }[] = [
  { value: "home", label: "Ana Sayfa" },
  { value: "services", label: "Hizmetler" },
  { value: "corporate", label: "Kurumsal" },
  { value: "stats", label: "Sayılarla Biz" },
];

interface ContentTabsProps {
  content: SiteContent;
}

// Yazı sekmeleri tek formdur; kaydederken hata başka sekmedeyse o sekmeye geçilir (Ayarlar sayfasıyla aynı).
// Ana Sayfa sekmesindeki video formun dışındadır, yüklendiği anda kaydedilir.
export function ContentTabs({ content }: ContentTabsProps) {
  const [tab, setTab] = useState<ContentTab>("home");
  const [saving, setSaving] = useState(false);
  const [heroVideo, setHeroVideo] = useState<HeroVideo | undefined>(content.heroVideo);
  const form = useForm<ContentValues, unknown, ContentOutput>({ resolver: zodResolver(contentSchema), defaultValues: toContentValues(content) });

  const submit = async (values: ContentOutput) => {
    setSaving(true);
    try {
      const result = await contentService.update(toSiteContent(values));
      form.reset(toContentValues(result.data));
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  const invalid = (fieldErrors: FieldErrors<ContentValues>) => {
    const first = (Object.keys(fieldErrors) as (keyof ContentValues)[])[0];
    if (!first) return;
    const target = FIELD_TABS[first];
    if (target !== tab) {
      setTab(target);
      toast.error(`"${TABS.find((x) => x.value === target)?.label}" sekmesinde düzeltilmesi gereken alan var.`);
    }
  };

  return (
    <form autoComplete="off" noValidate onSubmit={form.handleSubmit(submit, invalid)} className="flex flex-col gap-4">
      <Tabs value={tab} onValueChange={(v) => setTab(v as ContentTab)}>
        {/* Telefonda dört sekme iki satır */}
        <TabsList className="grid h-auto w-full grid-cols-2 sm:inline-flex sm:h-9 sm:w-auto">
          {TABS.map((x) => (
            <TabsTrigger key={x.value} value={x.value}>
              {x.label}
            </TabsTrigger>
          ))}
        </TabsList>

        <TabsContent value="home">
          {heroVideo && <HeroVideoCard video={heroVideo} onChanged={setHeroVideo} />}
        </TabsContent>

        <TabsContent value="services" className="flex flex-col gap-4">
          {content.services.map((service, index) => (
            <ServiceContentCard key={service.type} form={form} index={index} />
          ))}
        </TabsContent>

        <TabsContent value="corporate">
          <CorporateContentCard form={form} />
        </TabsContent>

        <TabsContent value="stats">
          <StatsContentCard form={form} />
        </TabsContent>
      </Tabs>

      {tab !== "home" && (
        <div className="flex justify-end">
          <Button type="submit" disabled={saving}>
            {saving ? "Kaydediliyor..." : "İçeriği kaydet"}
          </Button>
        </div>
      )}
    </form>
  );
}
