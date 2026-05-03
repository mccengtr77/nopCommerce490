# LEGAL_RESEARCH.md

Bu dosya, Türk e-ticaret yasal çerçevesinin **2026 yılı itibarıyla güncel** durumunu özetler. Plugin geliştirme sırasında kod yorumlarında bu maddelerden alıntı yapılmalıdır.

⚠️ **Yasal sorumluluk reddi**: Bu doküman teknik geliştirme rehberidir, hukuki danışmanlık değildir. Şüpheli durumlarda mutlaka avukata danışılmalı.

## İçindekiler

1. [6502 Tüketici Kanunu ve Mesafeli Sözleşmeler Yönetmeliği](#1-6502-tüketici-kanunu)
2. [6698 KVKK ve İlgili Tebliğler](#2-6698-kvkk)
3. [6563 E-Ticaret Kanunu, ETBİS, İYS](#3-6563-e-ticaret-kanunu)
4. [VUK 509/535 — E-Fatura ve E-Arşiv](#4-vuk-509535)
5. [Cezai Yaptırımlar Tablosu](#5-cezai-yaptırımlar)

---

## 1. 6502 Tüketici Kanunu

### Kanun ve İlgili Mevzuat

- **6502 sayılı Tüketicinin Korunması Hakkında Kanun**
- **Mesafeli Sözleşmeler Yönetmeliği** (RG 27.11.2014/29188)
- **Son değişiklik**: 24.05.2025 tarihli ve 32557 sayılı RG yönetmelik değişikliği
- **Yürürlük**: 1.1.2026

### 1.1 Ön Bilgilendirme Yükümlülüğü (Yönetmelik m.5)

E-ticaret sitelerinde **ödeme aşamasından önce** tüketiciye sunulması zorunlu bilgiler:

1. Satıcı/sağlayıcının kimlik bilgileri (ad, ünvan, adres, telefon, e-posta, KEP, MERSİS)
2. Sözleşme konusu mal/hizmetin temel nitelikleri
3. Vergiler dahil toplam fiyat
4. Ödeme, teslimat, ifa süresi
5. Cayma hakkı (varsa) — kullanım şartları, süre, usul
6. **YENİ (2026)**: Cayma hakkı kullanılırsa iade kargo ücretinin satıcı tarafından karşılanacağı
7. Cayma hakkı istisnaları (Yönetmelik m.15)
8. **YENİ (2026)**: Tüketici uyuşmazlığında arabuluculuk şartı bilgisi
9. Şikayet ve itiraz başvuru yolları (Tüketici Hakem Heyeti, Tüketici Mahkemesi)

### 1.2 Mesafeli Satış Sözleşmesi (Yönetmelik m.7)

- **Yazılı olarak yapılma zorunluluğu**
- Tüketiciye bir nüshasının verilmesi zorunlu (kalıcı veri saklayıcısı yeterli)
- İçerik: ÖBF'deki tüm bilgiler + tarafların hak ve yükümlülükleri + yetkili mahkeme

**SAKLAMA SÜRESİ: 3 YIL**

> Satıcının, mesafeli satış sözleşmesini, cayma hakkını tüketiciye bildirdiği metni, ön bilgilendirme formunu ve diğer hususlardaki her bir işleme dair belgeleri **3 yıl süreyle saklaması zorunludur**. İstenilmesi halinde bu belgeler yetkili resmi kurumlara ve tüketicilere verilir.

### 1.3 Cayma Hakkı (Yönetmelik m.9-14)

- **Süre**: Tüketici, malı teslim aldığı tarihten itibaren **14 gün** içinde herhangi bir gerekçe göstermeksizin sözleşmeden cayabilir
- **Hizmet sözleşmelerinde**: Sözleşmenin kurulduğu tarihten itibaren 14 gün
- **YENİ (2026) — İade kargo ücreti**: Cayma hakkını kullanan tüketicilerin **iade kargo ücretleri satıcı tarafından karşılanır**

### 1.4 Cayma Hakkı İstisnaları (Yönetmelik m.15)

Aşağıdaki sözleşmelerde tüketicinin **cayma hakkı yoktur**:

- (a) Fiyatı finansal piyasalardaki dalgalanmalara bağlı ürünler
- (b) Tüketicinin istekleri/kişisel ihtiyaçları doğrultusunda hazırlanan mallar
- (c) Çabuk bozulabilen veya son kullanma tarihi geçebilecek mallar
- (ç) Ambalajı açılmış olan mallardan iadesi sağlık ve hijyen açısından uygun olmayanlar
- (d) Tesliminden sonra başka ürünlerle karışan ve doğası gereği ayrıştırılması mümkün olmayan mallar
- (e) Ambalajı açılmış kitap, dijital içerik ve bilgisayar sarf malzemeleri
- (f) Süreli yayınlar (abonelik dışı)
- (g) Belirli tarih/dönemde yapılması gereken konaklama, taşıma, araç kiralama, yiyecek-içecek, eğlence
- (ğ) Elektronik ortamda anında ifa edilen hizmetler
- (h) Onayla başlanmış hizmetler (cayma süresi dolmadan)
- (ı) 2918 sayılı Karayolları Trafik Kanunu'na göre tescili zorunlu taşınırlar ve insansız hava araçları

**⚠️ ÖNEMLİ DEĞİŞİKLİK (2026)**:
> Daha önce cayma hakkı istisnası kapsamına alınan **cep telefonu, akıllı saat, tablet ve bilgisayar** gibi ürünler **yeniden cayma hakkı kapsamına dahil edilmiştir**. Tüketiciler bu ürünleri de mesafeli satışlarda 14 gün içerisinde iade edebilecektir.

Plugin geliştirirken bu kategorilere "cayma hakkı yok" işaretlemesi koyma!

### 1.5 İade Süresi ve Para İadesi

- Cayma bildirimi alındığı tarihten itibaren **14 gün içinde** tüm bedeller iade edilmeli
- Teslimat masrafları dahil
- Aynı ödeme aracıyla geri ödeme

---

## 2. 6698 KVKK

### Kanun ve İlgili Mevzuat

- **6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK)**
- **Aydınlatma Yükümlülüğünün Yerine Getirilmesinde Uyulacak Usul ve Esaslar Hakkında Tebliğ** (10.03.2018)
- **2026/347 sayılı KVKK İlke Kararı** (RG 24.03.2026) — **KRİTİK**
- **Yurt dışı aktarım yönetmeliği** (Temmuz 2024) — Standart Sözleşmeler (SCC), Bağlayıcı Şirket Kuralları (BCR)

### 2.1 Aydınlatma Yükümlülüğü (KVKK m.10)

Veri işleme **öncesinde** ilgili kişiye sunulması zorunlu bilgiler:

1. Veri sorumlusunun ve varsa temsilcisinin kimliği
2. Kişisel verilerin hangi amaçla işleneceği
3. İşlenen verilerin kimlere ve hangi amaçla aktarılabileceği
4. Kişisel veri toplamanın yöntemi ve hukuki sebebi
5. KVKK m.11'de sayılan diğer haklar
6. **YENİ (Temmuz 2024)**: Yurt dışı aktarım yapılıyorsa SCC/BCR mekanizması bilgisi

### 2.2 KVKK m.11 — İlgili Kişinin Hakları

Müşteri panelinde sunulması gereken hak listesi:

1. Verisinin işlenip işlenmediğini öğrenme
2. İşlenmişse buna ilişkin bilgi talep etme
3. İşleme amacını ve amaca uygun kullanılıp kullanılmadığını öğrenme
4. Yurt içinde veya yurt dışında aktarıldığı üçüncü kişileri bilme
5. Eksik/yanlış işlenmişse düzeltilmesini isteme
6. Silinmesini veya yok edilmesini isteme
7. Düzeltme, silme, yok etmenin aktarıldığı üçüncü kişilere bildirilmesini isteme
8. Otomatik sistemlerle analiz edilmesi sonucu aleyhine bir sonuç çıkmasına itiraz etme
9. Kanuna aykırı işleme nedeniyle zarara uğraması halinde tazminat isteme

**Yanıt süresi: 30 gün**

### 2.3 Açık Rıza (KVKK m.5)

- Belirli bir konuya ilişkin
- Bilgilendirmeye dayanan
- Özgür iradeyle açıklanan
- **Hizmetin rıza şartına bağlanması yasaktır**

### 2.4 ⚠️ 2026/347 İlke Kararı — AYRI METİN ZORUNLULUĞU

**RG 24.03.2026'da yayımlanan kritik karar**:

> Aydınlatma metni ile açık rıza metni artık ayrı ayrı düzenlenmek zorundadır. İç içe geçen veya aynı başlık altında birleştirilen metinler hukuka aykırı sayılacak ve idari para cezası riski doğuracaktır.

#### Pratik Sonuçlar

1. **Aydınlatma metni sonunda**: Yalnızca "okudum ve anladım" gibi bilgilendirme beyanı olabilir. **"Okudum ve kabul ediyorum"** veya **"açık rıza veriyorum"** ifadeleri **kesinlikle kullanılamaz**.

2. **Açık rıza ayrı**: Her veri işleme faaliyeti için modüler rıza alınmalı (pazarlama, profilleme, yurt dışı aktarım vb.).

3. **Aynı sayfada ayrı bölümler yetmez**: Mekanik olarak da ayrı olmalı; başlıklar ayrı, içerik ayrı, onay kutucukları ayrı.

4. **KVKK + ETK Ayrımı**: 6698 KVKK kapsamındaki "Açık Rıza" ile 6563 ETK kapsamındaki "Ticari İleti Onayı" tek bir kutucukta birleştirilemez. KVKK verinin işlenmesiyle ilgilenir; ETK verinin pazarlama silahı olarak kullanılmasıyla.

#### Kayıt Formunda Doğru Yapı

```
[Bilgilendirme - Aydınlatma]
☐ Aydınlatma metnini okudum ve anladım  (sadece bilgilendirme, rıza değil)

[KVKK Açık Rıza - Ayrı bölüm]
☐ Pazarlama amaçlı veri işlemeye açık rızam vardır
☐ Profilleme amaçlı veri işlemeye açık rızam vardır
☐ Verilerimin yurt dışına aktarılmasına açık rızam vardır

[ETK Ticari İleti Onayı - Ayrı bölüm]
☐ SMS yoluyla ticari ileti gönderilmesine onay veriyorum
☐ E-posta yoluyla ticari ileti gönderilmesine onay veriyorum
☐ Sesli arama yoluyla ticari ileti gönderilmesine onay veriyorum
```

**Hiçbir checkbox sipariş vermek/kayıt olmak için zorunlu olamaz.**

### 2.5 Çerez Yönetimi (KVKK Çerez Rehberi)

#### Çerez Kategorileri

1. **Kesinlikle Gerekli (Strictly Necessary)** — Açık rıza GEREKMEZ
   - Oturum çerezleri
   - Güvenlik çerezleri
   - Kullanıcı tercihi (dil seçimi vb.)
   - Sepet çerezleri (e-ticaret için işlevsel)

2. **İşlevsellik (Functional)** — Açık rıza GEREKİR
   - Tema tercihleri
   - Görüntüleme tercihleri

3. **Analitik (Analytics)** — Açık rıza GEREKİR
   - Google Analytics
   - Yandex Metrica
   - Hotjar vb.

4. **Pazarlama/Reklam (Marketing)** — Açık rıza GEREKİR
   - Facebook Pixel
   - Google Ads
   - Yeniden hedefleme çerezleri

#### Pratik Gereklilikler (KVKK Kararları)

> Çerez kullanımı hakkında internet sitesinde yer alan politikanın anlaşılmaz ve kapsamı belirtilmemiş bilgiler içermesi dolayısıyla çerezler hakkındaki aydınlatma yükümlülüğünün tam olarak yerine getirilmediği ihlal sayılır.

> Çerez kullanımına ilişkin işleme şartı olarak **meşru menfaatin zorunlu olduğunun iddia edilebilmesinin hukuken mümkün olmadığı** vurgulanmıştır. Açık rıza alınması gerekir.

> Çerez Politikası ayrı bir doküman olmalıdır (Gizlilik Politikası içine gömülmemeli).

### 2.6 Veri Saklama ve İmha

- Saklama süresi politikası tanımlanmalı
- Süresi dolan veri **silinmeli** veya **anonimleştirilmeli**
- VERBİS'e (Veri Sorumluları Sicil Bilgi Sistemi) kayıt zorunluluğu (belirli ölçek üstü)

#### Tipik Saklama Süreleri (E-ticaret)

| Veri | Süre | Hukuki Sebep |
|------|------|--------------|
| Aktif üyelik | Süresiz | Sözleşmenin ifası |
| Silinmiş üyelik | 6 ay | Yasal yükümlülük |
| Sipariş kayıtları | 10 yıl | VUK |
| Pazarlama izinleri | Rıza geçerliliği + 3 yıl | KVKK + ispat |
| Çerez verileri | 12 ay | KVKK |
| MSS/ÖBF | 3 yıl | Tüketici Kanunu |

---

## 3. 6563 E-Ticaret Kanunu

### Kanun ve İlgili Mevzuat

- **6563 sayılı Elektronik Ticaretin Düzenlenmesi Hakkında Kanun**
- **Ticari İletişim ve Ticari Elektronik İletiler Hakkında Yönetmelik** (15.07.2015)
- **Elektronik Ticaret Bilgi Sistemi ve Bildirim Yükümlülükleri Hakkında Tebliğ** (11.08.2017)
- **Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik** (29.12.2022)

### 3.1 ETBİS (Elektronik Ticaret Bilgi Sistemi)

#### Kayıt Yükümlülüğü

- **Faaliyete başlamadan önce** ETBİS'e kayıt zorunlu
- e-Devlet üzerinden başvuru
- **Tek seferlik kayıt**, ömür boyu geçerli
- Bilgi değişikliği 30 gün içinde bildirilmeli

#### Kapsam

Kayıt zorunluluğu olanlar:
- Kendi web sitesi/mobil uygulaması üzerinden satış yapanlar (B2C)
- Pazaryeri (aracı hizmet sağlayıcı) statüsündekiler
- Yurt içinde yerleşik yurt dışı pazaryerinden satış yapanlar (sınır ötesi)

Kayıt zorunluluğu **OLMAYANLAR**:
- Sadece pazaryerinde satış yapan ve kendi sitesi olmayanlar
- Sadece sosyal medya DM ile satış yapanlar (ama bu durum değişebilir)

#### Karekod Zorunluluğu

ETBİS kaydı sonrası verilen karekod (HTML kodu) sitenin **footer'ına** yerleştirilmeli. Tıklandığında `eticaret.gov.tr` üzerinde işletme bilgileri "Doğrulanmış" olarak gözükmeli.

### 3.2 Güven Damgası (Opsiyonel)

ETBİS ZORUNLU, Güven Damgası **opsiyonel**.

Güven Damgası gereksinimleri:
- Tüm işlemler EV SSL veya SSL üzerinden
- TSE onaylı A veya B sınıfı sızma testi (yıllık)
- Detaylı müşteri hizmetleri yapısı
- Talep ve şikayetlerin etkin yönetimi

### 3.3 İYS (İleti Yönetim Sistemi)

#### Kayıt Yükümlülüğü

> Ticari elektronik ileti göndermek isteyen tüm hizmet sağlayıcılar İYS'ye kayıt olmak **zorundadır**.

E-mail, SMS veya sesli arama ile ticari ileti göndermek isteyen tüm gerçek/tüzel kişiler.

#### Onay Yönetimi

1. **İYS dışında alınan onaylar**: 3 iş günü içinde İYS'ye yüklenmeli
2. **İYS'de onay olmayan kişiye gönderim yasak**
3. **Ret bildirimi**: Doğrudan satıcıya geldiğinde 3 iş günü içinde İYS'ye bildirilmeli

#### Onay Kanalları

- **SMS** (mesaj)
- **E-posta**
- **Sesli arama** (telefon)

Her kanal için **ayrı onay** alınmalı.

#### Onay Gerektirmeyen İletiler (İstisnalar)

- Sipariş bilgilendirme (kargo durumu, teslim, fatura)
- Borç hatırlatma
- Üyelik durumu bildirimleri
- Tahsilat bildirimleri
- Bilgi güncelleme talepleri
- Sermaye piyasası bilgilendirmeleri
- Tacir/esnaf alıcılar (ret hakkı saklı)

#### Saklama Süreleri

- **Onay kayıtları**: Onayın geçerliliği sona erdiği tarihten itibaren **3 yıl**
- **Diğer ileti kayıtları**: Kayıt tarihinden itibaren **3 yıl**

### 3.4 Aracı Hizmet Sağlayıcı Yükümlülükleri

7416 sayılı Kanun (1.7.2022) ile getirilen yenilikler:
- Pazaryerlerinde reklam ücreti sınırı
- Net hizmet bedeli sınırlamaları
- Marka koruma yükümlülükleri

---

## 4. VUK 509/535 — E-Fatura ve E-Arşiv

### Kanun ve İlgili Mevzuat

- **VUK 509 nolu Genel Tebliğ**
- **535 nolu Tebliğ değişikliği**
- **1.1.2026 itibarıyla**: e-Fatura mükellefi olmayan alıcılara düzenlenen faturalar için parasal sınır kaldırıldı (genel kural)

### 4.1 E-Fatura ve E-Arşiv Zorunluluğu (2026)

| Kapsam | Eşik (2025 hesap dönemi) | Geçiş Tarihi |
|--------|--------------------------|--------------|
| Genel | 3 milyon TL ve üzeri | 1.7.2026 |
| **E-ticaret** | **500 bin TL ve üzeri** | 1.7.2026 |
| Gayrimenkul sektörü | 500 bin TL ve üzeri | 1.7.2026 |
| Motorlu taşıt | 500 bin TL ve üzeri | 1.7.2026 |

**E-ticaret için özel düşük eşik**: 500 bin TL.

### 4.2 E-Fatura vs. E-Arşiv

- **E-Fatura**: Alıcı da e-fatura mükellefi ise zorunlu
- **E-Arşiv**: Alıcı vergi mükellefi olmayan tüketici veya e-fatura mükellefi olmayan vergi mükellefi

#### Otomatik Yönlendirme

Plugin geliştirirken sipariş geldiğinde:
1. Alıcının VKN/TCKN'sini al
2. GİB mükellef sorgusu (TurkeyCore'un `IGibMukellefService`)
3. Eğer e-fatura mükellefi → e-Fatura kes
4. Değilse → e-Arşiv kes

### 4.3 Ödeme Yöntemine Göre Fatura Limiti

> Basit usul + işletme hesabı esasıyla defter tutanlar:
> - 31.12.2026'ya kadar 3.000 TL altı işlemlerde kâğıt fatura düzenlenebilir
> - 3.000 TL üstü → e-Arşiv zorunlu
> - 1.1.2027 itibarıyla tüm işlemlerde e-Arşiv zorunlu

### 4.4 E-İrsaliye, E-SMM

- **E-İrsaliye**: Sevk irsaliyesi elektronik ortamda
- **E-SMM**: Serbest Meslek Makbuzu (avukat, mali müşavir vb.)

E-fatura mükellefiyse genelde bunlar da zorunlu.

### 4.5 Özel Entegratörler

GİB doğrudan entegrasyon yerine genelde özel entegratör kullanılır. Yaygın olanlar:

- e-Logo (Logo Yazılım)
- Mysoft
- İzibiz
- Foriba/Sovos
- DijitalPlanet
- Türkkep
- Nesbilgi
- Uyumsoft
- EDM
- N11Faturam

Her entegratörün API'si farklı. Plugin'de adapter pattern.

### 4.6 UBL-TR Format

Türkiye e-fatura formatı **UBL-TR** (Universal Business Language - Türkiye uyarlaması).

#### Standart Profiller

- **TICARIFATURA**: Ticari fatura (B2B)
- **TEMELFATURA**: Temel fatura
- **YOLCUBERABERFATURA**: Yolcu beraber fatura
- **EARSIVFATURA**: e-Arşiv
- **YATIRIMTESVIK**: Yatırım teşvik (Mehmet'in mevcut bilgisi var)
- **IHRACFATURA**: İhracat e-faturası

#### XSLT Şablonları

UBL-TR XML'i HTML'e dönüştürmek için XSLT şablonları kullanılır. Mehmet'in mevcut deneyimi burada değerli.

---

## 5. Cezai Yaptırımlar

### Mesafeli Sözleşmeler İhlali

> Yükümlülüklere aykırı hareket eden elektronik ticaret siteleri hakkında **her bir işlem için 200 TL** idari para cezası uygulanır.

Yıllık yeniden değerleme oranıyla artar.

### KVKK İhlalleri

| İhlal | Ceza (2026) |
|-------|-------------|
| Aydınlatma yükümlülüğüne aykırılık | 100.000 TL - 5.000.000 TL |
| Veri güvenliği yükümlülüğüne aykırılık | 100.000 TL - 5.000.000 TL |
| Kurul kararlarına uymama | 200.000 TL - 5.000.000 TL |
| VERBİS kayıt yükümlülüğüne aykırılık | 100.000 TL - 5.000.000 TL |

### ETBİS İhlali

> Sisteme kayıt olmayan ve gerekli bildirimleri yapmayan firmalara, 6563 sayılı Kanun kapsamında para cezası kesilir. 2024 yılı için 79.230 TL ila 396.150 TL arasında.

### İYS İhlali

| İhlal | Ceza (2024) |
|-------|-------------|
| Onaysız ticari ileti gönderme | 1.899 TL - 9.514 TL |
| Birden fazla alıcıya onaysız ileti | Yukarıdakinin **10 katına kadar** |
| İYS kayıt yükümlülüğüne aykırılık | 3.799 TL - 28.544 TL |

### E-Fatura Zorunluluğuna Aykırılık

VUK m.353 ve sonraki maddeler. Belirli oranlarda usulsüzlük cezası + e-fatura kesilmemiş işlem tutarının %10'u (özel usulsüzlük cezası).

---

## Plugin Geliştirme Sırasında Kanun Atıflarına Dair

Kod yorumlarında kanun maddesi atıfı yapma örnekleri:

```csharp
// Mesafeli Sözleşmeler Yönetmeliği m.5/(c) gereği iade kargo ücreti satıcıya aittir (1.1.2026 itibarıyla)
public decimal CalculateReturnShippingCost() { ... }

// KVKK m.10 gereği veri işleme öncesi aydınlatma yapılır
public async Task ShowDisclosureNoticeAsync() { ... }

// Mesafeli Sözleşmeler Yönetmeliği m.7 gereği MSS 3 yıl saklanır
[DataRetention(Years = 3, LegalBasis = "Mesafeli Sözleşmeler Yönetmeliği m.7")]
public class DistanceSalesContract : BaseEntity { ... }

// VUK 509 nolu Tebliğ - 535 değişikliği gereği e-fatura mükellefi sorgu
public async Task<bool> IsEFaturaMukellefiAsync(string vkn) { ... }
```

---

## Yararlı Kaynaklar

### Resmi Kaynaklar

- Mevzuat Bilgi Sistemi: https://www.mevzuat.gov.tr
- Resmi Gazete: https://www.resmigazete.gov.tr
- KVKK: https://www.kvkk.gov.tr
- ETBİS: https://eticaret.gov.tr
- İYS: https://iys.org.tr
- GİB e-Belge: https://ebelge.gib.gov.tr

### Önemli Tebliğler ve Yönetmelikler

- Mesafeli Sözleşmeler Yönetmeliği: https://www.mevzuat.gov.tr/mevzuat?MevzuatNo=20237
- Aydınlatma Yükümlülüğü Tebliği: https://www.kvkk.gov.tr/Icerik/2033/
- Çerez Uygulamaları Rehberi: https://www.kvkk.gov.tr → Rehberler bölümü
- ETBİS Tebliği: https://ticaret.gov.tr/duyurular
- VUK 509/535 Tebliği: https://ebelge.gib.gov.tr/dosyalar/

### Pratik Rehberler

- Lexpera (mevzuat takibi): https://www.lexpera.com.tr
- Paraşüt blog (e-fatura/e-ticaret): https://www.parasut.com/blog
- Türkkep blog: https://www.turkkep.com.tr/blog

---

## Bu Doküman Güncellemesi

Türk yasal çerçevesi sık değişiyor. Bu doküman **Mayıs 2026** itibarıyla günceldir.

Aşağıdaki durumlarda mutlaka güncellenmeli:
- Yeni bir KVKK ilke kararı yayımlandığında
- Mesafeli Sözleşmeler Yönetmeliği değişikliği olduğunda
- E-fatura geçiş eşikleri değiştiğinde
- Yeni cezalar belirlendiğinde
- İYS API kuralları değiştiğinde

Güncelleme yapıldığında commit mesajı: `docs(legal): <kısa açıklama>`
