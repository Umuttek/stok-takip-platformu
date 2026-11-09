using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data;
using StokTakip.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Hesap/Giris";
    options.AccessDeniedPath = "/Hesap/ErisimEngellendi";
});
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        // Model bağlama (ör. sayı alanına harf girilmesi) hata mesajlarını Türkçeleştir.
        var m = options.ModelBindingMessageProvider;
        m.SetValueMustBeANumberAccessor(alan => $"{alan} alanı geçerli bir sayı olmalıdır.");
        m.SetValueMustNotBeNullAccessor(alan => $"{alan} alanı zorunludur.");
        m.SetMissingBindRequiredValueAccessor(alan => $"{alan} alanı zorunludur.");
        m.SetAttemptedValueIsInvalidAccessor((deger, alan) => $"'{deger}' değeri {alan} alanı için geçersiz.");
        m.SetNonPropertyAttemptedValueIsInvalidAccessor(deger => $"'{deger}' değeri geçersiz.");
        m.SetUnknownValueIsInvalidAccessor(alan => $"{alan} alanı için girilen değer geçersiz.");
        m.SetValueIsInvalidAccessor(deger => $"'{deger}' değeri geçersiz.");
    });
var app = builder.Build();
using (var scope = app.Services.CreateScope()){await RolVeAdmin.OlusturAsync(scope.ServiceProvider, app.Configuration);}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
