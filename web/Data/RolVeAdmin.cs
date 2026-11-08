using Microsoft.AspNetCore.Identity;
using StokTakip.Models;
namespace StokTakip.Data;
public static class RolVeAdmin
{
    public static async Task OlusturAsync(IServiceProvider servisler, IConfiguration ayarlar)
    {
        var rolYoneticisi = servisler.GetRequiredService<RoleManager<IdentityRole>>();
        var kullaniciYoneticisi = servisler.GetRequiredService<UserManager<Kullanici>>();
        string[] roller = { "Yonetici", "Calisan" };
        foreach (var rol in roller)
        {
            if (!await rolYoneticisi.RoleExistsAsync(rol))
                await rolYoneticisi.CreateAsync(new IdentityRole(rol));
        }
        var adminEmail = ayarlar["Admin:Email"];
        var adminSifre = ayarlar["Admin:Sifre"];
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminSifre)) return;
        var admin = await kullaniciYoneticisi.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new Kullanici
            {
                UserName = adminEmail,
                Email = adminEmail,
                AdSoyad = "Sistem Yöneticisi",
                EmailConfirmed = true
            };
            var sonuc = await kullaniciYoneticisi.CreateAsync(admin, adminSifre);
            if (sonuc.Succeeded)
                await kullaniciYoneticisi.AddToRoleAsync(admin, "Yonetici");
        }
    }
}