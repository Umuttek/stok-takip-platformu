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
    public Dictionary<int, List<DepoKisaStok>> UrunDepoDagilimi { get; set; } = new();
    public SelectList KategoriListesi { get; set; } = default!;
    public string? Arama { get; set; }
    public int? KategoriId { get; set; }
    public bool SadeceKritik { get; set; }
    public int KritikSayisi { get; set; }

    public record DepoKisaStok(int DepoId, string DepoAdi, int Miktar);

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

        var urunIdleri = Urunler.Select(u => u.Id).ToList();
        var depoGruplari = await _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Depo)
            .Where(h => urunIdleri.Contains(h.UrunId) && h.Depo != null)
            .GroupBy(h => new { h.UrunId, h.DepoId, DepoAdi = h.Depo!.Ad })
            .Select(g => new
            {
                g.Key.UrunId,
                g.Key.DepoId,
                g.Key.DepoAdi,
                Miktar = g.Sum(x => x.HareketTuru == "Cikis" ? -x.Miktar : x.Miktar)
            })
            .Where(x => x.Miktar > 0)
            .ToListAsync();

        UrunDepoDagilimi = depoGruplari
            .GroupBy(x => x.UrunId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Miktar)
                      .Select(x => new DepoKisaStok(x.DepoId, x.DepoAdi, x.Miktar))
                      .ToList());

        var kategoriler = await _db.Kategoriler.AsNoTracking().OrderBy(k => k.Ad).ToListAsync();
        KategoriListesi = new SelectList(kategoriler, nameof(Kategori.Id), nameof(Kategori.Ad), KategoriId);
    }
}
