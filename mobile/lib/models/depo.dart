class DepoUrunStok {
  final int urunId;
  final String urunKodu;
  final String urunAdi;
  final int miktar;

  const DepoUrunStok({
    required this.urunId,
    required this.urunKodu,
    required this.urunAdi,
    required this.miktar,
  });

  factory DepoUrunStok.fromJson(Map<String, dynamic> json) => DepoUrunStok(
        urunId: (json['urunId'] as num).toInt(),
        urunKodu: (json['urunKodu'] ?? '') as String,
        urunAdi: (json['urunAdi'] ?? '') as String,
        miktar: (json['miktar'] as num).toInt(),
      );
}

class Depo {
  final int id;
  final String ad;
  final String? konum;
  final String? aciklama;
  final int toplamUrunAdeti;
  final int urunCesidiSayisi;
  final List<DepoUrunStok> urunler;

  const Depo({
    required this.id,
    required this.ad,
    this.konum,
    this.aciklama,
    this.toplamUrunAdeti = 0,
    this.urunCesidiSayisi = 0,
    this.urunler = const [],
  });

  factory Depo.fromJson(Map<String, dynamic> json) {
    final urunListesi = (json['urunler'] as List<dynamic>? ?? [])
        .map((e) => DepoUrunStok.fromJson(e as Map<String, dynamic>))
        .toList();
    return Depo(
      id: (json['id'] as num).toInt(),
      ad: (json['ad'] ?? '') as String,
      konum: json['konum'] as String?,
      aciklama: json['aciklama'] as String?,
      toplamUrunAdeti: (json['toplamUrunAdeti'] as num? ?? 0).toInt(),
      urunCesidiSayisi: (json['urunCesidiSayisi'] as num? ?? 0).toInt(),
      urunler: urunListesi,
    );
  }
}

