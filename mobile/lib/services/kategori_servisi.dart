import '../models/kategori.dart';
import 'api_service.dart';

class KategoriServisi {
  final ApiService _api = ApiService();

  Future<List<Kategori>> listele() async {
    final cevap = await _api.get('/api/kategoriler') as List<dynamic>;
    return cevap
        .map((e) => Kategori.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Kategori> ekle({
    required String ad,
    String? aciklama,
  }) async {
    final cevap = await _api.post('/api/kategoriler', {
      'ad': ad,
      'aciklama': aciklama,
    });
    return Kategori.fromJson(cevap as Map<String, dynamic>);
  }
}

