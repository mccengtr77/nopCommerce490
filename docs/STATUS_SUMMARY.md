# Proje Durum Özeti

**Son güncelleme**: 2026-05-03
**Faz**: 1 (Yasal Temel) — yasal-kritik bölüm büyük ölçüde tamam
**Deployment**: ✅ İlk başarılı uçtan uca deployment (2026-05-03, macOS + VM SQL Server 2019)

---

## Hızlı Bakış

| Metrik | Değer |
|---|---|
| Toplam plugin | 2 (TurkeyCore + TurkishConsumerLaw) |
| Toplam kod | ~11.100 satır plugin + ~2.600 satır test |
| Toplam test | **319 test, %100 geçer** (TurkeyCore 206 + ConsumerLaw 113) |
| Build durumu | ✅ Temiz, 0 hata |
| Runtime durumu | ✅ nopCommerce 4.90.4'e install edildi, tüm migration'lar başarılı |
| Yasal-kritik modül | 11/12 uçtan uca (sadece İYS API ve PDF üretimi yok) |

### İlk Deployment Doğrulaması (2026-05-03)

- **Stack**: macOS (Mac M4 Pro) + VMware Fusion Windows VM üzerindeki SQL Server 2019 (`SQLDEVELOPER` named instance, port 1433, TCP/IP registry'den enable edilerek)
- **Connection**: `Data Source=172.16.227.128;Initial Catalog=Nop490;...`
- **Setup wizard**: `App_Data/appsettings.json` → `ConnectionStrings.ConnectionString=""` ile tetiklendi (4.90'da `dataSettings.json` artık yok)
- **Migration sonuçları**:
  - TurkeyCore: 7 tablo + 81 il seed
  - TurkishConsumerLaw: 6 tablo + 12 cookie + 8 KVKK metni (1 disclosure + 7 explicit consent) + 2 contract template (MSS+ÖBF)
  - Tüm enum kolonları (`Category`, `Scope`, `RequestType`, `Status`, `Type`, `Reason`, `Source`, `MusteriTipi`) `INT NOT NULL` olarak doğru oluşturuldu
- **Runtime**: `http://localhost:63820` üzerinde HTTP 200, plugin'ler admin panel'de görünür

---

## Plugin Durum Tablosu

### 1. Nop.Plugin.Misc.TurkeyCore

📋 Spec: [`01-TurkeyCore-Prompt.md`](01-TurkeyCore-Prompt.md) | 📊 İlerleme: [`03-TurkeyCore-Progress.md`](03-TurkeyCore-Progress.md)

| Kapsam | Durum |
|---|:-:|
| Plugin iskeleti (plugin.json, Plugin.cs, Defaults, Settings, NopStartup) | ✅ |
| Domain (7 entity + enum) + 7 mapping builder | ✅ |
| Schema migration + 81 il seed | ✅ |
| TCKN/VKN/IBAN/GSM validasyon servisi (95 test) | ✅ |
| Lokasyon servisi (il/ilçe/mahalle, cache'li) | ✅ |
| TaxOffice servisi | ✅ |
| TCMB döviz kuru (XML feed + scheduled task + admin "Şimdi Güncelle") | ✅ |
| TurkishCustomer/Address servisleri (köprü, encrypt/decrypt) | ✅ |
| GİB Mukellef sorgu mock (white/black list + heuristic) | ✅ |
| AJAX API (5 endpoint: provinces/districts/neighborhoods/validate-tckn/validate-vkn) | ✅ |
| Admin Configure (4 kart) | ✅ |
| Storefront ViewComponent'leri (cascading dropdown + customer type) | ✅ |
| Lokalizasyon (TR ~80 + EN kritik) | ✅ |
| **Test sayısı** | **206** |
| ❌ İlçe/mahalle/vergi dairesi seed (CSV import aracı bekliyor) | — |
| ❌ KDV Tax Provider | — |
| ❌ Türkçe currency formatter | — |
| ❌ FluentValidation validator'ları | — |

### 2. Nop.Plugin.Misc.TurkishConsumerLaw

📋 Spec: [`02-TurkishConsumerLaw-Prompt.md`](02-TurkishConsumerLaw-Prompt.md) | 📊 İlerleme: [`04-TurkishConsumerLaw-Progress.md`](04-TurkishConsumerLaw-Progress.md)

| Modül | Backend | Storefront | Admin UI | Test |
|---|:-:|:-:|:-:|:-:|
| **ETBİS** (6563 sayılı Kanun) | ✅ | ✅ | ✅ | 10 |
| **Çerez Consent** (KVKK + GDPR, 4 kategori) | ✅ | ✅ | ✅ | 17 |
| **KVKK Aydınlatma + Açık Rıza** (2026/347 İlke Kararı) | ✅ | ✅ | ✅ | 23 |
| **KVKK m.11 Veri Sahibi Başvuruları** (30 gün deadline) | ✅ | ✅ | ✅ | 16 |
| **Withdrawal — Cayma** (14 gün, 2026 değişiklikleri) | ✅ | ✅ | ✅ | 31 |
| **Contracts MSS+ÖBF** (token replacer + OrderPlacedConsumer) | ✅ | ✅ | ✅ (HTML, PDF yok) | 16 |
| İYS API (3 günlük SLA) | ❌ | ❌ | ❌ | — |
| Warranty | ❌ | ❌ | ❌ | — |
| Complaints | ❌ | ❌ | ❌ | — |
| **Toplam test** | | | | **113** |

---

## 2026 Yasal Değişikliklerin Uygulanma Durumu

| Değişiklik | Yasal Kaynak | Plugin Uygulaması |
|---|---|:-:|
| Cep tel/tablet/bilgisayar cayma kapsamında | Yönetmelik m.15 (24.05.2025/32557) | ✅ Default ÖBF/MSS şablonunda belirtilmiş |
| İade kargo ücreti satıcı öder | Yönetmelik m.13/(2) | ✅ `WithdrawalSettings.ReturnShippingPaidBySeller=true` + storefront mesaj |
| Arabuluculuk şartı bilgilendirmesi | Yönetmelik m.5 | ✅ `ContractTokenReplacer.DefaultMediationNotice` + ÖBF şablonunda token |
| Aydınlatma + açık rıza ayrı doküman | KVKK 2026/347 İlke Kararı | ✅ `DisclosureText` + `ExplicitConsentText` ayrı entity'ler, ayrı UI bölümleri |
| KVKK rızası ile ETK onayı ayrı | KVKK 2026/347 | ✅ `ConsentScope` numerik aralık ayrımı + `mode="kvkk"`/`mode="etk"` |
| ETBİS karekod footer'da zorunlu | 6563 + ETBİS Yönetmeliği | ✅ `EtbisQrCode` ViewComponent + admin Configure |
| İYS'ye 3 iş günü içinde upload | 6563 | ❌ İYS modülü henüz yapılmadı |
| KVKK m.11'e 30 gün içinde yanıt | KVKK m.13/(2) | ✅ `LegalResponseDays = 30` constant + admin uyarı banner |

---

## Yasal Çerçeve Atıfları

Her servis kod yorumlarında ilgili kanun maddesine atıf yapıyor. Üç ana çerçeve:

- **6502 Tüketici Kanunu** + Mesafeli Sözleşmeler Yönetmeliği — Withdrawal modülü
- **6698 KVKK** + Aydınlatma Tebliği + 2026/347 İlke Kararı — KVKK modülleri
- **6563 E-Ticaret Kanunu** + ETBİS Yönetmeliği + İYS — ETBİS, Çerez, İYS modülleri

Detaylı yasal araştırma: [`LEGAL_RESEARCH.md`](LEGAL_RESEARCH.md)

---

## Faz Sonraki Adımlar

### Faz 1B Kapanışı (Bu plugin'lerin tamamı)
1. **TurkeyCore**: ilçe/mahalle/vergi dairesi seed import aracı + KDV Tax Provider + currency formatter
2. **ConsumerLaw**: İYS API + Warranty + Complaints + PDF üretimi (QuestPDF) + retention auto-purge
3. **Dağıtım**: Windows VM'de install/uninstall test, README, CHANGELOG, zip build script
4. Git tag: `turkey-core-v1.0` + `consumer-law-v1.0`

### Faz 2 (Ticari Çekirdek)
3 numaralı plugin: **Iyzico** ödeme provider'ı (yol haritasında).
Spec yok henüz — geliştirme öncesi `02-TurkishConsumerLaw-Prompt.md` benzeri spec oluşturulacak.

Tüm yol haritası: [`PLUGIN_ROADMAP.md`](PLUGIN_ROADMAP.md)

---

## Karar Kayıtları

22 stratejik/teknik karar belgelenmiş — [`PROJECT_DECISIONS.md`](PROJECT_DECISIONS.md)

Son eklenenler (Karar 11–22):
- Karar 11: TCMB XML parser saf statik fonksiyon
- Karar 12: Lokalizasyon static dictionary, tr-TR.xml yok
- Karar 13: Test setup'ta `Singleton<AppSettings>` init
- Karar 14: AJAX endpoint'leri auth'suz public
- Karar 15: Onay tabloları append-only audit log
- Karar 16: Tek `GeneratedContract` tablosu vs spec'in 2 entity'si
- Karar 17: KVKK 2026/347 İlke Kararı uygulaması (ayrı bölümler)
- Karar 18: Withdrawal state machine pure static function
- Karar 19: PDF yerine HTML (geçici, QuestPDF Faz 1B'de)
- Karar 20: Bilinmeyen token leave-as-is
- **Karar 21**: Multi-cascade FK yerine soft-FK (Indexed) — `TurkishAddressExtension`'da Province/District/Neighborhood referansları SQL Server "multiple cascade paths" hatasını önlemek için sadece `.Indexed()`
- **Karar 22**: Plugin enum kolonları için **explicit `.AsInt32()`** zorunlu — nopCommerce'in `Create.TableFor<T>()` enum property'lerini plugin assembly'lerinde otomatik üretmiyor; tüm `NopEntityBuilder<T>` builder'larında enum'lar açıkça tanımlanmalı

---

## Build / Test Çalıştırma (macOS)

```bash
# TurkeyCore build + test
cd src
dotnet build Plugins/Nop.Plugin.Misc.TurkeyCore/Nop.Plugin.Misc.TurkeyCore.csproj \
    -p:OutDir=/tmp/turkeycore-build/ -p:SolutionDir=$PWD/

dotnet test Tests/Nop.Plugin.Misc.TurkeyCore.Tests/Nop.Plugin.Misc.TurkeyCore.Tests.csproj \
    -p:SolutionDir=$PWD/

# ConsumerLaw build + test
dotnet build Plugins/Nop.Plugin.Misc.TurkishConsumerLaw/Nop.Plugin.Misc.TurkishConsumerLaw.csproj \
    -p:OutDir=/tmp/consumerlaw-build/ -p:SolutionDir=$PWD/

dotnet test Tests/Nop.Plugin.Misc.TurkishConsumerLaw.Tests/Nop.Plugin.Misc.TurkishConsumerLaw.Tests.csproj \
    -p:SolutionDir=$PWD/
```

> macOS'ta `-p:SolutionDir=$PWD/` parametresi şart — Windows VM'de Visual Studio default davranışıyla bu sorun yok.
