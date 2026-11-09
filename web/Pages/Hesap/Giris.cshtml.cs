using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StokTakip.Models;
namespace StokTakip.Pages.Hesap;
public class GirisModel : PageModel
{
    private readonly SignInManager<Kullanici> _signInManager;
    public GirisModel(SignInManager<Kullanici> signInManager)
    {
        _signInManager = signInManager;
    }
    [BindProperty]
    public GirisGirdi Girdi { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public class GirisGirdi
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
        [Display(Name = "Beni hatırla")]
        public bool BeniHatirla { get; set; }
    }
    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();
        var sonuc = await _signInManager.PasswordSignInAsync(
            Girdi.Email, Girdi.Sifre, Girdi.BeniHatirla, lockoutOnFailure: false);
        if (sonuc.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToPage("/Index");
        }
        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        return Page();
    }
}