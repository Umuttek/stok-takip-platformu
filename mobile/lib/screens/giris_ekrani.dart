import 'package:flutter/material.dart';
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
  void _girisYap() {
    if (!_formKey.currentState!.validate()) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => const AnaEkran()),
    );
  }
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 400),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Icon(Icons.inventory_2_outlined, size: 72),
                    const SizedBox(height: 16),
                    Text(
                      'Stok Takip',
                      textAlign: TextAlign.center,
                      style: Theme.of(context).textTheme.headlineMedium,
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Devam etmek için giriş yapın',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 32),
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
                          onPressed: () =>
                              setState(() => _sifreGizli = !_sifreGizli),
                        ),
                      ),
                      validator: _sifreDogrula,
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton(
                      onPressed: _girisYap,
                      child: const Text('Giriş Yap'),
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