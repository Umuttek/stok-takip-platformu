import '../models/urun.dart';
import 'api_service.dart';

class UrunServisi {
  final ApiService _api = ApiService();

  Future<String> sonrakiKoduGetir() async {
    final cevap = await _api.get('/api/urunler/sonraki-kod');
    return (cevap as Map<String, dynamic>)['urunKodu'] as String;
  }

  Future<List<Urun>> listele({String? arama}) async {
    var yol = '/api/urunler';
    if (arama != null && arama.trim().isNotEmpty) {
      yol += '?arama=${Uri.encodeQueryComponent(arama.trim())}';
    }
    final cevap = await _api.get(yol) as List<dynamic>;
    return cevap
        .map((e) => Urun.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Urun> getir(int id) async {
    final cevap = await _api.get('/api/urunler/$id');
    return Urun.fromJson(cevap as Map<String, dynamic>);
  }

  Future<Urun> ekle({
    required String ad,
    String? aciklama,
    required int kategoriId,
    required int kritikStokSeviyesi,
    required int mevcutMiktar,
    required int depoId,
    String? fotografBase64,
  }) async {
    final cevap = await _api.post('/api/urunler', {
      'ad': ad,
      'aciklama': aciklama,
      'kategoriId': kategoriId,
      'kritikStokSeviyesi': kritikStokSeviyesi,
      'mevcutMiktar': mevcutMiktar,
      'depoId': depoId,
      if (fotografBase64 != null && fotografBase64.isNotEmpty)
        'fotografBase64': fotografBase64,
    });
    return Urun.fromJson(cevap as Map<String, dynamic>);
  }

  Future<Urun> guncelle({
    required int id,
    required String ad,
    String? aciklama,
    required int kategoriId,
    required int kritikStokSeviyesi,
    String? fotografBase64,
    bool fotografiKaldir = false,
  }) async {
    final cevap = await _api.put('/api/urunler/$id', {
      'ad': ad,
      'aciklama': aciklama,
      'kategoriId': kategoriId,
      'kritikStokSeviyesi': kritikStokSeviyesi,
      if (fotografBase64 != null && fotografBase64.isNotEmpty)
        'fotografBase64': fotografBase64,
      'fotografiKaldir': fotografiKaldir,
    });
    return Urun.fromJson(cevap as Map<String, dynamic>);
  }
}
