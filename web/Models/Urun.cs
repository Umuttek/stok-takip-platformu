using System.ComponentModel.DataAnnotations;
namespace StokTakip.Models;
public class Urun
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün kodu zorunludur.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Ürün kodu {2}-{1} karakter arasında olmalıdır.")]
    [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "Ürün kodu yalnızca harf, rakam, '-' ve '_' içerebilir.")]
    [Display(Name = "Ürün Kodu")]
    public string UrunKodu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Ürün adı {2}-{1} karakter arasında olmalıdır.")]
    [Display(Name = "Ürün Adı")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Mevcut miktar {1} ile {2} arasında olmalıdır.")]
    [Display(Name = "Mevcut Miktar")]
    public int MevcutMiktar { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Kritik stok seviyesi {1} ile {2} arasında olmalıdır.")]
    [Display(Name = "Kritik Stok Seviyesi")]
    public int KritikStokSeviyesi { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir kategori seçin.")]
    [Display(Name = "Kategori")]
    public int KategoriId { get; set; }

    [Display(Name = "Kategori")]
    public Kategori? Kategori { get; set; }

    public List<StokHareketi> StokHareketleri { get; set; } = new();
    public List<StokIstegi> StokIstekleri { get; set; } = new();

    /// <summary>Mevcut miktar kritik seviyeye eşit veya altındaysa true.</summary>
    public bool KritikMi => MevcutMiktar <= KritikStokSeviyesi;
}
