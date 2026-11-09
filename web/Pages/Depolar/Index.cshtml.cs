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

    public record DepoSatiri(int Id, string Ad, string? Konum, string? Aciklama, int HareketSayisi);

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
        Depolar = await sorgu
            .OrderBy(d => d.Ad)
            .Select(d => new DepoSatiri(d.Id, d.Ad, d.Konum, d.Aciklama, d.StokHareketleri.Count))
            .ToListAsync();
    }
}

