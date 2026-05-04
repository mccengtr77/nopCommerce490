# CLAUDE.md

Bu dosya, **nopCommerce 4.90 fork'u** üzerinde Türkiye lokalizasyonu eklentilerini geliştiren bir projedir. Claude Code her oturum başlangıcında bu dosyayı okur ve projenin tüm bağlamını edinir.

## Proje Amacı

Bu fork, nopCommerce platformunu Türkiye e-ticaret pazarına özelleştiren bir dizi açık kaynak plugin'i barındırır. Mevcut paralı Türkiye eklentilerine alternatif olarak, **tek seferlik ödeme + kaynak kod erişimi** modeliyle ürünleştirilecektir.

## Geliştirici Profili

- **Teknik altyapı**: 15+ yıl .NET/C# tecrübesi, NetSis ERP entegrasyonları, Türk e-fatura/XSLT (YATIRIMTESVIK profili dahil), SQL Server administration, Flutter mobil, DevExpress XAF/WinForms
- **Geliştirme ortamı**: M4 Pro MacBook 48GB RAM, VMware Fusion ile Windows VM (nopCommerce build için), Windows Server
- **Dil**: Türkçe (kod ve teknik yorumlar dahil); kullanıcıya yönelik UI metinleri Türkçe + İngilizce
- **Çalışma modeli**: Tek geliştirici, paralel olarak hakediş/birim fiyat yazılımı ve mevcut ERP projeleri yürütülüyor

## Hedef Pazar ve Kullanıcılar

- **Birincil**: Türkiye'de kendi e-ticaret sitesini kuran KOBİ'ler ve geliştiriciler
- **İkincil**: nopCommerce ile çalışan Türk yazılım ajansları
- **Rakip**: Mevcut paralı eklentiler (yıllık lisans modeli, sürüm uyumsuzluk sorunları)
- **Farklılaştırıcımız**: Tek seferlik ödeme + kaynak kod + 1 yıl ücretsiz güncelleme

## Teknoloji Yığını

- **Platform**: nopCommerce 4.90.4+
- **Framework**: ASP.NET Core 9, .NET 9 SDK (9.0.100+)
- **ORM**: LinqToDB (nopCommerce'in tercihi), Entity Framework Core
- **Veritabanı**: SQL Server 2019+ (birincil), PostgreSQL/MySQL (ikincil destek)
- **Frontend**: Razor Pages, Bootstrap 5 (admin panel), jQuery (storefront), TinyMCE (rich-text editor)
- **PDF**: QuestPDF (MIT lisans, modern; iTextSharp AGPL nedeniyle kullanılmaz)
- **Validasyon**: FluentValidation
- **Test**: NUnit (nopCommerce konvansiyonu) + Moq + FluentAssertions
- **Build**: Visual Studio 2022 (Windows VM'de) veya `dotnet build` (CLI)

## Solution Yapısı

Bu solution **nopCommerce'in kendi `NopCommerce.sln` dosyasıdır**. Türkiye eklentilerimiz şu konumlara yerleştirilir:

```
nopCommerce/                                    # Fork kökü
├── CLAUDE.md                                   # Bu dosya
├── docs/                                       # Türkiye projesi dokümanları
│   ├── PROJECT_DECISIONS.md                    # Stratejik kararlar
│   ├── LEGAL_RESEARCH.md                       # Türkiye yasal çerçeve özeti
│   ├── PLUGIN_ROADMAP.md                       # Tüm planlanan eklentiler
│   ├── 01-TurkeyCore-Prompt.md                 # TurkeyCore plugin spec
│   ├── 02-TurkishConsumerLaw-Prompt.md         # ConsumerLaw plugin spec
│   ├── 03-TurkeyCore-Progress.md               # TurkeyCore fiili ilerleme raporu
│   └── 04-TurkishConsumerLaw-Progress.md       # ConsumerLaw fiili ilerleme raporu
├── src/
│   ├── Plugins/
│   │   ├── Nop.Plugin.Misc.TurkeyCore/         # FAZ 1A — MVP + köprü tamam
│   │   ├── Nop.Plugin.Misc.TurkishConsumerLaw/ # FAZ 1B — 6/9 modül tamam
│   │   ├── Nop.Plugin.Payments.Iyzico/         # FAZ 2 (planlandı)
│   │   ├── Nop.Plugin.Payments.PayTR/          # FAZ 2 (planlandı)
│   │   └── ...                                  # Yol haritasına bakın
│   ├── Tests/
│   │   ├── Nop.Plugin.Misc.TurkeyCore.Tests/           # 233 test
│   │   └── Nop.Plugin.Misc.TurkishConsumerLaw.Tests/   # 113 test
│   └── ... (nopCommerce kendi kodu)
```

**Önemli**: nopCommerce upstream güncellemelerini düzenli olarak `git pull upstream main` ile çek. `CLAUDE.md` ve `docs/` ile `src/Plugins/Nop.Plugin.Misc.Turkey*` dışındaki dosyalara dokunma.

## Aktif Geliştirme Önceliği

**FAZ 1 (Şu an): Yasal Temel — ✅ Uçtan uca canlı (macOS + VM SQL Server 2019, plugin v1.0.11 — 2026-05-04)**

1. **`Nop.Plugin.Misc.TurkeyCore`** — ✅ **install edildi, MVP altyapısı + köprü servisleri + döviz bazlı ürün fiyatı tamam**
   - Spec: `docs/01-TurkeyCore-Prompt.md`
   - **Fiili ilerleme**: `docs/03-TurkeyCore-Progress.md` (233 test geçer, ~3.700 satır)
   - **Tamamlananlar**: TCKN/VKN/IBAN/GSM validasyon, 81 il seed, TCMB kur servisi, Customer/Address extension, GİB mock, **döviz bazlı ürün fiyatı** (USD/EUR vb. ürün başına — persisted recalc + sepet kur lock + storefront badge)
   - Sıradaki Faz 1B: ilçe/mahalle/vergi dairesi CSV seed, KDV Tax Provider, Türkçe currency formatter, FluentValidation
2. **`Nop.Plugin.Misc.TurkishConsumerLaw`** — ✅ **install edildi, 6/9 modül uçtan uca tamam**
   - Spec: `docs/02-TurkishConsumerLaw-Prompt.md`
   - **Fiili ilerleme**: `docs/04-TurkishConsumerLaw-Progress.md` (113 test geçer, ~8.300 satır)
   - **Tamamlananlar**: ETBİS, Çerez Consent, KVKK Aydınlatma+Rıza+Audit, KVKK m.11 Başvuru, Withdrawal (cayma), Contracts (MSS+ÖBF)
   - **Kalan**: İYS API, Warranty, Complaints, MyConsentsList, PDF üretimi (QuestPDF), retention auto-purge

**Toplam test durumu**: 346 test geçer (TurkeyCore 233 + ConsumerLaw 113)

### Ürün Döviz Bazlı Fiyat — Mimari Notları (Karar 23–25)

İthalatçı/elektronik bayi senaryosu için. Tasarım:

- **Persisted recalc** (Karar 23): `IPriceCalculationService` decorator yerine; admin Save + TCMB scheduled task `Product.Price`'ı **DB'ye yazar**. Search/filter/discount/marketplace tek doğru fiyatı görür.
- **Form-integrated Save** (Karar 24): `TurkishProductExtension.X` prefix'i ile alanlar standart product form'una katılır; `ProductSavedConsumer` (`EntityInsertedEvent` + `EntityUpdatedEvent`) yakalar ve `HttpContext.Items` flag'i ile loop koruması yapar
- **Sepet kur lock**: `TurkishCartItemPriceLock` snapshot + `GetShoppingCartItemUnitPriceEvent` consumer (`StopProcessing=true`)
- **Storefront badge** (Pavilion): `ProductPriceBottom` + `ProductBoxAddinfoMiddle` zone'larına hook
- **Self-healing** (Karar 25): `WidgetSettingsRepairConsumer` (`AppStartedEvent`) plugin systemName'i `ActiveWidgetSystemNames`'da yoksa otomatik ekler — `InstallAsync`/`UpdateAsync` sessizce fail ettiyse kurtarır

**Varsayım**: Plugin Türkiye-spesifik. **PrimaryStoreCurrency = TRY** olmalı (Configuration → Currencies → "Türk Lirası → Birincil mağaza para birimi olarak işaretle"). USD/EUR primary'de hesap bozulur.

**Deployment notları (ilk install'da öğrenilenler — çok önemli)**:
- nopCommerce 4.90'da connection string `App_Data/appsettings.json` → `ConnectionStrings.ConnectionString` (eski `dataSettings.json` yok). Setup wizard'ı tetiklemek için bu değeri boş yap.
- Plugin entity'sindeki **enum property'leri** için `NopEntityBuilder<T>` builder'da **mutlaka explicit** `.WithColumn(nameof(X)).AsInt32().NotNullable()` ekle — `Create.TableFor<T>()` plugin assembly'sindeki enum'ları otomatik üretmiyor (bkz. PROJECT_DECISIONS Karar 22)
- Cascade riski olan FK'lar için (master-detail-detail zinciri) `.ForeignKey<T>()` yerine `.Indexed()` kullan (bkz. Karar 21)
- **Plugin update için `plugin.json` Version artırılmalı**: nopCommerce `UpdatePluginsAsync` sadece version değişikliğinde migration + `UpdateAsync` çalıştırır. Yeni feature için version artırma şart.
- **Widget plugin install kontrolü**: `IWidgetPlugin` arayüzü mevcut bir plugin'e sonradan eklendiyse, install zaten gerçekleşmiş olduğu için `InstallAsync` çalışmaz; `UpdateAsync`'e güvenmek de riskli (bizim case'imizde sessiz fail). `WidgetSettingsRepairConsumer` (Karar 25) bu durumu kurtarır.
- Plugin DLL stale olursa: `bin/`, `obj/`, `Presentation/Nop.Web/Plugins/Misc.*` klasörlerini sil + DB drop + setup wizard tekrar
- VM'deki SQL Server'a Mac'ten bağlanmak için TCP/IP registry üzerinden enable edilmeli (`MSSQL15.<instance>\MSSQLServer\SuperSocketNetLib\Tcp\IPAll` → `TcpPort=1433`, `TcpDynamicPorts=""`)

**FAZ 2-4 için** `docs/PLUGIN_ROADMAP.md` dosyasına bak.

## Kritik Yasal Bilgi (2026 Güncel)

Geliştirme sırasında kod yorumlarında ilgili kanun maddesine atıf yap. Detaylar `docs/LEGAL_RESEARCH.md` içindedir. Üç ana yasal çerçeve:

1. **6502 Tüketici Kanunu** + Mesafeli Sözleşmeler Yönetmeliği (1.1.2026 yürürlük)
   - Cep tel/tablet/bilgisayar **cayma hakkı kapsamında** (yeni)
   - İade kargo ücreti **satıcı öder** (yeni)
   - ÖBF'de arabuluculuk şartı bilgilendirmesi zorunlu (yeni)

2. **6698 KVKK** + 24.03.2026 tarihli 2026/347 İlke Kararı
   - Aydınlatma metni ile açık rıza metni **AYRI** dokümanlar olmalı (yeni)
   - "Okudum kabul ediyorum + açık rıza" tek kutucukta birleştirilemez
   - KVKK rızası ile ETK ticari ileti onayı da ayrı

3. **6563 E-Ticaret Kanunu** + ETBİS + İYS
   - ETBİS karekod footer'da zorunlu
   - İYS'ye onay 3 iş günü içinde upload zorunlu

## Geliştirme Standartları

### Kod Stili

- nopCommerce'in `.editorconfig` ve `Directory.Build.props` kurallarına uy
- Tüm IO işlemleri `async/await` — `Task` dönen metodlar `Async` suffix'i alır
- `<Nullable>enable</Nullable>` set et, nullable reference type kullan
- Hardcoded string yok — her şey `_localizationService.GetResourceAsync()` ile

### Naming

- Plugin sınıfları: `<Plugin>Plugin.cs`
- Defaults sınıfı: `<Plugin>Defaults.cs` (system name, route names, vb.)
- Service interface: `I<Service>Service`
- Domain entity: PascalCase, `BaseEntity` miras
- Türkçe alanlar Türkçe ad alabilir (örn. `TcKimlikNo`, `VergiDairesiId`) — ama public API'de İngilizce dokümante et

### Güvenlik

- Hassas veri (TCKN, VKN, IBAN) **şifreli sakla** — nopCommerce'in `IEncryptionService`'ini kullan
- Hassas veri **loglanmaz**
- PII alanlar admin paneli dışında gözükmez
- SHA-256 hash ile sözleşme manipülasyonu kontrolü

### Performans

- Sık erişilen veri (il/ilçe/mahalle, vergi daireleri) `IStaticCacheManager` ile 24 saat cache
- TCMB kur ve GİB mükellef sorguları cache'lensin
- Büyük seed migration'ları batch insert (1000'er kayıt)

### Multi-tenant ve Multi-language

- Tüm setting'ler `LimitedToStores` bayrağını dikkate alsın
- Her plugin TR ve EN lokalizasyonla gelsin
- Gelecekte AR, RU eklenebilecek şekilde generic tasarla

## Referans Plugin'ler (nopCommerce'den)

Yeni plugin geliştirmeye başlarken **mutlaka önce** şu mevcut plugin'leri incele:

- `src/Plugins/Nop.Plugin.Tax.FixedOrByCountryStateZip` — Tax provider pattern
- `src/Plugins/Nop.Plugin.Misc.SendinBlue` — External API entegrasyonu, background task, event consumer
- `src/Plugins/Nop.Plugin.Payments.PayPalCommerce` — Ödeme plugin pattern (FAZ 2 için)
- `src/Plugins/Nop.Plugin.Shipping.FixedByWeightByTotal` — Kargo plugin pattern (FAZ 2 için)

Bu plugin'lerin yapısını, naming convention'ını ve event consumer pattern'ini örnek al.

## Çalışma Akışı

### Yeni Plugin Geliştirme Sırası

1. **Önce** ilgili `docs/<Plugin>-Prompt.md` dosyasını oku
2. **Sonra** ilgili nopCommerce referans plugin'ini incele
3. **Aşamalı geliştirme**: plugin.json → BasePlugin → Infrastructure → Domain → Migrations → Services → Admin → Storefront → Tests
4. Her aşamada `dotnet build` ve `dotnet test` çalıştır
5. Test coverage hedefi: ≥ %75 (TurkeyCore için ≥ %80)

### Git Konvansiyonu

Commit mesajları:
```
feat(turkey-core): TCKN ve VKN validasyon servisleri eklendi
fix(consumer-law): MSS PDF Türkçe karakter render düzeltildi
docs(roadmap): Faz 2 ödeme eklentileri eklendi
test(turkey-core): IBAN validasyonu için 30 test case
```

Plugin tag'leri:
```
turkey-core-v0.1-validation
turkey-core-v1.0-mvp
consumer-law-v0.1-etbis
```

### Build ve Deploy

Plugin'i nopCommerce'e yüklemek için:
1. `dotnet publish src/Plugins/Nop.Plugin.Misc.TurkeyCore -c Release`
2. Çıktıyı `Nop.Plugin.Misc.TurkeyCore.zip` olarak paketle
3. nopCommerce admin → Configuration → Local plugins → Upload
4. Plugin'i Install et
5. Restart application

**Önemli**: zip içine `bin/` ve `obj/` klasörleri **gönderilmez**.

## Ticari Model

Her plugin üç katmanda dağıtılacak:

1. **Free Tier** — Açık kaynak, GitHub'da, basic özellikler
2. **Pro Bundle** — Tek seferlik ödeme, tüm özellikler, kaynak kod, 1 yıl güncelleme
3. **Enterprise** — Pro + özel kurulum + öncelikli destek

Lisans: MIT (Free Tier) + Commercial License (Pro/Enterprise için ayrı dosya)

## Bilinen Kısıtlamalar ve Risk Alanları

- **GİB e-fatura mükellef sorgu**: Public API yok, özel entegratörler (e-Logo, Mysoft, İzibiz) üzerinden gidilir. Mock'lu başla.
- **PTT mahalle veri tabanı**: Resmi indirilebilir CSV güncel olmayabilir. GitHub topluluk repolarına bak (`turkish-postal-codes`, `turkiye-il-ilce`)
- **İYS API sandbox**: Erişim için başvuru gerekli, başlangıçta mock'la
- **TCMB XML formatı**: Bazen değişebilir, parse hatası yakala
- **PDF Türkçe karakter**: Default font'lar bozabilir. Roboto veya OpenSans Turkish embedding zorunlu.

## Sık Sorulan Sorular

**S: Yeni bir plugin'e başlarken hangi sırayı izlemeliyim?**
C: 1) `docs/<Plugin>-Prompt.md` oku, 2) referans plugin incele, 3) plugin.json + BasePlugin'den başla, 4) aşamalı ilerle.

**S: Türkçe karakterler veritabanında nasıl saklanmalı?**
C: SQL Server'da `nvarchar` (Unicode). Migration'larda `IsUnicode(true)` mutlaka.

**S: nopCommerce'in mevcut entity'lerini değiştirmeli miyim?**
C: **Hayır.** Paralel `*Extension` tabloları kullan (örn. `TurkishCustomerExtension`). Upstream merge'leri zorlaştırma.

**S: Plugin'lerin birbirine bağımlılığını nasıl yönetmeli?**
C: `plugin.json`'da `DependsOnSystemNames`. Çekirdek paketten servisleri DI ile al.

**S: KDV oranlarını nereden alıyorum?**
C: GİB resmi tebliğleri. `TurkeyCore`'un `Services/Tax/KdvOranlari.cs` sabitleri. Yıllık güncellenir.

**S: Komisyon/ücret hesaplamaları kimde?**
C: Ödeme provider'larında (Faz 2). Çekirdek paket sadece KDV ve para birimi.

## Önemli Linkler

- nopCommerce GitHub: https://github.com/nopSolutions/nopCommerce
- nopCommerce Docs: https://docs.nopcommerce.com
- TCMB Kur Feed: https://www.tcmb.gov.tr/kurlar/today.xml
- KVKK Resmi: https://www.kvkk.gov.tr
- ETBİS: https://eticaret.gov.tr
- İYS: https://iys.org.tr
- GİB: https://www.gib.gov.tr

## Sorulara Yanıt Verme Tarzı

Bu projede çalışırken:

- Türkçe iletişim kur (kod yorumları dahil mümkünse)
- Yasal konularda **ilgili kanun maddesini** mutlaka belirt
- Belirsizlik varsa varsayım yapma, sor
- "Bu Türkiye'ye özgü mü?" sorusu varsa açıkla
- Mevcut nopCommerce pattern'lerini bozmadan ekle
- Senin (Mehmet) NetSis ERP, e-fatura/XSLT, SQL Server geçmişinden faydalan — gerek yoksa baştan anlatmaya gerek yok
