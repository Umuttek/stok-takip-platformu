import '../models/stok_hareketi.dart';
import 'api_service.dart';

class StokHareketServisi {
  final ApiService _api = ApiService();

  Future<List<StokHareketi>> listele({int? urunId, int? depoId}) async {
    final parametreler = <String>[];
    if (urunId != null) parametreler.add('urunId=$urunId');
    if (depoId != null) parametreler.add('depoId=$depoId');
    final sorgu = parametreler.isEmpty ? '' : '?${parametreler.join('&')}';

    final cevap =
        await _api.get('/api/stok-hareketleri$sorgu') as List<dynamic>;
    return cevap
        .map((e) => StokHareketi.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<StokHareketi> ekle({
    required int urunId,
    required int depoId,
    required String hareketTuru,
    required int miktar,
  }) async {
    final cevap = await _api.post('/api/stok-hareketleri', {
      'urunId': urunId,
      'depoId': depoId,
      'hareketTuru': hareketTuru,
      'miktar': miktar,
    });
    return StokHareketi.fromJson(cevap as Map<String, dynamic>);
  }
}

