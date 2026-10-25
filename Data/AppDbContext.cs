using Microsoft.EntityFrameworkCore;
using StokTakip.Models;
namespace StokTakip.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Depo> Depolar => Set<Depo>();
    public DbSet<StokHareketi> StokHareketleri => Set<StokHareketi>();
    public DbSet<StokIstegi> StokIstekleri => Set<StokIstegi>();
}