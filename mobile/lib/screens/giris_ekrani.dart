import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import '../tema/uygulama_tema.dart';
import 'ana_ekran.dart';

class GirisEkrani extends StatefulWidget {
  const GirisEkrani({super.key});
  @override
  State<GirisEkrani> createState() => _GirisEkraniState();
}

class _GirisEkraniState extends State<GirisEkrani> {
  final _formKey = GlobalKey<FormState>();
  final _epostaController = TextEditingController();
  final _sifreController = TextEditingController();
  bool _sifreGizli = true;
  bool _yukleniyor = false;
  String? _hata;

  @override
  void dispose() {
    _epostaController.dispose();
    _sifreController.dispose();
    super.dispose();
  }

  String? _epostaDogrula(String? deger) {
    if (deger == null || deger.trim().isEmpty) {
      return 'E-posta boş bırakılamaz.';
    }
    final desen = RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$');
    if (!desen.hasMatch(deger.trim())) {
      return 'Geçerli bir e-posta adresi girin.';
    }
    return null;
  }

  String? _sifreDogrula(String? deger) {
    if (deger == null || deger.isEmpty) {
      return 'Şifre boş bırakılamaz.';
    }
    if (deger.length < 6) {
      return 'Şifre en az 6 karakter olmalı.';
    }
    return null;
  }

  Future<void> _girisYap() async {
    if (_yukleniyor) return;
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _yukleniyor = true;
      _hata = null;
    });
    try {
      final oturum = await AuthService.instance
          .girisYap(_epostaController.text.trim(), _sifreController.text);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => anaEkranSec(oturum)),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _yukleniyor = false;
        _hata = e.mesaj;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final renkler = Theme.of(context).colorScheme;
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [Color(0xFF4F46E5), Color(0xFF7C3AED)],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 420),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      padding: const EdgeInsets.all(18),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.18),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.inventory_2_rounded,
                          size: 48, color: Colors.white),
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Stok Takip',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 30,
                        fontWeight: FontWeight.w800,
                        letterSpacing: 0.3,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Depo ve stok yönetimi',
                      style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.85)),
                    ),
                    const SizedBox(height: 28),
                    Card(
                      child: Padding(
                        padding: const EdgeInsets.all(24),
                        child: Form(
                          key: _formKey,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text(
                                'Giriş Yap',
                                style: Theme.of(context)
                                    .textTheme
                                    .titleLarge
                                    ?.copyWith(fontWeight: FontWeight.w700),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                'Devam etmek için hesabınızla giriş yapın.',
                                style: TextStyle(color: Colors.grey.shade600),
                              ),
                              const SizedBox(height: 24),
                              TextFormField(
                                controller: _epostaController,
                                keyboardType: TextInputType.emailAddress,
                                textInputAction: TextInputAction.next,
                                decoration: const InputDecoration(
                                  labelText: 'E-posta',
                                  prefixIcon: Icon(Icons.email_outlined),
                                ),
                                validator: _epostaDogrula,
                              ),
                              const SizedBox(height: 16),
                              TextFormField(
                                controller: _sifreController,
                                obscureText: _sifreGizli,
                                textInputAction: TextInputAction.done,
                                onFieldSubmitted: (_) => _girisYap(),
                                decoration: InputDecoration(
                                  labelText: 'Şifre',
                                  prefixIcon: const Icon(Icons.lock_outline),
                                  suffixIcon: IconButton(
                                    icon: Icon(_sifreGizli
                                        ? Icons.visibility_outlined
                                        : Icons.visibility_off_outlined),
                                    onPressed: () => setState(
                                        () => _sifreGizli = !_sifreGizli),
                                  ),
                                ),
                                validator: _sifreDogrula,
                              ),
                              if (_hata != null) ...[
                                const SizedBox(height: 16),
                                Container(
                                  padding: const EdgeInsets.all(12),
                                  decoration: BoxDecoration(
                                    color: renkler.errorContainer,
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Row(
                                    children: [
                                      Icon(Icons.error_outline,
                                          color: renkler.error, size: 20),
                                      const SizedBox(width: 8),
                                      Expanded(
                                        child: Text(
                                          _hata!,
                                          style: TextStyle(
                                              color: renkler.onErrorContainer),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                              const SizedBox(height: 24),
                              ElevatedButton(
                                onPressed: _yukleniyor ? null : _girisYap,
                                child: _yukleniyor
                                    ? const SizedBox(
                                        width: 22,
                                        height: 22,
                                        child: CircularProgressIndicator(
                                            strokeWidth: 2.4,
                                            color: UygulamaTema.anaRenk),
                                      )
                                    : const Text('Giriş Yap'),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
