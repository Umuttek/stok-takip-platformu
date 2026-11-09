using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.Urunler;

[Authorize(Roles = "Yonetici")]
public class SilModel : PageModel
{
    private readonly AppDbContext _db;
    public SilModel(AppDbContext db) => _db = db;

    public Urun Urun { get; set; } = new();
    public int HareketSayisi { get; set; }
    public int TalepSayisi { get; set; }
    public bool Silinebilir => HareketSayisi == 0 && TalepSayisi == 0;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var urun = await _db.Urunler.AsNoTracking().Include(u => u.Kategori).FirstOrDefaultAsync(u => u.Id == id);
        if (urun == null) return NotFound();
        Urun = urun;
        await BaglantilariSay(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var urun = await _db.Urunler.FindAsync(id);
        if (urun == null) return NotFound();

        // Silme "cascade" olduğu için ürünün stok geçmişi de silinirdi; geçmişi korumak için engelliyoruz.
        await BaglantilariSay(id);
        if (!Silinebilir)
        {
            TempData["Hata"] = $"\"{urun.Ad}\" ürününe ait stok hareketi veya talep kaydı olduğu için silinemez.";
            return RedirectToPage("Index");
        }

        _db.Urunler.Remove(urun);
        await _db.SaveChangesAsync();
        TempData["Basari"] = $"\"{urun.Ad}\" ürünü silindi.";
        return RedirectToPage("Index");
    }

    private async Task BaglantilariSay(int id)
    {
        HareketSayisi = await _db.StokHareketleri.CountAsync(h => h.UrunId == id);
        TalepSayisi = await _db.StokIstekleri.CountAsync(t => t.UrunId == id);
    }
}

