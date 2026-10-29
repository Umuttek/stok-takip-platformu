namespace StokTakip.Models;
public class StokHareketi
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public Urun? Urun { get; set; }
    public int DepoId { get; set; }
    public Depo? Depo { get; set; }
    public string HareketTuru { get; set; } = "Giris";
    public int Miktar { get; set; }
    public DateTime Tarih { get; set; } = DateTime.UtcNow;
    public string? KullaniciId { get; set; }
}