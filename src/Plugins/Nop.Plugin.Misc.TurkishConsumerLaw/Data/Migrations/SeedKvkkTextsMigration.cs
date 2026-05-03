using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

/// <summary>
/// Default KVKK aydınlatma + açık rıza metinlerini seed eder.
/// Admin bunları kendi şirket bilgileriyle özelleştirmeli — placeholder'lar [SİRKET ADI] vs.
///
/// 2026/347 İlke Kararı: aydınlatma metni içine "rıza" ifadesi yazılmaz.
/// Default şablonlar bu kuralı uygular.
/// </summary>
[NopMigration("2026/05/03 13:00:00:0000005",
    "Misc.TurkishConsumerLaw seed default KVKK texts",
    MigrationProcessType.Installation)]
public class SeedKvkkTextsMigration : Migration
{
    public override void Up()
    {
        SeedDisclosure();
        SeedExplicitConsents();
    }

    public override void Down()
    {
        // Schema migration tabloyu drop ettiği için ek temizliğe gerek yok
    }

    private void SeedDisclosure()
    {
        const string defaultDisclosure = """
            <h3>KİŞİSEL VERİLERİN İŞLENMESİNE İLİŞKİN AYDINLATMA METNİ</h3>
            <p>6698 sayılı Kişisel Verilerin Korunması Kanunu (&ldquo;KVKK&rdquo;) uyarınca,
            kişisel verileriniz veri sorumlusu sıfatıyla [ŞİRKET ADI] tarafından
            aşağıda açıklanan kapsamda işlenmektedir.</p>

            <h4>1. Veri Sorumlusunun Kimliği</h4>
            <p>[ŞİRKET ADI], [ADRES], MERSİS: [MERSİS NO]</p>

            <h4>2. İşlenen Kişisel Veriler ve İşleme Amaçları</h4>
            <ul>
              <li>Kimlik (ad, soyad, TC kimlik no): üyelik ve fatura</li>
              <li>İletişim (e-posta, telefon, adres): sipariş ve teslimat</li>
              <li>Müşteri işlem (sipariş geçmişi, ödeme bilgisi): hizmet sunumu</li>
              <li>Pazarlama (tercih ve analiz verileri): yalnızca açık rızanız ile</li>
            </ul>

            <h4>3. Hukuki Sebep</h4>
            <p>Kişisel verileriniz KVKK m.5 ve m.6 kapsamında, sözleşmenin kurulması/ifası,
            yasal yükümlülüklerin yerine getirilmesi ve açık rızanız hukuki sebeplerine dayalı olarak işlenir.</p>

            <h4>4. Aktarım</h4>
            <p>Kişisel verileriniz, hizmet sunumu için zorunlu olduğu ölçüde
            kargo, ödeme altyapı ve sunucu sağlayıcılarına aktarılır.
            Yurt dışına aktarım için ayrıca açık rızanız alınır (KVKK m.9).</p>

            <h4>5. Haklarınız (KVKK m.11)</h4>
            <p>Kişisel verilerinize ilişkin bilgi alma, düzeltme, silme, aktarımına itiraz etme
            ve zarar tazmini hakkınız bulunmaktadır. Başvurularınızı [E-POSTA] adresine
            iletebilirsiniz; en geç 30 gün içinde yanıtlanır.</p>

            <p><em>Bu metin yalnızca bilgilendirme amaçlıdır; işleme açık rıza bu metnin
            kabulü ile değil, ayrı sunulan rıza onayları ile alınır.</em></p>
            """;

        Insert.IntoTable(nameof(DisclosureText)).Row(new
        {
            Title = "KVKK Aydınlatma Metni (Varsayılan)",
            Content = defaultDisclosure,
            Version = "1.0",
            LimitedToStoreId = 0,
            IsActive = true,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = (DateTime?)null
        });
    }

    private void SeedExplicitConsents()
    {
        var consents = new (ConsentScope Scope, string Label, string Content, int Order)[]
        {
            (ConsentScope.KvkkPersonalData,
             "Kişisel verilerimin pazarlama amacıyla işlenmesine açık rıza veriyorum.",
             "Kişisel verilerimin (kimlik, iletişim, müşteri işlem) ürün/hizmet tanıtımı, kampanya bildirimleri ve müşteri deneyiminin iyileştirilmesi amacıyla işlenmesine; bu kapsamda [ŞİRKET ADI] tarafından KVKK m.5/(2) gereğince açık rıza ile işlenmesine onay veriyorum. Bu rızamı her zaman geri çekebileceğimi biliyorum.",
             10),

            (ConsentScope.KvkkProfiling,
             "Profilleme ve segmentasyon için işlenmesine açık rıza veriyorum.",
             "Sipariş geçmişi, gezinme verisi ve tercihlerimin profil oluşturma ve segmentasyon amacıyla işlenmesine açık rıza veriyorum. Profil bilgilerim üzerinden bana özel teklifler oluşturulabilir.",
             20),

            (ConsentScope.KvkkThirdPartyDomestic,
             "Yurt içindeki iş ortaklarına aktarımına açık rıza veriyorum.",
             "Pazarlama amaçlı iş ortakları (yurt içi) ile sınırlı veri paylaşımına açık rıza veriyorum.",
             30),

            (ConsentScope.KvkkOverseasTransfer,
             "Yurt dışına aktarımına açık rıza veriyorum (KVKK m.9).",
             "Bulut altyapı ve uluslararası iş ortakları ile veri paylaşımına, KVKK m.9 hükümleri çerçevesinde açık rıza veriyorum. Bu rıza olmadan yurt dışına veri aktarımı yapılmaz.",
             40),

            (ConsentScope.EtkSms,
             "Ticari amaçlı SMS gönderilmesine onay veriyorum.",
             "[ŞİRKET ADI] tarafından kampanya, indirim ve ürün tanıtım bildirimlerinin SMS yoluyla gönderilmesine, 6563 sayılı E-Ticaret Kanunu kapsamında onay veriyorum. Onayım İYS'ye kaydedilir; her zaman 'RED' SMS göndererek iptal edebilirim.",
             100),

            (ConsentScope.EtkEmail,
             "Ticari amaçlı e-posta gönderilmesine onay veriyorum.",
             "[ŞİRKET ADI] tarafından kampanya ve ürün bildirimlerinin e-posta yoluyla gönderilmesine onay veriyorum. Her e-postada bulunan 'abonelikten çık' linki ile iptal edebilirim.",
             110),

            (ConsentScope.EtkCall,
             "Ticari amaçlı arama yapılmasına onay veriyorum.",
             "[ŞİRKET ADI] tarafından kampanya ve ürün tanıtımı amacıyla telefonla arama yapılmasına onay veriyorum.",
             120)
        };

        var now = DateTime.UtcNow;
        foreach (var (scope, label, content, order) in consents)
        {
            Insert.IntoTable(nameof(ExplicitConsentText)).Row(new
            {
                Scope = (int)scope,
                ShortLabel = label,
                Content = content,
                Version = "1.0",
                LimitedToStoreId = 0,
                DefaultChecked = false,  // Açık rıza opt-in olmalı
                IsActive = true,
                DisplayOrder = order,
                CreatedOnUtc = now,
                UpdatedOnUtc = (DateTime?)null
            });
        }
    }
}
