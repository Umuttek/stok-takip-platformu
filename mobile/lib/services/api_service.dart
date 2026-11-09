import 'dart:convert';
import 'package:flutter/foundation.dart';
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
  ApiService._();
  static final ApiService _tekil = ApiService._();
  // Tüm uygulama aynı örneği (ve aynı token'ı) kullanır.
  factory ApiService() => _tekil;

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
    } on Exception catch (e) {
      debugPrint('API isteği başarısız (${ApiConfig.baseUrl}): $e');
      throw ApiException(
          'Sunucuya bağlanılamadı. API çalışıyor mu?\n(${ApiConfig.baseUrl}) $e');
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
    if (cevap.statusCode == 400 || cevap.statusCode == 409) {
      throw ApiException(
          _sunucuMesaji(cevap.body) ?? 'Geçersiz istek (${cevap.statusCode}).',
          durum: cevap.statusCode);
    }
    throw ApiException('Sunucu hatası (${cevap.statusCode}).',
        durum: cevap.statusCode);
  }

  /// Sunucunun {"mesaj": ...} veya doğrulama hatası ("errors") gövdesinden metin çıkarır.
  String? _sunucuMesaji(String govde) {
    try {
      final veri = jsonDecode(govde);
      if (veri is Map<String, dynamic>) {
        if (veri['mesaj'] is String) return veri['mesaj'] as String;
        final hatalar = veri['errors'];
        if (hatalar is Map && hatalar.isNotEmpty) {
          final ilk = hatalar.values.first;
          if (ilk is List && ilk.isNotEmpty) return ilk.first.toString();
        }
      }
    } catch (_) {}
    return null;
  }
}