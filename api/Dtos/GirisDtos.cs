using System.ComponentModel.DataAnnotations;

namespace StokTakip.Api.Dtos;

public class GirisIstegi
{
    [Required, EmailAddress]
    public string Eposta { get; set; } = string.Empty;

    [Required]
    public string Sifre { get; set; } = string.Empty;
}

public class GirisCevabi
{
    public string Token { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public DateTime BitisZamani { get; set; }
}

