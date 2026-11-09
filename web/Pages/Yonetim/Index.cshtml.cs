using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace StokTakip.Pages.Yonetim;
[Authorize(Roles = "Yonetici")]
public class IndexModel : PageModel
{
    public void OnGet() { }
}