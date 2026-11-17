class StokHareketi {
  final int id;
  final int urunId;
  final String urunKodu;
  final String urunAdi;
  final int depoId;
  final String depoAdi;
  final String hareketTuru;
  final int miktar;
  final DateTime tarih;
  final String kullaniciAdSoyad;

  const StokHareketi({
    required this.id,
    required this.urunId,
    required this.urunKodu,
    required this.urunAdi,
    required this.depoId,
    required this.depoAdi,
    required this.hareketTuru,
    required this.miktar,
    required this.tarih,
    required this.kullaniciAdSoyad,
  });

  bool get girisMi => hareketTuru == 'Giris';

  factory StokHareketi.fromJson(Map<String, dynamic> json) => StokHareketi(
        id: (json['id'] as num).toInt(),
        urunId: (json['urunId'] as num).toInt(),
        urunKodu: (json['urunKodu'] ?? '') as String,
        urunAdi: (json['urunAdi'] ?? '') as String,
        depoId: (json['depoId'] as num).toInt(),
        depoAdi: (json['depoAdi'] ?? '') as String,
        hareketTuru: (json['hareketTuru'] ?? 'Giris') as String,
        miktar: (json['miktar'] as num).toInt(),
        tarih: DateTime.tryParse(json['tarih'] as String? ?? '')?.toLocal() ??
            DateTime.now(),
        kullaniciAdSoyad: (json['kullaniciAdSoyad'] ?? '') as String,
      );
}

