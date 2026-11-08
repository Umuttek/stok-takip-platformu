import 'dart:convert';
import 'package:http/http.dart' as http;
import 'api_config.dart';
class ApiException implements Exception {
  final String mesaj;
  final int? durum;
  ApiException(this.mesaj, {this.durum});
  @override
  String toString() => mesaj;
}
class ApiService {
  String? token;
  Map<String, String> get _headers => {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };
  Uri _url(String yol) => Uri.parse('${ApiConfig.baseUrl}$yol');
  Future<dynamic> get(String yol) {
    return _istekGonder(() => http.get(_url(yol), headers: _headers));
  }
  Future<dynamic> post(String yol, Map<String, dynamic> veri) {
    return _istekGonder(() =>
        http.post(_url(yol), headers: _headers, body: jsonEncode(veri)));
  }
  Future<dynamic> put(String yol, Map<String, dynamic> veri) {
    return _istekGonder(() =>
        http.put(_url(yol), headers: _headers, body: jsonEncode(veri)));
  }
  Future<dynamic> delete(String yol) {
    return _istekGonder(() => http.delete(_url(yol), headers: _headers));
  }
  Future<dynamic> _istekGonder(Future<http.Response> Function() istek) async {
    final http.Response cevap;
    try {
      cevap = await istek().timeout(const Duration(seconds: 10));
    } on Exception {
      throw ApiException('Sunucuya bağlanılamadı. API çalışıyor mu?');
    }
    if (cevap.statusCode >= 200 && cevap.statusCode < 300) {
      if (cevap.body.isEmpty) return null;
      return jsonDecode(cevap.body);
    }
    if (cevap.statusCode == 401) {
      throw ApiException('Giriş yapmanız gerekiyor.', durum: 401);
    }
    if (cevap.statusCode == 403) {
      throw ApiException('Bu işlem için yetkiniz yok.', durum: 403);
    }
    throw ApiException('Sunucu hatası (${cevap.statusCode}).',
        durum: cevap.statusCode);
  }
}