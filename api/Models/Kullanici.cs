using Microsoft.AspNetCore.Identity;
namespace StokTakip.Models;
public class Kullanici : IdentityUser
{
    public string AdSoyad { get; set; } = string.Empty;
    public List<StokHareketi> StokHareketleri { get; set; } = new();
    public List<StokIstegi> StokIstekleri { get; set; } = new();
}