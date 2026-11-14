using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Pages.StokHareketleri;
[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    public IndexModel(AppDbContext context)
    {
        _context = context;
    }
    public List<StokHareketi> Hareketler { get; set; } = new();
    public async Task OnGetAsync()
    {
        Hareketler = await _context.StokHareketleri
            .Include(h => h.Urun)
            .Include(h => h.Depo)
            .Include(h => h.Kullanici)
            .OrderByDescending(h => h.Tarih)
            .ToListAsync();
    }
}