import { FileText } from "lucide-react";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { contentService } from "@/api/services/content.service";
import { useQuery } from "@/hooks/use-query";
import { ContentTabs } from "./components/content-tabs";

// Vitrin içeriği: ana sayfa videosu, Hizmetler, Kurumsal ve Sayılarla Biz yazıları (yalnız Yönetici).
export default function ContentPage() {
  const { data: content } = useQuery("site-content", contentService.get);

  return (
    <PageLayout className="overflow-y-auto">
      <PageHeader
        title="Site İçeriği"
        description="Ana sayfa videosu; Hizmetler, Kurumsal sayfası ve Sayılarla Biz bölümünün yazıları."
        icon={<FileText />}
      />
      {content ? <ContentTabs content={content} /> : <PageLoader />}
    </PageLayout>
  );
}
