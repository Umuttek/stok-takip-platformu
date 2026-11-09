using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StokTakip.Api.Dtos;
using StokTakip.Models;

namespace StokTakip.Api.Controllers;

[ApiController]
[Route("api/kullanicilar")]
[Authorize(Roles = "Yonetici")]
public class KullanicilarController : ControllerBase
{
    private readonly UserManager<Kullanici> _kullaniciYoneticisi;

    public KullanicilarController(UserManager<Kullanici> kullaniciYoneticisi)
    {
        _kullaniciYoneticisi = kullaniciYoneticisi;
    }

    [HttpPost]
    public async Task<IActionResult> Ekle(KullaniciEkleIstegi istek)
    {
        var eposta = istek.Eposta.Trim();

        if (await _kullaniciYoneticisi.FindByEmailAsync(eposta) != null)
            return Conflict(new { mesaj = "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var." });

        var kullanici = new Kullanici
        {
            UserName = eposta,
            Email = eposta,
            AdSoyad = istek.AdSoyad.Trim(),
            EmailConfirmed = true
        };

        var sonuc = await _kullaniciYoneticisi.CreateAsync(kullanici, istek.Sifre);
        if (!sonuc.Succeeded)
            return BadRequest(new { mesaj = string.Join(" ", sonuc.Errors.Select(HataMetni)) });

        var rol = istek.YoneticiYetkisi ? "Yonetici" : "Calisan";
        var rolSonuc = await _kullaniciYoneticisi.AddToRoleAsync(kullanici, rol);
        if (!rolSonuc.Succeeded)
        {
            // Rol atanamazsa rolsüz kullanıcı kalmasın.
            await _kullaniciYoneticisi.DeleteAsync(kullanici);
            return BadRequest(new { mesaj = "Kullanıcıya rol atanamadı: " + string.Join(" ", rolSonuc.Errors.Select(e => e.Description)) });
        }

        return Created(string.Empty, new
        {
            kullanici.Id,
            kullanici.AdSoyad,
            Eposta = kullanici.Email,
            Rol = rol
        });
    }

    private static string HataMetni(IdentityError hata) => hata.Code switch
    {
        "PasswordTooShort" => "Şifre en az 6 karakter olmalı.",
        "PasswordRequiresDigit" => "Şifre en az bir rakam içermeli.",
        "PasswordRequiresLower" => "Şifre en az bir küçük harf içermeli.",
        "DuplicateUserName" or "DuplicateEmail" => "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.",
        "InvalidEmail" => "Geçerli bir e-posta adresi girin.",
        _ => hata.Description
    };
}

