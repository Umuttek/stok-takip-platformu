import 'package:flutter/material.dart';
import '../services/api_service.dart';

class KullaniciEkleEkrani extends StatefulWidget {
  const KullaniciEkleEkrani({super.key});

  @override
  State<KullaniciEkleEkrani> createState() => _KullaniciEkleDurum();
}

class _KullaniciEkleDurum extends State<KullaniciEkleEkrani> {
  final _formKey = GlobalKey<FormState>();
  final _adController = TextEditingController();
  final _epostaController = TextEditingController();
  final _sifreController = TextEditingController();
  final _sifreTekrarController = TextEditingController();
  bool _sifreGizli = true;
  bool _yoneticiYetkisi = false;
  bool _yukleniyor = false;
  String? _hata;

  @override
  void dispose() {
    _adController.dispose();
    _epostaController.dispose();
    _sifreController.dispose();
    _sifreTekrarController.dispose();
    super.dispose();
  }

  String? _adDogrula(String? deger) {
    if (deger == null || deger.trim().isEmpty) {
      return 'Ad soyad boş bırakılamaz.';
    }
    return null;
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
    if (deger == null || deger.isEmpty) return 'Şifre boş bırakılamaz.';
    if (deger.length < 6) return 'Şifre en az 6 karakter olmalı.';
    return null;
  }

  String? _sifreTekrarDogrula(String? deger) {
    if (deger != _sifreController.text) return 'Şifreler eşleşmiyor.';
    return null;
  }

  Future<void> _kaydet() async {
    if (_yukleniyor) return;
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _yukleniyor = true;
      _hata = null;
    });
    try {
      await ApiService().post('/api/kullanicilar', {
        'adSoyad': _adController.text.trim(),
        'eposta': _epostaController.text.trim(),
        'sifre': _sifreController.text,
        'yoneticiYetkisi': _yoneticiYetkisi,
      });
      if (!mounted) return;
      final rol = _yoneticiYetkisi ? 'Yönetici' : 'Çalışan';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Kullanıcı eklendi ($rol).')),
      );
      Navigator.of(context).pop();
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
    return Scaffold(
      appBar: AppBar(title: const Text('Yeni Kullanıcı Ekle')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 400),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    TextFormField(
                      controller: _adController,
                      textCapitalization: TextCapitalization.words,
                      textInputAction: TextInputAction.next,
                      decoration: const InputDecoration(
                        labelText: 'Ad Soyad',
                        prefixIcon: Icon(Icons.person_outline),
                      ),
                      validator: _adDogrula,
                    ),
                    const SizedBox(height: 16),
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
                      textInputAction: TextInputAction.next,
                      decoration: InputDecoration(
                        labelText: 'Şifre',
                        prefixIcon: const Icon(Icons.lock_outline),
                        suffixIcon: IconButton(
                          icon: Icon(_sifreGizli
                              ? Icons.visibility_outlined
                              : Icons.visibility_off_outlined),
                          onPressed: () =>
                              setState(() => _sifreGizli = !_sifreGizli),
                        ),
                      ),
                      validator: _sifreDogrula,
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _sifreTekrarController,
                      obscureText: _sifreGizli,
                      textInputAction: TextInputAction.done,
                      onFieldSubmitted: (_) => _kaydet(),
                      decoration: const InputDecoration(
                        labelText: 'Şifre (Tekrar)',
                        prefixIcon: Icon(Icons.lock_outline),
                      ),
                      validator: _sifreTekrarDogrula,
                    ),
                    const SizedBox(height: 8),
                    CheckboxListTile(
                      contentPadding: EdgeInsets.zero,
                      controlAffinity: ListTileControlAffinity.leading,
                      title: const Text('Yönetici yetkisi ver'),
                      subtitle: Text(_yoneticiYetkisi
                          ? 'Kullanıcı Yönetici olacak.'
                          : 'İşaretlenmezse kullanıcı Çalışan olur.'),
                      value: _yoneticiYetkisi,
                      onChanged: _yukleniyor
                          ? null
                          : (v) => setState(() => _yoneticiYetkisi = v ?? false),
                    ),
                    if (_hata != null) ...[
                      const SizedBox(height: 8),
                      Text(
                        _hata!,
                        textAlign: TextAlign.center,
                        style: TextStyle(
                            color: Theme.of(context).colorScheme.error),
                      ),
                    ],
                    const SizedBox(height: 24),
                    ElevatedButton(
                      onPressed: _yukleniyor ? null : _kaydet,
                      child: _yukleniyor
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Kullanıcıyı Ekle'),
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

