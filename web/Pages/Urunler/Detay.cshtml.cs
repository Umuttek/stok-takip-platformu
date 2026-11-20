using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Pages.Urunler;

[Authorize]
public class DetayModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DetayModel(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public Urun Urun { get; set; } = default!;
    public string? FotografBase64 { get; set; }
    public List<DepoZimmetSatiri> DepoStoklari { get; set; } = new();
    public List<StokHareketi> SonHareketler { get; set; } = new();
    public int? VarsayilanDepoId { get; set; }

    public record DepoZimmetSatiri(int DepoId, string DepoAdi, string? DepoKonum, int Miktar);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var urun = await _db.Urunler
            .AsNoTracking()
            .Include(u => u.Kategori)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (urun == null)
            return NotFound();

        Urun = urun;
        FotografBase64 = UrunFormYardimcisi.FotografOku(_env, id);

        var tumDepolar = await _db.Depolar
            .AsNoTracking()
            .OrderBy(d => d.Ad)
            .ToListAsync();

        var hareketler = await _db.StokHareketleri
            .AsNoTracking()
            .Include(h => h.Depo)
            .Include(h => h.Kullanici)
            .Where(h => h.UrunId == id)
            .OrderByDescending(h => h.Tarih)
            .ThenByDescending(h => h.Id)
            .ToListAsync();

        DepoStoklari = tumDepolar
            .Select(d =>
            {
                var net = hareketler
                    .Where(h => h.DepoId == d.Id)
                    .Sum(h => h.HareketTuru == "Cikis" ? -h.Miktar : h.Miktar);

                return new DepoZimmetSatiri(d.Id, d.Ad, d.Konum, Math.Max(0, net));
            })
            .OrderByDescending(x => x.Miktar)
            .ThenBy(x => x.DepoAdi)
            .ToList();

        VarsayilanDepoId = DepoStoklari.FirstOrDefault()?.DepoId;
        SonHareketler = hareketler.Take(10).ToList();

        return Page();
    }
}
