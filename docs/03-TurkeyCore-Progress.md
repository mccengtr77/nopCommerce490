# 03 — TurkeyCore İlerleme Raporu

Bu dosya `Nop.Plugin.Misc.TurkeyCore` plugin'inin **fiili durumunu** belgeler. Spec için `01-TurkeyCore-Prompt.md` dosyasına bak; bu dosya neyin **gerçekten yapıldığını** anlatır.

**Son güncelleme**: 2026-05-04
**Durum**: ✅ Uçtan uca canlı (macOS + VM SQL Server 2019, plugin v1.0.11) — MVP altyapısı + döviz bazlı ürün fiyatı uçtan uca tamam
**Kod metriği**: ~3.700 satır plugin kodu + ~1.400 satır test (**233 test**, %100 geçer)

### Deployment Düzeltmeleri (2026-05-03)

İlk install çalışmasında iki hata tespit edildi ve düzeltildi:

1. **Multi-cascade FK** (Error 1785) — `TurkishAddressExtensionBuilder`'da `ProvinceId`/`DistrictId`/`NeighborhoodId` `.ForeignKey<T>()` yerine `.Indexed()` ile tanımlandı (bkz. [PROJECT_DECISIONS.md Karar 21](PROJECT_DECISIONS.md))
2. **Eksik enum kolonu** (`MusteriTipi`) — `TurkishCustomerExtensionBuilder`'a explicit `.AsInt32().NotNullable()` eklendi (bkz. [PROJECT_DECISIONS.md Karar 22](PROJECT_DECISIONS.md))

Düzeltme sonrası: 7 tablo + 81 il seed sorunsuz oluşturuluyor.

---

## Yapılanlar (Faz 1A — Backend Altyapı)

### Plugin İskeleti
- `plugin.json` (SystemName: `Misc.TurkeyCore`, v1.0.0, nop 4.90 hedefli)
- `TurkeyCorePlugin.cs` — `BasePlugin` + `IMiscPlugin`, install/uninstall lifecycle
- `TurkeyCoreDefaults.cs` — sabitler ve `CacheKey` instance'ları
- `TurkeyCoreSettings.cs` — `ISettings` (TCMB, GİB, validasyon zorunluluk, KDV)
- `Infrastructure/NopStartup.cs` — DI registration
- `Infrastructure/RouteProvider.cs` — admin Configure named route

### Domain & Migration
- 7 entity: `TurkishProvince`, `TurkishDistrict`, `TurkishNeighborhood`, `TurkishTaxOffice`, `TurkishCustomerExtension`, `TurkishAddressExtension`, `ExchangeRateLog`
- `TurkishCustomerType` enum (Individual / Corporate)
- `Data/Mapping/Builders/` — 7 `NopEntityBuilder<T>` (FK constraint'ler, kolon uzunlukları, `decimal(18,6)`)
- `Data/Migrations/SchemaMigration.cs` — tablolar
- `Data/Migrations/SeedProvincesMigration.cs` — **81 il** (plaka + alfabetik DisplayOrder)

### Servisler (`Services/`)
| Servis | Yetenek | Cache | Test |
|---|---|---|---|
| `ITurkishValidationService` | TCKN, VKN, IBAN-TR, GSM, Türkçe karakter normalize | — | 95 test |
| `ITurkishLocationService` | İl/ilçe/mahalle/posta kodu sorgu | 24h (default) | 17 test |
| `ITurkishTaxOfficeService` | Vergi dairesi CRUD + GİB kodu lookup | 24h (default) | 15 test |
| `ITcmbExchangeRateService` | TCMB feed parse, rate lookup, çapraz kur, geçmiş sorgu | 1h | 23 test |
| `ExchangeRateBackgroundTask` | `IScheduleTask` — 24h aralık (disabled by default) | — | — |

**Validasyon algoritmaları (resmi kaynaklara göre)**:
- TCKN: NVI 11-hane checksum + tüm-aynı reddi
- VKN: GİB 10-hane pozisyonel `tmp×2^(9-i) mod 9`
- IBAN-TR: ISO 13616 / MOD-97-10 (`BigInteger`)
- GSM: BTK numaralandırma planı (501-561 prefix listesi), `+90 5XX XXX XX XX` normalize

### Frontend Bileşenleri
- **AJAX Controller** (`Controllers/TurkeyCoreApiController.cs`):
  - `GET /Plugins/TurkeyCore/api/provinces`
  - `GET /Plugins/TurkeyCore/api/districts/{provinceId}`
  - `GET /Plugins/TurkeyCore/api/neighborhoods/{districtId}`
  - `POST /Plugins/TurkeyCore/api/validate-tckn`
  - `POST /Plugins/TurkeyCore/api/validate-vkn`
- **Admin Configure** (`Areas/Admin/`):
  - 4 kart: Veri Durumu, Genel Ayarlar, TCMB, GİB
  - "Şimdi Güncelle" butonu (TCMB feed elle çekme)
  - Son 30 kuru tablo
  - `[AuthorizeAdmin] + [Area(AreaNames.ADMIN)] + [AutoValidateAntiforgeryToken]`
- **Storefront ViewComponent'leri** (`Components/`):
  - `ProvinceDistrictSelector` — server-side il + AJAX cascading (vanilla JS, jQuery dependency yok)
  - `CustomerTypeSelector` — bireysel/kurumsal radio

### Lokalizasyon
- `Localization/LocaleResources.cs` — Türkçe (~80 string) + İngilizce kritik string'ler
- nopCommerce konvansiyonu (`AddOrUpdateLocaleResourceAsync`) — `tr-TR.xml` ayrı dosya değil

### Test Altyapısı
- `Tests/Nop.Plugin.Misc.TurkeyCore.Tests/` — NUnit + Moq + FluentAssertions
- `AssemblySetup.cs` — `Singleton<AppSettings>` init (CacheKey ctor'unun NRE atmasını önler)
- 233 test, hepsi geçer

---

## Yapılanlar (Faz 1B — Ürün Döviz Bazlı Fiyat, v1.0.11 — 2026-05-04)

İthalatçı/elektronik bayi senaryosu için "ürün başına farklı döviz" desteği. Tasarım kararı için bkz. [PROJECT_DECISIONS.md Karar 23–25](PROJECT_DECISIONS.md).

### Domain & Migration (2 yeni entity)
- `TurkishProductExtension` — `ProductId` FK + `BaseCurrencyId` (soft FK to Currency) + `BasePrice` + `BaseOldPrice?` + `BaseProductCost?`
- `TurkishCartItemPriceLock` — `ShoppingCartItemId` FK + `LockedUnitPrice` + `LockedRate` + `BaseCurrencyCode` + `LockedAtUtc`
- 2 yeni mapping builder + `SchemaMigration` güncellemesi + 2 incremental migration (idempotent)

### Servisler

| Servis | Yetenek |
|---|---|
| `ITurkishProductExtensionService` | CRUD + cache + `RecalculateAndPersistAsync` (tek ürün) + `RecalculateAllAsync` (toplu, scheduled task tarafından) + `ConvertBasePriceToTryAsync` (admin display) + `GetForeignBasePriceAsync` (storefront badge) |
| `ITurkishCartItemPriceLockService` | Cart item snapshot CRUD |

**Persisted recalc** yaklaşımı (Karar 23): `Product.Price/OldPrice/ProductCost` admin Save anında veya TCMB scheduled task'ta DB'ye yazılır. Search/filter/discount/marketplace tek doğru fiyatı görür. Plugin **PrimaryStoreCurrency = TRY** varsayar.

### Event Consumer'lar (3 yeni)

- **`ProductSavedConsumer`** (`EntityInsertedEvent<Product>` + `EntityUpdatedEvent<Product>`) — admin form'undan `TurkishProductExtension.X` alanlarını oku → upsert + recalc. `HttpContext.Items` flag ile loop koruması (recalc kendi event'ini tetikler).
- **`CartItemPriceLockConsumer`** (`EntityInsertedEvent<ShoppingCartItem>` + `EntityDeletedEvent<ShoppingCartItem>` + `GetShoppingCartItemUnitPriceEvent`) — sepete eklenince TL kur lock'lanır, `StopProcessing=true` ile lock'lı fiyat döner.
- **`WidgetSettingsRepairConsumer`** (`AppStartedEvent`) — defensive self-healing, `WidgetSettings.ActiveWidgetSystemNames`'da systemName yoksa otomatik ekler (Karar 25).

### TCMB Scheduled Task — Recalc Hook
`ExchangeRateBackgroundTask.ExecuteAsync` TCMB feed çektikten sonra `RecalculateAllAsync` çağırır → tüm extension'lı ürünler yeni kurla persistlenir.

### Admin UI (Widget Zone Pattern, Form-Integrated Save — Karar 24)

- `TurkishProductExtensionAdminViewComponent` — `AdminWidgetZones.ProductDetailsBlock` zone'una hook
- `Default.cshtml` — currency dropdown ("pasif" yok, default = primary store currency) + 3 fiyat input + canlı TRY ön gösterimi (`≈ X ₺`)
- JavaScript ile panel "Fiyatlar" kartının (`#product-price`) hemen altına taşınır + standart `Price/OldPrice/ProductCost` alanları gizlenir (admin tek "Kaydet" butonu yeterli — ayrı save endpoint yok)
- Yeni ürün create sayfasında da panel görünür; form post sonrası `EntityInsertedEvent` consumer extension'ı insert eder

### Storefront UI (Pavilion Tema Uyumlu)

- `ProductBaseCurrencyBadgeViewComponent` — iki public widget zone'a hook:
  - `PublicWidgetZones.ProductPriceBottom` — ürün detay sayfasında fiyatın altı (Pavilion `ProductTemplate.Simple.cshtml`)
  - `PublicWidgetZones.ProductBoxAddinfoMiddle` — kategori/listeleme product box'ı (Pavilion `_ProductBox.cshtml`)
- Badge görünür: ürünün `BaseCurrencyId != PrimaryStoreCurrencyId` (yani TRY dışı) ise `🌐 ≈ $300.00` pill-shaped, gri arka plan, tooltip "TCMB güncel kuruyla TRY'ye çevrilmiştir"
- Primary currency ile aynıysa hiç gösterilmez (kullanıcı zaten o fiyatı görüyor)

### IWidgetPlugin Entegrasyonu
`TurkeyCorePlugin` `IMiscPlugin` + `IWidgetPlugin` ikisini de implement eder. `GetWidgetZonesAsync` 3 zone döndürür: 1 admin (`ProductDetailsBlock`) + 2 public (`ProductPriceBottom`, `ProductBoxAddinfoMiddle`). `HideInWidgetList = true` (admin widget listesinde gizli, ana plugin listesinde görünür).

### Test (yeni 27 test)
`TurkishProductExtensionServiceTests` — 23 test (CRUD, recalc happy path + edge case'ler, RecalculateAllAsync hata izolasyonu, ConvertBasePriceToTryAsync). Plus 4 dolaylı test güncellemesi.

### Production Doğrulaması (2026-05-04)
- Test ürünü USD seçildi, BasePrice 300 → recalc çalıştı, `Product.Price = 13.515,06 TL` (1 USD = ~45 TL) DB'ye yazıldı
- Storefront `/test` sayfasında: `₺13.515,06` altında `≈ $300.00` badge görünür
- PrimaryStoreCurrency TRY'a alındı (Currencies sayfasından "Türk Lirası → Birincil mağaza para birimi olarak işaretle")
- WidgetSettings'te `Misc.TurkeyCore` ilk update'te eklenmedi (sebep belirsiz, sessiz fail) → SQL ile manuel eklendi → sonradan `WidgetSettingsRepairConsumer` ile self-healing yapıldı (gelecekteki kurulumlarda gerekmeyecek)

---

## Yapılmayanlar — Bilinçli Bırakılan Eksikler

### Veri Eksikleri (admin'in elle yüklemesi/import etmesi gereken)
- ❌ ~970 ilçe seed
- ❌ ~50.000 mahalle seed (PTT veri tabanı CSV'den)
- ❌ ~1.000 vergi dairesi seed (GİB listesi)

> Admin Configure sayfası "Veri Durumu" kartında bu eksiklikleri "Boş" badge'iyle gösterir.

### Servis Eksikleri
- ❌ `IGibMukellefService` — e-fatura mükellef sorgu (mock'lu başlanacak, gerçek entegratör Faz 2'de)
- ❌ `ITurkishCurrencyFormatter` — `1.234.567,89 ₺` formatı için
- ❌ `TurkishTaxProvider` (`ITaxProvider` impl) — kategori bazlı KDV
- ❌ Customer/Address extension için repository helper'lar (`GetTurkishExtensionAsync` extension method'u)

### Validator Eksikleri
- ❌ `TurkishCustomerValidator` (FluentValidation, kayıt formu)
- ❌ `TurkishAddressValidator` (checkout)

### Event Consumer Eksikleri
- ❌ `CustomerRegisteredConsumer` — kayıt sonrası TCKN/VKN doğrulama, GİB sorgu (background)
- ❌ `OrderPlacedConsumer` — fatura tipi (e-fatura/e-arşiv) belirleme

### UI Bileşen Eksikleri
- ❌ `KepAddressInput` ViewComponent — basit input olarak entegre edilebilir, gerek görülmedi
- ❌ `TcKimlikNoInput` ViewComponent — real-time validation için ayrı input
- ❌ Admin'de TaxOffice CRUD sayfası, Province/District read-only listing
- ❌ Admin'de TCMB kur geçmişi sayfası

### Dağıtım
- ❌ `Nop.Plugin.Misc.TurkeyCore.zip` build artifact üretim scripti
- ❌ README.md, CHANGELOG.md
- ❌ Plugin gerçek nopCommerce instance'ında (Windows VM) install/uninstall testi

---

## Önemli Tasarım Kararları

### TCKN/VKN Saklama Stratejisi
**Karar**: Şifreli halde `string(256)` kolonda saklanır (`IEncryptionService` ile).
**Gerekçe**: 6698 KVKK kapsamında PII; düz metin saklanırsa veri sızıntısı vebal yaratır.
**Uygulama notu**: Hassas alanlar admin paneli dışında sadece son 4 hane gösterilmeli (Faz 1B'de eklenecek).

### Cache Stratejisi
**Karar**: `IRepository.GetAllAsync` callback'ine `CacheKey` veriliyor — repository seviyesinde cache.
**Gerekçe**: Lokasyon/vergi dairesi gibi sabit veri için en az kod, en çok hit. nopCommerce'in kendi cache invalidation'ı çalışır.
**Test'te ortaya çıkan sorun**: `CacheKey` ctor'u `Singleton<AppSettings>` üzerinden `CacheConfig.DefaultCacheTime` okuyor — test ortamında AppSettings init değil → NRE. Çözüm: `AssemblySetup.cs` ile `[SetUpFixture]` `OneTimeSetUp`.

### Customer/Address Extension Pattern
**Karar**: nopCommerce'in `Customer` ve `Address` entity'lerini değiştirmiyoruz — paralel `TurkishCustomerExtension` ve `TurkishAddressExtension` tabloları.
**Gerekçe**: Upstream merge'leri zorlaştırmamak. CLAUDE.md FAQ'ında bu kural var.

### TCMB XML Parser Saf Statik Fonksiyon
**Karar**: `TcmbExchangeRateService.ParseFeedXml(string)` — instance gerektirmez.
**Gerekçe**: Test edilebilirlik (HttpClient mock'lamadan parser'ı doğrudan çağırabiliyoruz).

### Lokalizasyon: tr-TR.xml Yerine Static Class
**Karar**: nopCommerce konvansiyonu olan `AddOrUpdateLocaleResourceAsync(Dictionary)` kullanılıyor; tr-TR.xml dosyası yok.
**Gerekçe**: nopCommerce'in plugin lokalizasyon yöntemi bu. XML dosyaları sadece "language pack import" feature'ı içindir, plugin dağıtımında değil. Static `LocaleResources` class'ı düzenlemeyi kolaylaştırıyor.

### IBAN için BigInteger
**Karar**: 26 karakterlik IBAN'ın MOD-97 hesabı için `System.Numerics.BigInteger`.
**Gerekçe**: 26 hane × 2 (harf→2 hane) = ~50 basamaklı sayı, `long` taşar.

---

## Sıradaki Adımlar (Öncelik Sırası)

### Yakın Hedef — Faz 1B
1. **Veri import aracı** — admin tarafında CSV upload → ilçe/mahalle/vergi dairesi seed (en kritik eksik, storefront cascading bu olmadan boş)
2. **GİB mükellef sorgu** (mock impl) — `IGibMukellefService` interface + dummy "her zaman true" implementation
3. **FluentValidation validator'ları** — `TurkishCustomerValidator`, `TurkishAddressValidator`
4. **Event consumer'lar** — `CustomerRegisteredConsumer`, `OrderPlacedConsumer`
5. **Currency formatter** — `1.234.567,89 ₺`
6. **Customer extension helper method'ları** — `customer.GetTurkishExtensionAsync()` vb.

### Dağıtım (Faz 1A kapanışı için)
1. Windows VM'de gerçek nopCommerce'a install/uninstall test
2. README.md (kurulum, kullanım, FAQ)
3. CHANGELOG.md (v1.0.0 release notes)
4. Build script: `dotnet publish` → zip paketleme (bin/obj hariç)
5. Git tag: `turkey-core-v1.0-mvp`

### Sonra (Faz 1C — TurkishConsumerLaw'a geçiş)
- `Nop.Plugin.Misc.TurkishConsumerLaw` başlatma — bağımlılığı `TurkeyCore`
- `docs/02-TurkishConsumerLaw-Prompt.md` spec'i takip

---

## Build ve Test Çalıştırma Notları

```bash
# Build (macOS, OutputPath override gerekli)
dotnet build src/Plugins/Nop.Plugin.Misc.TurkeyCore/Nop.Plugin.Misc.TurkeyCore.csproj \
    -p:OutDir=/tmp/turkeycore-build/ \
    -p:SolutionDir=/Users/mehmet/Projects/NopCommerce/nopCommerce490/src/

# Test
dotnet test src/Tests/Nop.Plugin.Misc.TurkeyCore.Tests/Nop.Plugin.Misc.TurkeyCore.Tests.csproj \
    -p:SolutionDir=/Users/mehmet/Projects/NopCommerce/nopCommerce490/src/
```

> **Not**: macOS'ta `$(SolutionDir)` makrosu csproj'da boş çözümlendiği için `OutputPath` `/Presentation/...` (kök filesystem) yazmaya çalışıyor. `-p:SolutionDir=...` parametresi şart. Windows VM'de Visual Studio default davranışıyla bu sorun yok.
