using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StokTakip.Models;
namespace StokTakip.Pages.Hesap;
public class KayitModel : PageModel
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly SignInManager<Kullanici> _signInManager;
    public KayitModel(UserManager<Kullanici> userManager, SignInManager<Kullanici> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    [BindProperty]
    public KayitGirdi Girdi { get; set; } = new();
    public class KayitGirdi
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre Tekrar")]
        public string SifreTekrar { get; set; } = string.Empty;
    }
    public void OnGet() {}
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var kullanici = new Kullanici
        {
            UserName = Girdi.Email,
            Email = Girdi.Email,
            AdSoyad = Girdi.AdSoyad
        };
        var sonuc = await _userManager.CreateAsync(kullanici, Girdi.Sifre);
        if (sonuc.Succeeded)
        {
            await _userManager.AddToRoleAsync(kullanici, "Calisan");
            await _signInManager.SignInAsync(kullanici, isPersistent: false);
            return RedirectToPage("/Index");
        }
        foreach (var hata in sonuc.Errors)
            ModelState.AddModelError(string.Empty, hata.Description);
        return Page();
    }
}