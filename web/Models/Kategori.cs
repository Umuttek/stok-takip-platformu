using System.ComponentModel.DataAnnotations;
namespace StokTakip.Models;
public class Kategori
{
    public int Id {get; set;}

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı {2}-{1} karakter arasında olmalıdır.")]
    [Display(Name = "Kategori Adı")]
    public string Ad {get; set;} = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama {get; set;}

    public List<Urun> Urunler {get; set;} = new();
}
