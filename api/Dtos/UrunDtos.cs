using System.ComponentModel.DataAnnotations;

namespace StokTakip.Api.Dtos;

public class UrunCevabi
{
    public int Id { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int MevcutMiktar { get; set; }
    public int KritikStokSeviyesi { get; set; }
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; } = string.Empty;
    public bool KritikMi { get; set; }
    public string? FotografBase64 { get; set; }
    public List<UrunDepoStokDto> DepoStoklari { get; set; } = new();
}

public class UrunDepoStokDto
{
    public int DepoId { get; set; }
    public string DepoAdi { get; set; } = string.Empty;
    public string? DepoKonum { get; set; }
    public int Miktar { get; set; }
}

public class UrunEkleIstegi
{
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Ürün adı 2-150 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir kategori seçin.")]
    public int KategoriId { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Kritik stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int KritikStokSeviyesi { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Başlangıç miktarı 0 veya daha büyük olmalıdır.")]
    public int MevcutMiktar { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lütfen ürünün zimmetleneceği depoyu seçin.")]
    public int DepoId { get; set; }

    public string? FotografBase64 { get; set; }
}

public class UrunGuncelleIstegi
{
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Ürün adı 2-150 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir kategori seçin.")]
    public int KategoriId { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Kritik stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int KritikStokSeviyesi { get; set; }

    public string? FotografBase64 { get; set; }
    public bool FotografiKaldir { get; set; }
}
