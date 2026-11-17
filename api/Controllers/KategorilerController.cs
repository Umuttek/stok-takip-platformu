using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakip.Api.Dtos;
using StokTakip.Data;
using StokTakip.Models;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/kategoriler")]
[Authorize]
public class KategorilerController : ControllerBase
{
    private readonly AppDbContext _db;

    public KategorilerController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<KategoriCevabi>>> Listele()
    {
        var kategoriler = await _db.Kategoriler
            .AsNoTracking()
            .OrderBy(k => k.Ad)
            .Select(k => new KategoriCevabi
            {
                Id = k.Id,
                Ad = k.Ad,
                Aciklama = k.Aciklama
            })
            .ToListAsync();

        return Ok(kategoriler);
    }

    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<ActionResult<KategoriCevabi>> Ekle(KategoriKaydetIstegi istek)
    {
        var ad = istek.Ad.Trim();
        if (await _db.Kategoriler.AnyAsync(k => k.Ad.ToLower() == ad.ToLower()))
            return Conflict(new { mesaj = "Bu isimde bir kategori zaten var." });

        var kategori = new Kategori
        {
            Ad = ad,
            Aciklama = string.IsNullOrWhiteSpace(istek.Aciklama) ? null : istek.Aciklama.Trim()
        };

        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new KategoriCevabi
        {
            Id = kategori.Id,
            Ad = kategori.Ad,
            Aciklama = kategori.Aciklama
        });
    }
}

