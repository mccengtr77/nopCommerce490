# Claude Code Prompt: Nop.Plugin.Misc.TurkeyCore

## Görev Tanımı

nopCommerce 4.90 için Türkiye lokalizasyonunun **çekirdek temelini** oluşturan bir plugin geliştir. Bu plugin, diğer tüm Türkiye-spesifik eklentilerin (ödeme, kargo, e-fatura, tüketici hukuku) bağımlı olacağı **foundation** paketidir. Tek başına bir e-ticaret işlevi sunmaz; altyapı sağlar.

## Bağlam

- **Hedef Platform**: nopCommerce 4.90.4+
- **Teknoloji**: ASP.NET Core 9, .NET 9 SDK, LinqToDB, Entity Framework Core
- **Veritabanı**: SQL Server 2019+
- **IDE**: Visual Studio 2022 (17.12+) veya JetBrains Rider
- **Pazar**: Türkiye e-ticaret sektörü
- **Yasal Çerçeve**: 6502 Tüketici Kanunu, 6698 KVKK, 6563 E-Ticaret Kanunu, VUK 509/535 nolu Tebliğ

## Plugin Yapısı

Standart nopCommerce plugin mimarisini takip et. `BasePlugin` sınıfından miras alan ana plugin sınıfı, `plugin.json` tanımlayıcı dosya, `IConsumer<T>` event consumer'ları, `Startup.cs` ile DI registration.

```
Nop.Plugin.Misc.TurkeyCore/
├── plugin.json
├── TurkeyCorePlugin.cs                    # BasePlugin, IMiscPlugin
├── TurkeyCoreDefaults.cs                  # Sabitler (system name, route names, vb.)
├── Infrastructure/
│   ├── DependencyRegistrar.cs             # IDependencyRegistrar
│   ├── NopStartup.cs                      # Servisleri DI'a kaydet
│   └── RouteProvider.cs                   # IRouteProvider (admin sayfaları için)
├── Domain/
│   ├── Extensions/
│   │   ├── CustomerTurkeyExtension.cs     # Customer için TR alanları
│   │   ├── AddressTurkeyExtension.cs      # Address için il/ilçe/mahalle
│   │   └── OrderTurkeyExtension.cs        # Order için fatura tipi vb.
│   ├── TurkishProvince.cs                 # İl
│   ├── TurkishDistrict.cs                 # İlçe
│   ├── TurkishNeighborhood.cs             # Mahalle
│   ├── TurkishTaxOffice.cs                # Vergi dairesi
│   ├── TurkishCustomerType.cs             # Bireysel/Kurumsal enum
│   └── ExchangeRateLog.cs                 # TCMB kur geçmişi
├── Data/
│   ├── Mapping/                           # FluentMigrator/LinqToDB mapping
│   │   ├── TurkishProvinceMap.cs
│   │   ├── TurkishDistrictMap.cs
│   │   ├── TurkishNeighborhoodMap.cs
│   │   └── TurkishTaxOfficeMap.cs
│   └── Migrations/
│       ├── 20260101_InitialSchema.cs       # Tablolar oluştur
│       ├── 20260102_SeedProvinces.cs       # 81 il
│       ├── 20260103_SeedTaxOffices.cs      # ~1000 vergi dairesi
│       └── 20260104_ExtendCustomerSchema.cs
├── Services/
│   ├── Validation/
│   │   ├── ITurkishValidationService.cs
│   │   ├── TurkishValidationService.cs    # TCKN, VKN, IBAN, GSM
│   │   └── TurkishValidationDefaults.cs
│   ├── Location/
│   │   ├── ITurkishLocationService.cs
│   │   ├── TurkishLocationService.cs       # İl/ilçe/mahalle sorgu
│   │   └── PostalCodeLookupService.cs
│   ├── TaxOffice/
│   │   ├── ITurkishTaxOfficeService.cs
│   │   └── TurkishTaxOfficeService.cs
│   ├── CustomerType/
│   │   ├── ICustomerTypeService.cs
│   │   └── CustomerTypeService.cs          # Bireysel/Kurumsal yönetimi
│   ├── ExchangeRate/
│   │   ├── ITcmbExchangeRateService.cs
│   │   ├── TcmbExchangeRateService.cs      # TCMB XML feed
│   │   └── ExchangeRateBackgroundTask.cs
│   ├── GibLookup/
│   │   ├── IGibMukellefService.cs
│   │   ├── GibMukellefService.cs           # E-fatura mükellef sorgu
│   │   └── GibMukellefCache.cs
│   ├── Localization/
│   │   ├── ITurkishCurrencyFormatter.cs
│   │   └── TurkishCurrencyFormatter.cs
│   └── Tax/
│       ├── TurkishTaxProvider.cs           # ITaxRateProvider impl
│       └── KdvOranlari.cs                  # %0, %1, %10, %20
├── Areas/Admin/
│   ├── Controllers/
│   │   ├── TurkeyCoreSettingsController.cs
│   │   ├── ProvinceController.cs
│   │   ├── DistrictController.cs
│   │   ├── NeighborhoodController.cs
│   │   └── TaxOfficeController.cs
│   ├── Models/                              # ViewModel'ler
│   ├── Validators/                          # FluentValidation
│   ├── Factories/                           # Model factory pattern
│   └── Views/
├── Components/                              # ViewComponents (storefront)
│   ├── ProvinceDistrictSelector/
│   ├── CustomerTypeSelector/
│   └── KepAddressInput/
├── Localization/
│   ├── tr-TR.xml                            # Türkçe çeviri
│   └── en-US.xml                            # İngilizce çeviri
├── Events/
│   ├── CustomerRegisteredConsumer.cs        # Kayıt sonrası TR doğrulama
│   └── OrderPlacedConsumer.cs                # Sipariş sonrası işlem
└── Validators/
    ├── TurkishCustomerValidator.cs
    └── TurkishAddressValidator.cs
```

## Detaylı Geliştirme Görevleri

### 1. Validasyon Servisleri (`ITurkishValidationService`)

#### TC Kimlik No Algoritması

```
- 11 haneli sayı olmalı
- İlk hane 0 olamaz
- 10. hane: ((1+3+5+7+9. hanelerin toplamı × 7) - (2+4+6+8. hanelerin toplamı)) mod 10
- 11. hane: İlk 10 hanenin toplamının birler basamağı
- 11. hane çift olmalı
- Aynı rakamdan oluşan numaralar (11111111110) reddedilmeli
- Karakter kontrolü: sadece rakam
```

Implement et:
```csharp
public interface ITurkishValidationService
{
    bool ValidateTcKimlikNo(string tckn);
    bool ValidateVergiNo(string vkn);
    bool ValidateIbanTr(string iban);
    bool ValidateGsmNumber(string gsm);
    string NormalizeGsmNumber(string gsm);  // +90 5XX XXX XX XX formatına çevir
    string NormalizeTurkishCharacters(string input);  // İ→I, ğ→g, vb.
}
```

#### Vergi No Algoritması (10 haneli)

GİB tarafından belirlenen algoritma:
- 10 haneli sayı
- Her bir hane için pozisyonel ağırlık hesapla
- Son hane checksum

Internet'te resmi algoritma araştır ve doğrula. Test case'leri ekle.

#### IBAN TR Validasyonu

- TR + 24 rakam = 26 karakter
- Boşluksuz format (kullanıcı boşluk girebilir, normalize et)
- MOD-97 algoritması: ülke kodunu sona taşı, harfleri sayıya çevir (T=29, R=27), sonucun 97'ye bölümünden kalan 1 olmalı

#### GSM Numara Validasyonu

Türkiye operatör prefix listesi (Turkcell, Vodafone, Türk Telekom):
- 530-539, 540-549, 550-559 (geleneksel Turkcell)
- 542, 543, 544, 545, 546, 547, 548 (Vodafone'a geçen seriler)
- vb. — operatör listelerinden güncel prefix'leri al

### 2. Veri Setleri ve Seed Migration'lar

#### İl/İlçe/Mahalle/Posta Kodu

PTT'nin resmi posta kodu veri tabanından (CSV) alınmış SQL seed dosyaları oluştur:

```sql
-- Tablolar:
TurkishProvince        (Id, Name, PlateCode)              -- 81 kayıt
TurkishDistrict        (Id, ProvinceId, Name)             -- ~970 kayıt
TurkishNeighborhood    (Id, DistrictId, Name, PostalCode) -- ~50.000+ kayıt
```

Migration dosyalarında SQL `INSERT` statement'ları kullan. CSV'den seed üretmek için `tools/SeedGenerator/` adında küçük bir konsol uygulaması ekle.

**Önemli**: Bu kadar büyük seed migration'ı tek seferde uygulamak yavaş olabilir. Batch insert kullan (1000'er kayıt).

#### Vergi Daireleri

GİB'in yayınladığı vergi dairesi listesinden ~1000 kayıt. İl bazlı.

```sql
TurkishTaxOffice (Id, ProvinceId, Name, Code)
```

### 3. Customer ve Address Domain Genişletme

nopCommerce'in mevcut `Customer` ve `Address` entity'lerini değiştirmeden, **paralel tablolar** kullan:

```csharp
[Table("TurkishCustomerExtension")]
public class TurkishCustomerExtension : BaseEntity
{
    public int CustomerId { get; set; }  // FK to nopCommerce Customer
    public string TcKimlikNo { get; set; }
    public string VergiNo { get; set; }
    public int? VergiDairesiId { get; set; }
    public string Mersis { get; set; }
    public string TicaretSicilNo { get; set; }
    public string KepAdresi { get; set; }
    public TurkishCustomerType MusteriTipi { get; set; }
    public bool? IsEFaturaMukellefi { get; set; }  // GİB sorgusundan, nullable
    public DateTime? EFaturaMukellefiKontrolTarihi { get; set; }
}

[Table("TurkishAddressExtension")]
public class TurkishAddressExtension : BaseEntity
{
    public int AddressId { get; set; }  // FK to nopCommerce Address
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? NeighborhoodId { get; set; }
    public string BinaNo { get; set; }
    public string DaireNo { get; set; }
}
```

Customer ve Address entity'lerini extend eden extension method'ları yaz:

```csharp
public static class CustomerTurkeyExtensions
{
    public static async Task<TurkishCustomerExtension> GetTurkishExtensionAsync(this Customer customer);
    public static async Task SetTcKimlikNoAsync(this Customer customer, string tckn);
    // vb.
}
```

### 4. KDV Provider

```csharp
public class TurkishTaxProvider : ITaxProvider
{
    // nopCommerce'in standart KDV oranlarını TR oranları ile override et
    // Kategori bazlı KDV: 
    //   - Gıda, kitap: %1
    //   - Giyim, ev eşyası, beyaz eşya: %20
    //   - Sağlık ürünleri (bazı): %10
    //   - Lüks ürünler: %20 + ÖTV
    //   - İhracat: %0
}
```

ÖTV altyapısı (telefon, alkol, tütün, akaryakıt) için ayrı `IOtvProvider` interface'i. Şimdilik abstract, implementation Phase 2'de.

### 5. TCMB Döviz Kuru Servisi

TCMB'nin günlük döviz kurları XML feed'inden yararlan: `https://www.tcmb.gov.tr/kurlar/today.xml`

```csharp
public interface ITcmbExchangeRateService
{
    Task<decimal> GetRateAsync(string sourceCurrency, string targetCurrency);
    Task RefreshRatesAsync();  // Background task tarafından çağrılır
    Task<List<ExchangeRateLog>> GetHistoricalRatesAsync(string currency, DateTime from, DateTime to);
}
```

Background task: Her gün 16:00'da TCMB feed'ini çek, veritabanına logla, nopCommerce'in `Currency` entity'sini güncelle. nopCommerce'in `IScheduleTask` interface'ini implement et.

### 6. GİB Mükellef Sorgu Servisi

GİB'in e-fatura mükellef sorgu servisi:
- Resmi servis: `https://earsivportal.efatura.gov.tr/efaturaservices/...`
- Alternatif: Özel entegratörlerin (e-Logo, Mysoft, İzibiz) public API'leri var mı kontrol et

```csharp
public interface IGibMukellefService
{
    Task<bool> IsEFaturaMukellefiAsync(string vknOrTckn);
    Task<MukellefInfo> GetMukellefInfoAsync(string vknOrTckn);
}
```

24 saat TTL cache kullan (nopCommerce'in `IStaticCacheManager`). Mock implementation ile başla; gerçek servis bilgisi product owner'dan gelecek.

### 7. Türkçe Lokalizasyon

`Localization/tr-TR.xml` dosyasında tüm string'ler:

```xml
<Language Name="Türkçe" IsRightToLeft="false">
  <LocaleResource Name="Plugins.Misc.TurkeyCore.Validation.TcKimlikNo.Invalid">
    <Value>Geçersiz TC Kimlik No</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Misc.TurkeyCore.Validation.VergiNo.Invalid">
    <Value>Geçersiz Vergi Kimlik No</Value>
  </LocaleResource>
  <!-- ~150-200 lokalize string -->
</Language>
```

Para formatı: `1.234.567,89 ₺` (binlik nokta, ondalık virgül).
Tarih formatı: `dd.MM.yyyy`, `dd.MM.yyyy HH:mm`.

### 8. Admin Panel Sayfaları

nopCommerce admin teması ile uyumlu:

- **Vergi Dairesi Yönetimi** — CRUD, il bazlı filtre, Excel import/export
- **İl/İlçe/Mahalle Yönetimi** — Read-only liste (sistem verisi), sadece admin görüntüler
- **TurkeyCore Ayarları** — TCMB kur güncelleme sıklığı, GİB API endpoint, cache TTL vb.
- **TCMB Kur Geçmişi** — Günlük log görüntüleme

DataTables.NET ile grid, FluentValidation ile validation, BootstrapValidator ile client-side.

### 9. View Components (Storefront)

Müşteri kayıt formu ve checkout'ta kullanılacak:

- `ProvinceDistrictSelector` — Cascading dropdown (İl seçilince ilçe yüklenir, ilçe seçilince mahalle)
- `CustomerTypeSelector` — Bireysel/Kurumsal radio button + ilgili formu göster/gizle
- `KepAddressInput` — KEP adresi girişi (kurumsal için)
- `TcKimlikNoInput` — Real-time validation

AJAX endpoint'leri:
- `GET /turkeycore/api/districts/{provinceId}` — İlçe listesi
- `GET /turkeycore/api/neighborhoods/{districtId}` — Mahalle listesi
- `POST /turkeycore/api/validate-tckn` — TCKN validasyonu

### 10. Event Consumer'lar

```csharp
public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    // Kayıt sırasında TCKN/VKN validasyonu
    // Kurumsal müşteri ise GİB e-fatura mükellef sorgusu (background)
}

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    // Sipariş geldiğinde fatura tipini belirle (e-fatura/e-arşiv)
    // GenericAttribute olarak Order'a kaydet
}
```

## Test Gereksinimleri

`Nop.Plugin.Misc.TurkeyCore.Tests` projesi oluştur:

- **xUnit** veya **NUnit** kullan (nopCommerce konvansiyonuna göre)
- **Moq** veya **NSubstitute** mock için
- **FluentAssertions** assertion için

Mutlaka kapsanması gerekenler:
- TCKN algoritması (geçerli 20 örnek + geçersiz 20 örnek + edge case'ler)
- VKN algoritması
- IBAN TR validasyonu
- GSM normalizasyonu (farklı format girişleri)
- Türkçe karakter normalizasyonu
- Currency formatter (TR-tr culture)
- Tax provider (kategori bazlı KDV)

## plugin.json İçeriği

```json
{
  "Group": "Misc",
  "FriendlyName": "Türkiye Core - Türkiye E-Ticaret Çekirdek Modülü",
  "SystemName": "Misc.TurkeyCore",
  "Version": "1.0.0",
  "SupportedVersions": ["4.90"],
  "Author": "[Senin Adın/Şirketin]",
  "DisplayOrder": 1,
  "FileName": "Nop.Plugin.Misc.TurkeyCore.dll",
  "Description": "nopCommerce için Türkiye lokalizasyon temel paketi: TC Kimlik/Vergi No validasyonu, il/ilçe/mahalle veri seti, vergi daireleri, KDV oranları, TCMB döviz kuru, GİB mükellef sorgu, Türkçe yerelleştirme.",
  "Dependencies": []
}
```

## Bağımlılıklar (NuGet)

`Nop.Web.Framework` ve `Nop.Services` referansları zorunlu (nopCommerce solution içinden). Ek paketler:

- `FluentValidation.AspNetCore` (zaten nopCommerce'te var)
- `System.Net.Http` (TCMB ve GİB için, .NET 9'da built-in)
- `EPPlus` veya `ClosedXML` (Excel import/export için, isteğe bağlı)

**Eklemeyin**: nopCommerce'in halihazırda kullandığı paketlere ek versiyon eklemeyin (conflict riski).

## Kalite Kriterleri

1. **Kod stili**: nopCommerce'in `.editorconfig` ve `Directory.Build.props` kurallarına uy
2. **Async**: Tüm IO işlemleri `async/await`. `Task` dönen metodlar `Async` suffix'i.
3. **Null safety**: `<Nullable>enable</Nullable>` set et, nullable reference type kullan
4. **Loglama**: nopCommerce'in `ILogger` interface'i (Serilog wrapper). Hassas veri (TCKN, VKN) loglanmaz
5. **Güvenlik**: TCKN gibi PII alanlar veritabanında **şifreli** saklanmalı (nopCommerce'in `IEncryptionService` kullan)
6. **Performans**: İl/ilçe/mahalle sorguları `IStaticCacheManager` ile 24 saat cache'lensin
7. **Hata yönetimi**: Custom exception hierarchy: `TurkeyCoreException`, `ValidationException`, `GibServiceException`
8. **Lokalizasyon**: Hardcoded string yok — her şey `_localizationService.GetResourceAsync()` ile

## Çıktı Beklentileri

Plugin'i geliştirmeye başlarken şu sırayı izle:

1. **İlk olarak** `plugin.json`, `TurkeyCorePlugin.cs`, `Infrastructure/` klasörü ve DI registration
2. **İkinci olarak** Domain entity'leri ve migration'lar (boş başla, seed sonra)
3. **Üçüncü olarak** Validasyon servisleri (en önemli, en çok test gerektiriyor)
4. **Dördüncü olarak** Lokalizasyon dosyaları
5. **Beşinci olarak** Admin controller'lar ve view'lar
6. **Altıncı olarak** TCMB ve GİB servisleri
7. **Son olarak** Storefront view component'leri

Her aşamada:
- Build sorunsuz olmalı
- Mevcut testler geçer durumda olmalı
- Kod review için commit mesajı: `feat(turkey-core): <kısa açıklama>` formatında

## Önemli Notlar

- **nopCommerce kaynak kodunu inceleyerek mevcut pattern'lere uy** — özellikle `Nop.Plugin.Tax.FixedOrByCountryStateZip` ve `Nop.Plugin.Misc.SendinBlue` paketlerini referans al
- **Plugin lifecycle**: `InstallAsync()` ile migration ve seed, `UninstallAsync()` ile temiz kaldırma
- **Multi-store desteği**: Eklenecek tüm setting'ler `LimitedToStores` bayrağını dikkate alsın
- **Multi-language**: Sadece TR ve EN değil, gelecekte AR, RU eklenebilecek şekilde generic tasarla

## Soru-Cevap

Geliştirme sırasında belirsiz noktalar olursa şu kaynaklardan teyit et:
- nopCommerce 4.90 dokümantasyonu: https://docs.nopcommerce.com
- nopCommerce GitHub: https://github.com/nopSolutions/nopCommerce
- TC Kimlik algoritması: NVI resmi dokümantasyonu
- KDV oranları: GİB resmi tebliğleri
- TCMB döviz kurları: https://www.tcmb.gov.tr/kurlar/today.xml

## Definition of Done

Plugin tamamlanmış sayılır:
- [ ] Tüm testler geçiyor (coverage ≥ %80)
- [ ] nopCommerce 4.90.4 üzerinde install/uninstall sorunsuz
- [ ] Admin panelinde tüm sayfalar erişilebilir
- [ ] TR ve EN lokalizasyon tam
- [ ] README.md (kurulum, kullanım, FAQ)
- [ ] CHANGELOG.md
- [ ] Build artifact: `Nop.Plugin.Misc.TurkeyCore.zip` (nopCommerce upload formatına uygun)
