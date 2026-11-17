class UrunDepoStok {
  final int depoId;
  final String depoAdi;
  final String? depoKonum;
  final int miktar;

  const UrunDepoStok({
    required this.depoId,
    required this.depoAdi,
    this.depoKonum,
    required this.miktar,
  });

  factory UrunDepoStok.fromJson(Map<String, dynamic> json) => UrunDepoStok(
        depoId: (json['depoId'] as num).toInt(),
        depoAdi: (json['depoAdi'] ?? '') as String,
        depoKonum: json['depoKonum'] as String?,
        miktar: (json['miktar'] as num).toInt(),
      );
}

class Urun {
  final int id;
  final String urunKodu;
  final String ad;
  final String? aciklama;
  final int mevcutMiktar;
  final int kritikStokSeviyesi;
  final int kategoriId;
  final String kategoriAdi;
  final String? fotografBase64;
  final List<UrunDepoStok> depoStoklari;

  const Urun({
    required this.id,
    required this.urunKodu,
    required this.ad,
    this.aciklama,
    required this.mevcutMiktar,
    required this.kritikStokSeviyesi,
    required this.kategoriId,
    this.kategoriAdi = '',
    this.fotografBase64,
    this.depoStoklari = const [],
  });

  factory Urun.fromJson(Map<String, dynamic> json) {
    final depolar = (json['depoStoklari'] as List<dynamic>? ?? [])
        .map((e) => UrunDepoStok.fromJson(e as Map<String, dynamic>))
        .toList();
    return Urun(
      id: (json['id'] as num).toInt(),
      urunKodu: json['urunKodu'] as String,
      ad: json['ad'] as String,
      aciklama: json['aciklama'] as String?,
      mevcutMiktar: (json['mevcutMiktar'] as num).toInt(),
      kritikStokSeviyesi: (json['kritikStokSeviyesi'] as num).toInt(),
      kategoriId: (json['kategoriId'] as num).toInt(),
      kategoriAdi: json['kategoriAdi'] as String? ?? '',
      fotografBase64: json['fotografBase64'] as String?,
      depoStoklari: depolar,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'urunKodu': urunKodu,
      'ad': ad,
      'aciklama': aciklama,
      'mevcutMiktar': mevcutMiktar,
      'kritikStokSeviyesi': kritikStokSeviyesi,
      'kategoriId': kategoriId,
      'fotografBase64': fotografBase64,
    };
  }

  bool get kritikSeviyede => mevcutMiktar <= kritikStokSeviyesi;
  bool get fotografiVar =>
      fotografBase64 != null && fotografBase64!.trim().isNotEmpty;
}
