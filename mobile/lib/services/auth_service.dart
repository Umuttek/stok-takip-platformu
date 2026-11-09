import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../models/oturum.dart';
import 'api_service.dart';

class AuthService {
  AuthService._();
  static final AuthService instance = AuthService._();

  static const _tokenAnahtari = 'jwt_token';
  static const _rolAnahtari = 'rol';
  static const _adAnahtari = 'ad_soyad';
  static const _epostaAnahtari = 'eposta';

  final FlutterSecureStorage _depo = const FlutterSecureStorage();
  final ApiService _api = ApiService();

  Oturum? aktifOturum;

  Future<Oturum> girisYap(String eposta, String sifre) async {
    final Oturum oturum;
    try {
      final cevap = await _api.post('/api/auth/giris', {
        'eposta': eposta,
        'sifre': sifre,
      });
      oturum = Oturum.fromJson(cevap as Map<String, dynamic>);
    } on ApiException catch (e) {
      if (e.durum == 401) {
        throw ApiException('E-posta veya şifre hatalı.', durum: 401);
      }
      rethrow;
    }
    await _depo.write(key: _tokenAnahtari, value: oturum.token);
    await _depo.write(key: _rolAnahtari, value: oturum.rol);
    await _depo.write(key: _adAnahtari, value: oturum.adSoyad);
    await _depo.write(key: _epostaAnahtari, value: oturum.eposta);
    _oturumuUygula(oturum);
    return oturum;
  }

  /// Uygulama açılışında saklı oturumu yükler. Yoksa null döner.
  Future<Oturum?> kayitliOturumuYukle() async {
    final token = await _depo.read(key: _tokenAnahtari);
    final rol = await _depo.read(key: _rolAnahtari);
    if (token == null || rol == null) return null;
    final oturum = Oturum(
      token: token,
      rol: rol,
      adSoyad: await _depo.read(key: _adAnahtari) ?? '',
      eposta: await _depo.read(key: _epostaAnahtari) ?? '',
    );
    _oturumuUygula(oturum);
    return oturum;
  }

  Future<void> cikisYap() async {
    await _depo.deleteAll();
    aktifOturum = null;
    _api.token = null;
  }

  void _oturumuUygula(Oturum oturum) {
    aktifOturum = oturum;
    _api.token = oturum.token;
  }
}

