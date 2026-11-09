using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Kategoriler;

[Authorize(Roles = "Yonetici")]
public class DuzenleModel : PageModel
{
    private readonly AppDbContext _db;
    public DuzenleModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Kategori Kategori { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var kategori = await _db.Kategoriler.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id);
        if (kategori == null) return NotFound();
        Kategori = kategori;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var kayit = await _db.Kategoriler.FindAsync(id);
        if (kayit == null) return NotFound();

        Kategori.Id = id;
        Kategori.Ad = Kategori.Ad?.Trim() ?? string.Empty;
        if (ModelState.IsValid &&
            await _db.Kategoriler.AnyAsync(k => k.Id != id && k.Ad.ToLower() == Kategori.Ad.ToLower()))
            ModelState.AddModelError("Kategori.Ad", "Bu isimde bir kategori zaten var.");
        if (!ModelState.IsValid)
            return Page();

        // Yalnızca formdaki alanlar güncellenir (overposting'e karşı).
        kayit.Ad = Kategori.Ad;
        kayit.Aciklama = Kategori.Aciklama;
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{kayit.Ad}\" kategorisi güncellendi.";
        return RedirectToPage("Index");
    }
}

