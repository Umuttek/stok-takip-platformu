using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Urunler;

[Authorize(Roles = "Yonetici")]
public class DuzenleModel : PageModel
{
    private readonly AppDbContext _db;
    public DuzenleModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Urun Urun { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var urun = await _db.Urunler.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (urun == null) return NotFound();
        Urun = urun;
        await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var kayit = await _db.Urunler.FindAsync(id);
        if (kayit == null) return NotFound();

        Urun.Id = id;
        if (ModelState.IsValid)
        {
            UrunFormYardimcisi.Normallestir(Urun);
            await UrunFormYardimcisi.EkDogrulamaAsync(_db, ModelState, Urun, haricId: id);
        }
        if (!ModelState.IsValid)
        {
            await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
            return Page();
        }

        // Yalnızca formdaki alanlar güncellenir (overposting'e karşı).
        kayit.UrunKodu = Urun.UrunKodu;
        kayit.Ad = Urun.Ad;
        kayit.Aciklama = Urun.Aciklama;
        kayit.KategoriId = Urun.KategoriId;
        kayit.MevcutMiktar = Urun.MevcutMiktar;
        kayit.KritikStokSeviyesi = Urun.KritikStokSeviyesi;
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{kayit.Ad}\" ürünü güncellendi.";
        return RedirectToPage("Index");
    }
}

