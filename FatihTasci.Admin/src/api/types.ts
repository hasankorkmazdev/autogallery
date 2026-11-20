// Sunucu DTO'larının TypeScript karşılıkları (FatihTasci.Core/Models/Domain).
// Enum'lar sayısal değerleriyle gelir; etiketler GET /api/lookups'tan okunur.

export interface ServiceResult<T> {
  code: number;
  message: string;
  data: T;
}

export interface PagedList<T> {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: T[];
}

export interface EnumOption {
  value: number;
  label: string;
}

export type Lookups = Record<
  | "vehicleStatus"
  | "fuelType"
  | "transmission"
  | "bodyType"
  | "drivetrain"
  | "condition"
  | "plateOrigin"
  | "bodyPart"
  | "bodyPartState"
  | "featureCategory"
  | "leadType"
  | "leadStatus"
  | "sellDamage"
  | "userRole"
  | "socialPlatform",
  EnumOption[]
>;

export const VehicleStatus = {
  Draft: 0,
  Published: 1,
  Reserved: 2,
  Sold: 3,
  Archived: 4,
} as const;
export type VehicleStatusValue = (typeof VehicleStatus)[keyof typeof VehicleStatus];

export const FuelType = {
  Gasoline: 1,
  Diesel: 2,
  LpgGasoline: 3,
  Hybrid: 4,
  PlugInHybrid: 5,
  Electric: 6,
} as const;

export const VehicleCondition = {
  Used: 1,
  New: 2,
} as const;

export const BodyPartState = {
  Original: 0,
  LocalPainted: 1,
  Painted: 2,
  Replaced: 3,
} as const;

export interface User {
  id: string;
  name: string;
  surname: string;
  eMail: string;
  role: number;
}

// Sunucudaki UserRole enum'ı (yetkiler: CLAUDE.md "Kararlar")
export const UserRole = {
  Admin: 1,
  Staff: 2,
} as const;

export interface UserListItem {
  id: string;
  name: string;
  surname: string;
  eMail: string;
  /** Sitede danışmanı olduğu ilanlarda görünen iletişim telefonu */
  phone: string | null;
  role: number;
  isActive: boolean;
  lastLoginDate: string | null;
  createDate: string;
}

/** Araca atanabilecek danışman (tüm kullanıcılar, pasifler dahil) */
export interface Advisor {
  id: string;
  fullName: string;
  phone: string | null;
  isActive: boolean;
}

export interface SaveUserRequest {
  name: string;
  surname: string;
  eMail: string;
  phone: string | null;
  role: number;
  isActive: boolean;
  /** Yalnız yeni kullanıcıda gönderilir */
  password?: string;
}

/** Token yanıtta dönmez, sunucu HttpOnly çereze yazar. */
export interface LoginResponse {
  user: User;
  expiresAt: string;
}

// --- Katalog

export interface Brand {
  id: number;
  name: string;
  slug: string;
  logoUrl: string | null;
  sortOrder: number;
  isActive: boolean;
  seriesCount: number;
}

export interface Series {
  id: number;
  brandId: number;
  name: string;
  slug: string;
  sortOrder: number;
  isActive: boolean;
  modelCount: number;
}

export interface CarModel {
  id: number;
  seriesId: number;
  name: string;
  slug: string;
  isActive: boolean;
}

export interface Color {
  id: number;
  name: string;
  hexCode: string;
}

export interface Feature {
  id: number;
  category: number;
  name: string;
  sortOrder: number;
  isActive: boolean;
}

export interface FeatureGroup {
  category: number;
  categoryName: string;
  features: Feature[];
}

// --- Araç

export interface VehicleListItem {
  id: string;
  listingNo: number;
  title: string;
  brandName: string;
  seriesName: string;
  modelName: string;
  year: number | null;
  kilometer: number | null;
  price: number | null;
  campaignPrice: number | null;
  showPrice: boolean;
  status: VehicleStatusValue;
  coverUrl: string | null;
  photoCount: number;
  isFeatured: boolean;
  isOpportunity: boolean;
  /** Danışmanın adı soyadı; atanmamışsa null */
  advisorName: string | null;
  publicUrl: string;
  createDate: string;
  lastUpdateDate: string | null;
}

export interface VehicleMedia {
  id: string;
  smallUrl: string;
  mediumUrl: string;
  largeUrl: string;
  width: number;
  height: number;
  sortOrder: number;
  isCover: boolean;
}

export interface BodyPartItem {
  part: number;
  state: number;
  note: string | null;
}

export interface VehicleDetail {
  id: string;
  listingNo: number;
  slug: string;
  status: VehicleStatusValue;
  publicUrl: string;
  brandId: number;
  seriesId: number;
  carModelId: number;
  brandName: string;
  seriesName: string;
  modelName: string;
  title: string;
  year: number | null;
  kilometer: number | null;
  fuelType: number | null;
  transmission: number | null;
  bodyType: number | null;
  engineCc: number | null;
  powerHp: number | null;
  drivetrain: number | null;
  batteryKwh: number | null;
  rangeKm: number | null;
  exteriorColorId: number | null;
  interiorColor: string | null;
  condition: number;
  hasWarranty: boolean;
  warrantyEndDate: string | null;
  heavyDamageRecord: boolean;
  plateOrigin: number;
  acceptsTrade: boolean;
  tramerAmount: number | null;
  bodyParts: BodyPartItem[];
  featureIds: number[];
  media: VehicleMedia[];
  /** YouTube bağlantısı */
  videoUrl: string | null;
  /** Yüklenen video (MP4); yoksa null */
  videoFile: VideoFile | null;
  expertiseReportUrl: string | null;
  price: number | null;
  campaignPrice: number | null;
  showPrice: boolean;
  description: string | null;
  advisorId: string | null;
  isFeatured: boolean;
  featuredOrder: number;
  isOpportunity: boolean;
  viewCount: number;
  publishedAt: string | null;
  soldAt: string | null;
}

export interface PublishCheckItem {
  key: string;
  label: string;
  passed: boolean;
  blocking: boolean;
  step: number;
}

export interface PublishCheck {
  canPublish: boolean;
  items: PublishCheckItem[];
}

// --- Talepler

export interface Lead {
  id: string;
  type: number;
  status: number;
  fullName: string;
  phone: string;
  email: string | null;
  message: string | null;
  preferredDate: string | null;
  internalNote: string | null;
  vehicleId: string | null;
  vehicleListingNo: number | null;
  vehicleTitle: string | null;
  // Yalnız "Aracını Sat / Takas" talebinde (vitrindeki fotoğraflı teklif formu)
  sellBrand: string | null;
  sellModel: string | null;
  sellPackage: string | null;
  sellYear: number | null;
  sellKm: number | null;
  sellExpectedPrice: number | null;
  sellDamage: number | null;
  media: LeadMedia[];
  kvkkConsentAt: string;
  createDate: string;
}

/**
 * Talep fotoğrafı. url oturum çereziyle erişilen API ucudur (/api/leads/{id}/media/{mediaId}).
 */
export interface LeadMedia {
  id: string;
  url: string;
}

// --- Showroom (ana sayfa "Galeri Atmosferi")

export interface ShowroomScene {
  id: number;
  tag: string;
  title: string;
  sortOrder: number;
  isActive: boolean;
  mediumUrl: string | null;
  largeUrl: string | null;
}

export interface SaveShowroomScene {
  tag: string;
  title: string;
  isActive: boolean;
}

// --- Teslimatlar

export interface Delivery {
  id: number;
  vehicleTitle: string;
  customerName: string | null;
  city: string | null;
  /** Teslim tarihi (gün; saat 00:00) */
  deliveredAt: string;
  isActive: boolean;
  /** 800 px WebP (kart) */
  mediumUrl: string | null;
  /** 1600 px WebP (tam ekran) */
  largeUrl: string | null;
  /** İsteğe bağlı video; sitede tam ekranda fotoğrafın yerine oynar */
  video: VideoFile | null;
}

export interface SaveDelivery {
  vehicleTitle: string;
  customerName: string;
  city: string;
  /** yyyy-MM-dd */
  deliveredAt: string;
  isActive: boolean;
  /** Düzenlemede mevcut videoyu kaldırır (yeni video gönderilmediyse) */
  removeVideo: boolean;
}

/** Yeni seçilen video ve tarayıcının ondan aldığı ilk kare (afiş). */
export interface VideoUploadFiles {
  video: File;
  poster: Blob;
}

// --- Video (araç, teslimat)

/**
 * Panelden yüklenen video: MP4 olduğu gibi, afiş videonun ilk karesi (WebP). width / height afişin boyutudur
 * (oranı videoyla aynı).
 */
export interface VideoFile {
  url: string;
  posterUrl: string;
  /** Küçük afiş (araç galerisinin küçük resmi); teslimatta null */
  posterSmallUrl: string | null;
  width: number;
  height: number;
}

// --- Ayarlar

export interface SiteSettings {
  galleryName: string | null;
  /** Salt okunur: logo /settings/logo uçlarıyla değişir */
  logoPath?: string | null;
  logoUrl?: string | null;
  watermarkEnabled: boolean;
  phone: string | null;
  whatsApp: string | null;
  email: string | null;
  address: string | null;
  latitude: string | null;
  longitude: string | null;
  workingHours: string | null;
  /** Sıralı; vitrinde alt bilgi ve iletişim sayfasında gösterilir */
  socialLinks: SocialLink[];
}

export interface SocialLink {
  /** SocialPlatform enum değeri; adları lookups.socialPlatform'da */
  platform: number;
  url: string;
}

// --- Site İçeriği (vitrin yazıları; yalnız Yönetici)

/** SiteServiceType enum'u (sunucuyla aynı); sıra vitrindeki sıradır */
export const SiteServiceType = {
  BuyVehicle: 1,
  Consignment: 2,
  SellVehicle: 3,
} as const;
export type SiteServiceTypeValue = (typeof SiteServiceType)[keyof typeof SiteServiceType];

export interface ServiceContent {
  type: SiteServiceTypeValue;
  /** Salt okunur: hizmetin adı (enum Display) */
  title?: string;
  /** Salt okunur: vitrindeki adresi, /hizmetler/{slug} */
  path?: string;
  summary: string;
  body: string;
}

export interface StatItem {
  value: string;
  label: string;
}

export interface SiteContent {
  services: ServiceContent[];
  corporateIntro: string;
  corporateAbout: string;
  corporateVision: string;
  corporateMission: string;
  /** "Sayılarla Biz" rakamları, sıralı */
  stats: StatItem[];
  /** Salt okunur: ana sayfa videosu /content/hero-video uçlarıyla değişir */
  heroVideo?: HeroVideo;
}

export interface HeroVideo {
  /** Masaüstü videosu */
  videoHdUrl: string;
  /** Telefon videosu; yüklenen videoda masaüstüyle aynı dosya */
  videoSdUrl: string;
  /** Afiş (videodan önce görünen ilk kare), 1920 px */
  posterUrl: string;
  posterSmallUrl: string;
  /** false: varsayılan drone videosu */
  isCustom: boolean;
}

export interface WatermarkJobStatus {
  isRunning: boolean;
  total: number;
  processed: number;
  failed: number;
  startedAt: string | null;
  finishedAt: string | null;
}
