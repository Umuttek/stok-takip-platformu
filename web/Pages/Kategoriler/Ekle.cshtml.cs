using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Kategoriler;

[Authorize(Roles = "Yonetici")]
public class EkleModel : PageModel
{
    private readonly AppDbContext _db;
    public EkleModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Kategori Kategori { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        Kategori.Ad = Kategori.Ad?.Trim() ?? string.Empty;
        if (ModelState.IsValid && await AdKullanimdaMi(Kategori.Ad))
            ModelState.AddModelError("Kategori.Ad", "Bu isimde bir kategori zaten var.");
        if (!ModelState.IsValid)
            return Page();

        _db.Kategoriler.Add(Kategori);
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{Kategori.Ad}\" kategorisi eklendi.";
        return RedirectToPage("Index");
    }

    private Task<bool> AdKullanimdaMi(string ad) =>
        _db.Kategoriler.AnyAsync(k => k.Ad.ToLower() == ad.ToLower());
}

