using System.ComponentModel.DataAnnotations;

namespace StokTakip.Api.Dtos;

public class DepoCevabi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Konum { get; set; }
    public string? Aciklama { get; set; }
    public int ToplamUrunAdeti { get; set; }
    public int UrunCesidiSayisi { get; set; }
    public List<DepoUrunStokDto> Urunler { get; set; } = new();
}

public class DepoUrunStokDto
{
    public int UrunId { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Miktar { get; set; }
}

public class DepoKaydetIstegi
{
    [Required(ErrorMessage = "Depo adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Depo adı 2-100 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
    public string? Konum { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }
}

