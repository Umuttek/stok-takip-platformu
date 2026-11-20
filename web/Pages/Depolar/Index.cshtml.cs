using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;

namespace StokTakip.Pages.Depolar;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<DepoSatiri> Depolar { get; set; } = new();
    public string? Arama { get; set; }

    public record DepoUrunStokSatiri(int UrunId, string UrunKodu, string UrunAdi, int Miktar);

    public record DepoSatiri(
        int Id,
        string Ad,
        string? Konum,
        string? Aciklama,
        int HareketSayisi,
        int UrunCesidiSayisi,
        int ToplamUrunAdeti,
        List<DepoUrunStokSatiri> Urunler);

    public async Task OnGetAsync(string? arama)
    {
        Arama = arama?.Trim();
        var sorgu = _db.Depolar.AsNoTracking();
        if (!string.IsNullOrEmpty(Arama))
        {
            var aranan = Arama.ToLower();
            sorgu = sorgu.Where(d => d.Ad.ToLower().Contains(aranan) ||
                                     (d.Konum != null && d.Konum.ToLower().Contains(aranan)));
        }

        var depoListesi = await sorgu.OrderBy(d => d.Ad).ToListAsync();
        var depoIdleri = depoListesi.Select(d => d.Id).ToList();

        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
            .Where(h => depoIdleri.Contains(h.DepoId))
            .ToListAsync();

        Depolar = depoListesi.Select(d =>
        {
            var depoHareketleri = hareketler.Where(h => h.DepoId == d.Id).ToList();
            var zimmetliUrunler = depoHareketleri
                .Where(h => h.Urun != null)
                .GroupBy(h => new { h.UrunId, h.Urun!.UrunKodu, h.Urun.Ad })
                .Select(g => new DepoUrunStokSatiri(
                    g.Key.UrunId,
                    g.Key.UrunKodu,
                    g.Key.Ad,
                    g.Sum(x => x.HareketTuru == "Cikis" ? -x.Miktar : x.Miktar)))
                .Where(x => x.Miktar > 0)
                .OrderBy(x => x.UrunAdi)
                .ToList();

            return new DepoSatiri(
                d.Id,
                d.Ad,
                d.Konum,
                d.Aciklama,
                depoHareketleri.Count,
                zimmetliUrunler.Count,
                zimmetliUrunler.Sum(x => x.Miktar),
                zimmetliUrunler);
        }).ToList();
    }
}
