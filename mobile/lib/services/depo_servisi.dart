import '../models/depo.dart';
import 'api_service.dart';

class DepoServisi {
  final ApiService _api = ApiService();

  Future<List<Depo>> listele() async {
    final cevap = await _api.get('/api/depolar') as List<dynamic>;
    return cevap
        .map((e) => Depo.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Depo> getir(int id) async {
    final cevap = await _api.get('/api/depolar/$id');
    return Depo.fromJson(cevap as Map<String, dynamic>);
  }

  Future<Depo> ekle({
    required String ad,
    String? konum,
    String? aciklama,
  }) async {
    final cevap = await _api.post('/api/depolar', {
      'ad': ad,
      'konum': konum,
      'aciklama': aciklama,
    });
    return Depo.fromJson(cevap as Map<String, dynamic>);
  }

  Future<Depo> guncelle({
    required int id,
    required String ad,
    String? konum,
    String? aciklama,
  }) async {
    final cevap = await _api.put('/api/depolar/$id', {
      'ad': ad,
      'konum': konum,
      'aciklama': aciklama,
    });
    return Depo.fromJson(cevap as Map<String, dynamic>);
  }

  Future<void> sil(int id) async {
    await _api.delete('/api/depolar/$id');
  }
}

