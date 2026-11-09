using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Kategoriler;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<KategoriSatiri> Kategoriler { get; set; } = new();
    public string? Arama { get; set; }

    public record KategoriSatiri(int Id, string Ad, string? Aciklama, int UrunSayisi);

    public async Task OnGetAsync(string? arama)
    {
        Arama = arama?.Trim();
        var sorgu = _db.Kategoriler.AsNoTracking();
        if (!string.IsNullOrEmpty(Arama))
        {
            var aranan = Arama.ToLower();
            sorgu = sorgu.Where(k => k.Ad.ToLower().Contains(aranan));
        }
        Kategoriler = await sorgu
            .OrderBy(k => k.Ad)
            .Select(k => new KategoriSatiri(k.Id, k.Ad, k.Aciklama, k.Urunler.Count))
            .ToListAsync();
    }
}

