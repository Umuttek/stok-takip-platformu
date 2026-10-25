# Stok Takip Platformu

Bu projede amaç, ürün ve malzemelerin stok durumlarını takip etmek, stok giriş-çıkış işlemlerini yönetmek ve depo süreçlerini kolaylaştırmaktır.
Bu proje, depo çalışanları ve yöneticilerin ürün/parça stoklarını merkezi bir sistem üzerinden takip edebilmesini, stok giriş-çıkış işlemlerini kaydedebilmesini sağlayan ağ tabanlı bir uygulamadır. ASP.NET Core Razor Pages ve Web API kullanılarak geliştirilmiştir.

## Temel Özellikler
- Ürün ve kategori yönetimi
- Stok giriş ve çıkış kayıtları
- Depo ve malzeme talebi yönetimi
- Kritik stok seviyesi uyarıları
- Kullanıcı girişi ve rol bazlı yetkilendirme
- Web ve mobil uygulama arasında API üzerinden veri paylaşımı
## Kullanılacak Teknolojiler
- ASP.NET Core Razor Pages & Web API
- Entity Framework Core
- PostgreSQL
- Swagger
## Kurulum - İlk Hafta
Projeyi .NET 10 ve PostgreSQL ile geliştiriyoruz. Çalıştırmak için ikisinin de bilgisayarda kurulu olması gerekiyor.
İlk olarak pgAdmin'de StokTakipDb adında bir veritabanı oluşturduk. Veritabanı şifremizin GitHub'a gitmemesi için bağlantı bilgisini appsettings.json'a yazmadık, user-secrets ile kendi bilgisayarımızda sakladık. Projeyi çalıştırmak isteyen biri proje klasöründe şu komutu kendi şifresiyle çalıştırmalı:
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=StokTakipDb;Username=postgres;Password=sifreniz"
Sonrasında dotnet run komutuyla proje çalışıyor.
Şu an tablolarımızın sadece C# sınıflarını yazdık, veritabanında henüz oluşmadılar. Migration işlemlerini 2. haftada yapacağız.