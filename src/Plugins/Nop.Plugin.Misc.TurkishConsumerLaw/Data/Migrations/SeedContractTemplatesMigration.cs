using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

/// <summary>
/// Default MSS ve ÖBF şablonları. Admin kendi şirket bilgileriyle özelleştirir.
/// 2026 değişiklikleri default şablonlarda işlenmiştir:
/// - Cep tel/tablet/bilgisayar cayma kapsamında
/// - İade kargo ücreti satıcıda
/// - Arabuluculuk şartı bilgisi (ÖBF'de zorunlu)
/// </summary>
[NopMigration("2026/05/03 13:00:00:0000009",
    "Misc.TurkishConsumerLaw seed default contract templates",
    MigrationProcessType.Installation)]
public class SeedContractTemplatesMigration : Migration
{
    public override void Up()
    {
        var now = DateTime.UtcNow;

        Insert.IntoTable(nameof(ContractTemplate)).Row(new
        {
            Type = (int)ContractTemplateType.Mss,
            Name = "MSS — Mesafeli Satış Sözleşmesi (Varsayılan)",
            HtmlContent = DefaultMssTemplate(),
            Version = "1.0",
            IsActive = true,
            DisplayOrder = 1,
            LimitedToStoreId = 0,
            CreatedOnUtc = now,
            UpdatedOnUtc = (DateTime?)null
        });

        Insert.IntoTable(nameof(ContractTemplate)).Row(new
        {
            Type = (int)ContractTemplateType.Obf,
            Name = "ÖBF — Ön Bilgilendirme Formu (Varsayılan, 2026 güncel)",
            HtmlContent = DefaultObfTemplate(),
            Version = "1.0",
            IsActive = true,
            DisplayOrder = 1,
            LimitedToStoreId = 0,
            CreatedOnUtc = now,
            UpdatedOnUtc = (DateTime?)null
        });
    }

    public override void Down() { }

    private static string DefaultMssTemplate() => """
        <h2>MESAFELİ SATIŞ SÖZLEŞMESİ</h2>

        <h4>1. TARAFLAR</h4>
        <p><strong>SATICI:</strong></p>
        <ul>
          <li>Unvan: {{SatıcıAdı}}</li>
          <li>Adres: {{SatıcıAdresi}}</li>
          <li>Telefon: {{SatıcıTelefon}}</li>
          <li>KEP: {{SatıcıKep}}</li>
          <li>MERSİS: {{SatıcıMersis}}</li>
        </ul>

        <p><strong>ALICI:</strong></p>
        <ul>
          <li>Ad Soyad: {{AlıcıAdı}}</li>
          <li>Adres: {{AlıcıAdresi}}</li>
          <li>Telefon: {{AlıcıTelefon}}</li>
          <li>TCKN/VKN: {{AlıcıTcKimlik}}{{AlıcıVergiNo}}</li>
        </ul>

        <h4>2. SÖZLEŞME KONUSU</h4>
        <p>Sipariş No: <strong>{{SiparişNo}}</strong>, Tarih: {{SiparişTarihi}}</p>
        <p>Bu sözleşme konusu ürün/hizmet aşağıda belirtilmiştir:</p>
        {{ÜrünListesi}}

        <h4>3. ÜCRET VE ÖDEME</h4>
        <ul>
          <li>Ara Toplam: {{AraToplam}}</li>
          <li>KDV Toplam: {{KdvToplam}}</li>
          <li>Kargo Bedeli: {{KargoBedeli}}</li>
          <li>İndirim: {{İndirim}}</li>
          <li><strong>Genel Toplam: {{GenelToplam}}</strong></li>
          <li>Ödeme Yöntemi: {{ÖdemeYöntemi}}</li>
        </ul>

        <h4>4. TESLİMAT</h4>
        <p>Teslimat adresi: {{TeslimatAdresi}}</p>
        <p>Fatura adresi: {{FaturaAdresi}}</p>
        <p>Kargo firması: {{KargoFirması}}</p>
        <p>Tahmini teslim süresi: {{TahminiTeslimSüresi}}</p>

        <h4>5. CAYMA HAKKI</h4>
        <p>Tüketici, ürünü teslim aldığı tarihten itibaren <strong>{{CaymaSüresi}} gün</strong>
        içinde gerekçe göstermeksizin cayma hakkını kullanabilir.</p>
        <p><strong>2026 güncellemesi:</strong> İade kargo ücreti SATICI tarafından karşılanır
        (Mesafeli Sözleşmeler Yönetmeliği m.13/(2)).</p>

        <h4>6. UYUŞMAZLIKLARIN ÇÖZÜMÜ</h4>
        <p>{{ArabuluculukBilgisi}}</p>

        <p><em>Sözleşmenin elektronik ortamda onayı, taraflar açısından bağlayıcıdır.
        Onay tarihinde sözleşmenin SHA-256 hash değeri ile manipülasyon kontrolü yapılır.</em></p>
        """;

    private static string DefaultObfTemplate() => """
        <h2>ÖN BİLGİLENDİRME FORMU</h2>

        <h4>SATICININ KİMLİĞİ</h4>
        <p>{{SatıcıAdı}} — {{SatıcıAdresi}} — {{SatıcıTelefon}} — KEP: {{SatıcıKep}} — MERSİS: {{SatıcıMersis}}</p>

        <h4>ÜRÜN BİLGİLERİ</h4>
        {{ÜrünListesi}}

        <h4>TOPLAM ÜCRET</h4>
        <p>Ara: {{AraToplam}} | KDV: {{KdvToplam}} | Kargo: {{KargoBedeli}} |
        İndirim: {{İndirim}} | <strong>Genel Toplam: {{GenelToplam}}</strong></p>
        <p>Ödeme yöntemi: {{ÖdemeYöntemi}}</p>

        <h4>TESLİMAT</h4>
        <p>Adres: {{TeslimatAdresi}} | Kargo: {{KargoFirması}} | Süre: {{TahminiTeslimSüresi}}</p>

        <h4>CAYMA HAKKI VE KOŞULLAR</h4>
        <p>Tüketici teslimden itibaren <strong>{{CaymaSüresi}} gün</strong> içinde cayma hakkını
        gerekçesiz kullanabilir. <strong>İade kargo ücreti SATICI'ya aittir</strong>
        (2026 değişikliği — Mesafeli Sözleşmeler Yönetmeliği m.13/(2)).</p>

        <p><strong>2026 GÜNCEL:</strong> Cep telefonu, akıllı saat, tablet, bilgisayar gibi ürünler
        artık cayma hakkı kapsamındadır (önceki istisna kaldırıldı).</p>

        <h4>UYUŞMAZLIK ÇÖZÜMÜ — ARABULUCULUK</h4>
        <p>{{ArabuluculukBilgisi}}</p>

        <p><em>Bu form, mesafeli sözleşmenin kurulmasından önce tüketiciye sunulmuştur
        (Mesafeli Sözleşmeler Yönetmeliği m.5).</em></p>
        """;
}
