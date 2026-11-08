import 'package:flutter/material.dart';
import '../services/api_service.dart';
class AnaEkran extends StatefulWidget {
  const AnaEkran({super.key});
  @override
  State<AnaEkran> createState() => _AnaEkranDurum();
}
class _AnaEkranDurum extends State<AnaEkran> {
  final ApiService _api = ApiService();
  bool _yukleniyor = false;
  String? _sonuc;
  Future<void> _baglantiyiTestEt() async {
    setState(() {
      _yukleniyor = true;
      _sonuc = null;
    });
    String mesaj;
    try {
      final veri = await _api.get('/weatherforecast') as List;
      mesaj = 'Bağlantı başarılı: ${veri.length} kayıt alındı.';
    } on ApiException catch (e) {
      mesaj = e.mesaj;
    }
    if (!mounted) return;
    setState(() {
      _yukleniyor = false;
      _sonuc = mesaj;
    });
  }
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Stok Takip')),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.inventory_2_outlined, size: 64),
              const SizedBox(height: 16),
              const Text('Stok Takip Uygulaması'),
              const SizedBox(height: 32),
              ElevatedButton(
                onPressed: _yukleniyor ? null : _baglantiyiTestEt,
                child: _yukleniyor
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Bağlantıyı test et'),
              ),
              if (_sonuc != null) ...[
                const SizedBox(height: 16),
                Text(_sonuc!, textAlign: TextAlign.center),
              ],
            ],
          ),
        ),
      ),
    );
  }
}