namespace StokTakip.Models;
public class Depo
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Konum { get; set; }
    public string? Aciklama { get; set; }
    public List<StokHareketi> StokHareketleri { get; set; } = new();
    public List<StokIstegi> StokIstekleri { get; set; } = new();
}