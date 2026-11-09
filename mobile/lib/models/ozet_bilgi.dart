class OzetBilgi {
  final int urunCesidi;
  final int toplamStok;
  final int kritikStokUrunSayisi;

  const OzetBilgi({
    required this.urunCesidi,
    required this.toplamStok,
    required this.kritikStokUrunSayisi,
  });

  factory OzetBilgi.fromJson(Map<String, dynamic> json) => OzetBilgi(
        urunCesidi: (json['urunCesidi'] as num).toInt(),
        toplamStok: (json['toplamStok'] as num).toInt(),
        kritikStokUrunSayisi: (json['kritikStokUrunSayisi'] as num).toInt(),
      );
}

