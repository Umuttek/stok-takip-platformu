using System.ComponentModel.DataAnnotations;
namespace StokTakip.Models;
public class Depo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Depo adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Depo adı {2}-{1} karakter arasında olmalıdır.")]
    [Display(Name = "Depo Adı")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Konum en fazla {1} karakter olabilir.")]
    [Display(Name = "Konum")]
    public string? Konum { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public List<StokHareketi> StokHareketleri { get; set; } = new();
    public List<StokIstegi> StokIstekleri { get; set; } = new();
}
