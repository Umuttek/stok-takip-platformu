using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Depolar;

[Authorize(Roles = "Yonetici")]
public class DuzenleModel : PageModel
{
    private readonly AppDbContext _db;
    public DuzenleModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Depo Depo { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var depo = await _db.Depolar.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (depo == null) return NotFound();
        Depo = depo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var kayit = await _db.Depolar.FindAsync(id);
        if (kayit == null) return NotFound();

        Depo.Id = id;
        Depo.Ad = Depo.Ad?.Trim() ?? string.Empty;
        if (ModelState.IsValid &&
            await _db.Depolar.AnyAsync(d => d.Id != id && d.Ad.ToLower() == Depo.Ad.ToLower()))
            ModelState.AddModelError("Depo.Ad", "Bu isimde bir depo zaten var.");
        if (!ModelState.IsValid)
            return Page();

        // Yalnızca formdaki alanlar güncellenir (overposting'e karşı).
        kayit.Ad = Depo.Ad;
        kayit.Konum = Depo.Konum;
        kayit.Aciklama = Depo.Aciklama;
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{kayit.Ad}\" deposu güncellendi.";
        return RedirectToPage("Index");
    }
}

