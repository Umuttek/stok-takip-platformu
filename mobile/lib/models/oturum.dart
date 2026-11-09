class Oturum {
  final String token;
  final String rol;
  final String adSoyad;
  final String eposta;

  const Oturum({
    required this.token,
    required this.rol,
    required this.adSoyad,
    required this.eposta,
  });

  bool get yoneticiMi => rol == 'Yonetici';

  factory Oturum.fromJson(Map<String, dynamic> json) => Oturum(
        token: json['token'] as String,
        rol: json['rol'] as String,
        adSoyad: (json['adSoyad'] ?? '') as String,
        eposta: (json['eposta'] ?? '') as String,
      );
}

