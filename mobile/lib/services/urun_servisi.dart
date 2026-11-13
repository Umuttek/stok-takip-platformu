import '../models/urun.dart';
import 'api_service.dart';
class UrunServisi {
  final ApiService _api = ApiService();
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
}