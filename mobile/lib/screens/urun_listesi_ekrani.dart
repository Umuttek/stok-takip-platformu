import 'dart:async';
import 'package:flutter/material.dart';
import '../models/urun.dart';
import '../services/api_service.dart';
import '../services/urun_servisi.dart';
class UrunListesiEkrani extends StatefulWidget {
  const UrunListesiEkrani({super.key});
  @override
  State<UrunListesiEkrani> createState() => _UrunListesiEkraniState();
}
class _UrunListesiEkraniState extends State<UrunListesiEkrani> {
  final _servis = UrunServisi();
  final _aramaController = TextEditingController();
  Timer? _aramaZamanlayici;
  List<Urun> _urunler = [];
  bool _yukleniyor = true;
  String? _hata;
  @override
  void initState() {
    super.initState();
    _yukle();
  }
  @override
  void dispose() {
    _aramaZamanlayici?.cancel();
    _aramaController.dispose();
    super.dispose();
  }
  Future<void> _yukle() async {
    setState(() {
      _yukleniyor = true;
      _hata = null;
    });
    try {
      final urunler = await _servis.listele(arama: _aramaController.text);
      if (!mounted) return;
      setState(() {
        _urunler = urunler;
        _yukleniyor = false;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _hata = e.mesaj;
        _yukleniyor = false;
      });
    }
  }
  void _aramaDegisti(String metin) {
    _aramaZamanlayici?.cancel();
    _aramaZamanlayici = Timer(const Duration(milliseconds: 400), _yukle);
  }
  void _detayaGit(Urun urun) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('${urun.ad} detay ekranı yakında eklenecek.')),
    );
  }
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Ürünler')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            child: TextField(
              controller: _aramaController,
              onChanged: _aramaDegisti,
              textInputAction: TextInputAction.search,
              decoration: const InputDecoration(
                hintText: 'Ürün adı veya kodu ile ara',
                prefixIcon: Icon(Icons.search),
              ),
            ),
          ),
          Expanded(child: _icerik()),
        ],
      ),
    );
  }
  Widget _icerik() {
    if (_yukleniyor) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_hata != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline, size: 48),
              const SizedBox(height: 12),
              Text(_hata!, textAlign: TextAlign.center),
              const SizedBox(height: 12),
              OutlinedButton.icon(
                onPressed: _yukle,
                icon: const Icon(Icons.refresh),
                label: const Text('Tekrar dene'),
              ),
            ],
          ),
        ),
      );
    }
    if (_urunler.isEmpty) {
      return const Center(child: Text('Ürün bulunamadı.'));
    }
    return RefreshIndicator(
      onRefresh: _yukle,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
        itemCount: _urunler.length,
        separatorBuilder: (context, index) => const SizedBox(height: 8),
        itemBuilder: (context, index) => _urunKarti(_urunler[index]),
      ),
    );
  }
  Widget _urunKarti(Urun urun) {
    final kritik = urun.kritikSeviyede;
    final Color renk =
        kritik ? Colors.orange : Theme.of(context).colorScheme.primary;
    return Card(
      child: ListTile(
        onTap: () => _detayaGit(urun),
        leading: CircleAvatar(
          backgroundColor: renk.withValues(alpha: 0.15),
          child: Icon(
            kritik ? Icons.warning_amber_rounded : Icons.inventory_2_outlined,
            color: renk,
          ),
        ),
        title: Text(
          urun.ad,
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Text('${urun.urunKodu} • ${urun.kategoriAdi}'),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Text(
              '${urun.mevcutMiktar}',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w700,
                color: renk,
              ),
            ),
            Text(
              kritik ? 'Kritik' : 'adet',
              style: TextStyle(
                fontSize: 12,
                color: kritik ? renk : Colors.grey.shade600,
              ),
            ),
          ],
        ),
      ),
    );
  }
}