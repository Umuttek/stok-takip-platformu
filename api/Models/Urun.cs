namespace StokTakip.Models;
public class Urun
{
    public int Id { get; set; }
    public string UrunKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int MevcutMiktar { get; set; }
    public int KritikStokSeviyesi { get; set; }
    public int KategoriId { get; set; }
    public Kategori? Kategori { get; set; }
    public List<StokHareketi> StokHareketleri { get; set; } = new();
    public List<StokIstegi> StokIstekleri { get; set; } = new();
}