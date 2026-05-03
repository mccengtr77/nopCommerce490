# Claude Code İçin Kullanım Rehberi

Bu klasörde nopCommerce Türkiye lokalizasyonu için iki adet prompt dosyası bulunmaktadır:

1. **`01-TurkeyCore-Prompt.md`** — Temel altyapı paketi (önce bu)
2. **`02-TurkishConsumerLaw-Prompt.md`** — Tüketici hukuku paketi (sonra)

## Nasıl Kullanılır

### Önerilen Sıra

**ÖNCE TurkeyCore'u tamamla, sonra TurkishConsumerLaw'a geç.** İkincisi birincisine bağımlı.

### Adım 1: Çalışma Ortamını Hazırla

```bash
# nopCommerce kaynak kodunu klonla (eğer yoksa)
git clone https://github.com/nopSolutions/nopCommerce.git
cd nopCommerce
git checkout release-4.90

# Plugin'lerin build edileceği dizin
mkdir -p src/Plugins/Nop.Plugin.Misc.TurkeyCore
mkdir -p src/Plugins/Nop.Plugin.Misc.TurkishConsumerLaw
```

### Adım 2: Claude Code'u Başlat

```bash
cd src/Plugins/Nop.Plugin.Misc.TurkeyCore
claude
```

### Adım 3: Prompt'u Ver

Claude Code başladığında şu komutu kullan:

```
Sana bir prompt dosyası vereceğim. Bu prompt'u dikkatlice oku ve içindeki tüm gereksinimleri karşılayan nopCommerce 4.90 plugin'ini sıfırdan geliştir.

@/path/to/01-TurkeyCore-Prompt.md
```

Ya da prompt dosyasının içeriğini doğrudan kopyala-yapıştır.

### Adım 4: Aşamalı Geliştirme

Tüm plugin'i tek seferde istemek yerine, prompt'taki sıraya göre adım adım ilerle:

```
1. "Önce plugin.json, TurkeyCorePlugin.cs ve Infrastructure klasörünü oluştur."
2. "Şimdi domain entity'leri ve migration'ları ekle."
3. "Validasyon servislerini implement et — TCKN ve VKN ile başla."
4. "Bu servisler için xUnit testleri yaz."
5. ... ve böyle devam et
```

### Adım 5: Doğrulama

Her aşamadan sonra:

```bash
# Build
dotnet build

# Test
dotnet test

# nopCommerce'e yükleyip dene (manuel)
# 1. Plugin .zip'ini oluştur
# 2. nopCommerce admin → Configuration → Local plugins → Upload
# 3. Plugin'i Install et
# 4. Hataları kontrol et
```

## Önemli İpuçları

### 1. Claude Code'a Bağlam Verme

nopCommerce'in nasıl çalıştığını Claude Code'a referans göstermen önemli. İlk prompt'a şunları da ekle:

```
Lütfen önce mevcut bir referans plugin'i incele:
- @/path/to/nopCommerce/src/Plugins/Nop.Plugin.Tax.FixedOrByCountryStateZip
- @/path/to/nopCommerce/src/Plugins/Nop.Plugin.Misc.SendinBlue

Bu plugin'lerin yapısını, naming convention'ını ve event consumer pattern'ini öğren. Sonra bizim plugin'imizi aynı stilde geliştir.
```

### 2. Senin TurkeyCore'da Eksik Olabilecek Şeyler

Geliştirme sırasında şu noktalara özellikle dikkat et veya Claude Code'a hatırlat:

- **GİB e-fatura mükellef sorgu servisinin gerçek endpoint'i**: Public API yok, özel entegratörler (e-Logo, Mysoft, İzibiz) üzerinden gidilmeli. Mock'la başlat, sonra entegratör seçimi yapıldığında implement et.
- **TCMB XML format'ı**: Bazen değişiyor, parse hatası yakala
- **PTT mahalle veri tabanı**: Resmi indirilebilir mi, yoksa GitHub'da topluluk projesi var mı (`turkish-postal-codes`, `turkiye-il-ilce` repolarına bak)

### 3. TurkishConsumerLaw'da Karmaşık Olan Yerler

- **MSS/ÖBF token replacer**: HTML içine token enjekte etmek dikkat ister, XSS açığına dikkat
- **PDF Türkçe karakter**: QuestPDF'te font embedding (`Open Sans Turkish` veya `Roboto`) — varsayılan font Türkçe karakterleri bozabilir
- **İYS API mock'lama**: Test için sandbox erişimi al, gerçek API ile entegrasyon en sona kalsın
- **2026 KVKK ilke kararı**: Mevcut tek-metin yaklaşımlı diğer plugin'lerle uyum sorunu olabilir, override ile çöz

### 4. Maliyet Optimizasyonu

Claude Code Pro planındaysan:

- Her bir alt görev için ayrı bir oturum aç (context küçük tut)
- "Plan modu" kullan: önce plan, sonra implement
- Test yazımını ayrı oturumda yap
- Refactoring'i ayrı yap

### 5. Versiyonlama

Her aşama sonunda commit:

```bash
git add .
git commit -m "feat(turkey-core): TCKN ve VKN validasyon servisleri eklendi"
```

Plugin tag'leri:
```bash
git tag turkey-core-v0.1-validation
git tag turkey-core-v0.2-domain
git tag turkey-core-v1.0-mvp
```

## Sorun Giderme

### "Bu plugin nopCommerce'e yüklenmedi"

- `plugin.json`'ın `SupportedVersions` doğru mu?
- DLL ismi `FileName` ile eşleşiyor mu?
- `bin/` ve `obj/` klasörleri zip'in içine gönderildi mi (gönderilmemeli!)

### "Migration başarısız"

- nopCommerce migration sıralamasını dikkate aldın mı?
- `BaseEntity` miras aldın mı?
- Foreign key'lerin doğru mu?

### "Storefront'ta view component görünmüyor"

- `IWidgetPlugin` mi olmalıydı `IMiscPlugin` yerine?
- Widget zone'a kayıt yapıldı mı?

## İlerleme Takibi

Her plugin için yaklaşık süreler (tek geliştirici, yarı zamanlı):

| Plugin | Tahmini Süre |
|--------|-------------|
| TurkeyCore (MVP) | 4-5 hafta |
| TurkeyCore (Tam) | 6-8 hafta |
| TurkishConsumerLaw (MVP) | 6-7 hafta |
| TurkishConsumerLaw (Tam) | 9-11 hafta |

İkisi birden, full-time çalışıldığında 3-4 ay.

## Sonraki Adımlar

Bu iki paket tamamlandıktan sonra:
- `Nop.Plugin.Payments.Iyzico`
- `Nop.Plugin.Payments.PayTR`
- `Nop.Plugin.Shipping.TurkishCarriers`
- `Nop.Plugin.Misc.EFatura`

için de aynı yöntemle prompt dosyaları hazırlanabilir.

İyi kodlamalar! 🚀
