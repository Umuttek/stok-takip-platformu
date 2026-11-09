using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Urunler;

[Authorize(Roles = "Yonetici")]
public class EkleModel : PageModel
{
    private readonly AppDbContext _db;
    public EkleModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Urun Urun { get; set; } = new();

    public async Task OnGetAsync(int? kategoriId)
    {
        if (kategoriId.HasValue) Urun.KategoriId = kategoriId.Value;
        await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            UrunFormYardimcisi.Normallestir(Urun);
            await UrunFormYardimcisi.EkDogrulamaAsync(_db, ModelState, Urun);
        }

        if (!ModelState.IsValid)
        {
            await UrunFormYardimcisi.KategorileriYukleAsync(_db, ViewData, Urun.KategoriId);
            return Page();
        }

        _db.Urunler.Add(Urun);
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{Urun.Ad}\" ürünü eklendi.";
        return RedirectToPage("Index");
    }
}
