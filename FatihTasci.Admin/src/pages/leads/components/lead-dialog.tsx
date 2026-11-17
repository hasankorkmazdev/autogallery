import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { FieldGroup } from "@/components/ui/field";
import { SelectField, TextareaField } from "@/components/form/form-fields";
import { leadsService } from "@/api/services/leads.service";
import type { Lead } from "@/api/types";
import { formatDate, formatDateTime, formatKm, formatPrice, publicSiteUrl } from "@/lib/format";
import { labelOf, useLookupStore } from "@/store/lookup-store";

// LeadType.SellOrTrade: vitrindeki "Aracımı Satmak İstiyorum" fotoğraflı teklif formu
const SELL_OR_TRADE = 3;

const schema = z.object({
  status: z.number(),
  internalNote: z.string().max(2000, "Not en fazla 2000 karakter olabilir."),
});
type Values = z.infer<typeof schema>;

interface LeadDialogProps {
  lead: Lead | null;
  onOpenChange: (open: boolean) => void;
  onSaved: () => void;
}

function Detail({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="text-sm">{children}</span>
    </div>
  );
}

// Teklif formundan gelen araç bilgileri (Marka / Model / Paket, yıl, km, hasar, beklenen fiyat).
function SellDetails({ lead }: { lead: Lead }) {
  const lookups = useLookupStore((state) => state.lookups);
  const vehicle = [lead.sellBrand, lead.sellModel, lead.sellPackage].filter(Boolean).join(" / ");
  return (
    <>
      <div className="col-span-2">
        <Detail label="Araç (Marka / Model / Paket)">{vehicle || "-"}</Detail>
      </div>
      <Detail label="Model yılı">{lead.sellYear ?? "-"}</Detail>
      <Detail label="Kilometre">{formatKm(lead.sellKm)}</Detail>
      <Detail label="Hasar durumu">{labelOf(lookups?.sellDamage, lead.sellDamage)}</Detail>
      <Detail label="Beklenen fiyat">{formatPrice(lead.sellExpectedPrice)}</Detail>
    </>
  );
}

// Talep fotoğrafları oturumla korunan API ucundan gelir; tıklayınca tam boyu yeni sekmede açılır.
function LeadPhotos({ lead }: { lead: Lead }) {
  if (lead.media.length === 0) {
    return <Detail label="Fotoğraflar">Fotoğraf eklenmemiş.</Detail>;
  }
  return (
    <div className="flex flex-col gap-2">
      <span className="text-xs text-muted-foreground">Fotoğraflar ({lead.media.length})</span>
      <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
        {lead.media.map((media, index) => (
          <a key={media.id} href={media.url} target="_blank" rel="noreferrer"
            className="block aspect-[4/3] overflow-hidden rounded-md border bg-muted">
            <img src={media.url} alt={`Fotoğraf ${index + 1}`} loading="lazy"
              className="size-full object-cover transition-opacity hover:opacity-90" />
          </a>
        ))}
      </div>
    </div>
  );
}

// Talep ayrıntısı; personel durumu günceller ve not ekler.
export function LeadDialog({ lead, onOpenChange, onSaved }: LeadDialogProps) {
  const lookups = useLookupStore((state) => state.lookups);
  const [saving, setSaving] = useState(false);
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { status: 0, internalNote: "" } });

  useEffect(() => {
    if (lead) form.reset({ status: lead.status, internalNote: lead.internalNote ?? "" });
  }, [lead, form]);

  const submit = async (values: Values) => {
    if (!lead) return;
    setSaving(true);
    try {
      const result = await leadsService.update(lead.id, values.status, values.internalNote.trim() || null);
      toast.success(result.message);
      onSaved();
      onOpenChange(false);
    } catch (error) {
      toast.error((error as Error).message);
    } finally {
      setSaving(false);
    }
  };

  const isSell = lead?.type === SELL_OR_TRADE;

  return (
    <Dialog open={lead !== null} onOpenChange={onOpenChange}>
      <DialogContent className={isSell ? "max-w-2xl" : "max-w-lg"}>
        <DialogHeader>
          <DialogTitle>{lead?.fullName}</DialogTitle>
          <DialogDescription>
            {labelOf(lookups?.leadType, lead?.type)} · {formatDateTime(lead?.createDate)}
          </DialogDescription>
        </DialogHeader>
        {lead && (
          <div className="grid grid-cols-2 gap-3">
            <Detail label="Telefon">
              <a href={`tel:${lead.phone}`} className="underline">{lead.phone}</a>
            </Detail>
            <Detail label="E-posta">{lead.email ?? "-"}</Detail>
            {isSell && <SellDetails lead={lead} />}
            {!isSell && (
              <>
                <Detail label="Araç">
                  {lead.vehicleListingNo ? (
                    <a href={publicSiteUrl(`/arac/${lead.vehicleListingNo}`)} target="_blank" rel="noreferrer" className="underline">
                      {lead.vehicleTitle} ({lead.vehicleListingNo})
                    </a>
                  ) : (
                    "-"
                  )}
                </Detail>
                <Detail label="Tercih edilen tarih">{formatDate(lead.preferredDate)}</Detail>
              </>
            )}
            <div className="col-span-2">
              <Detail label={isSell ? "Hasar ve ek bilgiler" : "Mesaj"}>
                <span className="whitespace-pre-line">{lead.message ?? "-"}</span>
              </Detail>
            </div>
            {isSell && (
              <div className="col-span-2">
                <LeadPhotos lead={lead} />
              </div>
            )}
            <div className="col-span-2">
              <Detail label="KVKK onayı">{formatDateTime(lead.kvkkConsentAt)}</Detail>
            </div>
          </div>
        )}
        <form id="lead-form" autoComplete="off" noValidate onSubmit={form.handleSubmit(submit)}>
          <FieldGroup>
            <Controller control={form.control} name="status" render={({ field }) => (
              <SelectField id="lead-status" label="Durum" options={lookups?.leadStatus ?? []}
                description="Arandı, teklif verildi ya da kapandı olarak işaretleyin."
                value={field.value} onChange={(v) => field.onChange(v ?? 0)} />
            )} />
            <TextareaField id="lead-note" label="İç not" rows={4} maxLength={2000}
              description="Yalnız panelde görünür." error={form.formState.errors.internalNote?.message}
              {...form.register("internalNote")} />
          </FieldGroup>
        </form>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Kapat
          </Button>
          <Button type="submit" form="lead-form" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
