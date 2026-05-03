# 04 — TurkishConsumerLaw İlerleme Raporu

Bu dosya `Nop.Plugin.Misc.TurkishConsumerLaw` plugin'inin **fiili durumunu** belgeler. Spec için `02-TurkishConsumerLaw-Prompt.md` dosyasına bak; bu dosya neyin **gerçekten yapıldığını** anlatır.

**Son güncelleme**: 2026-05-03
**Durum**: ✅ İlk uçtan uca deployment başarılı — 9 modülden 6'sı uçtan uca tamam, yasal-kritik modüller hazır
**Bağımlılık**: ✅ TurkeyCore (FAZ 1A)
**Kod metriği**: ~8.300 satır plugin kodu + ~1.500 satır test (113 test, %100 geçer)

### Deployment Düzeltmeleri (2026-05-03)

İlk install denemesinde tüm builder'larda enum kolonları eksik oluştuğu (`Invalid column name 'Category' / 'Scope' / 'Status' / 'Type' / 'Reason' / 'Source'`) tespit edildi. **7 builder, 11 enum kolonu** için explicit `.AsInt32().NotNullable()` eklendi:

- `CookieDefinitionBuilder` → `Category`
- `ConsentRecordBuilder` → `Scope`, `Source`
- `ExplicitConsentTextBuilder` → `Scope`
- `DataSubjectRequestBuilder` → `RequestType`, `Status`
- `WithdrawalRequestBuilder` → `Reason`, `Status`
- `ContractTemplateBuilder` → `Type`
- `GeneratedContractBuilder` → `Type`

Detay: [PROJECT_DECISIONS.md Karar 22](PROJECT_DECISIONS.md).

**Düzeltme sonrası migration sonuçları**:
- `CookieDefinition` tablosu + 12 default cookie seed
- `DisclosureText` + `ExplicitConsentText` tabloları + 1 disclosure + 7 explicit consent metni
- `ConsentRecord` tablosu (append-only audit log)
- `DataSubjectRequest` + `WithdrawalRequest` tabloları
- `ContractTemplate` tablosu + 2 default şablon (MSS + ÖBF)
- `GeneratedContract` tablosu

Tüm enum kolonları `INT NOT NULL` olarak doğru oluştu, runtime'da plugin admin Configure sayfaları sorunsuz açılıyor.

---

## Modül Durum Özeti

| # | Modül | Backend | Storefront UI | Admin UI | Test |
|---|---|:-:|:-:|:-:|:-:|
| 1 | **ETBİS** (6563 sayılı Kanun) | ✅ | ✅ | ✅ | 10 |
| 2 | **Çerez Consent** (KVKK + GDPR) | ✅ | ✅ | ✅ | 17 |
| 3 | **KVKK Aydınlatma + Açık Rıza** (2026/347 İlke Kararı) | ✅ | ✅ | ✅ | 23 |
| 4 | **KVKK m.11 Veri Sahibi Başvuruları** | ✅ | ✅ | ✅ | 16 |
| 5 | **Withdrawal — Cayma Hakkı** (14 gün, 2026 değişiklikleri) | ✅ | ✅ | ✅ | 31 |
| 6 | **Contracts — MSS + ÖBF** | ✅ | ✅ | ✅ (PDF yerine HTML) | 16 |
| 7 | İYS API | ❌ | ❌ | ❌ | — |
| 8 | Warranty | ❌ | ❌ | ❌ | — |
| 9 | Complaints | ❌ | ❌ | ❌ | — |

**Toplam test**: 113 (TurkishConsumerLaw) + 188 (TurkeyCore) = **301 test geçer**

---

## Modül 1: ETBİS

[Domain/Etbis/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Domain/Etbis) · [Services/Etbis/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Services/Etbis) · [Components/EtbisQrCode/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Components/EtbisQrCode)

- `EtbisRegistration` entity — MERSİS/Ünvan/QR HTML/Verification URL/multi-store
- `IEtbisService` — multi-store fallback, cache'li
- `EtbisQrCode` ViewComponent — footer karekod (`Html.Raw`, trusted input)
- Admin: Configure sayfasında ETBİS kartı (QR HTML textarea + meta alanlar)

## Modül 2: Çerez Consent

[Domain/Cookies/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Domain/Cookies) · [Services/Cookies/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Services/Cookies) · [Components/CookieConsentBanner/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Components/CookieConsentBanner)

- `CookieCategory` enum (Necessary/Functional/Analytics/Marketing)
- `CookieDefinition` — site çerez kataloğu
- `CookieConsent` — audit log (SHA-256 hash, IP/UA, policy version)
- **12 default tanım seed**: nopCommerce + GA + Facebook Pixel + plugin'in kendi consent cookie'si
- `CookieConsentBanner` — modal banner, kategori toggle'lar, vanilla JS
- AJAX endpoint: `POST /api/cookie-consent`
- Admin: Cookie definitions CRUD

## Modül 3: KVKK (Aydınlatma + Açık Rıza + Audit)

[Domain/Kvkk/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Domain/Kvkk) · [Services/Kvkk/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Services/Kvkk)

**2026/347 İlke Kararı uyumu** — Aydınlatma ve açık rıza AYRI:
- `ConsentScope` enum (4 KVKK + 3 ETK, numerik aralık ayırımı)
- `ConsentSource` (Registration/Profile/Banner/Withdrawal/AdminManual/Checkout)
- `DisclosureText` (versiyonlu, tek aktif)
- `ExplicitConsentText` (scope başına, opt-in default)
- `ConsentRecord` (append-only audit, SHA-256, IP/UA)
- **Default seed**: 1 disclosure + 7 explicit consent metni
- `KvkkDisclosureNotice` ViewComponent — sadece "okudum" beyanı (rıza yok)
- `ExplicitConsentCheckboxes` — `mode="kvkk"` veya `mode="etk"` ayrı bölümler
- AJAX endpoint: `POST /api/kvkk-consent`
- **CustomerRegisteredConsumer** — kayıt formundaki tcl-consent[] otomatik kaydeder
- Admin: KVKK metin editörü (disclosure tek form + explicit consent listesi/edit)

## Modül 4: KVKK m.11 Veri Sahibi Başvuruları

[Domain/DataSubjectRequests/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Domain/DataSubjectRequests) · [Services/DataSubjectRequests/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Services/DataSubjectRequests)

- `DataSubjectRequestType` (KVKK m.11 — 8 hak + Portability)
- `DataSubjectRequestStatus` (Submitted/InReview/AdditionalInfoRequested/Approved/Rejected/Withdrawn)
- `DataSubjectRequest` — IP/UA/Email/StoreId, **30 gün deadline** (KVKK m.13/(2))
- `IDataSubjectRequestService.LegalResponseDays = 30` constant (yasa değişirse test bilinçli kırılır)
- `DataSubjectRequestForm` ViewComponent — anonim+kayıtlı başvuru, 9 başvuru türü dropdown
- AJAX endpoint: `POST /api/data-subject-request`
- Admin: liste (süre aşımı uyarı banner, renk kodlu satırlar) + yanıt formu

## Modül 5: Withdrawal — Cayma Hakkı

[Domain/Withdrawal/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Domain/Withdrawal) · [Services/Withdrawal/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Services/Withdrawal)

**2026 Mesafeli Sözleşmeler Yönetmeliği değişiklikleri uyumu**:
- `WithdrawalStatus` (9-state state machine — Pending → Approved → InTransit → Received → Inspected → Refunded → Completed; Rejected/Cancelled terminal)
- `WithdrawalReason` (8 sebep, opsiyonel)
- `IWithdrawalEligibilityChecker` — 14 gün + Shipment.DeliveryDateUtc fallback
- `IsValidTransition(from, to)` public static — 8 valid path, terminal'lardan geri dönüş yok
- 2026 kuralları: cep tel/tablet/bilgisayar artık kapsamda; **iade kargo ücretini satıcı öder**
- `WithdrawalRequestForm` ViewComponent — sipariş bazlı, eligibility ön kontrol
- AJAX endpoints: eligibility GET + submit POST (TOCTTOU re-check)
- Admin: 9-status filtre listesi + dinamik state machine UI (next-state'ler `IsValidTransition`'dan üretilir)

## Modül 6: Contracts — MSS + ÖBF

[Domain/Contracts/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Domain/Contracts) · [Services/Contracts/](../src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Services/Contracts)

- `ContractTemplateType` (Mss/Obf)
- `ContractTemplate` — admin yönetilen, versiyonlu
- `GeneratedContract` — sipariş anında üretilen instance, **3 yıl saklama**, SHA-256
- **2 default şablon seed** — Türkçe yasal metin + 2026 değişiklikleri (cep tel cayma kapsamında, satıcı kargo, arabuluculuk şartı)
- `IContractTokenReplacer` — Türkçe regex `\{\{(?<name>[A-Za-zĞğÜüŞşİıÖöÇç_]...)\}\}`, **25 token** desteği, bilinmeyen token leave-as-is
- `IContractGenerationService` — Order'dan ContractTokenContext build, idempotent üretim
- **OrderPlacedConsumer** — sipariş anında otomatik MSS+ÖBF üretim
- `ContractAcceptance` ViewComponent — checkout'ta 2 ayrı checkbox (ÖBF "okudum" + MSS "okudum kabul ediyorum")
- `ContractDownloadController` — kayıtlı müşteri kendi sözleşmesini HTML olarak indirir
- Admin: şablon CRUD (monospace HTML editor + token referans alert) + üretilenler liste/inline view

**Atlanan**: PDF üretimi (QuestPDF nuget gerek). HTML zaten yasal olarak geçerli.

---

## Tasarım Kararları (Bu Plugin'e Özgü)

### TurkeyCore Köprü Kullanımı
ConsumerLaw, TurkeyCore'un `ITurkishCustomerService` üzerinden TCKN/VKN okuyor (şifreli saklanan veriyi plain olarak alıp HTML token replace'te kullanıyor). Bu sayede ConsumerLaw'un kendi şifreleme yükü yok.

### Single GeneratedContract vs Spec'in 2 Entity'si
Spec'te `DistanceSalesContract` ve `PreliminaryInfoForm` ayrı entity önerilmiş. DRY tercihi ile **tek tablo + Type field** yapıldı. Domain class içinde yorum satırıyla bu sapma açıkça not edildi.

### State Machine Pure Function
`WithdrawalService.IsValidTransition(from, to)` static metod — UI'da next-state listesi otomatik üretiliyor. Test'te 14 case [TestCase] ile valid + invalid yollar doğrulanıyor.

### 2026/347 İlke Kararı Uygulaması
- Aydınlatma metni içinde "rıza" sözcüğü YOK (default şablon kontrol edildi)
- KVKK m.10 + KVKK rıza + ETK ileti onayı 3 ayrı bölüm
- Her scope için ayrı checkbox + ayrı `ConsentRecord` satırı
- Default işaretsiz (opt-in, KVKK m.5 gereği)

### 30 Gün Deadline Constant Test
`DataSubjectRequestService.LegalResponseDays.Should().Be(30)` — yasa değişirse test bilinçli kırılır → developer farkındalık.

### TOCTTOU Önlemi (Withdrawal)
Form yükleme + submit arası eligibility değişebilir. Submit endpoint'inde re-check var.

### CustomerRegisteredConsumer Defensive Defaults
- Form context yoksa (API kayıt) sessizce çık
- Bilinmeyen scope int'leri otomatik atılır
- Hata olursa kayıt akışını bloklamaz, log'a yazar

### Token Replacer Bilinmeyen Token Bırakır
`{{Bilinmeyen}}` token replace edilmeden kalır. Admin metni preview'da görür, hatayı düzeltir. Sessizce silmek yerine bu davranış admin'in farkına varmasını sağlar.

---

## Storefront UI Bütünlüğü: 7/9 ViewComponent

| ViewComponent | Tema Entegrasyonu |
|---|---|
| ✅ EtbisQrCode | Footer'a `@await Component.InvokeAsync("EtbisQrCode")` |
| ✅ CookieConsentBanner | Layout'a global ekleme |
| ✅ KvkkDisclosureNotice | Kayıt formu üstüne |
| ✅ ExplicitConsentCheckboxes (mode="kvkk") | Kayıt formuna |
| ✅ ExplicitConsentCheckboxes (mode="etk") | Kayıt formuna |
| ✅ DataSubjectRequestForm | Public sayfa (örn. /kvkk-basvuru) |
| ✅ WithdrawalRequestForm | Müşteri sipariş detayında |
| ✅ ContractAcceptance | Checkout son adımında |
| ❌ MyConsentsList | Müşteri panelinde (sonraki sprint) |

---

## Admin Sayfaları

Configure ana sayfasında 5 hızlı erişim butonu + 4 kart (ETBİS/KVKK/Çerez/Cayma ayarları). Detaylı yönetim için ayrı sayfalar:

- `/Admin/DataSubjectRequest/List` — KVKK m.11 başvuru yönetimi
- `/Admin/Kvkk/Texts` — Aydınlatma + açık rıza metin editörü
- `/Admin/CookieDefinition/List` — Çerez tanım CRUD
- `/Admin/Withdrawal/List` — Cayma talepleri state machine
- `/Admin/Contracts/Templates` — MSS/ÖBF şablon CRUD
- `/Admin/Contracts/Generated` — Üretilen sözleşmeler liste

---

## Yapılmayanlar — Bilinçli Bırakılan Eksikler

### Faz 1B (Diğer Sprint'ler)
- ❌ **İYS REST API** — `IIysApiClient`, sync queue, 3 günlük SLA background task
- ❌ **Warranty** — Garanti belgesi yönetimi, üretici/ithalatçı CRUD
- ❌ **Complaints** — Müşteri şikayet, hakem heyeti yönlendirme
- ❌ **MyConsentsList** ViewComponent — müşterinin geçmiş rızaları + geri çekme
- ❌ **PDF üretimi** — QuestPDF nuget eklenmesi (HTML zaten yasal yeterli)
- ❌ **OrderRefundedConsumer / CustomerDeletedConsumer** — cayma tamamlandı / KVKK silme bildirimi
- ❌ **3 yıl retention auto-purge** — `IDataRetentionService` cron task
- ❌ **Kargo plugin entegrasyonu** — withdraw için otomatik tracking number

### Atlanan Pragmatik Tercihler
- E-posta otomatik gönderim (DSR yanıt, withdrawal status değişimi) — admin'in manuel mail göndermesi gerekiyor şimdi
- Inline TinyMCE editor (HTML textarea kullanılıyor)
- DataTables.NET grid (basit HTML table)
- Excel import/export
- E2E testler (Playwright/Selenium)

---

## Build ve Test Çalıştırma

```bash
# Build
dotnet build src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Nop.Plugin.Misc.TurkishConsumerLaw.csproj \
    -p:OutDir=/tmp/consumerlaw-build/ \
    -p:SolutionDir=/Users/mehmet/Projects/NopCommerce/nopCommerce490/src/

# Test
dotnet test src/Tests/Nop.Plugin.Misc.TurkishConsumerLaw.Tests/Nop.Plugin.Misc.TurkishConsumerLaw.Tests.csproj \
    -p:SolutionDir=/Users/mehmet/Projects/NopCommerce/nopCommerce490/src/
```

---

## Sıradaki Adımlar

1. **İYS API entegrasyonu** — REST client + sync queue + 3 günlük SLA background task
2. **Warranty** modülü — orta karmaşıklık
3. **Complaints** modülü — düşük karmaşıklık
4. **PDF üretimi** — QuestPDF nuget (ayrı sprint)
5. **Faz 1 kapanışı**: Windows VM'de gerçek install/uninstall test, README.md, CHANGELOG.md, build/zip script

Faz 1A (TurkeyCore) ve Faz 1B'nin **yasal-kritik bölümü** (ConsumerLaw 6/9 modül) tamam. Plugin sahaya çıkmaya hazır seviyede.
