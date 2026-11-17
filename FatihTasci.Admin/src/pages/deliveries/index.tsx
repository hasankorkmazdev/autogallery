import { useState } from "react";
import { ExternalLink, Handshake, Plus } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { FieldDescription } from "@/components/ui/field";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { useQuery } from "@/hooks/use-query";
import { publicSiteUrl } from "@/lib/format";
import { deliveriesService } from "@/api/services/deliveries.service";
import type { Delivery } from "@/api/types";
import { DeleteDeliveryDialog } from "./components/delete-delivery-dialog";
import { DeliveryCard } from "./components/delivery-card";
import { DeliveryDialog, type DeliveryDialogState } from "./components/delivery-dialog";

// Müşteriye teslim edilen araçların fotoğrafları (Personel ve Yönetici). Sitede ana sayfada en yeni 8'i, tamamı /teslimatlar'da.
export default function DeliveriesPage() {
  const query = useQuery("deliveries", deliveriesService.getAll);
  // İşlemlerden dönen güncel liste; henüz işlem yapılmadıysa sorgu sonucu kullanılır.
  const [changed, setChanged] = useState<Delivery[] | null>(null);
  const [dialog, setDialog] = useState<DeliveryDialogState | null>(null);
  const [deleting, setDeleting] = useState<Delivery | null>(null);
  const deliveries = changed ?? query.data;

  const remove = async (delivery: Delivery) => {
    try {
      const result = await deliveriesService.remove(delivery.id);
      setChanged(result.data);
      toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
    }
  };

  const visibleCount = deliveries?.filter((x) => x.isActive && x.mediumUrl).length ?? 0;

  const renderContent = () => {
    if (!deliveries) return <PageLoader />;
    if (deliveries.length === 0) {
      return (
        <FieldDescription>
          Henüz teslimat yok. “Teslimat ekle” ile başlayın; ana sayfadaki Teslimatlar bölümü ilk teslimatla görünür.
        </FieldDescription>
      );
    }
    return (
      <div className="flex flex-col gap-3">
        <FieldDescription>
          Sitede {visibleCount} / {deliveries.length} teslimat görünüyor. Sıra teslim tarihidir (en yeni önce); ana sayfada en
          yeni 8 teslimat gösterilir.
        </FieldDescription>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 2xl:grid-cols-6">
          {deliveries.map((delivery) => (
            <DeliveryCard key={delivery.id} delivery={delivery} onEdit={(x) => setDialog({ delivery: x })} onDelete={setDeleting} />
          ))}
        </div>
      </div>
    );
  };

  return (
    <PageLayout className="overflow-y-auto">
      <PageHeader
        title="Teslimatlar"
        description="Müşterilere teslim edilen araçların fotoğrafları; sitede ana sayfada ve Teslimatlar sayfasında görünür."
        icon={<Handshake />}
        actions={
          <>
            <Button variant="outline" asChild>
              <a href={publicSiteUrl("/teslimatlar")} target="_blank" rel="noreferrer">
                <ExternalLink className="w-4 h-4 mr-2" />
                Sayfayı aç
              </a>
            </Button>
            <Button disabled={!deliveries} onClick={() => setDialog({ delivery: null })}>
              <Plus className="w-4 h-4 mr-2" />
              Teslimat ekle
            </Button>
          </>
        }
      />
      {renderContent()}
      <DeliveryDialog state={dialog} onOpenChange={(open) => !open && setDialog(null)} onSaved={setChanged} />
      <DeleteDeliveryDialog delivery={deleting} onOpenChange={(open) => !open && setDeleting(null)} onConfirm={remove} />
    </PageLayout>
  );
}
