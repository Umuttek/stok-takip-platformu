using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StokTakip.Api.Dtos;
using StokTakip.Models;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Kullanici> _kullaniciYoneticisi;
    private readonly IConfiguration _ayarlar;

    public AuthController(UserManager<Kullanici> kullaniciYoneticisi, IConfiguration ayarlar)
    {
        _kullaniciYoneticisi = kullaniciYoneticisi;
        _ayarlar = ayarlar;
    }

    [HttpPost("giris")]
    [AllowAnonymous]
    public async Task<ActionResult<GirisCevabi>> Giris(GirisIstegi istek)
    {
        var kullanici = await _kullaniciYoneticisi.FindByEmailAsync(istek.Eposta);
        if (kullanici == null || !await _kullaniciYoneticisi.CheckPasswordAsync(kullanici, istek.Sifre))
            return Unauthorized(new { mesaj = "E-posta veya şifre hatalı." });

        var roller = await _kullaniciYoneticisi.GetRolesAsync(kullanici);
        var rol = roller.FirstOrDefault() ?? "Calisan";

        var bitis = DateTime.UtcNow.AddHours(8);
        var claimler = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kullanici.Id),
            new(ClaimTypes.Email, kullanici.Email ?? string.Empty),
            new(ClaimTypes.Name, kullanici.AdSoyad),
            new(ClaimTypes.Role, rol)
        };

        var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_ayarlar["Jwt:Anahtar"]!));
        var jeton = new JwtSecurityToken(
            issuer: _ayarlar["Jwt:Yayinci"],
            audience: _ayarlar["Jwt:Hedef"],
            claims: claimler,
            expires: bitis,
            signingCredentials: new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256));

        return new GirisCevabi
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jeton),
            Rol = rol,
            AdSoyad = kullanici.AdSoyad,
            Eposta = kullanici.Email ?? string.Empty,
            BitisZamani = bitis
        };
    }

    [HttpGet("ben")]
    [Authorize]
    public IActionResult Ben() => Ok(new
    {
        Eposta = User.FindFirstValue(ClaimTypes.Email),
        AdSoyad = User.FindFirstValue(ClaimTypes.Name),
        Rol = User.FindFirstValue(ClaimTypes.Role)
    });
}

