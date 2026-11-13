using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakip.Api.Dtos;
using StokTakip.Data;
using StokTakip.Models;
namespace StokTakip.Api.Controllers;
[ApiController]
[Route("api/urunler")]
[Authorize]
public class UrunlerController : ControllerBase
{
    private readonly AppDbContext _db;
    public UrunlerController(AppDbContext db) => _db = db;
    private static readonly Expression<Func<Urun, UrunCevabi>> CevabaCevir = u => new UrunCevabi
    {
        Id = u.Id,
        UrunKodu = u.UrunKodu,
        Ad = u.Ad,
        Aciklama = u.Aciklama,
        MevcutMiktar = u.MevcutMiktar,
        KritikStokSeviyesi = u.KritikStokSeviyesi,
        KategoriId = u.KategoriId,
        KategoriAdi = u.Kategori != null ? u.Kategori.Ad : "",
        KritikMi = u.MevcutMiktar <= u.KritikStokSeviyesi
    };
    [HttpGet]
    public async Task<ActionResult<List<UrunCevabi>>> Listele([FromQuery] string? arama)
    {
        var sorgu = _db.Urunler.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(arama))
        {
            var desen = $"%{arama.Trim()}%";
            sorgu = sorgu.Where(u =>
                EF.Functions.ILike(u.Ad, desen) ||
                EF.Functions.ILike(u.UrunKodu, desen));
        }
        var urunler = await sorgu
            .OrderBy(u => u.Ad)
            .Select(CevabaCevir)
            .ToListAsync();
        return Ok(urunler);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UrunCevabi>> Getir(int id)
    {
        var urun = await _db.Urunler
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(CevabaCevir)
            .FirstOrDefaultAsync();
        if (urun == null)
            return NotFound(new { mesaj = "Ürün bulunamadı." });

        return Ok(urun);
    }
}