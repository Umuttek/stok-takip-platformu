namespace StokTakip.Models;
public class Kategori
{
    public int Id {get; set;}
    public string Ad {get; set;} = string.Empty;
    public string? Aciklama {get; set;}
    public List<Urun> Urunler {get; set;} = new();
}