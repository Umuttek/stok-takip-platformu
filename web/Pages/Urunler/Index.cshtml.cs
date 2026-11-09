using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Urunler;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Urun> Urunler { get; set; } = new();
    public SelectList KategoriListesi { get; set; } = default!;
    public string? Arama { get; set; }
    public int? KategoriId { get; set; }
    public bool SadeceKritik { get; set; }
    public int KritikSayisi { get; set; }

    public async Task OnGetAsync(string? arama, int? kategoriId, bool sadeceKritik)
    {
        Arama = arama?.Trim();
        KategoriId = kategoriId;
        SadeceKritik = sadeceKritik;

        var sorgu = _db.Urunler.AsNoTracking().Include(u => u.Kategori).AsQueryable();
        if (!string.IsNullOrEmpty(Arama))
        {
            var aranan = Arama.ToLower();
            sorgu = sorgu.Where(u => u.Ad.ToLower().Contains(aranan) || u.UrunKodu.ToLower().Contains(aranan));
        }
        if (KategoriId.HasValue)
            sorgu = sorgu.Where(u => u.KategoriId == KategoriId);
        if (SadeceKritik)
            sorgu = sorgu.Where(u => u.MevcutMiktar <= u.KritikStokSeviyesi);

        Urunler = await sorgu.OrderBy(u => u.Ad).ToListAsync();
        KritikSayisi = await _db.Urunler.CountAsync(u => u.MevcutMiktar <= u.KritikStokSeviyesi);

        var kategoriler = await _db.Kategoriler.AsNoTracking().OrderBy(k => k.Ad).ToListAsync();
        KategoriListesi = new SelectList(kategoriler, nameof(Kategori.Id), nameof(Kategori.Ad), KategoriId);
    }
}

