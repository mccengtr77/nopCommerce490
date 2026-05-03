# PLUGIN_ROADMAP.md

Bu doküman, Türkiye nopCommerce eklentileri projesinin **tüm planlanan plugin'lerinin** yol haritasını içerir. Geliştirme sırası, bağımlılıklar, tahmini süreler ve ticari katmanlandırma burada belirlenir.

## Öncelik Tablosu

| # | Plugin | Faz | Süre (yarı zamanlı) | Önem | Müşteri Talebi | Durum |
|---|--------|-----|---------------------|------|----------------|-------|
| 1 | TurkeyCore | 1 | 4-5 hafta | ⭐⭐⭐⭐⭐ | Olmazsa olmaz (foundation) | ⚙️ MVP (bkz. `03-TurkeyCore-Progress.md`) |
| 2 | TurkishConsumerLaw | 1 | 6-7 hafta | ⭐⭐⭐⭐⭐ | Yasal zorunluluk | ⚙️ 6/9 modül uçtan uca (bkz. `04-TurkishConsumerLaw-Progress.md`) |
| 3 | Iyzico | 2 | 2 hafta | ⭐⭐⭐⭐⭐ | %80+ | ⏸ |
| 4 | PayTR | 2 | 1.5 hafta | ⭐⭐⭐⭐⭐ | %60+ | ⏸ |
| 5 | TurkishCarriers | 2 | 4-6 hafta | ⭐⭐⭐⭐⭐ | %95+ | ⏸ |
| 6 | EFatura | 2 | 3-4 hafta | ⭐⭐⭐⭐⭐ | Yasal zorunluluk | ⏸ |
| 7 | TaksitTablosu | 2 | 1.5 hafta | ⭐⭐⭐⭐ | %80+ | ⏸ |
| 8 | Param | 3 | 1.5 hafta | ⭐⭐⭐ | %30+ | ⏸ |
| 9 | TrendyolConnector | 3 | 3 hafta | ⭐⭐⭐⭐ | %70+ | ⏸ |
| 10 | HepsiburadaConnector | 3 | 3 hafta | ⭐⭐⭐⭐ | %50+ | ⏸ |
| 11 | AlışverişKredisi | 3 | 2 hafta | ⭐⭐⭐ | %30+ | ⏸ |
| 12 | TurkishMarketing | 3 | 2 hafta | ⭐⭐⭐ | %40+ | ⏸ |
| 13 | N11Connector | 4 | 3 hafta | ⭐⭐⭐ | %30+ | ⏸ |
| 14 | CicekSepetiConnector | 4 | 2 hafta | ⭐⭐ | %20+ | ⏸ |
| 15 | NetsisErpConnector | 4 | 4 hafta | ⭐⭐⭐⭐ | Mehmet için kritik | ⏸ |
| 16 | LogoErpConnector | 4 | 4 hafta | ⭐⭐⭐ | %40+ | ⏸ |
| 17 | HızlıTeslimat | 4 | 2 hafta | ⭐⭐⭐ | %50+ | ⏸ |
| 18 | B2BTurkey | 4 | 3 hafta | ⭐⭐⭐ | %30+ | ⏸ |
| 19 | SadakatPuanı | 4 | 2 hafta | ⭐⭐ | %40+ | ⏸ |
| 20 | UrunYorumYildiz | 4 | 2 hafta | ⭐⭐⭐ | %60+ | ⏸ |

**Toplam tahmini süre (tek geliştirici, yarı zamanlı): ~14-18 ay**  
**Tek geliştirici, tam zamanlı: ~7-9 ay**

---

## FAZ 1 — Yasal Temel (Şu An)

### 1. Nop.Plugin.Misc.TurkeyCore — ⚙️ MVP altyapısı tamam

**Detaylı spec**: `docs/01-TurkeyCore-Prompt.md`
**Fiili durum**: `docs/03-TurkeyCore-Progress.md` (152 test, 2.800 satır kod)

**Sağladıkları**:
- ✅ TC Kimlik No, Vergi No, IBAN, GSM validasyonu (95 test)
- ✅ 81 il seed (ilçe/mahalle/vergi dairesi: admin import bekliyor)
- ✅ Customer/Address domain extension'ları (TCKN, VKN, MERSİS, KEP)
- ✅ TCMB döviz kuru servisi (XML feed, scheduled task, manuel "Şimdi Güncelle")
- ✅ Admin Configure sayfası (4 kart: veri durumu, ayarlar, TCMB, GİB)
- ✅ Storefront ViewComponent'leri (cascading dropdown, jQuery dependency yok)
- ✅ AJAX endpoint'leri (provinces / districts / neighborhoods / validate-tckn / validate-vkn)
- ✅ Türkçe lokalizasyon (~80 string) + İngilizce kritik string'ler
- ❌ KDV Tax Provider (Faz 1B)
- ❌ GİB mükellef sorgu servisi (mock impl Faz 1B)
- ❌ Türkçe para/tarih formatter (Faz 1B)
- ❌ Event consumer'lar, FluentValidation validator'ları (Faz 1B)
- ❌ İlçe/mahalle/vergi dairesi seed verisi — admin import aracı

**Bağımlılık**: Yok (foundation)
**Tier**: Free (MIT)

### 2. Nop.Plugin.Misc.TurkishConsumerLaw — ⚙️ 6/9 modül uçtan uca tamam

**Detaylı spec**: `docs/02-TurkishConsumerLaw-Prompt.md`
**Fiili durum**: `docs/04-TurkishConsumerLaw-Progress.md` (113 test, ~8.300 satır kod)

**Sağladıkları**:
- ✅ ETBİS karekod entegrasyonu (footer ViewComponent + admin Configure)
- ✅ Mesafeli Satış Sözleşmesi (MSS) + Ön Bilgilendirme Formu (ÖBF) — token replacer + OrderPlacedConsumer + 25 token + checkout acceptance
- ✅ 14 gün cayma hakkı (2026 yeni kurallarla — cep tel kapsamda, satıcı kargo) — eligibility checker + 9-state machine + admin yönetim
- ✅ KVKK aydınlatma + açık rıza yönetimi (2026/347 İlke Kararı — ayrı metin standardı, 4 KVKK + 3 ETK scope)
- ✅ KVKK m.11 veri sahibi başvurusu (30 gün deadline, anonim+kayıtlı, admin yanıt akışı)
- ✅ Çerez consent (4 kategori, banner, 12 default tanım seed, audit trail)
- ✅ CustomerRegisteredConsumer — kayıt formundan otomatik consent kaydı
- ❌ İYS API entegrasyonu (3 günlük SLA) — sonraki sprint
- ❌ Garanti belgesi yönetimi — sonraki sprint
- ❌ Tüketici şikayet ve hakem heyeti modülü — sonraki sprint
- ❌ Veri saklama ve imha politikası (auto-purge cron) — sonraki sprint
- ❌ PDF üretimi (QuestPDF) — HTML zaten yasal yeterli, sonraki sprint

**Bağımlılık**: TurkeyCore (`ITurkishCustomerService` ile TCKN/VKN köprüsü kullanıyor)
**Tier**: Free (MIT) + Pro (gelişmiş özellikler)

---

## FAZ 2 — Ticari Çekirdek

Faz 1 tamamlanınca müşteriye satılabilir minimum paket için.

### 3. Nop.Plugin.Payments.Iyzico

**Sağladıkları**:
- 3D Secure 2.0
- Taksit tablosu (banka bazlı)
- Kart saklama (One-Click Payment)
- iyzico Korumalı Alışveriş
- Marketplace alt üye işyeri (multi-vendor için)
- İade/iptal API
- Webhook handler
- Sandbox/Live toggle

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

### 4. Nop.Plugin.Payments.PayTR

**Sağladıkları**:
- iFrame ödeme akışı
- 3D Secure 2.0
- BIN sorgu API (kart bazlı taksit gösterimi)
- Linkle ödeme alma (B2B için)
- Webhook + bildirim URL
- İade/iptal

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

### 5. Nop.Plugin.Shipping.TurkishCarriers

Tek plugin altında ortak `AbstractTurkishShippingProvider` + her firma için provider.

**Sağladıkları**:
- **Aras Kargo** — REST API
- **Yurtiçi Kargo** — SOAP API
- **MNG Kargo / DHL eCommerce** — REST API
- **PTT Kargo** — REST API
- **Sürat Kargo** — REST API
- **HepsiJet** — REST API
- Desi/kg hesaplama
- Otomatik etiket basımı (PDF/ZPL/PNG)
- Toplu kargo işleme
- Kapıdan alım (pickup) talebi
- Tahmini teslimat süresi
- Müşteriye SMS/e-posta bildirim

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

### 6. Nop.Plugin.Misc.EFatura

**Sağladıkları**:

Özel entegratör adapter'ları:
- e-Logo
- Mysoft
- İzibiz
- Foriba/Sovos
- DijitalPlanet
- Türkkep
- N11Faturam

Özellikler:
- Otomatik fatura kesimi (`OrderPaidEvent` consumer)
- E-Fatura / E-Arşiv otomatik yönlendirme (GİB mükellef sorgusuyla)
- UBL-TR XML üretimi (XSLT şablon)
- YATIRIMTESVIK profili desteği (Mehmet'in mevcut bilgisi)
- E-İrsaliye üretimi
- Toplu fatura kesimi
- Fatura PDF gösterimi (müşteri panelinde)
- İade faturası otomasyonu
- E-SMM desteği
- GTİP No alanı (ihracat için)

**Bağımlılık**: TurkeyCore + TurkishConsumerLaw  
**Tier**: Pro

### 7. Nop.Plugin.Misc.TaksitTablosu

**Sağladıkları**:
- BIN bazlı taksit gösterimi (kart numarası girince hangi banka, hangi taksit, oran)
- Kategori bazlı taksit (elektronik en fazla 12, gıda 0)
- Banka bazlı taksit yasakları (KDV indirimli/ÖTV ürünlerde)
- Vade farkı/komisyon yansıtma seçeneği

**Bağımlılık**: TurkeyCore + Iyzico veya PayTR  
**Tier**: Pro

---

## FAZ 3 — Genişletilmiş Pazar

Faz 2 sonrası rekabet avantajı kazanmak için.

### 8. Nop.Plugin.Payments.Param

**Sağladıkları**:
- TP_WMD_UCD (3D Secure)
- TP_Islem_Odeme_OnProv (provizyon)
- KK_Sakla (kart saklama)
- Komisyon iadesi API

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

### 9. Nop.Plugin.Marketplace.Trendyol

**Sağladıkları**:
- Ürün listeleme + onay süreci
- Stok/fiyat senkronizasyonu (real-time)
- Sipariş çekme (pull)
- Trendyol kargo etiket entegrasyonu
- Komisyon hesaplama
- İade yönetimi

**Bağımlılık**: TurkeyCore  
**Tier**: Enterprise

### 10. Nop.Plugin.Marketplace.Hepsiburada

**Sağladıkları**:
- Ürün listeleme
- HepsiJet entegrasyonu
- Hepsiburada Premium ürün
- Stok/fiyat sync

**Bağımlılık**: TurkeyCore + TurkishCarriers  
**Tier**: Enterprise

### 11. Nop.Plugin.Misc.AlışverişKredisi

**Sağladıkları**:
- Hangikredi, BKM API entegrasyonu
- Banka onay akışı (TCKN + GSM)
- Çoklu banka teklif karşılaştırma
- Sözleşme PDF

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

### 12. Nop.Plugin.Misc.TurkishMarketing

**Sağladıkları**:
- WhatsApp Business API (sipariş bildirimleri)
- NetGSM, İletiMerkezi, Mutlu Cell SMS
- SMS OTP doğrulama
- Akakçe XML feed
- Google Merchant Center Türkiye feed

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

---

## FAZ 4 — Niş ve Enterprise

### 13. Nop.Plugin.Marketplace.N11

**Bağımlılık**: TurkeyCore + EFatura (N11Faturam)  
**Tier**: Enterprise

### 14. Nop.Plugin.Marketplace.CicekSepeti

**Bağımlılık**: TurkeyCore  
**Tier**: Enterprise

### 15. Nop.Plugin.Erp.Netsis

**Sağladıkları**:
- NetSis ERP cari/stok/sipariş senkronizasyonu
- Çift yönlü stok güncelleme
- Sipariş aktarımı
- Müşteri eşleştirme

**Bağımlılık**: TurkeyCore + EFatura  
**Tier**: Enterprise (özel kurulum)

**Özel not**: Mehmet'in mevcut NetSis tecrübesi burada altın değerinde. Bu plugin onun kendi müşterilerine de doğrudan satılabilir.

### 16. Nop.Plugin.Erp.Logo

**Sağladıkları**:
- Logo Tiger (MFN integration)
- Logo İşbaşı (cloud) entegrasyonu
- Cari/stok/fatura sync

**Bağımlılık**: TurkeyCore  
**Tier**: Enterprise

### 17. Nop.Plugin.Shipping.HızlıTeslimat

**Sağladıkları**:
- Aynı gün teslimat kuralları (saat, lokasyon, stok)
- Ertesi gün teslimat (Yarın Kapında)
- Randevulu teslimat (gün/saat seçimi)
- Teslimat noktası seçimi (PUDO — kargo otomatları, AVM noktaları)
- Kapı önüne bırak / komşuya teslim

**Bağımlılık**: TurkeyCore + TurkishCarriers  
**Tier**: Pro

### 18. Nop.Plugin.Misc.B2BTurkey

**Sağladıkları**:
- Bayi/dealer rol yönetimi
- Bayi özel fiyat listesi
- Açık hesap (cari) ile satış
- Toplu sipariş Excel ile
- Vade farkı hesaplama
- Yıllık ciro indirimi

**Bağımlılık**: TurkeyCore  
**Tier**: Enterprise

### 19. Nop.Plugin.Misc.SadakatPuanı

**Sağladıkları**:
- Trendyol/Hepsiburada Premium benzeri abonelik
- Puan kazanma/harcama
- Ödül kataloğu
- Doğum günü/yıldönümü kuponu

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

### 20. Nop.Plugin.Misc.UrunYorumYildiz

**Sağladıkları**:
- Trendyol benzeri detaylı yorum (boy/kilo bilgisi giysi için)
- Fotoğraflı yorum
- Video yorum
- "Bu satıcıdan satın alanlar" filtresi
- AI bazlı yorum özetleyici (Mehmet'in Ollama deneyimi)

**Bağımlılık**: TurkeyCore  
**Tier**: Pro

---

## Bağımlılık Grafiği

```
                    TurkeyCore (foundation)
                          |
        +-----------------+------------------+
        |                                    |
TurkishConsumerLaw                     [tüm diğer pluginler]
        |
        +-> EFatura
        +-> Iyzico (rıza kayıt için)
        +-> Diğerleri (KVKK için)


Iyzico, PayTR, Param ───┐
                        ├──> TaksitTablosu
                        │
                        ├──> AlışverişKredisi


TurkishCarriers ───────┐
                        ├──> HızlıTeslimat
                        ├──> Hepsiburada (HepsiJet için)


EFatura ───────────────┐
                        ├──> Netsis ERP
                        ├──> Logo ERP
                        ├──> N11 (N11Faturam)


[Tümü] ─────────────────> B2BTurkey, SadakatPuanı, UrunYorumYildiz, TurkishMarketing
```

---

## Ticari Katmanlandırma Özeti

### Free Tier (Açık Kaynak — MIT)

- TurkeyCore (basic özellikler)
- TurkishConsumerLaw (basic — sadece zorunlu yasal uyumluluk)
- Iyzico veya PayTR'den biri (basic)
- TurkishCarriers'tan 1 firma (Aras veya Yurtiçi)

### Pro Bundle (~Tek seferlik fiyat)

- Tüm Faz 1 + Faz 2 + Faz 3 plugin'leri (tam özellik)
- Kaynak kod erişimi
- 1 yıl ücretsiz güncelleme
- E-posta destek

### Enterprise (Custom Quote)

- Pro + Faz 4 plugin'leri (pazaryeri konnektörleri, ERP entegrasyonları)
- Özel kurulum + migrasyon desteği
- SLA + öncelikli destek
- Müşteri-spesifik özelleştirme

---

## Geliştirme Sırası — Önerilen

```
Ay 1-2: TurkeyCore (MVP)
Ay 3-4: TurkishConsumerLaw (MVP)
        ↑
        İlk satılabilir paket!
        ↓
Ay 5: Iyzico
Ay 6: PayTR
Ay 7: TurkishCarriers (MVP - 3 firma)
Ay 8-9: EFatura (1-2 entegratör)
Ay 10: TaksitTablosu
        ↑
        Pro Bundle hazır!
        ↓
Ay 11+: Trendyol, Hepsiburada, NetSis ERP (sırası müşteri talebine göre)
```

---

## Roadmap Değişiklik Kayıt

Roadmap'te değişiklik yapılırken bu bölüme not ekle:

### v1.0 — 2 Mayıs 2026
İlk versiyon. 20 plugin, 4 faz tanımlandı. Toplam tahmini süre: 14-18 ay yarı zamanlı.
