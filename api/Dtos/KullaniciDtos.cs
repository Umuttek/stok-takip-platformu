using System.ComponentModel.DataAnnotations;

namespace StokTakip.Api.Dtos;

public class KullaniciEkleIstegi
{
    [Required(ErrorMessage = "Ad soyad boş bırakılamaz.")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta boş bırakılamaz."),
     EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
    public string Sifre { get; set; } = string.Empty;

    /// <summary>true ise Yonetici, false ise Calisan rolü verilir.</summary>
    public bool YoneticiYetkisi { get; set; }
}

