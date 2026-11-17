import 'package:flutter/material.dart';
import '../models/depo.dart';
import '../models/stok_hareketi.dart';
import '../models/urun.dart';
import '../services/api_service.dart';
import '../services/depo_servisi.dart';
import '../services/stok_hareket_servisi.dart';
import '../services/urun_servisi.dart';

class StokHareketEkrani extends StatefulWidget {
  final int? baslangicUrunId;
  final int? baslangicDepoId;
  final String baslangicTuru;

  const StokHareketEkrani({
    super.key,
    this.baslangicUrunId,
    this.baslangicDepoId,
    this.baslangicTuru = 'Giris',
  });

  @override
  State<StokHareketEkrani> createState() => _StokHareketEkraniState();
}

class _StokHareketEkraniState extends State<StokHareketEkrani> {
  final _formKey = GlobalKey<FormState>();
  final _hareketServisi = StokHareketServisi();
  final _urunServisi = UrunServisi();
  final _depoServisi = DepoServisi();
  final _miktarCtrl = TextEditingController(text: '1');

  List<Urun> _urunler = [];
  List<Depo> _depolar = [];
  List<StokHareketi> _hareketler = [];

  late String _hareketTuru;
  int? _seciliUrunId;
  int? _seciliDepoId;

  bool _yukleniyor = true;
  bool _kaydediliyor = false;
  String? _hata;

  @override
  void initState() {
    super.initState();
    _hareketTuru = widget.baslangicTuru;
    _seciliUrunId = widget.baslangicUrunId;
    _seciliDepoId = widget.baslangicDepoId;
    _verileriYukle();
  }

  @override
  void dispose() {
    _miktarCtrl.dispose();
    super.dispose();
  }

  Future<void> _verileriYukle() async {
    setState(() {
      _yukleniyor = true;
      _hata = null;
    });
    try {
      final sonuclar = await Future.wait([
        _urunServisi.listele(),
        _depoServisi.listele(),
        _hareketServisi.listele(urunId: widget.baslangicUrunId),
      ]);
      if (!mounted) return;
      final urunler = sonuclar[0] as List<Urun>;
      final depolar = sonuclar[1] as List<Depo>;
      final hareketler = sonuclar[2] as List<StokHareketi>;

      setState(() {
        _urunler = urunler;
        _depolar = depolar;
        _hareketler = hareketler;

        if (_seciliUrunId == null && urunler.isNotEmpty) {
          _seciliUrunId = urunler.first.id;
        }
        if (_seciliDepoId == null && depolar.isNotEmpty) {
          _seciliDepoId = depolar.first.id;
        }
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

  int get _seciliDepodakiMevcutStok {
    if (_seciliUrunId == null || _seciliDepoId == null) return 0;
    final urun = _urunler.where((u) => u.id == _seciliUrunId).firstOrNull;
    if (urun == null) return 0;
    final depoStok = urun.depoStoklari
        .where((d) => d.depoId == _seciliDepoId)
        .firstOrNull;
    return depoStok?.miktar ?? 0;
  }

  Future<void> _islemiKaydet() async {
    if (_kaydediliyor) return;
    if (!_formKey.currentState!.validate()) return;
    if (_seciliDepoId == null || _seciliUrunId == null) {
      setState(() => _hata = 'Lütfen depo ve ürün seçin.');
      return;
    }

    final miktar = int.parse(_miktarCtrl.text.trim());
    setState(() {
      _kaydediliyor = true;
      _hata = null;
    });

    try {
      await _hareketServisi.ekle(
        urunId: _seciliUrunId!,
        depoId: _seciliDepoId!,
        hareketTuru: _hareketTuru,
        miktar: miktar,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            _hareketTuru == 'Giris'
                ? '$miktar adet stok seçilen depoya eklendi.'
                : '$miktar adet stok seçilen depodan düşüldü.',
          ),
        ),
      );
      setState(() => _kaydediliyor = false);
      await _verileriYukle();
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _kaydediliyor = false;
        _hata = e.mesaj;
      });
    }
  }

  String _tarihFormatla(DateTime t) {
    final g = t.day.toString().padLeft(2, '0');
    final a = t.month.toString().padLeft(2, '0');
    final s = t.hour.toString().padLeft(2, '0');
    final d = t.minute.toString().padLeft(2, '0');
    return '$g.$a.${t.year} $s:$d';
  }

  @override
  Widget build(BuildContext context) {
    final girisMi = _hareketTuru == 'Giris';
    final vurguRenk =
        girisMi ? const Color(0xFF0D9488) : const Color(0xFFE11D48);

    return Scaffold(
      appBar: AppBar(title: const Text('Stok Giriş / Çıkış')),
      body: _yukleniyor
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _verileriYukle,
              child: ListView(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 28),
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(18),
                      child: Form(
                        key: _formKey,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            SegmentedButton<String>(
                              segments: const [
                                ButtonSegment(
                                  value: 'Giris',
                                  icon: Icon(Icons.add_circle_outline),
                                  label: Text('Stok Girişi (+)'),
                                ),
                                ButtonSegment(
                                  value: 'Cikis',
                                  icon: Icon(Icons.remove_circle_outline),
                                  label: Text('Stok Çıkışı / Satış (-)'),
                                ),
                              ],
                              selected: {_hareketTuru},
                              onSelectionChanged: (secim) {
                                setState(() {
                                  _hareketTuru = secim.first;
                                  _hata = null;
                                });
                              },
                            ),
                            const SizedBox(height: 16),
                            DropdownButtonFormField<int>(
                              value: _depolar
                                      .any((d) => d.id == _seciliDepoId)
                                  ? _seciliDepoId
                                  : null,
                              decoration: const InputDecoration(
                                labelText: 'İşlem Yapılacak Depo *',
                                prefixIcon: Icon(Icons.warehouse_outlined),
                              ),
                              items: _depolar
                                  .map(
                                    (d) => DropdownMenuItem<int>(
                                      value: d.id,
                                      child: Text(d.ad),
                                    ),
                                  )
                                  .toList(),
                              onChanged: (v) =>
                                  setState(() => _seciliDepoId = v),
                            ),
                            const SizedBox(height: 14),
                            DropdownButtonFormField<int>(
                              value: _urunler
                                      .any((u) => u.id == _seciliUrunId)
                                  ? _seciliUrunId
                                  : null,
                              isExpanded: true,
                              decoration: const InputDecoration(
                                labelText: 'Ürün *',
                                prefixIcon: Icon(Icons.inventory_2_outlined),
                              ),
                              items: _urunler
                                  .map(
                                    (u) => DropdownMenuItem<int>(
                                      value: u.id,
                                      child: Text(
                                        '${u.ad} (${u.urunKodu})',
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ),
                                  )
                                  .toList(),
                              onChanged: (v) =>
                                  setState(() => _seciliUrunId = v),
                            ),
                            if (_seciliDepoId != null &&
                                _seciliUrunId != null) ...[
                              const SizedBox(height: 10),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 12, vertical: 10),
                                decoration: BoxDecoration(
                                  color: vurguRenk.withValues(alpha: 0.1),
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Row(
                                  children: [
                                    Icon(Icons.info_outline,
                                        size: 18, color: vurguRenk),
                                    const SizedBox(width: 8),
                                    Expanded(
                                      child: Text(
                                        'Seçili depodaki mevcut stok: $_seciliDepodakiMevcutStok adet',
                                        style: TextStyle(
                                          color: vurguRenk,
                                          fontWeight: FontWeight.w700,
                                          fontSize: 13,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ],
                            const SizedBox(height: 14),
                            TextFormField(
                              controller: _miktarCtrl,
                              keyboardType: TextInputType.number,
                              decoration: InputDecoration(
                                labelText: girisMi
                                    ? 'Depoya Eklenecek Miktar *'
                                    : 'Depodan Çıkarılacak Miktar *',
                                prefixIcon: const Icon(Icons.numbers_rounded),
                              ),
                              validator: (v) {
                                final sayi = int.tryParse(v?.trim() ?? '');
                                if (sayi == null || sayi < 1) {
                                  return 'En az 1 adet girmelisiniz.';
                                }
                                if (!girisMi &&
                                    sayi > _seciliDepodakiMevcutStok) {
                                  return 'Bu depoda yalnızca $_seciliDepodakiMevcutStok adet stok var.';
                                }
                                return null;
                              },
                            ),
                            if (_hata != null) ...[
                              const SizedBox(height: 12),
                              Text(
                                _hata!,
                                style: TextStyle(
                                  color: Theme.of(context).colorScheme.error,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ],
                            const SizedBox(height: 16),
                            ElevatedButton.icon(
                              style: ElevatedButton.styleFrom(
                                backgroundColor: vurguRenk,
                              ),
                              onPressed: _kaydediliyor ? null : _islemiKaydet,
                              icon: Icon(
                                girisMi
                                    ? Icons.download_rounded
                                    : Icons.upload_rounded,
                              ),
                              label: Text(
                                girisMi
                                    ? 'Depoya Stok Girişi Yap'
                                    : 'Depodan Stok Çıkışı Yap',
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 22),
                  const Padding(
                    padding: EdgeInsets.only(left: 4, bottom: 10),
                    child: Text(
                      'Son Stok Hareketleri',
                      style:
                          TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
                    ),
                  ),
                  if (_hareketler.isEmpty)
                    const Card(
                      child: Padding(
                        padding: EdgeInsets.all(16),
                        child: Text('Henüz stok hareketi kaydı bulunmuyor.'),
                      ),
                    )
                  else
                    for (final h in _hareketler) ...[
                      Card(
                        child: ListTile(
                          leading: CircleAvatar(
                            backgroundColor: (h.girisMi
                                    ? const Color(0xFF0D9488)
                                    : const Color(0xFFE11D48))
                                .withValues(alpha: 0.12),
                            child: Icon(
                              h.girisMi
                                  ? Icons.south_west_rounded
                                  : Icons.north_east_rounded,
                              color: h.girisMi
                                  ? const Color(0xFF0D9488)
                                  : const Color(0xFFE11D48),
                            ),
                          ),
                          title: Text(
                            h.urunAdi,
                            style: const TextStyle(fontWeight: FontWeight.w700),
                          ),
                          subtitle: Text(
                            '${h.depoAdi} • ${_tarihFormatla(h.tarih)}'
                            '${h.kullaniciAdSoyad.isNotEmpty ? ' • ${h.kullaniciAdSoyad}' : ''}',
                          ),
                          trailing: Text(
                            '${h.girisMi ? '+' : '-'}${h.miktar}',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.w800,
                              color: h.girisMi
                                  ? const Color(0xFF0D9488)
                                  : const Color(0xFFE11D48),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 8),
                    ],
                ],
              ),
            ),
    );
  }
}

