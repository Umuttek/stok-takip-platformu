using System.ComponentModel.DataAnnotations;

namespace StokTakip.Api.Dtos;

public class StokHareketiCevabi
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int DepoId { get; set; }
    public string DepoAdi { get; set; } = string.Empty;
    public string HareketTuru { get; set; } = "Giris";
    public int Miktar { get; set; }
    public DateTime Tarih { get; set; }
    public string KullaniciAdSoyad { get; set; } = string.Empty;
}

public class StokHareketiEkleIstegi
{
    [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir ürün seçin.")]
    public int UrunId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lütfen işlemin yapılacağı depoyu seçin.")]
    public int DepoId { get; set; }

    [Required(ErrorMessage = "Hareket türü zorunludur.")]
    [RegularExpression("^(Giris|Cikis)$", ErrorMessage = "Hareket türü 'Giris' veya 'Cikis' olmalıdır.")]
    public string HareketTuru { get; set; } = "Giris";

    [Range(1, 1_000_000, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int Miktar { get; set; }
}

