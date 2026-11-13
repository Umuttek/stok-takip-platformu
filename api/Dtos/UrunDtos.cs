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
}