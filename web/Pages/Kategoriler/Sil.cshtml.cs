using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Kategoriler;

[Authorize(Roles = "Yonetici")]
public class SilModel : PageModel
{
    private readonly AppDbContext _db;
    public SilModel(AppDbContext db) => _db = db;

    public Kategori Kategori { get; set; } = new();
    public int UrunSayisi { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var kategori = await _db.Kategoriler.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id);
        if (kategori == null) return NotFound();
        Kategori = kategori;
        UrunSayisi = await _db.Urunler.CountAsync(u => u.KategoriId == id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var kategori = await _db.Kategoriler.FindAsync(id);
        if (kategori == null) return NotFound();

        // Veritabanında silme "cascade" olduğu için ürünü olan kategori silinirse
        // tüm ürünleri de silinir. Bunu engelliyoruz.
        if (await _db.Urunler.AnyAsync(u => u.KategoriId == id))
        {
            TempData["Hata"] = $"\"{kategori.Ad}\" kategorisinde ürün bulunduğu için silinemez. Önce ürünleri başka kategoriye taşıyın veya silin.";
            return RedirectToPage("Index");
        }

        _db.Kategoriler.Remove(kategori);
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{kategori.Ad}\" kategorisi silindi.";
        return RedirectToPage("Index");
    }
}

