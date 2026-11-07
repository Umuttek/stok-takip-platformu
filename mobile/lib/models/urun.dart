class Urun {
  final int id;
  final String urunKodu;
  final String ad;
  final String? aciklama;
  final int mevcutMiktar;
  final int kritikStokSeviyesi;
  final int kategoriId;
  const Urun({
    required this.id,
    required this.urunKodu,
    required this.ad,
    this.aciklama,
    required this.mevcutMiktar,
    required this.kritikStokSeviyesi,
    required this.kategoriId,
  });
  factory Urun.fromJson(Map<String, dynamic> json) {
    return Urun(
      id: json['id'] as int,
      urunKodu: json['urunKodu'] as String,
      ad: json['ad'] as String,
      aciklama: json['aciklama'] as String?,
      mevcutMiktar: json['mevcutMiktar'] as int,
      kritikStokSeviyesi: json['kritikStokSeviyesi'] as int,
      kategoriId: json['kategoriId'] as int,
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
    };
  }
  bool get kritikSeviyede => mevcutMiktar < kritikStokSeviyesi;
}