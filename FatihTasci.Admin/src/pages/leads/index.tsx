import { useEffect, useState } from "react";
import { Inbox, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { PageContent, PageHeader, PageLayout } from "@/components/layout/page-layout";
import { ListPagination } from "@/components/list-pagination";
import { leadsService } from "@/api/services/leads.service";
import type { Lead } from "@/api/types";
import { useQuery } from "@/hooks/use-query";
import { useLookupStore } from "@/store/lookup-store";
import { LeadTable } from "./components/lead-table";
import { LeadDialog } from "./components/lead-dialog";

const PAGE_SIZE = 20;
const ALL = "all";

export default function LeadsPage() {
  const lookups = useLookupStore((state) => state.lookups);
  const [status, setStatus] = useState<number | undefined>(undefined);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageIndex, setPageIndex] = useState(0);
  const [refreshId, setRefreshId] = useState(0);
  const [selected, setSelected] = useState<Lead | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPageIndex(0);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  const params = { pageIndex, pageSize: PAGE_SIZE, status, search: debouncedSearch || undefined };
  const { data, loading } = useQuery(JSON.stringify({ ...params, refreshId }), () => leadsService.getList(params));

  return (
    <PageLayout>
      <PageHeader
        title="Talepler"
        description="Siteden gelen bilgi, test sürüşü ve iletişim talepleri."
        icon={<Inbox className="w-8 h-8" />}
        actions={
          <Button variant="outline" size="icon" title="Yenile" onClick={() => setRefreshId((x) => x + 1)}>
            <RefreshCw className="w-4 h-4" />
          </Button>
        }
      />
      <PageContent>
        <div className="flex flex-col gap-4">
          <div className="flex flex-col gap-2 md:flex-row">
            <Input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Ad, telefon ya da e-posta ile ara"
              aria-label="Talep ara"
              autoComplete="off"
              className="md:w-80"
            />
            <Select
              value={status === undefined ? ALL : String(status)}
              onValueChange={(v) => {
                setStatus(v === ALL ? undefined : Number(v));
                setPageIndex(0);
              }}>
              <SelectTrigger className="md:w-48" aria-label="Durum filtresi">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={ALL}>Tüm durumlar</SelectItem>
                {(lookups?.leadStatus ?? []).map((option) => (
                  <SelectItem key={option.value} value={String(option.value)}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <LeadTable items={data?.items ?? []} loading={loading} onOpen={setSelected} />
          <ListPagination
            pageIndex={pageIndex}
            totalPages={data?.totalPages ?? 0}
            totalCount={data?.totalCount ?? 0}
            onPageChange={setPageIndex}
          />
        </div>
      </PageContent>
      <LeadDialog
        lead={selected}
        onOpenChange={(open) => !open && setSelected(null)}
        onSaved={() => setRefreshId((x) => x + 1)}
      />
    </PageLayout>
  );
}
