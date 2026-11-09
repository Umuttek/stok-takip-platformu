import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import 'ana_ekran.dart';
import 'giris_ekrani.dart';

/// Saklı token varsa girişi atlayıp role göre ana ekrana yönlendirir.
class AcilisEkrani extends StatefulWidget {
  const AcilisEkrani({super.key});

  @override
  State<AcilisEkrani> createState() => _AcilisEkraniDurum();
}

class _AcilisEkraniDurum extends State<AcilisEkrani> {
  @override
  void initState() {
    super.initState();
    _yonlendir();
  }

  Future<void> _yonlendir() async {
    final oturum = await AuthService.instance.kayitliOturumuYukle();
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) =>
            oturum == null ? const GirisEkrani() : anaEkranSec(oturum),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(child: CircularProgressIndicator()),
    );
  }
}

