# PROJECT_DECISIONS.md

Bu dosya, Türkiye nopCommerce eklentileri projesinde verilen stratejik kararları ve gerekçelerini belgeler. İleride "neden böyle yapmıştık?" sorusu çıktığında referans alınmalıdır.

## Karar 1: Platform Seçimi — nopCommerce

**Tarih**: Mayıs 2026  
**Karar**: nopCommerce 4.90 üzerinde geliştirme yapılacak.

### Değerlendirilen Alternatifler

1. **Orchard Core + OrchardCore.Commerce** — Reddedildi
2. **DuxCommerce** — Reddedildi (henüz tam açık kaynak değil, küçük topluluk)
3. **Sıfırdan ASP.NET Core ile geliştirme** — Reddedildi (zaman maliyeti çok yüksek)

### Neden nopCommerce

- **Olgunluk**: 15+ yıllık proje, kurumsal seviyede stabil. Volvo, Puma, Reebok, Columbia gibi global markalar kullanıyor.
- **Geniş plugin ekosistemi ve dokümantasyon**: Stack Overflow'da Türkçe dahil sorulara çözüm bulunabiliyor.
- **Hazır admin panel ve domain modeli**: Sipariş, müşteri, vergi, kargo, indirim altyapısı zaten var. Biz sadece *adapter* yazacağız.
- **Plugin mimarisi olgun ve dökümante**: `IPlugin`, `BasePlugin`, `IConsumer<T>`, `IDependencyRegistrar` pattern'leri net.
- **NetSis ERP entegrasyonuyla uyumlu domain**: Mehmet'in mevcut ERP'sindeki müşteri/ürün/sipariş yapıları nopCommerce modelleri ile yakın.
- **C#/Dapper/SQL Server ekosistemi**: Mehmet'in mevcut yetkinlikleri ile %100 örtüşür. nopCommerce LinqToDB kullanıyor (Dapper'a yakın felsefe), öğrenme eğrisi düşük.
- **DevExpress XAF benzeri "her şey kutuda" hissi**: Mehmet'in tarzına uygun.

### Neden Orchard Core Değil

- **Olgunluk eksikliği**: OrchardCore.Commerce hâlâ MVP/erken aşama (1.x). Çekirdek e-ticaret özellikleri (gelişmiş envanter, kargo, vergi sağlayıcıları, kupon motoru) eksik.
- **Türkiye lokalizasyonu yok**: Sıfırdan yazılması gerekirdi.
- **Küçük topluluk**: Sorun yaşandığında çözüm bulmak zor.
- **Plugin ekosistemi neredeyse yok**.

### Hangi Durumda Tekrar Değerlendirilir

- Multi-tenant SaaS modeli kurulacaksa (her müşteriye izole bir e-ticaret sitesi)
- Ana ihtiyaç içerik yönetimi ve e-ticaret onun bir uzantısıysa

---

## Karar 2: Plugin'leri Sıfırdan Yazma

**Tarih**: Mayıs 2026  
**Karar**: Mevcut paralı Türkiye eklentilerini satın almak yerine sıfırdan yazılacak.

### Gerekçe

- Mevcut paralı eklentiler **yıllık lisans modeli** kullanıyor — sürekli ödeme
- nopCommerce sürüm yükseltmelerinde plugin uyumluluk garantisi yok (yıllık ödeme yapsan bile)
- Sıfırdan yazılan kodun **ürünleştirilebilir** olması (bkz. Karar 4)
- Mehmet'in mevcut C# ve e-fatura/XSLT bilgisi, geliştirme süresini ciddi azaltıyor

### Süre Tahmini

- Tek geliştirici, yarı zamanlı: 6-9 ay tüm faz 1+2
- Tek geliştirici, tam zamanlı: 3-4 ay tüm faz 1+2

---

## Karar 3: İlk Önce Yasal Temel

**Tarih**: Mayıs 2026  
**Karar**: Geliştirme sırası: TurkeyCore → TurkishConsumerLaw → ödeme/kargo/e-fatura

### Gerekçe

- **TurkeyCore** çekirdek altyapı; diğer her şey buna bağımlı
- **TurkishConsumerLaw** yasal zorunluluk; bunsuz çalışan e-ticaret sitesi açılamaz (ETBİS, MSS, KVKK)
- Ödeme/kargo/e-fatura ekonomik değer katar ama yasal temel olmadan kullanılamaz
- Yasal uyumluluk paketi pazarda **en güçlü farklılaştırıcı** — paralı eklentilerde dahi 2026 KVKK ilke kararı henüz yansımamış olabilir

### Reddedilen Alternatif

Ödeme entegrasyonuyla başlamak: Mantıklıydı çünkü "para getiren" kısım. Ama yasal uyumluluk olmadan müşteri siteyi yayına alamaz, dolayısıyla ödeme entegrasyonunun değeri sıfır olur.

---

## Karar 4: Ticari Model

**Tarih**: Mayıs 2026  
**Karar**: Üç katmanlı dağıtım — Free / Pro / Enterprise

### Modeller

1. **Free Tier (Açık Kaynak — MIT)**
   - GitHub'da açık kaynak
   - Temel özellikler (1 ödeme + 1 kargo + temel KVKK)
   - Topluluk desteği (GitHub Issues)
   - Amaç: Pazara giriş, marka bilinirliği

2. **Pro Bundle (Commercial License)**
   - Tüm ödeme + kargo + e-fatura paketi
   - **Tek seferlik ödeme + kaynak kod erişimi**
   - 1 yıl ücretsiz güncelleme
   - E-posta destek
   - Amaç: Ana gelir kaynağı

3. **Enterprise (Custom Quote)**
   - Pro + pazaryeri konnektörleri (Trendyol, Hepsiburada vb.)
   - ERP entegrasyonları (NetSis, Logo, Mikro)
   - Özel kurulum
   - Öncelikli destek + SLA
   - Amaç: Yüksek-değer müşteri

### Pazardaki Konumlandırma

> "Yıllık lisans cendere değil — bir kez öder, ömür boyu kullanırsın. Kaynak kod senin."

### Mevcut Paralı Eklentilere Karşı Avantaj

| Mevcut Eklentiler | Bizim Modelimiz |
|-------------------|-----------------|
| Yıllık lisans | Tek seferlik |
| Kaynak kod kapalı | Kaynak kod açık |
| Sürüm uyumluluğu garantisi yok | 1 yıl ücretsiz güncelleme |
| 2026 KVKK ilke kararı geç yansır | Geliştirilen sürümlerde hemen var |

---

## Karar 5: Tek Repo Yaklaşımı (Fork)

**Tarih**: Mayıs 2026  
**Karar**: nopCommerce fork'u içinde plugin'ler `src/Plugins/` altında geliştirilir.

### Değerlendirilen Alternatifler

1. **Fork yaklaşımı (seçildi)**: nopCommerce fork'u + `CLAUDE.md` ve `docs/` solution kökünde
2. **Submodule yaklaşımı**: nopCommerce submodule, plugin'ler ayrı repoda
3. **Symlink yaklaşımı**: Pluginler ayrı, build sırasında nopCommerce'e symlink

### Neden Fork

- **Pratik**: Tek `dotnet build` ile her şey derleniyor
- **IDE deneyimi**: Visual Studio/Rider'da tüm solution tek ekranda
- **Debug kolaylığı**: nopCommerce'in iç servislerine breakpoint koymak kolay
- **Test**: Plugin testleri nopCommerce test altyapısı ile entegre

### Risk

Upstream güncellemeleri çekerken `CLAUDE.md` ve `docs/` çakışabilir. **Çözüm**: Bu dosyaları sadece kendi fork'unda tut, upstream'e PR atma.

---

## Karar 6: PDF Üretimi — QuestPDF

**Tarih**: Mayıs 2026  
**Karar**: PDF üretimi için QuestPDF kullanılacak (iTextSharp değil).

### Gerekçe

- **QuestPDF**: MIT lisansı (community sürümü), modern API, fluent syntax, Türkçe karakter desteği iyi
- **iTextSharp 7+**: AGPL lisansı — ticari ürün için ya commercial license alınmalı ya da open source zorunluluğu

Ticari ürünleştirmede AGPL kabul edilemez (kaynak kodumuzu açmak zorunda kalırız).

### Türkçe Karakter Notu

Default font'lar (Helvetica vb.) Türkçe karakteri bozar. Kullanılacak fontlar:
- **Roboto** (Apache 2.0)
- **Open Sans** (Apache 2.0)
- **Inter** (OFL)

Font'lar plugin'in `Fonts/` klasöründe embed edilecek.

---

## Karar 7: Test Framework — NUnit

**Tarih**: Mayıs 2026  
**Karar**: NUnit + Moq + FluentAssertions

### Gerekçe

- **NUnit**: nopCommerce'in kendi konvansiyonu (xUnit yerine NUnit kullanıyor)
- Mevcut nopCommerce test pattern'leri ile uyum
- **Moq**: De facto .NET mock library
- **FluentAssertions**: Daha okunabilir test assertion'ları

### Test Coverage Hedefleri

- TurkeyCore: ≥ %80 (validasyon algoritmaları kritik)
- TurkishConsumerLaw: ≥ %75 (yasal uyumluluk, hata kabul edilemez)
- Diğer plugin'ler: ≥ %70

---

## Karar 8: 2026 KVKK İlke Kararı Uyum Stratejisi

**Tarih**: Mayıs 2026  
**Karar**: Aydınlatma metni ile açık rıza metni **ayrı dokümanlar** olarak tutulacak.

### Bağlam

24.03.2026 tarihli ve 2026/347 sayılı KVKK İlke Kararı, "aydınlatma + açık rıza" tek metin yaklaşımını yasakladı. Birçok mevcut eklentide hâlâ tek metin yaklaşımı var.

### Bizim Yaklaşımımız

- `DisclosureText` entity'si — sadece aydınlatma metni
- `ExplicitConsentText` entity'si — her bir veri işleme amacı için ayrı
- `ConsentRecord` entity'si — kim, ne zaman, hangi rızayı verdi/geri çekti
- Storefront'ta 4 farklı bölüm:
  1. Aydınlatma metni okundu beyanı (sadece bilgilendirme)
  2. KVKK açık rıza checkbox'ları (her amaç ayrı)
  3. ETK ticari ileti onayları (SMS/email/arama ayrı)
  4. Çerez consent (ayrı kategoriler)

### Migration Stratejisi (mevcut sistemler için)

Mevcut müşterilerin tek-metin onayları **geçersiz** kabul edilir. Login sırasında banner gösterilerek tekrar onay alınır.

---

## Karar 9: GİB E-Fatura Mükellef Sorgu — Mock'la Başla

**Tarih**: Mayıs 2026  
**Karar**: TurkeyCore'un GİB sorgu servisi mock implementation ile başlayacak.

### Gerekçe

- GİB'in **public API'si yok** — özel entegratörler (e-Logo, Mysoft, İzibiz) üzerinden gidilir
- Hangi entegratörle çalışılacağı müşteri-bazlı bir karar (her müşterinin kendi tercih edebilir)
- TurkeyCore çekirdek paket, entegratör-agnostik kalmalı

### Implementasyon

```csharp
public interface IGibMukellefService { ... }
public class MockGibMukellefService : IGibMukellefService { ... }  // Default
public class ELogoGibMukellefService : IGibMukellefService { ... } // Pro
public class MysoftGibMukellefService : IGibMukellefService { ... } // Pro
```

E-fatura plugin'i (Faz 2) gerçek implementasyonları sağlayacak.

---

## Karar 10: Domain Genişletme — Paralel Tablolar

**Tarih**: Mayıs 2026  
**Karar**: nopCommerce'in `Customer` ve `Address` entity'lerini değiştirmek yerine paralel tablolar kullanılacak.

### Yapı

```csharp
[Table("TurkishCustomerExtension")]
public class TurkishCustomerExtension : BaseEntity
{
    public int CustomerId { get; set; }  // FK to nopCommerce Customer
    public string TcKimlikNo { get; set; }
    public string VergiNo { get; set; }
    // ...
}
```

### Gerekçe

- nopCommerce upstream güncellemelerini çekerken çakışma olmaz
- Türkiye paketleri uninstall edilirse nopCommerce normal çalışmaya devam eder
- Plugin migration'ları sadece kendi tablolarını yönetir

### Trade-off

- Performans: Customer sorgularında join gerekiyor — `IStaticCacheManager` ile çözülür
- Karmaşıklık: Extension method'lar gerekli — `customer.GetTurkishExtensionAsync()` pattern'i

---

## Karar 11: TCMB XML Parser Saf Statik Fonksiyon

**Tarih**: Mayıs 2026
**Karar**: `TcmbExchangeRateService.ParseFeedXml(string)` — instance gerektirmeyen statik metod.

### Gerekçe

- HttpClient mock'lamadan parser'ı doğrudan test edebiliyoruz (`TcmbXmlParserTests` 9 test)
- Saf fonksiyon: aynı XML her zaman aynı sonucu verir, yan etkisiz
- Aynı parser CSV import aracında veya manuel re-import senaryosunda da kullanılabilir

### Trade-off

- Service `protected virtual` ile sarmalanabilirdi (override için), ama statik daha açık ve test edilebilir.

---

## Karar 12: Lokalizasyon — Static Dictionary, tr-TR.xml Yok

**Tarih**: Mayıs 2026
**Karar**: Plugin'in lokalizasyon kaynakları `Localization/LocaleResources.cs` static class'ında Dictionary olarak; `tr-TR.xml` ayrı dosya yok.

### Gerekçe

- nopCommerce plugin konvansiyonu: `ILocalizationService.AddOrUpdateLocaleResourceAsync(Dictionary)` ile install sırasında DB'ye yazılır
- `tr-TR.xml` formatı sadece **language pack import** feature'ı için, plugin dağıtımında değil
- Static class IDE auto-complete ve build-time hata yakalama avantajı verir
- Spec'te (`01-TurkeyCore-Prompt.md`) önerilen `tr-TR.xml` aslında yanlış yönlendirme

### Implementasyon

```csharp
// Localization/LocaleResources.cs
public static class LocaleResources
{
    public static readonly IDictionary<string, string> Turkish = new Dictionary<string, string> { ... };
    public static readonly IDictionary<string, string> English = new Dictionary<string, string> { ... };
}

// TurkeyCorePlugin.InstallAsync()
await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.Turkish);
var en = (await _languageService.GetAllLanguagesAsync(true))
    .FirstOrDefault(l => l.UniqueSeoCode == "en");
if (en is not null)
    await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.English, en.Id);
```

---

## Karar 13: Test Setup'ta `Singleton<AppSettings>` Init

**Tarih**: Mayıs 2026
**Karar**: Test projesinde `[SetUpFixture]` ile `Singleton<AppSettings>.Instance` boş `CacheConfig` ile init ediliyor.

### Sorun

nopCommerce'in `CacheKey` constructor'ı:
```csharp
public int CacheTime { get; set; } = Singleton<AppSettings>.Instance.Get<CacheConfig>().DefaultCacheTime;
```
Test ortamında AppSettings init edilmediğinde `NullReferenceException`.

### Çözüm

`AssemblySetup.cs`:
```csharp
[SetUpFixture]
public class AssemblySetup
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        Singleton<AppSettings>.Instance ??= new AppSettings(new List<IConfig> { new CacheConfig() });
    }
}
```

### Trade-off

- `CacheKey.CacheTime` testlerde default'a düşer (production'da AppSettings'ten okur)
- Plugin kodu `{ CacheTime = ... }` property initializer override'ı kullanmıyor (kullanırsa ctor zaten crash eder)
- Eğer plugin'in kendi cache TTL'i olacaksa: `IStaticCacheManager.SetAsync(key, value)` ile manuel set edilmeli

---

## Karar 14: AJAX Endpoint'leri Auth'suz Public

**Tarih**: Mayıs 2026
**Karar**: `TurkeyCoreApiController` (provinces/districts/neighborhoods/validate-tckn) auth gerektirmez.

### Gerekçe

- İl/ilçe/mahalle public veridir (KVKK kapsamında değil)
- TCKN/VKN validasyonu pure fonksiyondur — DB'ye dokunmaz, kişisel veri kaydetmez
- Storefront kayıt formundaki anonymous kullanıcılar için erişilebilir olması şart

### Güvenlik

- POST endpoint'lerinde anti-forgery default aktif (ASP.NET Core default davranış)
- Rate limiting plugin seviyesinde değil, nopCommerce framework seviyesinde (varsa) uygulanmalı
- TCKN/VKN response'unda sadece "valid/invalid" döner — kişisel veri sızıntısı yok

---

## Karar 15: Onay Tabloları Append-Only Audit Log

**Tarih**: Mayıs 2026
**Karar**: KVKK rıza kayıtları (`ConsentRecord`), çerez consent (`CookieConsent`) ve sözleşme onayları (`GeneratedContract`) **append-only** olarak tasarlandı. UPDATE yok; her değişiklik yeni satır.

### Gerekçe

- **KVKK m.7 ispat yükümlülüğü**: Veri sorumlusu rıza alındığını ispatlamak zorunda. Tarihsel zincir görünmeli.
- **Manipülasyon kontrolü**: SHA-256 hash her satırda. Kayıt silinir/değişirse hash uyuşmaz.
- **Effective state hesaplama**: Scope başına en güncel kayıt = mevcut etkin rıza.
- **Withdrawal akışı**: Müşteri rızasını geri çekerse yeni "denied" satırı eklenir; eski "granted" satır silinmez.

### Trade-off

- Tablo boyutu zamanla büyür → 10 yıl saklama sonrası `IDataRetentionService` ile silme (Faz 1B'de)
- Effective state için her sorguda tarih sırasına göre group by gerek → küçük performans maliyeti, cache ile çözülebilir

---

## Karar 16: Tek `GeneratedContract` Tablosu vs Spec'in 2 Entity'si

**Tarih**: Mayıs 2026
**Karar**: MSS ve ÖBF için tek `GeneratedContract` tablosu kullanıldı, `Type` field ile ayrım. Spec'te ayrı entity'ler önerilmişti.

### Gerekçe

- DRY: Aynı yapıda iki tablo gereksiz duplikasyon
- Sorgu kolaylığı: "Bir siparişin tüm sözleşmeleri" → tek query
- Domain class'ında bu sapma yorumla açıkça belirtildi

### İptal Edilirse

İleride MSS ve ÖBF için divergent alanlar gerekirse (örn. ÖBF'ye özel ek alan), `GeneratedContract` base class olur, MSS/ÖBF inheritance ile ayrılır. Mevcut Type field migration'da query filter olarak kalır.

---

## Karar 17: KVKK 2026/347 İlke Kararı Uygulaması

**Tarih**: Mayıs 2026
**Karar**: Aydınlatma metni içinde "rıza" sözcüğü YOK. KVKK rıza ve ETK ileti onayı 3 ayrı bölüm. Her scope için ayrı `<input type="checkbox">` ve ayrı `ConsentRecord` satırı.

### Uygulama Detayları

- **Default disclosure metni** içinde "Bu metin yalnızca bilgilendirme amaçlıdır; işleme açık rıza bu metnin kabulü ile değil, ayrı sunulan rıza onayları ile alınır." beyanı var
- `KvkkDisclosureNotice` ViewComponent'inde "okudum" checkbox altında "Bu kutucuk yalnızca okuma beyanıdır" açıklaması
- `ExplicitConsentCheckboxes` `mode="kvkk"` ve `mode="etk"` ile fizikiel olarak iki ayrı `<fieldset>` render ediliyor
- `ConsentScope` enum'da numerik aralık ayrımı (1-9 = KVKK, 10+ = ETK), `IsKvkkScope`/`IsEtkScope` static helper

### Default İşaretsiz Kuralı

`ExplicitConsentText.DefaultChecked = false` migration'da. Admin manuel açabilir ama UI'da "KVKK m.5 gereği opt-in olmalı" uyarı banner'ı var. Default işaretli yapmak hukuka aykırı olabilir.

---

## Karar 18: Withdrawal State Machine — `IsValidTransition` Static Function

**Tarih**: Mayıs 2026
**Karar**: Cayma talebinin 9 durumlu state machine'i `WithdrawalService.IsValidTransition(from, to)` public static metodla yönetiliyor. Admin UI'da next-state butonları otomatik üretiliyor.

### Gerekçe

- Domain logic UI'a sızmıyor: UI sadece valid transition'ları soruyor
- Test edilebilirlik: 14 case [TestCase] ile valid + invalid yollar test edildi
- Yanlış geçiş gelirse `InvalidOperationException` (örn. Pending → Refunded)

### State Tablosu

```
Pending → Approved | Rejected | Cancelled
Approved → InTransit
InTransit → Received
Received → Inspected
Inspected → Refunded
Refunded → Completed
```

Terminal state'ler (Rejected, Cancelled, Completed) → geri dönüş YOK.

---

## Karar 19: PDF Yerine HTML (Geçici)

**Tarih**: Mayıs 2026
**Karar**: MSS/ÖBF üretiminde QuestPDF nuget paketi yerine HTML olarak saklanıyor; müşteri indirirken HTML wrap document olarak alıyor.

### Gerekçe

- Yasal yeterlilik: HTML format yasal olarak geçerli (elektronik onay + SHA-256 hash)
- nuget paketi eklemek nopCommerce plugin output handling'i ile karmaşık
- HTML zaten browser'da yazdırılabilir (tarayıcı PDF olarak kaydedebilir)
- QuestPDF entegrasyonu ayrı sprint'e bırakıldı (Türkçe karakter font embedding ayrıca test gerekir)

### Plan B

Faz 2'de QuestPDF nuget eklenir, `IContractPdfGenerator` arayüzü implement edilir. Mevcut HTML içerik PDF'e dönüştürülebilir (HTML → PDF dönüşüm zaten standart QuestPDF özelliği).

---

## Karar 20: Bilinmeyen Token Leave-As-Is

**Tarih**: Mayıs 2026
**Karar**: `ContractTokenReplacer` bilinmeyen `{{...}}` token'ları sessizce silmek yerine olduğu gibi bırakır.

### Gerekçe

- Admin şablonda typo yaparsa (`{{ÜrünListsi}}` gibi) preview'da görür ve düzeltir
- Sessizce silmek "neden bilgi eksik?" şeklinde admin'i yanıltır
- Production'da gerçek müşteri sözleşmesinde `{{Bilinmeyen}}` görmek admin için kırmızı bayraktır

---

## Karar 21: Multi-Cascade FK Yerine Soft-FK (Indexed)

**Tarih**: 2026-05-03
**Karar**: `TurkishAddressExtension` tablosunda `ProvinceId`, `DistrictId`, `NeighborhoodId` kolonları **hard FK** (`.ForeignKey<T>()`) yerine **soft FK** (`.Indexed()`) ile tanımlanır. Sadece `AddressId` hard FK olarak kalır.

### Sorun

İlk migration çalıştırıldığında SQL Server şu hatayı verdi:

```
Introducing FOREIGN KEY constraint 'FK_TurkishAddressExtension_DistrictId_TurkishDistrict_Id'
on table 'TurkishAddressExtension' may cause cycles or multiple cascade paths.
(Error 1785)
```

### Kök Neden

`TurkishProvince → TurkishDistrict → TurkishAddressExtension` zinciri ile `TurkishProvince → TurkishAddressExtension` doğrudan zinciri **iki cascade yolu** oluşturuyor. SQL Server bunu cycle riski olarak reddediyor.

### Çözüm

Province/District/Neighborhood için cascading delete'e ihtiyaç yok (lokasyon master data nadiren silinir, silinirse adres kaydı kalmalı). Bu yüzden `.Indexed()` ile sadece performans için index bırakıldı, FK constraint kaldırıldı.

```csharp
.WithColumn(nameof(TurkishAddressExtension.AddressId)).AsInt32().NotNullable().ForeignKey<Address>()
.WithColumn(nameof(TurkishAddressExtension.ProvinceId)).AsInt32().Nullable().Indexed()
.WithColumn(nameof(TurkishAddressExtension.DistrictId)).AsInt32().Nullable().Indexed()
.WithColumn(nameof(TurkishAddressExtension.NeighborhoodId)).AsInt32().Nullable().Indexed()
```

### Sonuç

- Migration sorunsuz çalışıyor
- Application-level integrity check `TurkishLocationService` üzerinden yapılıyor
- TurkishCustomerExtension'da `VergiDairesiId` de aynı mantıkla `.Indexed()` (vergi dairesi master data)

---

## Karar 22: Plugin Enum Kolonları İçin Explicit `.AsInt32()`

**Tarih**: 2026-05-03
**Karar**: Tüm `NopEntityBuilder<T>` builder'larında, entity'deki enum property'leri **açıkça** `.WithColumn(nameof(...)).AsInt32().NotNullable()` ile tanımlanır.

### Sorun

İlk install denemesinde plugin migration'ları başarılı görünüyordu, ama runtime'da:

```
Invalid column name 'Category'.
Invalid column name 'Scope'.
Invalid column name 'Status'.
...
```

### Kök Neden

nopCommerce'in `Create.TableFor<T>()` helper'ı, enum property'lerini **plugin assembly'lerinde** otomatik üretmiyor. Çekirdek nopCommerce entity'lerinde bu çalışıyor (assembly load order farklı), ama plugin assembly'leri için enum tipleri reflection sırasında atlanıyor — kolon hiç oluşturulmuyor.

### Çözüm

Tüm enum property'leri **builder'da explicit** olarak tanımlanır:

```csharp
.WithColumn(nameof(CookieDefinition.Category)).AsInt32().NotNullable()
.WithColumn(nameof(WithdrawalRequest.Reason)).AsInt32().NotNullable()
.WithColumn(nameof(WithdrawalRequest.Status)).AsInt32().NotNullable()
// ... vs.
```

### Etkilenen Builder'lar (8 builder, 12 enum kolonu)

- `TurkishCustomerExtensionBuilder` — `MusteriTipi`
- `CookieDefinitionBuilder` — `Category`
- `ConsentRecordBuilder` — `Scope`, `Source`
- `ExplicitConsentTextBuilder` — `Scope`
- `DataSubjectRequestBuilder` — `RequestType`, `Status`
- `WithdrawalRequestBuilder` — `Reason`, `Status`
- `ContractTemplateBuilder` — `Type`
- `GeneratedContractBuilder` — `Type`

### Sonuç

- Tüm tablolar enum kolonlarıyla doğru oluşturuluyor (verified: `[Category] INT NOT NULL`)
- **Kural**: Yeni bir plugin entity'sine enum eklendiğinde, mutlaka builder'a da explicit kolon tanımı eklenmeli — yoksa runtime'da fark edilen sessiz bir hata oluşur

### Ek Öğrenme — Stale DLL Cache

Bug fix sonrası rebuild ile sorun devam ettiyse: `bin/`, `obj/`, ve `Plugins/Misc.TurkeyCore`, `Plugins/Misc.TurkishConsumerLaw` çıktı klasörlerinin **tamamı silinmeli**, ardından full solution rebuild + DB drop + setup wizard tekrar çalıştırılmalı.

---

## Karar 23: Ürün Döviz Bazlı Fiyat — Persisted Recalc (Canlı Override Değil)

**Tarih**: 2026-05-04
**Karar**: Ürün başına döviz bazlı fiyat tanımı için **persisted recalc** yaklaşımı kullanılır: admin Save anında veya TCMB scheduled task tetiklendiğinde `Product.Price/OldPrice/ProductCost` alanları DB'ye yazılır. Storefront fiyat akışı (search, filter, discount, marketplace, tier price) tek doğru fiyatı (TRY cinsinden) görür.

### Reddedilen Alternatif

İlk denenen yaklaşım: `IPriceCalculationService` decorator ile `Product.Price`'ı runtime'da bellek-içi mutate etmek (canlı çevrim). Bu yaklaşım şu yan etkilerden ötürü **terk edildi**:

- **Search/filter**: DB sorgusu raw `Product.Price` kullanır → bizim 3350 TL ürün yanlış banda düşer
- **Sıralama**: "Fiyat artan" sorgusu raw değer üzerinden çalışır
- **Search index** (Lucene): index raw değeri tutar, arama yanlış sıralama
- **Tier prices, attribute combination**: Decorator `overriddenProductPrice != null` ise skip eder, tier/varyant fiyatları çevrilmez
- **% indirim hesabı**: Discount engine raw değer üzerinden % uygular, beklenen TL bazlı indirim olmaz
- **CacheProductPrices**: İlk render'da cache'lenir, kur değişse bile güncellenmez
- **Marketplace plugin'leri** (Faz 3 Trendyol/Hepsiburada): API'lara raw değer push edilir → pazaryeri yanlış fiyat görür
- **Bulk update**: `UPDATE Product SET Price = Price * 1.1` çalışırsa extension etkilenmez

### Persisted Recalc Trade-off

- Kur değişikliği storefront'a anlık yansımaz; günlük TCMB scheduled task ile yansır (admin manuel "Şimdi Güncelle" + recalc çalıştırabilir)
- Sepete eklenince `TurkishCartItemPriceLock` snapshot'ı zaten kur lock'lar — kullanıcı için tutarlı

### Tetiklendiği Yerler

- `ProductSavedConsumer` (admin Save): `EntityInsertedEvent<Product>` + `EntityUpdatedEvent<Product>` → form'dan oku → upsert + `RecalculateAndPersistAsync`
- `ExchangeRateBackgroundTask` (scheduled): TCMB feed çekildikten sonra `RecalculateAllAsync` ile tüm extension'lı ürünler recalc

### Plugin Türkiye-Spesifik Varsayım

`GetEffectiveRateAsync` "TRY"/"TL" identity (1.0) döndürür; başka primary'lerde TCMB rate (TRY karşılığı) kullanılır. Plugin **PrimaryStoreCurrency = TRY** varsayar — site primary'si TRY olmalı (Configuration → Currencies → Türk Lirası "Birincil mağaza para birimi olarak işaretle"). USD/EUR primary'de hesap bozulur.

---

## Karar 24: Form-Integrated Save (Ayrı AJAX Endpoint Değil)

**Tarih**: 2026-05-04
**Karar**: Admin product edit panelinin alanları (`BaseCurrencyId`, `BasePrice`, vb.) `TurkishProductExtension.X` prefix'iyle nopCommerce'in **standart product form'una** katılır. Admin "Kaydet" basınca, `EntityInsertedEvent<Product>`/`EntityUpdatedEvent<Product>` consumer'ı (`ProductSavedConsumer`) form'dan okur ve extension'ı upsert + recalc eder.

### Reddedilen Alternatifler

- **Ayrı AJAX endpoint** (`POST /Admin/TurkishProductExtensionAdmin/Save`): Admin için iki "Kaydet" butonu UX karmaşası yaratıyor
- **JavaScript ile standart Kaydet butonuna hook**: Hacky, framework-friendly değil

### Pattern Referansı

Mevcut `AddressLocationConsumer` aynı pattern'i kullanıyor — adres formundan İl/İlçe/Mahalle alanlarını okuyup `TurkishAddressExtension`'a senkronize eder. Bu yaklaşım nopCommerce konvansiyonuyla uyumlu.

### Loop Koruması

`RecalculateAndPersistAsync` `_productService.UpdateProductAsync` çağırır → `EntityUpdatedEvent<Product>` tekrar publish edilir → consumer tekrar form'dan okur → sonsuz loop. `HttpContext.Items["TurkeyCore.ProductSavedConsumer.Processing"]` flag'i ile loop önlenir; recalc çağrısı flag set, sonra remove.

### "Pasif" Seçeneği Kaldırıldı

Dropdown'da "— (pasif) —" yoktur. Default seçili currency = `CurrencySettings.PrimaryStoreCurrencyId`. Admin primary'i seçerse "döviz çevirmeyen extension" olur (TRY identity, recalc no-op). Bu basitleştirme: her ürün için her zaman bir extension vardır, varlık/yokluk kararı kalkar.

---

## Karar 25: WidgetSettings Self-Healing Consumer

**Tarih**: 2026-05-04
**Karar**: Plugin lifecycle hook'larına (`InstallAsync`/`UpdateAsync`) ek olarak, `IConsumer<AppStartedEvent>` (`WidgetSettingsRepairConsumer`) her app startup'ta `WidgetSettings.ActiveWidgetSystemNames` listesinde plugin systemName'inin varlığını kontrol eder, yoksa otomatik ekler.

### Bağlam

İlk install'da `IWidgetPlugin` arayüzü plugin'e eklendikten sonra, mevcut install için `UpdateAsync` çağrılır ve widget settings'i güncellemesi beklenir. Ancak fiili gözlem: bu güncelleme bir nedenle **sessizce fail oldu** (logs incelenmeden kesin neden bilinmiyor). Sonuç: storefront widget zone'ları çalışmıyor, badge görünmüyor.

### Çözüm

Defensive consumer: her startup'ta tek satırlık check, listede yoksa ekle + log. Yan etkisiz; varsa no-op. Bu pattern, plugin runtime'ında kritik invariant'ı (widget aktivasyonu) garantiler.

### Genelleştirilebilir Kural

Plugin'in çalışması için DB'de bulunması zorunlu state (settings, schedule task'lar, vb.) için sadece `InstallAsync`/`UpdateAsync`'a güvenilmemeli. Defensive `AppStartedEvent` consumer'ı ile self-healing yapılması daha güvenilir. Trade-off: küçük startup overhead, ama plugin update senaryolarında robustness sağlar.

---

## Açık Sorular ve Bekleyen Kararlar

### A1. Author / Şirket Adı

**Durum**: Açık  
**Notu**: Mehmet'in marka/şirket adı netleşmedi. Geçici olarak `[Mehmet — TODO]` kullanılıyor.

### A2. PTT Mahalle Veri Tabanı Kaynağı

**Durum**: Araştırılacak  
**Seçenekler**:
- PTT resmi (güncellik problemi olabilir)
- GitHub topluluk repolarından (örn. `tarihsuz/turkiye-mahalle-il-ilce`)
- Ücretli veri sağlayıcıları (DataProvider, vb.)

### A3. İYS Sandbox Erişimi

**Durum**: Faz 1 sonunda başvurulacak  
**Notu**: Test için erişim alınmalı, plugin başlangıçta mock'la geliştirilmeli

### A4. Pazaryeri Entegrasyonları Önceliği

**Durum**: Faz 3'te netleşecek  
**Sıralama düşüncesi**: Trendyol → Hepsiburada → N11 → Çiçeksepeti

---

## Karar Değişikliği Süreci

Bu kararlardan birini değiştirmek gerektiğinde:

1. Bu dosyaya yeni bir "Karar X" bölümü ekle
2. Eski karar bölümüne `**SUPERSEDED BY [Karar X]**` notu düş
3. Eski kararı silme — geçmiş referans için sakla
4. Git commit: `docs(decisions): Karar X - <kısa açıklama>`
