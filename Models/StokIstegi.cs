namespace StokTakip.Models;
public class StokIstegi
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public Urun? Urun { get; set; }
    public int DepoId { get; set; }
    public Depo? Depo { get; set; }
    public int Miktar { get; set; }
    public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;
    public string Durum { get; set; } = "Beklemede";
    public string? TalepSahibiId { get; set; }
}