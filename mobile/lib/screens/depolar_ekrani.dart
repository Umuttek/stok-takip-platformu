import 'package:flutter/material.dart';
import '../models/depo.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import '../services/depo_servisi.dart';

class DepolarEkrani extends StatefulWidget {
  const DepolarEkrani({super.key});

  @override
  State<DepolarEkrani> createState() => _DepolarEkraniState();
}

class _DepolarEkraniState extends State<DepolarEkrani> {
  final DepoServisi _servis = DepoServisi();
  List<Depo> _depolar = [];
  bool _yukleniyor = true;
  String? _hata;

  bool get _yoneticiMi => AuthService.instance.aktifOturum?.yoneticiMi ?? false;

  @override
  void initState() {
    super.initState();
    _yukle();
  }

  Future<void> _yukle() async {
    setState(() {
      _yukleniyor = true;
      _hata = null;
    });
    try {
      final depolar = await _servis.listele();
      if (!mounted) return;
      setState(() {
        _depolar = depolar;
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

  Future<void> _depoFormGoster({Depo? mevcut}) async {
    final adCtrl = TextEditingController(text: mevcut?.ad ?? '');
    final konumCtrl = TextEditingController(text: mevcut?.konum ?? '');
    final aciklamaCtrl = TextEditingController(text: mevcut?.aciklama ?? '');
    final formKey = GlobalKey<FormState>();
    bool kaydediliyor = false;
    String? hataMesaji;

    final sonuc = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (ctx, setModalState) {
            Future<void> kaydet() async {
              if (!formKey.currentState!.validate()) return;
              setModalState(() {
                kaydediliyor = true;
                hataMesaji = null;
              });
              try {
                if (mevcut == null) {
                  await _servis.ekle(
                    ad: adCtrl.text.trim(),
                    konum: konumCtrl.text.trim(),
                    aciklama: aciklamaCtrl.text.trim(),
                  );
                } else {
                  await _servis.guncelle(
                    id: mevcut.id,
                    ad: adCtrl.text.trim(),
                    konum: konumCtrl.text.trim(),
                    aciklama: aciklamaCtrl.text.trim(),
                  );
                }
                if (ctx.mounted) Navigator.of(ctx).pop(true);
              } on ApiException catch (e) {
                setModalState(() {
                  kaydediliyor = false;
                  hataMesaji = e.mesaj;
                });
              }
            }

            return Container(
              padding: EdgeInsets.fromLTRB(
                20,
                20,
                20,
                MediaQuery.of(ctx).viewInsets.bottom + 24,
              ),
              decoration: const BoxDecoration(
                color: Color(0xFFF4F5FB),
                borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
              ),
              child: Form(
                key: formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      mevcut == null ? 'Yeni Depo Ekle' : 'Depoyu Düzenle',
                      style: const TextStyle(
                        fontSize: 19,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: adCtrl,
                      decoration: const InputDecoration(
                        labelText: 'Depo Adı *',
                        prefixIcon: Icon(Icons.warehouse_outlined),
                      ),
                      validator: (v) => (v == null || v.trim().length < 2)
                          ? 'Depo adı en az 2 karakter olmalı.'
                          : null,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: konumCtrl,
                      decoration: const InputDecoration(
                        labelText: 'Konum',
                        prefixIcon: Icon(Icons.location_on_outlined),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: aciklamaCtrl,
                      maxLines: 2,
                      decoration: const InputDecoration(
                        labelText: 'Açıklama',
                        prefixIcon: Icon(Icons.notes_rounded),
                      ),
                    ),
                    if (hataMesaji != null) ...[
                      const SizedBox(height: 10),
                      Text(
                        hataMesaji!,
                        style: TextStyle(
                          color: Theme.of(ctx).colorScheme.error,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: kaydediliyor ? null : kaydet,
                      child: kaydediliyor
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : Text(mevcut == null ? 'Depo Ekle' : 'Güncelle'),
                    ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );

    if (sonuc == true && mounted) {
      _yukle();
    }
  }

  Future<void> _depoSil(Depo depo) async {
    final onay = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Depoyu Sil'),
        content:
            Text('"${depo.ad}" deposunu silmek istediğinize emin misiniz?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('İptal'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Sil'),
          ),
        ],
      ),
    );
    if (onay != true) return;

    try {
      await _servis.sil(depo.id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('"${depo.ad}" deposu silindi.')),
      );
      _yukle();
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.mesaj)),
      );
    }
  }

  void _depoDetayGoster(Depo depo) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) {
        return Container(
          constraints: BoxConstraints(
            maxHeight: MediaQuery.of(ctx).size.height * 0.75,
          ),
          padding: const EdgeInsets.fromLTRB(20, 20, 20, 24),
          decoration: const BoxDecoration(
            color: Color(0xFFF4F5FB),
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFF4F46E5).withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(14),
                    ),
                    child: const Icon(
                      Icons.warehouse_rounded,
                      color: Color(0xFF4F46E5),
                      size: 28,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          depo.ad,
                          style: const TextStyle(
                            fontSize: 19,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                        if ((depo.konum ?? '').isNotEmpty)
                          Text(
                            depo.konum!,
                            style: TextStyle(color: Colors.grey.shade600),
                          ),
                      ],
                    ),
                  ),
                  if (_yoneticiMi) ...[
                    IconButton(
                      icon: const Icon(Icons.edit_outlined),
                      onPressed: () {
                        Navigator.of(ctx).pop();
                        _depoFormGoster(mevcut: depo);
                      },
                    ),
                    IconButton(
                      icon: const Icon(Icons.delete_outline, color: Colors.red),
                      onPressed: () {
                        Navigator.of(ctx).pop();
                        _depoSil(depo);
                      },
                    ),
                  ],
                ],
              ),
              if ((depo.aciklama ?? '').isNotEmpty) ...[
                const SizedBox(height: 10),
                Text(
                  depo.aciklama!,
                  style: TextStyle(color: Colors.grey.shade700),
                ),
              ],
              const SizedBox(height: 16),
              Text(
                'Depodaki Zimmetli Ürünler (${depo.urunler.length} çeşit • ${depo.toplamUrunAdeti} adet)',
                style: const TextStyle(
                  fontWeight: FontWeight.w800,
                  fontSize: 15,
                ),
              ),
              const SizedBox(height: 10),
              if (depo.urunler.isEmpty)
                const Card(
                  child: Padding(
                    padding: EdgeInsets.all(16),
                    child: Row(
                      children: [
                        Icon(Icons.info_outline, color: Colors.grey),
                        SizedBox(width: 10),
                        Expanded(
                          child: Text('Bu depoda henüz zimmetli ürün stoğu yok.'),
                        ),
                      ],
                    ),
                  ),
                )
              else
                Flexible(
                  child: ListView.separated(
                    shrinkWrap: true,
                    itemCount: depo.urunler.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (_, index) {
                      final u = depo.urunler[index];
                      return Card(
                        child: ListTile(
                          leading: const Icon(
                            Icons.inventory_2_outlined,
                            color: Color(0xFF4F46E5),
                          ),
                          title: Text(
                            u.urunAdi,
                            style: const TextStyle(fontWeight: FontWeight.w700),
                          ),
                          subtitle: Text('Kod: ${u.urunKodu}'),
                          trailing: Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 12,
                              vertical: 6,
                            ),
                            decoration: BoxDecoration(
                              color: const Color(0xFF0D9488)
                                  .withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(20),
                            ),
                            child: Text(
                              '${u.miktar} adet',
                              style: const TextStyle(
                                color: Color(0xFF0D9488),
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                ),
            ],
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Depolar')),
      floatingActionButton: _yoneticiMi
          ? FloatingActionButton.extended(
              onPressed: () => _depoFormGoster(),
              icon: const Icon(Icons.add),
              label: const Text('Yeni Depo'),
            )
          : null,
      body: _govde(),
    );
  }

  Widget _govde() {
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
    if (_depolar.isEmpty) {
      return const Center(
        child: Text('Henüz kayıtlı depo bulunmuyor.'),
      );
    }
    return RefreshIndicator(
      onRefresh: _yukle,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 88),
        itemCount: _depolar.length,
        separatorBuilder: (_, __) => const SizedBox(height: 10),
        itemBuilder: (_, index) {
          final d = _depolar[index];
          return Card(
            child: ListTile(
              onTap: () => _depoDetayGoster(d),
              leading: CircleAvatar(
                backgroundColor:
                    const Color(0xFF4F46E5).withValues(alpha: 0.12),
                child: const Icon(
                  Icons.warehouse_rounded,
                  color: Color(0xFF4F46E5),
                ),
              ),
              title: Text(
                d.ad,
                style: const TextStyle(fontWeight: FontWeight.w700),
              ),
              subtitle: Text(
                [
                  if ((d.konum ?? '').isNotEmpty) d.konum!,
                  '${d.urunCesidiSayisi} çeşit ürün',
                ].join(' • '),
              ),
              trailing: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    '${d.toplamUrunAdeti}',
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w800,
                      color: Color(0xFF0D9488),
                    ),
                  ),
                  Text(
                    'toplam adet',
                    style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

