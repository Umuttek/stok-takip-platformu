using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Depolar;

[Authorize(Roles = "Yonetici")]
public class SilModel : PageModel
{
    private readonly AppDbContext _db;
    public SilModel(AppDbContext db) => _db = db;

    public Depo Depo { get; set; } = new();
    public int HareketSayisi { get; set; }
    public int TalepSayisi { get; set; }
    public bool Silinebilir => HareketSayisi == 0 && TalepSayisi == 0;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var depo = await _db.Depolar.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (depo == null) return NotFound();
        Depo = depo;
        await BaglantilariSay(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var depo = await _db.Depolar.FindAsync(id);
        if (depo == null) return NotFound();

        // Silme "cascade" olduğu için depoya ait stok hareketleri ve talepler de silinirdi; engelliyoruz.
        await BaglantilariSay(id);
        if (!Silinebilir)
        {
            TempData["Hata"] = $"\"{depo.Ad}\" deposuna ait stok hareketi veya talep kaydı olduğu için silinemez.";
            return RedirectToPage("Index");
        }

        _db.Depolar.Remove(depo);
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{depo.Ad}\" deposu silindi.";
        return RedirectToPage("Index");
    }

    private async Task BaglantilariSay(int id)
    {
        HareketSayisi = await _db.StokHareketleri.CountAsync(h => h.DepoId == id);
        TalepSayisi = await _db.StokIstekleri.CountAsync(t => t.DepoId == id);
    }
}

