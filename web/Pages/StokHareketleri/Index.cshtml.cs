using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.StokHareketleri;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<StokHareketi> Hareketler { get; set; } = new();
    public SelectList UrunListesi { get; set; } = default!;
    public SelectList DepoListesi { get; set; } = default!;

    public string? Arama { get; set; }
    public int? UrunId { get; set; }
    public int? DepoId { get; set; }
    public string? Tur { get; set; }

    public int ToplamHareketSayisi { get; set; }
    public int GirisIslemSayisi { get; set; }
    public int CikisIslemSayisi { get; set; }
    public long ToplamGirisAdedi { get; set; }
    public long ToplamCikisAdedi { get; set; }

    public async Task OnGetAsync(string? arama, int? urunId, int? depoId, string? tur)
    {
        Arama = arama?.Trim();
        UrunId = urunId;
        DepoId = depoId;
        Tur = (tur == "Giris" || tur == "Cikis") ? tur : null;

        var sorgu = _context.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Urun)
                .ThenInclude(u => u!.Kategori)
            .Include(h => h.Depo)
            .Include(h => h.Kullanici)
            .AsQueryable();

        if (!string.IsNullOrEmpty(Arama))
        {
            var aranan = Arama.ToLower();
            sorgu = sorgu.Where(h =>
                (h.Urun != null && (h.Urun.Ad.ToLower().Contains(aranan) || h.Urun.UrunKodu.ToLower().Contains(aranan))) ||
                (h.Depo != null && h.Depo.Ad.ToLower().Contains(aranan)) ||
                (h.Kullanici != null && h.Kullanici.AdSoyad.ToLower().Contains(aranan)));
        }

        if (UrunId.HasValue)
            sorgu = sorgu.Where(h => h.UrunId == UrunId.Value);

        if (DepoId.HasValue)
            sorgu = sorgu.Where(h => h.DepoId == DepoId.Value);

        if (!string.IsNullOrEmpty(Tur))
            sorgu = sorgu.Where(h => h.HareketTuru == Tur);

        Hareketler = await sorgu
            .OrderByDescending(h => h.Tarih)
            .ThenByDescending(h => h.Id)
            .ToListAsync();

        ToplamHareketSayisi = Hareketler.Count;
        GirisIslemSayisi = Hareketler.Count(h => h.HareketTuru == "Giris");
        CikisIslemSayisi = Hareketler.Count(h => h.HareketTuru == "Cikis");
        ToplamGirisAdedi = Hareketler.Where(h => h.HareketTuru == "Giris").Sum(h => (long)h.Miktar);
        ToplamCikisAdedi = Hareketler.Where(h => h.HareketTuru == "Cikis").Sum(h => (long)h.Miktar);

        var urunler = await _context.Urunler.AsNoTracking().OrderBy(u => u.Ad).ToListAsync();
        var depolar = await _context.Depolar.AsNoTracking().OrderBy(d => d.Ad).ToListAsync();

        UrunListesi = new SelectList(urunler, nameof(Urun.Id), nameof(Urun.Ad), UrunId);
        DepoListesi = new SelectList(depolar, nameof(Depo.Id), nameof(Depo.Ad), DepoId);
    }
}
