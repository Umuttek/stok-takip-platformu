class Kategori {
  final int id;
  final String ad;
  final String? aciklama;

  const Kategori({
    required this.id,
    required this.ad,
    this.aciklama,
  });

  factory Kategori.fromJson(Map<String, dynamic> json) => Kategori(
        id: (json['id'] as num).toInt(),
        ad: (json['ad'] ?? '') as String,
        aciklama: json['aciklama'] as String?,
      );
}

