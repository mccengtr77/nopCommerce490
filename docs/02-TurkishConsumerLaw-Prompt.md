# Claude Code Prompt: Nop.Plugin.Misc.TurkishConsumerLaw

## Görev Tanımı

nopCommerce 4.90 için Türk **tüketici hukuku ve regülasyon uyumluluğu** paketi geliştir. Bu plugin, Türk e-ticaret sitelerinin yasal olarak zorunlu yükümlülüklerini otomatize eder: ETBİS karekod, Mesafeli Satış Sözleşmesi (MSS), Ön Bilgilendirme Formu (ÖBF), 14 gün cayma hakkı, KVKK aydınlatma + açık rıza yönetimi (2026 yeni standart), çerez consent, İYS (İleti Yönetim Sistemi) entegrasyonu, garanti belgesi, tüketici şikayet/hakem heyeti modülü.

## Bağımlılık

Bu plugin **`Nop.Plugin.Misc.TurkeyCore`** paketine bağımlıdır. TurkeyCore'un sağladığı servisleri kullanır: validation (TCKN, VKN), location (il/ilçe), customer extension, tax office service, GİB mükellef sorgu.

## Bağlam — Yasal Çerçeve

- **6502 sayılı Tüketicinin Korunması Hakkında Kanun** + Mesafeli Sözleşmeler Yönetmeliği (RG 27.11.2014/29188, son değişiklik 24.05.2025/32557, yürürlük 01.01.2026)
- **6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK)** + Aydınlatma Yükümlülüğü Tebliği + 18.02.2026 tarihli ve 2026/347 sayılı KVKK İlke Kararı (RG 24.03.2026)
- **6563 sayılı Elektronik Ticaretin Düzenlenmesi Hakkında Kanun** + Ticari İletişim ve Ticari Elektronik İletiler Hakkında Yönetmelik
- **VUK 509 nolu Tebliğ** (e-fatura/e-arşiv) + 535 nolu değişiklik

### 2026 Yılında Yürürlüğe Giren KRİTİK Yenilikler

Plugin tasarımında bu yeni kuralları **mutlaka** uygula:

1. **Cayma hakkı kapsam genişlemesi**: Cep telefonu, akıllı saat, tablet, bilgisayar gibi ürünler yeniden cayma hakkı kapsamına alındı. Eskiden istisnaydılar, artık 14 gün iade edilebilirler.

2. **İade kargo ücreti satıcıda**: Cayma hakkı kullanılırsa iade kargo ücretini **satıcı öder**. Müşteriden iade kargo ücreti talep edilemez.

3. **Arabuluculuk şartı bilgilendirmesi**: ÖBF'de "parasal sınırlar dahilinde tüketici mahkemesinin görevine giren uyuşmazlıklarda mahkemeye başvurmadan önce arabulucuya başvurma şartı bulunduğu" bilgisi zorunlu hale geldi.

4. **KVKK ayrı metin zorunluluğu**: Aydınlatma metni ile açık rıza metni **artık ayrı ayrı** düzenlenmeli. Birleşik metin, "Okudum ve kabul ediyorum + açık rıza veriyorum" gibi tek kutucuk hukuka aykırı. Aydınlatma metni sonunda yalnızca "okudum ve anladım" beyanı olabilir; rıza ifadesi olmaz. KVKK rızası ve ETK ticari ileti onayı da AYRI olmalı.

## Plugin Yapısı

```
Nop.Plugin.Misc.TurkishConsumerLaw/
├── plugin.json                              # Dependency: Misc.TurkeyCore
├── TurkishConsumerLawPlugin.cs
├── TurkishConsumerLawDefaults.cs
├── Infrastructure/
│   ├── DependencyRegistrar.cs
│   ├── NopStartup.cs
│   └── RouteProvider.cs
├── Domain/
│   ├── Etbis/
│   │   └── EtbisRegistration.cs
│   ├── Contracts/
│   │   ├── DistanceSalesContract.cs         # MSS kayıtları
│   │   ├── PreliminaryInfoForm.cs           # ÖBF kayıtları
│   │   └── ContractTemplate.cs              # Şablon yönetimi
│   ├── Withdrawal/
│   │   ├── WithdrawalRequest.cs             # Cayma hakkı talepleri
│   │   ├── WithdrawalStatus.cs              # Enum: Pending/Approved/Rejected/Completed
│   │   └── WithdrawalReason.cs              # Cayma nedeni (opsiyonel)
│   ├── Kvkk/
│   │   ├── DisclosureText.cs                # Aydınlatma metni şablonu
│   │   ├── ExplicitConsentText.cs           # Açık rıza metni
│   │   ├── ConsentRecord.cs                 # Müşterilerin onay kayıtları
│   │   ├── ConsentCategory.cs               # Pazarlama, profilleme, yurtdışı vb.
│   │   ├── DataSubjectRequest.cs            # KVKK m.11 başvurular
│   │   └── DataSubjectRequestType.cs        # Erişim/Silme/Düzeltme/Aktarım
│   ├── Cookies/
│   │   ├── CookieConsent.cs                 # Çerez onayları
│   │   ├── CookieCategory.cs                # Zorunlu/İşlevsellik/Analitik/Pazarlama
│   │   └── CookieDefinition.cs              # Tanımlı çerezler kataloğu
│   ├── Iys/
│   │   ├── IysApprovalRecord.cs             # İYS onay kayıtları
│   │   ├── IysApprovalChannel.cs            # SMS/EMAIL/CALL
│   │   ├── IysApprovalStatus.cs             # ONAY/RET/BEKLEMEDE
│   │   ├── IysApprovalSource.cs             # HS_FIRMA/HS_ARACI/IYS
│   │   └── IysSyncQueueItem.cs              # 3 günlük upload kuyruğu
│   ├── Warranty/
│   │   ├── WarrantyCertificate.cs           # Garanti belgesi
│   │   └── WarrantyManufacturer.cs          # Üretici/ithalatçı bilgisi
│   └── Complaints/
│       ├── ConsumerComplaint.cs
│       └── ComplaintArbitrationStatus.cs
├── Data/
│   └── Migrations/
│       ├── 20260201_InitialSchema.cs
│       ├── 20260202_SeedDefaultTemplates.cs
│       └── 20260203_SeedCookieDefinitions.cs
├── Services/
│   ├── Etbis/
│   │   ├── IEtbisService.cs
│   │   └── EtbisService.cs
│   ├── Contracts/
│   │   ├── IDistanceSalesContractService.cs
│   │   ├── DistanceSalesContractService.cs
│   │   ├── IPreliminaryInfoFormService.cs
│   │   ├── PreliminaryInfoFormService.cs
│   │   ├── IContractTemplateService.cs
│   │   ├── ContractTemplateService.cs
│   │   └── IContractTokenReplacer.cs        # {{ÜrünListesi}} vb. tokenlar
│   ├── Withdrawal/
│   │   ├── IWithdrawalService.cs
│   │   ├── WithdrawalService.cs
│   │   ├── IWithdrawalEligibilityChecker.cs # Cayma hakkı var mı?
│   │   └── WithdrawalEligibilityChecker.cs
│   ├── Kvkk/
│   │   ├── IDisclosureTextService.cs
│   │   ├── IExplicitConsentService.cs
│   │   ├── IConsentRecordService.cs
│   │   ├── IDataSubjectRequestService.cs
│   │   └── IDataRetentionService.cs         # Saklama süresi sonu otomatik anonimleştirme
│   ├── Cookies/
│   │   ├── ICookieConsentService.cs
│   │   └── ICookieDefinitionService.cs
│   ├── Iys/
│   │   ├── IIysApiClient.cs                 # İYS REST API wrapper
│   │   ├── IysApiClient.cs
│   │   ├── IIysApprovalService.cs
│   │   ├── IysApprovalService.cs
│   │   ├── IIysSyncService.cs               # 3 günlük upload job
│   │   └── IysSyncBackgroundTask.cs
│   ├── Pdf/
│   │   ├── IContractPdfGenerator.cs         # MSS/ÖBF için PDF üretimi
│   │   ├── ContractPdfGenerator.cs          # QuestPDF veya iTextSharp
│   │   └── IWarrantyPdfGenerator.cs
│   └── Warranty/
│       └── IWarrantyService.cs
├── Areas/Admin/
│   ├── Controllers/
│   │   ├── EtbisController.cs
│   │   ├── ContractTemplateController.cs    # MSS/ÖBF şablon editörü
│   │   ├── WithdrawalController.cs           # Cayma talepleri yönetimi
│   │   ├── DisclosureTextController.cs       # Aydınlatma metni
│   │   ├── ExplicitConsentController.cs      # Açık rıza metni (AYRI!)
│   │   ├── CookieDefinitionController.cs
│   │   ├── IysSettingsController.cs
│   │   ├── WarrantyController.cs
│   │   ├── DataSubjectRequestController.cs   # KVKK başvuruları
│   │   └── ComplaintController.cs
│   ├── Models/
│   ├── Validators/
│   ├── Factories/
│   └── Views/
├── Components/
│   ├── EtbisQrCode/                         # Footer için karekod
│   ├── CookieConsentBanner/                 # Pop-up
│   ├── ContractAcceptance/                  # Checkout'ta MSS+ÖBF onay
│   ├── KvkkDisclosureNotice/                # Müşteri kayıt formunda
│   ├── ExplicitConsentCheckboxes/           # AYRI rıza checkbox'ları
│   ├── EtkConsentCheckbox/                  # Ticari ileti onayı (AYRI)
│   └── WithdrawalRequestForm/                # Müşteri panelinde
├── Controllers/
│   ├── WithdrawalController.cs              # Public, müşteri talebi için
│   ├── DataSubjectRequestController.cs      # Public, KVKK başvurusu
│   ├── CookieConsentController.cs           # AJAX onay kayıt
│   └── ContractDownloadController.cs        # Müşteri sözleşme PDF indirir
├── Localization/
│   ├── tr-TR.xml
│   └── en-US.xml
├── Templates/                                # Default şablonlar
│   ├── DefaultMssTemplate.html
│   ├── DefaultObfTemplate.html
│   ├── DefaultDisclosureText.html
│   ├── DefaultExplicitConsent.html
│   └── DefaultWarrantyCertificate.html
└── Events/
    ├── OrderPlacedConsumer.cs               # MSS/ÖBF üret + sakla
    ├── OrderRefundedConsumer.cs             # Cayma süreci tamamlandı
    ├── CustomerRegisteredConsumer.cs        # KVKK + ETK onayları kaydet
    └── CustomerDeletedConsumer.cs           # Veri imhası tetikle
```

## Detaylı Geliştirme Görevleri

### 1. ETBİS Modülü

```csharp
public class EtbisRegistration : BaseEntity
{
    public string EtbisCode { get; set; }              // Karekod HTML kodu
    public string MersisNo { get; set; }
    public string TradeName { get; set; }              // Ticari ünvan
    public DateTime RegistrationDate { get; set; }
    public string EtbisVerificationUrl { get; set; }   // eticaret.gov.tr/...
    public bool IsActive { get; set; }
}
```

**Admin sayfası**: ETBİS karekod HTML kodunu yapıştırma alanı, bilgi alanları, footer'da gösterim toggle'ı.

**Storefront**: `EtbisQrCode` view component, footer'da otomatik render. Hem master layout'a otomatik ekle hem de plugin tarafından `IWidgetPlugin` olarak da yayınla.

### 2. MSS + ÖBF Şablon ve Üretim Sistemi

#### Şablon yönetimi

```csharp
public class ContractTemplate : BaseEntity
{
    public ContractTemplateType Type { get; set; }     // Mss/Obf
    public string Name { get; set; }
    public string HtmlContent { get; set; }            // Token'lı HTML
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string LimitedToCustomerRoles { get; set; } // B2B/B2C ayrı şablon
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}
```

#### Token replacer

```csharp
public interface IContractTokenReplacer
{
    Task<string> ReplaceTokensAsync(string template, Order order);
}

// Desteklenen tokenlar:
// {{SatıcıAdı}}, {{SatıcıAdresi}}, {{SatıcıTelefon}}, {{SatıcıKep}}, {{SatıcıMersis}}
// {{AlıcıAdı}}, {{AlıcıAdresi}}, {{AlıcıTelefon}}, {{AlıcıTcKimlik}}, {{AlıcıVergiNo}}
// {{SiparişNo}}, {{SiparişTarihi}}
// {{ÜrünListesi}} (tablo halinde)
// {{AraToplam}}, {{KdvToplam}}, {{KargoBedeli}}, {{İndirim}}, {{GenelToplam}}
// {{TeslimatAdresi}}, {{FaturaAdresi}}
// {{ÖdemeYöntemi}}, {{TaksitBilgisi}}
// {{KargoFirması}}, {{TahminiTeslimSüresi}}
// {{CaymaSüresi}} (default 14 gün), {{CaymaIstisnasıVar}} (true/false)
// {{ArabuluculukBilgisi}} (2026 yeni — sabit metin token'ı)
```

#### PDF üretimi

`QuestPDF` (MIT lisans, modern) veya `iTextSharp 7` (mature) kullan. Türkçe karakter desteği için font embedding.

```csharp
public interface IContractPdfGenerator
{
    Task<byte[]> GenerateMssPdfAsync(DistanceSalesContract contract);
    Task<byte[]> GenerateObfPdfAsync(PreliminaryInfoForm form);
}
```

#### MSS/ÖBF Kayıt Saklama

```csharp
public class DistanceSalesContract : BaseEntity
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string ContractContent { get; set; }        // Token replace edilmiş HTML
    public byte[] PdfContent { get; set; }              // PDF binary (veya path)
    public string AcceptanceIp { get; set; }
    public string AcceptanceUserAgent { get; set; }
    public DateTime AcceptedOnUtc { get; set; }
    public string ContentHash { get; set; }             // SHA-256 (manipulasyon kontrolü)
    public DateTime ExpiresOnUtc { get; set; }          // CreatedOn + 3 yıl
}
```

**3 yıl saklama**: `IDataRetentionService` dolan kayıtları `IsArchived` olarak işaretler veya silinmiş tablo'ya taşır. Saklama süresi konfigüre edilebilir.

#### Checkout entegrasyonu

`ContractAcceptance` view component checkout son adımda:
- ÖBF metnini göster (modal veya inline)
- "Ön bilgilendirme formunu okudum" checkbox (zorunlu)
- "Mesafeli satış sözleşmesini okudum ve kabul ediyorum" checkbox (zorunlu)
- Onaysız "Siparişi tamamla" tıklanamaz
- Onay anında IP, UA, timestamp, hash ile kayıt

### 3. Cayma Hakkı Modülü

#### Eligibility Checker

```csharp
public interface IWithdrawalEligibilityChecker
{
    Task<WithdrawalEligibilityResult> CheckAsync(int orderId);
}

public class WithdrawalEligibilityResult
{
    public bool IsEligible { get; set; }
    public string ReasonIfNotEligible { get; set; }
    public DateTime? DeadlineUtc { get; set; }         // Teslim + 14 gün
    public List<int> EligibleProductIds { get; set; }  // Tüm/bazı ürünler iade edilebilir
}
```

**Cayma hakkı istisnaları** (Yönetmelik m.15) — kategori veya ürün bazlı işaretle:
- Kişiselleştirilmiş ürünler
- Ambalajı açılmış hijyenik ürünler (kozmetik, iç giyim)
- Ambalajı açılmış kitap/CD/yazılım
- Fiyatı finansal piyasalara bağlı ürünler (altın, gümüş)
- Anında teslim edilen dijital ürünler (yazılım indirme, online kurs)
- Konaklama, ulaşım, etkinlik biletleri
- Süreli yayınlar
- Tescili zorunlu taşınırlar

**ÖNEMLİ 2026 GÜNCELLEMESİ**: Cep telefonu, akıllı saat, tablet, bilgisayar artık cayma hakkı kapsamında. Eski istisna listesinden çıkardıklarına dikkat et — bunları "cayma hakkı yok" diye işaretleme.

#### Withdrawal Akışı

```csharp
public enum WithdrawalStatus
{
    Pending,           // Müşteri talep oluşturdu
    Approved,          // Admin onayladı
    Rejected,          // Admin reddetti
    InTransit,         // İade kargosu yola çıktı
    Received,          // İade ürün firmaya ulaştı
    Inspected,         // Ürün incelendi (hasar kontrolü)
    Refunded,          // Para iadesi yapıldı
    Completed,         // Süreç tamamlandı
    Cancelled          // Müşteri iptal etti
}
```

Süreç:
1. Müşteri panelinde "İade Talebi Oluştur" — `WithdrawalRequestForm` component
2. Eligibility check (otomatik)
3. Cayma nedeni seç (zorunlu değil ama analiz için kayıt)
4. Admin onayı (manuel veya otomatik)
5. **İade kargo kodu otomatik üret** (kargo plugin'i ile entegrasyon — IShippingService event)
6. **Kargo ücreti satıcıda** (2026 yeni kural — müşteriden tahsil etme)
7. Ürün geri alındı bildirimi
8. Hasar kontrolü
9. Para iadesi (ödeme plugin'i ile entegrasyon — IPaymentService event)
10. Stok geri ekleme
11. İade faturası üretimi (e-fatura plugin'i ile entegrasyon)

Her aşama için event publish et (`WithdrawalApprovedEvent`, `WithdrawalRefundedEvent` vb.) — diğer pluginler dinleyebilsin.

### 4. KVKK Modülü (2026 Yeni Standart — KRİTİK)

#### Domain Tasarımı

```csharp
public class DisclosureText : BaseEntity
{
    public string Title { get; set; }
    public string Content { get; set; }                // HTML
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveUntilUtc { get; set; }
    public bool IsActive { get; set; }
    public string Version { get; set; }                // v1.0, v1.1 vb.
}

public class ExplicitConsentText : BaseEntity
{
    public string Title { get; set; }
    public string Content { get; set; }                // AYRI metin, aydınlatmadan bağımsız
    public ConsentCategory Category { get; set; }
    public bool IsActive { get; set; }
    public string Version { get; set; }
}

public enum ConsentCategory
{
    Marketing,                  // Pazarlama amaçlı veri işleme
    Profiling,                  // Profilleme/segmentasyon
    InternationalTransfer,      // Yurt dışı aktarım
    ThirdPartySharing,          // Üçüncü taraflarla paylaşım
    SensitiveData,              // Özel nitelikli kişisel veri
    AnalyticsCookies,           // Analitik çerez
    MarketingCookies            // Pazarlama çerezleri
}

public class ConsentRecord : BaseEntity
{
    public int CustomerId { get; set; }
    public ConsentCategory Category { get; set; }
    public bool IsGranted { get; set; }                // True=onay, False=ret/geri çekme
    public int ExplicitConsentTextId { get; set; }     // Hangi metni onayladı
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string ConsentSource { get; set; }          // Registration/Profile/CheckoutFlow
}
```

#### Aydınlatma Metni

KVKK m.10 zorunlu unsurları (Tebliğ m.5):
1. Veri sorumlusunun ve varsa temsilcisinin kimliği
2. Kişisel verilerin hangi amaçla işleneceği
3. İşlenen kişisel verilerin kimlere ve hangi amaçla aktarılabileceği
4. Kişisel veri toplamanın yöntemi ve hukuki sebebi
5. KVKK m.11'de sayılan diğer haklar
6. **YENİ (Temmuz 2024 sonrası)**: Yurt dışı aktarım yapılıyorsa SCC/BCR mekanizması bilgisi

Default şablon hazırla, admin düzenleyebilsin.

#### Açık Rıza Metinleri (AYRI!)

Aydınlatma metninden **tamamen ayrı dokümanlar**. Her bir veri işleme amacı için ayrı metin:

- "Pazarlama amaçlı veri işleme açık rıza metni"
- "Profilleme amaçlı veri işleme açık rıza metni"
- "Yurt dışı aktarım açık rıza metni"
- vb.

Her birinin sonunda: "Yukarıdaki açıklamayı okudum, **açık rızamı veriyorum**." (yalnızca rıza, başka şey değil)

Müşteri kayıt formunda:

```
[Aydınlatma metnini okudum] (info link, sadece açma butonu)

Açık rızalarınız (her biri AYRI checkbox, hiçbiri zorunlu değil):
☐ Pazarlama faaliyetleri için verilerimin işlenmesine açık rızam vardır
☐ Profilleme/segmentasyon için verilerimin işlenmesine açık rızam vardır
☐ Verilerimin yurt dışına aktarılmasına açık rızam vardır

(AYRI bölüm - ETK kapsamı)
☐ Tarafıma SMS yoluyla ticari ileti gönderilmesine onay veriyorum
☐ Tarafıma e-posta yoluyla ticari ileti gönderilmesine onay veriyorum
☐ Tarafıma sesli arama yoluyla ticari ileti gönderilmesine onay veriyorum
```

**ÖNEMLİ**: Hiçbir checkbox sipariş vermek için zorunlu olamaz. "Hizmetin rıza şartına bağlanması" yasağı.

#### KVKK m.11 Veri Sahibi Hakları

Müşteri panelinde "Verilerim" sayfası:
- Verilerimi göster (data export, JSON/PDF)
- Verilerimi düzelt
- Verilerimi sil (account deletion request)
- Verilerimi başka kuruma aktar (data portability)
- Açık rızalarımı geri çek (toggle her kategori için)
- İşleme şartı/amacı bilgisi al

Her başvuru `DataSubjectRequest` olarak kayıt, **30 gün içinde** yanıt zorunlu, takvim sistemli admin uyarıları.

#### Veri Saklama ve İmha

```csharp
public interface IDataRetentionService
{
    Task RunRetentionPolicyAsync();        // Background task
}
```

Her kategori için saklama süresi:
- Üyelik (aktif): süresiz
- Üyelik (silindi): 6 ay (sonra tamamen sil)
- Sipariş kayıtları: 10 yıl (VUK)
- Pazarlama izinleri: rıza geçerliliği + 3 yıl
- Çerez verileri: 12 ay
- MSS/ÖBF: 3 yıl

Süre dolan veriler için **anonimleştirme**:
- Ad, soyad → "Silinmiş Müşteri"
- E-posta → "deleted-{id}@anonymized.local"
- Adres → null
- Sipariş kayıtları kalır (muhasebe için), ama müşteri ilişkisi kopuk

### 5. Çerez Yönetimi

KVKK rehberindeki 4 kategori:

```csharp
public enum CookieCategory
{
    StrictlyNecessary,      // Zorunlu - rıza gerekmez (oturum, güvenlik)
    Functional,             // İşlevsellik - rıza gerekir
    Analytics,              // Analitik (Google Analytics dahil) - rıza gerekir
    Marketing               // Pazarlama/reklam - rıza gerekir
}
```

```csharp
public class CookieDefinition : BaseEntity
{
    public string CookieName { get; set; }            // _ga, sessionId, vb.
    public CookieCategory Category { get; set; }
    public string Provider { get; set; }              // 1st party / Google / Meta
    public string Purpose { get; set; }
    public string Duration { get; set; }              // "1 yıl", "Oturum", "30 gün"
    public bool IsActive { get; set; }
}
```

#### Cookie Consent Banner

İlk ziyarette pop-up:
- "Hepsini Kabul Et" + "Hepsini Reddet" + "Tercihlerimi Yönet"
- "Tercihlerimi Yönet" → 4 toggle (zorunlu hariç 3'ü)
- Reddedilen kategorinin script'leri **çalışmasın** (script tag blocking)
- 12 ay sonra tekrar sor

**Önemli**: Çerez politikası ayrı bir doküman olmalı, KVKK aydınlatma metnini içermez.

#### Script Blocking

`<script type="text/plain" data-cookie-category="analytics">` formatında script'ler. Onay verilince `type="text/javascript"` yapılır.

Veya nopCommerce'in widget sistemi ile entegre — onaysız widget'lar render edilmez.

### 6. İYS (İleti Yönetim Sistemi) Modülü

#### İYS API Client

İYS REST API: `https://api.iys.org.tr/sps/{iys_kodu}/...`

```csharp
public interface IIysApiClient
{
    Task<IysAuthToken> AuthenticateAsync();
    Task<IysApprovalResult> AddApprovalAsync(IysApprovalRequest request);
    Task<IysApprovalResult> AddApprovalsBulkAsync(List<IysApprovalRequest> requests);  // Max 1000
    Task<List<IysApprovalRecord>> QueryApprovalsAsync(string recipient);
    Task<IysApprovalResult> RemoveApprovalAsync(IysApprovalRequest request);  // Ret işlemi
    Task<bool> CheckApprovalAsync(string recipient, IysApprovalChannel channel);
}

public class IysApprovalRequest
{
    public IysApprovalChannel Channel { get; set; }       // SMS/EMAIL/CALL
    public string Recipient { get; set; }                 // Numara veya e-posta
    public IysApprovalStatus Status { get; set; }         // ONAY/RET
    public IysApprovalSource Source { get; set; }         // HS_FIRMA/HS_ARACI/IYS
    public string ConsentDate { get; set; }               // ISO 8601
    public IysRecipientType RecipientType { get; set; }   // BIREYSEL/TACIR
}
```

JWT token cache (5 dakika geçerli).

#### Onay Akışı

Müşteri kayıt formunda 3 ayrı checkbox (SMS/E-posta/Arama). Tıklandığında:
1. `IysApprovalRecord` oluştur
2. Sync kuyruğuna ekle (`IysSyncQueueItem`)
3. Background task her 6 saatte kuyruğu işler — **3 iş günü içinde** İYS'ye upload (yasal süre)

#### İstisna Tag'leme

Bazı iletiler İYS onayı gerektirmez:
- Sipariş bilgilendirmeleri (kargo durumu, teslim, fatura)
- Borç hatırlatma
- Üyelik durumu bildirimleri
- Tahsilat
- Bilgi güncelleme

Bu kategoriler `IsTransactional=true` olarak işaretle, İYS sorgusu bypass edilir.

#### Ret Yönetimi

Müşteri sitenin kendi panelinden ret talep ederse → 3 iş günü içinde İYS'ye bildir.
Müşteri İYS'den ret yaparsa → İYS callback ile bildirir, sistem güncellenir.

### 7. Garanti Belgesi

```csharp
public class WarrantyManufacturer : BaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Mersis { get; set; }
    public bool IsImporter { get; set; }            // İthalatçı mı, üretici mi?
}

public class WarrantyCertificate : BaseEntity
{
    public int ProductId { get; set; }
    public int WarrantyManufacturerId { get; set; }
    public int WarrantyMonths { get; set; }          // 24 ay default
    public string ServiceCenters { get; set; }       // Yetkili servis bilgileri
    public string Conditions { get; set; }
}
```

Sipariş sonrası ürün için garanti belgesi PDF'i otomatik üretilir, faturayla birlikte e-posta gönderilir veya müşteri panelinden indirilebilir.

### 8. Tüketici Şikayet ve Hakem Heyeti Modülü

```csharp
public class ConsumerComplaint : BaseEntity
{
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public ComplaintStatus Status { get; set; }
    public decimal? DisputedAmount { get; set; }      // Parasal tutar
    public bool IsArbitrationRequired { get; set; }   // 2026 yeni kural — gerekli mi?
    public string AdminResponse { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ResolvedOnUtc { get; set; }
}
```

Müşteri panelinde şikayet formu. Disputed amount Tüketici Hakem Heyeti yıllık parasal sınırı altındaysa "Hakem Heyeti'ne başvurun" bilgilendirmesi. Üstündeyse "Arabulucuya başvurun, sonrasında Tüketici Mahkemesi" (2026 yeni kural).

**Parasal sınırlar her yıl güncellenir**, admin'den ayarlanabilir bir setting tutmalı.

### 9. Admin Panel Sayfaları

Tüm modüller için admin sayfaları:

- **ETBİS Ayarları** — karekod kodu, doğrulama URL, footer toggle
- **Sözleşme Şablonları** — MSS/ÖBF için TinyMCE/CKEditor ile HTML editör. Token listesi sidebar'da. Önizleme.
- **Cayma Talepleri** — Liste, durum yönetimi, manuel onay/red, otomatik kargo kodu üretimi butonu
- **KVKK Aydınlatma Metni** — Versiyonlu, eski versiyonlar saklı (geriye dönük denetim için)
- **KVKK Açık Rıza Metinleri** — Her kategori için **ayrı** metin
- **Çerez Tanımları** — Çerez kataloğu CRUD, kategorize
- **İYS Ayarları** — IYS kodu, marka adı, API credentials. Sync queue durumu, hata logları.
- **KVKK Veri Sahibi Başvuruları** — Liste, 30 gün sayacı, yanıt formu
- **Garanti Üreticileri** — Üretici/ithalatçı CRUD
- **Şikayetler** — Müşteri şikayetleri, admin yanıt, hakem heyeti yönlendirmesi

### 10. Storefront View Component'leri

- `EtbisQrCode` — Footer'da
- `CookieConsentBanner` — Modal/toast
- `ContractAcceptance` — Checkout'ta MSS+ÖBF modal
- `KvkkDisclosureNotice` — Kayıt formu üstünde
- `ExplicitConsentCheckboxes` — KVKK rıza checkbox'ları (kayıt formunda, AYRI bölüm)
- `EtkConsentCheckbox` — ETK ticari ileti onayları (AYRI bölüm)
- `WithdrawalRequestForm` — Müşteri panelinde
- `DataSubjectRequestForm` — KVKK m.11 başvuru
- `MyConsentsList` — Müşterinin geçmiş rızaları, geri çekme butonu

### 11. Event Consumer'lar

```csharp
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    // 1. MSS ve ÖBF üret
    // 2. Token replace et
    // 3. PDF oluştur
    // 4. DistanceSalesContract ve PreliminaryInfoForm kaydet
    // 5. Müşteriye e-posta ile gönder
    // 6. Garanti belgesi gerekli ürünler için garanti üret
}

public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    // 1. KVKK aydınlatma okundu mu kontrol
    // 2. Açık rıza checkbox'larını ConsentRecord'a kaydet
    // 3. ETK ticari ileti onaylarını IysApprovalRecord'a kaydet
    // 4. İYS sync kuyruğuna ekle
}

public class CustomerDeletedConsumer : IConsumer<CustomerDeletedEvent>
{
    // 1. Saklama süresi gerekiyorsa anonimleştir
    // 2. Aktif rızaları çek
    // 3. İYS'ye ret bildir
}
```

## Test Gereksinimleri

`Nop.Plugin.Misc.TurkishConsumerLaw.Tests` projesi:

- TCKN/VKN validasyon testleri (TurkeyCore'dan miras)
- Withdrawal eligibility checker — tüm istisna senaryoları
- Token replacer — özel karakterli ürün adları, uzun listeler
- KVKK consent kayıt — IP, UA, hash doğrulama
- Cookie consent — script blocking testi
- İYS sync — mock client ile 3 günlük SLA testi
- PDF generator — Türkçe karakter doğru render

**E2E testler** (Playwright veya Selenium):
- Misafir kullanıcı kayıt akışı (KVKK + ETK ayrı checkbox'lar görünüyor mu?)
- Checkout akışı (MSS/ÖBF onayı zorunlu mu?)
- Cayma talebi oluşturma akışı
- Cookie consent toggle
- Veri sahibi başvuru formu

## plugin.json İçeriği

```json
{
  "Group": "Misc",
  "FriendlyName": "Türkiye Tüketici Hukuku - Yasal Uyumluluk Paketi",
  "SystemName": "Misc.TurkishConsumerLaw",
  "Version": "1.0.0",
  "SupportedVersions": ["4.90"],
  "Author": "[Senin Adın/Şirketin]",
  "DisplayOrder": 2,
  "FileName": "Nop.Plugin.Misc.TurkishConsumerLaw.dll",
  "Description": "Türk tüketici hukuku uyumluluk paketi: ETBİS karekod, Mesafeli Satış Sözleşmesi (MSS), Ön Bilgilendirme Formu (ÖBF), 14 gün cayma hakkı (2026 güncel), KVKK aydınlatma + açık rıza yönetimi (2026 yeni standart), çerez consent, İYS entegrasyonu, garanti belgesi, tüketici şikayet yönetimi.",
  "DependsOnSystemNames": ["Misc.TurkeyCore"]
}
```

## Bağımlılıklar (NuGet)

- `QuestPDF` (MIT, modern PDF library) — PDF üretimi için. Türkçe font: Roboto veya OpenSans embed et.
- `HtmlAgilityPack` — HTML token replace ve sanitization
- `System.Net.Http` — İYS API ve dış servisler
- TurkeyCore'dan miras alınanlar (FluentValidation, vb.)

## Kalite Kriterleri

1. **Yasal doğruluk**: Her bir özellik ilgili kanun maddesine atıf yapan kod yorumu içersin (örnek: `// Mesafeli Sözleşmeler Yönetmeliği m.5/(c)`)
2. **Audit log**: Tüm yasal işlemler (rıza alındı, sözleşme oluşturuldu, cayma kullanıldı) audit log tablosuna kayıt
3. **Geriye dönük uyumluluk**: Eski sözleşme şablon versiyonları saklanmalı (denetim için)
4. **Dijital imza altyapısı**: Onay kayıtlarında SHA-256 hash. İleride elektronik imza eklenebilir.
5. **Çoklu dil**: Sistem Türkçe odaklı ama metinler EN için de tutuldu (yabancı müşteri durumu)
6. **GDPR uyumluluğu**: KVKK aslında GDPR ile büyük ölçüde uyumlu. AB müşterileri için extra GDPR özelliklerine açık tasarım.

## Önemli Notlar

- **Yasal sorumluluk reddi**: README ve admin panel uyarısı: "Bu plugin yasal uyumluluğu kolaylaştırır ancak hukuki danışmanlık yerine geçmez. Şüpheli durumlarda avukatınıza danışın."
- **Şablon güncelleme**: Default şablonların yıllık gözden geçirme uyarısı admin'e
- **2026 KVKK İlke Kararı**: Mevcut müşteriler için **migrate** akışı: Tek metinli onayları "geçersiz" işaretle, müşteriden tekrar onay iste (login sırasında banner)
- **Hassas veri şifreleme**: TurkeyCore'un `IEncryptionService`'i ile sözleşme PDF içindeki TCKN/VKN şifreli sakla

## Kaynaklar

- Mesafeli Sözleşmeler Yönetmeliği: https://www.mevzuat.gov.tr/mevzuat?MevzuatNo=20237
- KVKK: https://www.kvkk.gov.tr
- KVKK Aydınlatma Yükümlülüğü Tebliği: https://www.kvkk.gov.tr/Icerik/2033/Aydinlatma-Yukumlulugu-
- KVKK 2026/347 İlke Kararı (24.03.2026 RG): Aydınlatma ve açık rıza ayrımı
- ETBİS: https://eticaret.gov.tr
- İYS: https://iys.org.tr — API dokümantasyonu için ileti yönetim sistemi geliştirici portali
- 6502 Tüketici Kanunu: https://www.mevzuat.gov.tr (kanun no 6502)
- 6563 E-Ticaret Kanunu: https://www.mevzuat.gov.tr (kanun no 6563)

## Definition of Done

- [ ] TurkeyCore plugin'i kurulu ve çalışır durumda olmalı
- [ ] Tüm migration'lar sorunsuz uygulanır
- [ ] Default şablonlar yüklendi (MSS, ÖBF, KVKK, çerez, garanti)
- [ ] 4 ayrı onay kategorisi storefront'ta ayrı görünüyor (KVKK m.10 metni + KVKK rızaları + ETK onayları + Çerez consent)
- [ ] Test coverage ≥ %75
- [ ] Cayma akışı tüm aşamalarıyla test edildi
- [ ] İYS sync 3 günlük SLA içinde çalışıyor (mock test)
- [ ] PDF üretimi Türkçe karakterleri doğru render ediyor
- [ ] Admin paneli tüm sayfaları gezilebilir
- [ ] README, CHANGELOG, LEGAL_DISCLAIMER dosyaları
- [ ] Build artifact: `Nop.Plugin.Misc.TurkishConsumerLaw.zip`
