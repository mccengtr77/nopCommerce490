# TurkeyCore Seed Data

Bu klasördeki CSV dosyaları plugin assembly'sine **embedded resource** olarak gömülür ve plugin install sırasında migration'lar tarafından okunarak veritabanına yazılır.

## Dosyalar

| Dosya | Format | Şu an | Tam set |
|---|---|---|---|
| `provinces.csv` | `Plate \| Name` | ✅ 81 il (tam) | ✅ Tam |
| `districts.csv` | `ProvincePlate \| Name` | ⚠️ ~110 ilçe (büyük şehirler) | ❌ ~973 ilçe gerek |
| `neighborhoods.csv` | `ProvincePlate \| DistrictName \| Name \| PostalCode` | ⚠️ ~150 mahalle (örnek) | ❌ ~50.000+ mahalle gerek |
| `taxoffices.csv` | `ProvincePlate \| Name \| Code` | ⚠️ ~75 vergi dairesi (büyük şehirler) | ❌ ~1000 vergi dairesi gerek |

> **Pipe `|` delimiter** kullanılıyor — Türkçe il/ilçe adlarındaki virgül karışıklığını önler.
> İlk satır header (atılır), `#` ile başlayan satırlar yorum (atılır).

## Tam Veri Setini Eklemek

### 1. Districts (~973 ilçe)

**Açık kaynaklar:**
- T.C. İçişleri Bakanlığı resmi liste (PDF'den dönüştürmek gerek)
- GitHub: `turkiye-il-ilce-mahalle-koy-csv`, `turkey-administrative-divisions` benzeri repo'lar (MIT/CC-0 lisanslı)

**Format dönüşümü gerekirse:**
```bash
# Source CSV: il_id, ilce_adi → bizim format: plate|name
awk -F',' 'NR>1 {print $1 "|" $2}' source.csv > districts.csv
```

### 2. Neighborhoods (~50.000+ mahalle + posta kodu)

**Açık kaynaklar:**
- **PTT resmi**: https://postakodu.ptt.gov.tr — indirilebilir CSV (bazı dönemlerde, güncellik kontrol et)
- GitHub topluluk repo'ları (lisans kontrol et)

**Format kuralı:**
- Header'a `ProvincePlate|DistrictName|Name|PostalCode` ekleyin
- DistrictName **tam olarak districts.csv'deki adla** eşleşmeli (case-sensitive)
- Posta kodu 5 hane string

**Performans uyarısı:** 50.000+ satır migration'da tek tek INSERT edilirse yavaştır. Production'da bulk insert için `SqlBulkCopy` veya benzeri optimize edilebilir; şu anki migration tek-tek INSERT yapıyor (~1-2 dakika 50k için kabul edilebilir, sadece install sırasında çalışır).

### 3. Tax Offices (~1000 vergi dairesi)

**Açık kaynaklar:**
- **GİB resmi**: https://www.gib.gov.tr → Vergi Daireleri listesi (PDF, manuel dönüşüm)
- E-fatura entegratörlerinin (e-Logo, Mysoft, İzibiz) sağladığı listeler

**Format:** `ProvincePlate|Name|Code` (Code = 4 haneli GİB kodu)

## Veri Eklendikten Sonra

CSV dosyalarını güncelledikten sonra:

1. **Plugin'i rebuild et** — embedded resource olarak yenilenir
2. **Plugin'i uninstall + install** — yeni migration timestamp'iyle ayrı bir seed migration eklemek daha güvenli (mevcut müşteri verilerini bozmamak için)
3. Veya **admin import aracı** ekle (gelecekteki sürüm) — admin runtime'da CSV upload eder

## Migration Sırası

1. `SchemaMigration` — tabloları oluşturur (`2026/05/03 12:00:00:0000001`)
2. `SeedProvincesMigration` — 81 il (`...:0000002`)
3. `SeedDistrictsMigration` — Province lookup ile ilçeler (`...:0000003`)
4. `SeedNeighborhoodsMigration` — District lookup ile mahalleler (`...:0000004`)
5. `SeedTaxOfficesMigration` — Province lookup ile vergi daireleri (`...:0000005`)

Migration'lar `Execute.WithConnection` ile parent ID lookup yapar — CSV'lerde plate code/district name natural key kullanılır, auto-generated Id'ye bağımlılık yoktur.

## Lisans Notu

Bu CSV dosyalarındaki veriler **kamuya açık idari bilgilerdir** (il/ilçe adları, posta kodları, vergi dairesi adları). Plugin MIT lisansıyla dağıtılır. Eklenecek üçüncü taraf veri setlerinin lisanslarını kontrol etmek (özellikle MIT/CC-0/Apache uyumluluğu) admin sorumluluğundadır.
