import 'dart:convert';
import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../models/depo.dart';
import '../models/kategori.dart';
import '../models/urun.dart';
import '../services/api_service.dart';
import '../services/depo_servisi.dart';
import '../services/kategori_servisi.dart';
import '../services/urun_servisi.dart';

class UrunFormEkrani extends StatefulWidget {
  final Urun? duzenlenecekUrun;

  const UrunFormEkrani({super.key, this.duzenlenecekUrun});

  @override
  State<UrunFormEkrani> createState() => _UrunFormEkraniState();
}

class _UrunFormEkraniState extends State<UrunFormEkrani> {
  final _formKey = GlobalKey<FormState>();
  final _urunServisi = UrunServisi();
  final _kategoriServisi = KategoriServisi();
  final _depoServisi = DepoServisi();
  final _imagePicker = ImagePicker();

  late final TextEditingController _adCtrl;
  late final TextEditingController _aciklamaCtrl;
  late final TextEditingController _kritikCtrl;
  late final TextEditingController _miktarCtrl;

  List<Kategori> _kategoriler = [];
  List<Depo> _depolar = [];
  int? _seciliKategoriId;
  int? _seciliDepoId;
  String? _atanacakKod;

  Uint8List? _fotografBytes;
  String? _yeniFotografBase64;
  bool _fotografiKaldir = false;

  bool _veriYukleniyor = true;
  bool _kaydediliyor = false;
  String? _hata;

  bool get _duzenlemeModu => widget.duzenlenecekUrun != null;

  @override
  void initState() {
    super.initState();
    final u = widget.duzenlenecekUrun;
    _adCtrl = TextEditingController(text: u?.ad ?? '');
    _aciklamaCtrl = TextEditingController(text: u?.aciklama ?? '');
    _kritikCtrl =
        TextEditingController(text: '${u?.kritikStokSeviyesi ?? 5}');
    _miktarCtrl = TextEditingController(text: '${u?.mevcutMiktar ?? 10}');
    _seciliKategoriId = u?.kategoriId;
    _atanacakKod = u?.urunKodu;

    if (u != null && u.fotografiVar) {
      try {
        _fotografBytes = base64Decode(u.fotografBase64!);
      } catch (_) {}
    }

    _listeleriYukle();
  }

  @override
  void dispose() {
    _adCtrl.dispose();
    _aciklamaCtrl.dispose();
    _kritikCtrl.dispose();
    _miktarCtrl.dispose();
    super.dispose();
  }

  Future<void> _listeleriYukle() async {
    setState(() {
      _veriYukleniyor = true;
      _hata = null;
    });
    try {
      final istekler = <Future<dynamic>>[
        _kategoriServisi.listele(),
        _depoServisi.listele(),
        if (!_duzenlemeModu) _urunServisi.sonrakiKoduGetir(),
      ];
      final sonuclar = await Future.wait(istekler);
      if (!mounted) return;

      final kategoriler = sonuclar[0] as List<Kategori>;
      final depolar = sonuclar[1] as List<Depo>;
      final sonrakiKod = !_duzenlemeModu ? sonuclar[2] as String : _atanacakKod;

      setState(() {
        _kategoriler = kategoriler;
        _depolar = depolar;
        _atanacakKod = sonrakiKod;
        if (_seciliKategoriId == null && kategoriler.isNotEmpty) {
          _seciliKategoriId = kategoriler.first.id;
        }
        if (_seciliDepoId == null && depolar.isNotEmpty) {
          _seciliDepoId = depolar.first.id;
        }
        _veriYukleniyor = false;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _hata = e.mesaj;
        _veriYukleniyor = false;
      });
    }
  }

  Future<void> _fotografSec(ImageSource kaynak) async {
    try {
      final dosya = await _imagePicker.pickImage(
        source: kaynak,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 75,
      );
      if (dosya == null) return;
      final bytes = await dosya.readAsBytes();
      if (!mounted) return;
      setState(() {
        _fotografBytes = bytes;
        _yeniFotografBase64 = base64Encode(bytes);
        _fotografiKaldir = false;
      });
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Fotoğraf seçilemedi: $e')),
      );
    }
  }

  void _fotografKaynakSecimiGoster() {
    showModalBottomSheet(
      context: context,
      builder: (ctx) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.photo_library_outlined),
              title: const Text('Galeriden Seç'),
              onTap: () {
                Navigator.of(ctx).pop();
                _fotografSec(ImageSource.gallery);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_camera_outlined),
              title: const Text('Kamera ile Çek'),
              onTap: () {
                Navigator.of(ctx).pop();
                _fotografSec(ImageSource.camera);
              },
            ),
            if (_fotografBytes != null)
              ListTile(
                leading: const Icon(Icons.delete_outline, color: Colors.red),
                title: const Text('Fotoğrafı Kaldır',
                    style: TextStyle(color: Colors.red)),
                onTap: () {
                  Navigator.of(ctx).pop();
                  setState(() {
                    _fotografBytes = null;
                    _yeniFotografBase64 = null;
                    _fotografiKaldir = true;
                  });
                },
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _hizliKategoriEkle() async {
    final adCtrl = TextEditingController();
    final eklenen = await showDialog<Kategori>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Yeni Kategori Ekle'),
        content: TextField(
          controller: adCtrl,
          autofocus: true,
          decoration: const InputDecoration(labelText: 'Kategori Adı'),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('İptal'),
          ),
          FilledButton(
            onPressed: () async {
              final ad = adCtrl.text.trim();
              if (ad.length < 2) return;
              try {
                final k = await _kategoriServisi.ekle(ad: ad);
                if (ctx.mounted) Navigator.of(ctx).pop(k);
              } on ApiException catch (e) {
                if (!ctx.mounted) return;
                ScaffoldMessenger.of(ctx)
                    .showSnackBar(SnackBar(content: Text(e.mesaj)));
              }
            },
            child: const Text('Ekle'),
          ),
        ],
      ),
    );
    if (eklenen != null && mounted) {
      await _listeleriYukle();
      setState(() => _seciliKategoriId = eklenen.id);
    }
  }

  Future<void> _hizliDepoEkle() async {
    final adCtrl = TextEditingController();
    final konumCtrl = TextEditingController();
    final eklenen = await showDialog<Depo>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Yeni Depo Ekle'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: adCtrl,
              autofocus: true,
              decoration: const InputDecoration(labelText: 'Depo Adı *'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: konumCtrl,
              decoration: const InputDecoration(labelText: 'Konum'),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('İptal'),
          ),
          FilledButton(
            onPressed: () async {
              final ad = adCtrl.text.trim();
              if (ad.length < 2) return;
              try {
                final d = await _depoServisi.ekle(
                  ad: ad,
                  konum: konumCtrl.text.trim(),
                );
                if (ctx.mounted) Navigator.of(ctx).pop(d);
              } on ApiException catch (e) {
                if (!ctx.mounted) return;
                ScaffoldMessenger.of(ctx)
                    .showSnackBar(SnackBar(content: Text(e.mesaj)));
              }
            },
            child: const Text('Ekle'),
          ),
        ],
      ),
    );
    if (eklenen != null && mounted) {
      await _listeleriYukle();
      setState(() => _seciliDepoId = eklenen.id);
    }
  }

  Future<void> _kaydet() async {
    if (_kaydediliyor) return;
    if (!_formKey.currentState!.validate()) return;
    if (_seciliKategoriId == null) {
      setState(() => _hata = 'Lütfen bir kategori seçin.');
      return;
    }
    if (!_duzenlemeModu && _seciliDepoId == null) {
      setState(() => _hata = 'Lütfen ürünün zimmetleneceği depoyu seçin.');
      return;
    }

    setState(() {
      _kaydediliyor = true;
      _hata = null;
    });

    try {
      final kritik = int.parse(_kritikCtrl.text.trim());
      if (_duzenlemeModu) {
        final guncellenen = await _urunServisi.guncelle(
          id: widget.duzenlenecekUrun!.id,
          ad: _adCtrl.text.trim(),
          aciklama: _aciklamaCtrl.text.trim(),
          kategoriId: _seciliKategoriId!,
          kritikStokSeviyesi: kritik,
          fotografBase64: _yeniFotografBase64,
          fotografiKaldir: _fotografiKaldir,
        );
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('"${guncellenen.ad}" güncellendi.')),
        );
        Navigator.of(context).pop(guncellenen);
      } else {
        final miktar = int.parse(_miktarCtrl.text.trim());
        final eklenen = await _urunServisi.ekle(
          ad: _adCtrl.text.trim(),
          aciklama: _aciklamaCtrl.text.trim(),
          kategoriId: _seciliKategoriId!,
          kritikStokSeviyesi: kritik,
          mevcutMiktar: miktar,
          depoId: _seciliDepoId!,
          fotografBase64: _yeniFotografBase64,
        );
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
                '"${eklenen.ad}" (${eklenen.urunKodu}) eklendi ve depoya zimmetlendi.'),
          ),
        );
        Navigator.of(context).pop(eklenen);
      }
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _kaydediliyor = false;
        _hata = e.mesaj;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final anaRenk = Theme.of(context).colorScheme.primary;

    return Scaffold(
      appBar: AppBar(
        title: Text(_duzenlemeModu ? 'Ürün Düzenle' : 'Yeni Ürün Ekle'),
      ),
      body: _veriYukleniyor
          ? const Center(child: CircularProgressIndicator())
          : SafeArea(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      // Otomatik barkod / ürün kodu bilgi kartı
                      Card(
                        child: ListTile(
                          leading: Container(
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(
                              color: anaRenk.withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: Icon(Icons.qr_code_2_rounded, color: anaRenk),
                          ),
                          title: Text(
                            _duzenlemeModu
                                ? 'Ürün Barkod Kodu'
                                : 'Otomatik Atanacak Barkod Kodu',
                            style: TextStyle(
                              fontSize: 12,
                              color: Colors.grey.shade600,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          subtitle: Padding(
                            padding: const EdgeInsets.only(top: 2),
                            child: Text(
                              _atanacakKod ?? 'Otomatik üretilecek',
                              style: const TextStyle(
                                fontSize: 17,
                                fontWeight: FontWeight.w800,
                                letterSpacing: 1.1,
                                color: Colors.black87,
                              ),
                            ),
                          ),
                          trailing: Container(
                            padding: const EdgeInsets.symmetric(
                                horizontal: 10, vertical: 4),
                            decoration: BoxDecoration(
                              color: Colors.teal.withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: const Text(
                              'EAN-13',
                              style: TextStyle(
                                color: Colors.teal,
                                fontWeight: FontWeight.w700,
                                fontSize: 11,
                              ),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),

                      // İsteğe bağlı ürün fotoğrafı alanı
                      GestureDetector(
                        onTap: _fotografKaynakSecimiGoster,
                        child: Card(
                          clipBehavior: Clip.antiAlias,
                          child: _fotografBytes != null
                              ? Stack(
                                  children: [
                                    Image.memory(
                                      _fotografBytes!,
                                      height: 190,
                                      width: double.infinity,
                                      fit: BoxFit.cover,
                                    ),
                                    Positioned(
                                      top: 10,
                                      right: 10,
                                      child: FilledButton.tonalIcon(
                                        onPressed:
                                            _fotografKaynakSecimiGoster,
                                        icon: const Icon(Icons.edit, size: 18),
                                        label: const Text('Değiştir'),
                                      ),
                                    ),
                                  ],
                                )
                              : Padding(
                                  padding: const EdgeInsets.symmetric(
                                      vertical: 22, horizontal: 16),
                                  child: Column(
                                    children: [
                                      Icon(
                                        Icons.add_a_photo_outlined,
                                        size: 36,
                                        color: anaRenk,
                                      ),
                                      const SizedBox(height: 8),
                                      const Text(
                                        'Ürün Fotoğrafı Ekle (İsteğe Bağlı)',
                                        style: TextStyle(
                                            fontWeight: FontWeight.w700),
                                      ),
                                      const SizedBox(height: 2),
                                      Text(
                                        'Galeri veya kameradan fotoğraf yükleyebilirsiniz',
                                        style: TextStyle(
                                          fontSize: 12,
                                          color: Colors.grey.shade600,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                        ),
                      ),
                      const SizedBox(height: 16),

                      TextFormField(
                        controller: _adCtrl,
                        textCapitalization: TextCapitalization.words,
                        decoration: const InputDecoration(
                          labelText: 'Ürün Adı *',
                          prefixIcon: Icon(Icons.inventory_2_outlined),
                        ),
                        validator: (v) => (v == null || v.trim().length < 2)
                            ? 'Ürün adı en az 2 karakter olmalı.'
                            : null,
                      ),
                      const SizedBox(height: 14),
                      Row(
                        children: [
                          Expanded(
                            child: DropdownButtonFormField<int>(
                              value: _kategoriler
                                      .any((k) => k.id == _seciliKategoriId)
                                  ? _seciliKategoriId
                                  : null,
                              decoration: const InputDecoration(
                                labelText: 'Kategori *',
                                prefixIcon: Icon(Icons.category_outlined),
                              ),
                              items: _kategoriler
                                  .map(
                                    (k) => DropdownMenuItem<int>(
                                      value: k.id,
                                      child: Text(k.ad),
                                    ),
                                  )
                                  .toList(),
                              onChanged: (v) =>
                                  setState(() => _seciliKategoriId = v),
                            ),
                          ),
                          const SizedBox(width: 8),
                          IconButton.filledTonal(
                            onPressed: _hizliKategoriEkle,
                            icon: const Icon(Icons.add),
                            tooltip: 'Yeni Kategori Ekle',
                          ),
                        ],
                      ),
                      const SizedBox(height: 14),
                      TextFormField(
                        controller: _kritikCtrl,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Kritik Stok Seviyesi *',
                          helperText:
                              'Stok bu miktara veya altına düştüğünde kritik uyarı verir.',
                          prefixIcon:
                              Icon(Icons.notification_important_outlined),
                        ),
                        validator: (v) {
                          final sayi = int.tryParse(v?.trim() ?? '');
                          if (sayi == null || sayi < 0) {
                            return '0 veya daha büyük bir sayı girin.';
                          }
                          return null;
                        },
                      ),
                      if (!_duzenlemeModu) ...[
                        const SizedBox(height: 20),
                        const Text(
                          'Depo Zimmet ve Başlangıç Stoğu',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                        const SizedBox(height: 10),
                        Row(
                          children: [
                            Expanded(
                              child: DropdownButtonFormField<int>(
                                value: _depolar
                                        .any((d) => d.id == _seciliDepoId)
                                    ? _seciliDepoId
                                    : null,
                                decoration: const InputDecoration(
                                  labelText: 'Zimmetlenecek Depo *',
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
                            ),
                            const SizedBox(width: 8),
                            IconButton.filledTonal(
                              onPressed: _hizliDepoEkle,
                              icon: const Icon(Icons.add),
                              tooltip: 'Yeni Depo Ekle',
                            ),
                          ],
                        ),
                        const SizedBox(height: 14),
                        TextFormField(
                          controller: _miktarCtrl,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'Eklenecek Stok Miktarı (Adet) *',
                            helperText:
                                'Girilen miktar doğrudan seçilen depoya zimmetlenir.',
                            prefixIcon: Icon(Icons.numbers_rounded),
                          ),
                          validator: (v) {
                            final sayi = int.tryParse(v?.trim() ?? '');
                            if (sayi == null || sayi < 0) {
                              return '0 veya daha büyük bir adet girin.';
                            }
                            return null;
                          },
                        ),
                      ],
                      const SizedBox(height: 14),
                      TextFormField(
                        controller: _aciklamaCtrl,
                        maxLines: 3,
                        decoration: const InputDecoration(
                          labelText: 'Açıklama',
                          prefixIcon: Icon(Icons.description_outlined),
                        ),
                      ),
                      if (_hata != null) ...[
                        const SizedBox(height: 14),
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color:
                                Theme.of(context).colorScheme.errorContainer,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: Text(
                            _hata!,
                            style: TextStyle(
                              color: Theme.of(context)
                                  .colorScheme
                                  .onErrorContainer,
                            ),
                          ),
                        ),
                      ],
                      const SizedBox(height: 24),
                      ElevatedButton(
                        onPressed: _kaydediliyor ? null : _kaydet,
                        child: _kaydediliyor
                            ? const SizedBox(
                                width: 22,
                                height: 22,
                                child:
                                    CircularProgressIndicator(strokeWidth: 2),
                              )
                            : Text(
                                _duzenlemeModu
                                    ? 'Değişiklikleri Kaydet'
                                    : 'Ürünü Ekle ve Depoya Zimmetle',
                              ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
    );
  }
}
