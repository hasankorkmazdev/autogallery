import { useCallback, useEffect, useState } from "react";
import { ListChecks, Plus } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { catalogService } from "@/api/services/catalog.service";
import type { Feature, FeatureGroup } from "@/api/types";
import { FeatureDialog, type FeatureDialogState } from "./components/feature-dialog";
import { FeatureGroupCard } from "./components/feature-group-card";

export default function FeaturesPage() {
  const [groups, setGroups] = useState<FeatureGroup[] | null>(null);
  const [dialog, setDialog] = useState<FeatureDialogState | null>(null);

  const load = useCallback(() => {
    catalogService
      .getFeatures(true)
      .then(setGroups)
      .catch((error: Error) => toast.error(error.message));
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  // Yeni sıra hemen gösterilir; kaydedilemezse liste sunucudan yeniden yüklenir.
  const reorder = async (category: number, ordered: Feature[]) => {
    setGroups((prev) => prev?.map((g) => (g.category === category ? { ...g, features: ordered } : g)) ?? prev);
    try {
      const result = await catalogService.reorderFeatures(category, ordered.map((x) => x.id));
      setGroups(result.data);
    } catch (error) {
      toast.error((error as Error).message);
      load();
    }
  };

  const renderGroups = () => {
    if (!groups) return <PageLoader />;
    return (
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        {groups.map((group) => (
          <FeatureGroupCard
            key={group.category}
            group={group}
            onReorder={reorder}
            onAdd={(category) => setDialog({ feature: null, category })}
            onEdit={(feature) => setDialog({ feature, category: feature.category })}
          />
        ))}
      </div>
    );
  };

  return (
    <PageLayout>
      <PageHeader
        title="Donanımlar"
        description="Araç sihirbazındaki donanım listesi. Satırları sürükleyerek sıralayın; sihirbaz ve site bu sırayı kullanır."
        icon={<ListChecks />}
        actions={
          <Button onClick={() => setDialog({ feature: null, category: null })}>
            <Plus className="w-4 h-4 mr-2" />
            Donanım ekle
          </Button>
        }
      />
      {renderGroups()}
      <FeatureDialog state={dialog} onOpenChange={(open) => !open && setDialog(null)} onSaved={load} />
    </PageLayout>
  );
}
