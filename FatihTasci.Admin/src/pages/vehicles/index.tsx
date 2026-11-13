import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Car, Plus, RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { PageContent, PageHeader, PageLayout } from "@/components/layout/page-layout";
import { ListPagination } from "@/components/list-pagination";
import { vehiclesService } from "@/api/services/vehicles.service";
import type { VehicleListItem, VehicleStatusValue } from "@/api/types";
import { useQuery } from "@/hooks/use-query";
import { VehicleFilters } from "./components/vehicle-filters";
import { VehicleTable } from "./components/vehicle-table";

const PAGE_SIZE = 20;

export default function VehiclesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState<number | undefined>(undefined);
  const [pageIndex, setPageIndex] = useState(0);
  const [refreshId, setRefreshId] = useState(0);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPageIndex(0);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  const params = { pageIndex, pageSize: PAGE_SIZE, status, search: debouncedSearch || undefined };
  const { data, loading } = useQuery(JSON.stringify({ ...params, refreshId }), () => vehiclesService.getList(params));

  const handleStatusChange = useCallback(async (vehicle: VehicleListItem, newStatus: VehicleStatusValue) => {
    try {
      const result = await vehiclesService.updateStatus(vehicle.id, newStatus);
      toast.success(result.message);
      setRefreshId((x) => x + 1);
    } catch (error) {
      toast.error((error as Error).message);
    }
  }, []);

  const handleCopy = useCallback(
    async (vehicle: VehicleListItem) => {
      try {
        const result = await vehiclesService.copy(vehicle.id);
        toast.success(result.message);
        navigate(`/vehicles/${result.data.id}?step=2`);
      } catch (error) {
        toast.error((error as Error).message);
      }
    },
    [navigate],
  );

  return (
    <PageLayout>
      <PageHeader
        title="Araçlar"
        description="Stoktaki araçları yönetin, yenisini ekleyin ya da durumunu değiştirin."
        icon={<Car className="w-8 h-8" />}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" size="icon" title="Yenile" onClick={() => setRefreshId((x) => x + 1)}>
              <RefreshCw className="w-4 h-4" />
            </Button>
            <Button onClick={() => navigate("/vehicles/new")}>
              <Plus className="w-4 h-4 mr-2" />
              Yeni araç
            </Button>
          </div>
        }
      />
      <PageContent>
        <div className="flex flex-col gap-4">
          <VehicleFilters
            search={search}
            status={status}
            onSearchChange={setSearch}
            onStatusChange={(value) => {
              setStatus(value);
              setPageIndex(0);
            }}
          />
          <VehicleTable
            items={data?.items ?? []}
            loading={loading}
            onStatusChange={handleStatusChange}
            onCopy={handleCopy}
          />
          <ListPagination
            pageIndex={pageIndex}
            totalPages={data?.totalPages ?? 0}
            totalCount={data?.totalCount ?? 0}
            onPageChange={setPageIndex}
          />
        </div>
      </PageContent>
    </PageLayout>
  );
}
