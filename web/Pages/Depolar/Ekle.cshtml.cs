using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Depolar;

[Authorize(Roles = "Yonetici")]
public class EkleModel : PageModel
{
    private readonly AppDbContext _db;
    public EkleModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Depo Depo { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        Depo.Ad = Depo.Ad?.Trim() ?? string.Empty;
        if (ModelState.IsValid &&
            await _db.Depolar.AnyAsync(d => d.Ad.ToLower() == Depo.Ad.ToLower()))
            ModelState.AddModelError("Depo.Ad", "Bu isimde bir depo zaten var.");
        if (!ModelState.IsValid)
            return Page();

        _db.Depolar.Add(Depo);
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{Depo.Ad}\" deposu eklendi.";
        return RedirectToPage("Index");
    }
}

