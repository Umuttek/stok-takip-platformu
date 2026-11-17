import 'dart:convert';
import 'dart:typed_data';
import 'package:flutter/material.dart';
import '../models/urun.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import '../services/urun_servisi.dart';
import 'stok_hareket_ekrani.dart';
import 'urun_form_ekrani.dart';

class UrunDetayEkrani extends StatefulWidget {
  final Urun urun;

  const UrunDetayEkrani({super.key, required this.urun});

  @override
  State<UrunDetayEkrani> createState() => _UrunDetayEkraniState();
}

class _UrunDetayEkraniState extends State<UrunDetayEkrani> {
  final UrunServisi _servis = UrunServisi();
  late Urun _urun;
  bool _yukleniyor = false;

  bool get _yoneticiMi => AuthService.instance.aktifOturum?.yoneticiMi ?? false;

  @override
  void initState() {
    super.initState();
    _urun = widget.urun;
    _guncelVeriyiGetir();
  }

  Future<void> _guncelVeriyiGetir() async {
    setState(() => _yukleniyor = true);
    try {
      final guncel = await _servis.getir(_urun.id);
      if (!mounted) return;
      setState(() {
        _urun = guncel;
        _yukleniyor = false;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _yukleniyor = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.mesaj)),
      );
    }
  }

  Future<void> _duzenleAc() async {
    final guncel = await Navigator.of(context).push<Urun>(
      MaterialPageRoute(
        builder: (_) => UrunFormEkrani(duzenlenecekUrun: _urun),
      ),
    );
    if (guncel != null && mounted) {
      _guncelVeriyiGetir();
    }
  }

  Future<void> _stokHareketAc(String tur) async {
    await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => StokHareketEkrani(
          baslangicUrunId: _urun.id,
          baslangicDepoId: _urun.depoStoklari.isNotEmpty
              ? _urun.depoStoklari.first.depoId
              : null,
          baslangicTuru: tur,
        ),
      ),
    );
    if (mounted) _guncelVeriyiGetir();
  }

  Uint8List? _fotografCoz() {
    if (!_urun.fotografiVar) return null;
    try {
      return base64Decode(_urun.fotografBase64!);
    } catch (_) {
      return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    final kritik = _urun.kritikSeviyede;
    final durumRenk =
        kritik ? const Color(0xFFD97706) : const Color(0xFF16A34A);
    final anaRenk = Theme.of(context).colorScheme.primary;
    final aciklama = (_urun.aciklama ?? '').trim();
    final fotoBytes = _fotografCoz();

    // Stok sayısına göre azalan sıralı liste (0 stoklu depolar da dahil)
    final siraliDepolar = [..._urun.depoStoklari]
      ..sort((a, b) {
        final cmp = b.miktar.compareTo(a.miktar);
        return cmp != 0 ? cmp : a.depoAdi.compareTo(b.depoAdi);
      });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Ürün Detayı'),
        actions: [
          if (_yoneticiMi)
            IconButton(
              onPressed: _duzenleAc,
              icon: const Icon(Icons.edit_outlined),
              tooltip: 'Ürünü Düzenle',
            ),
          IconButton(
            onPressed: _yukleniyor ? null : _guncelVeriyiGetir,
            icon: _yukleniyor
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.refresh_rounded),
            tooltip: 'Yenile',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _guncelVeriyiGetir,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 28),
          children: [
            if (fotoBytes != null) ...[
              Card(
                clipBehavior: Clip.antiAlias,
                child: Image.memory(
                  fotoBytes,
                  height: 220,
                  width: double.infinity,
                  fit: BoxFit.cover,
                ),
              ),
              const SizedBox(height: 14),
            ],
            _ustOzetKarti(anaRenk, durumRenk, kritik),
            const SizedBox(height: 16),
            if (kritik) ...[
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: durumRenk.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(16),
                  border:
                      Border.all(color: durumRenk.withValues(alpha: 0.35)),
                ),
                child: Row(
                  children: [
                    Icon(Icons.warning_amber_rounded, color: durumRenk),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        'Bu ürünün mevcut stoğu kritik seviyenin (${_urun.kritikStokSeviyesi}) altında veya bu seviyeye eşit.',
                        style: TextStyle(
                          color: Colors.orange.shade900,
                          fontWeight: FontWeight.w600,
                          fontSize: 13,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
            ],
            Row(
              children: [
                Expanded(
                  child: _stokSayacKarti(
                    baslik: 'Mevcut Miktar',
                    deger: '${_urun.mevcutMiktar}',
                    altYazi: 'Toplam stok adeti',
                    ikon: Icons.inventory_2_rounded,
                    renk: kritik ? durumRenk : anaRenk,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: _stokSayacKarti(
                    baslik: 'Kritik Stok Seviyesi',
                    deger: '${_urun.kritikStokSeviyesi}',
                    altYazi: 'Uyarı eşiği',
                    ikon: Icons.notification_important_outlined,
                    renk: const Color(0xFF6366F1),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _stokHareketAc('Giris'),
                    icon: const Icon(Icons.add_circle_outline,
                        color: Color(0xFF0D9488)),
                    label: const Text('Stok Girişi'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _stokHareketAc('Cikis'),
                    icon: const Icon(Icons.remove_circle_outline,
                        color: Color(0xFFE11D48)),
                    label: const Text('Stok Çıkışı'),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 20),
            const Padding(
              padding: EdgeInsets.only(left: 4, bottom: 10),
              child: Text(
                'Depo Zimmet Dağılımı',
                style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
              ),
            ),
            if (siraliDepolar.isEmpty)
              const Card(
                child: Padding(
                  padding: EdgeInsets.all(16),
                  child: Text('Sistemde kayıtlı depo bulunmuyor.'),
                ),
              )
            else
              Card(
                child: Column(
                  children: [
                    for (var i = 0; i < siraliDepolar.length; i++) ...[
                      if (i > 0)
                        const Divider(height: 1, indent: 56, endIndent: 16),
                      _depoSatiri(siraliDepolar[i]),
                    ],
                  ],
                ),
              ),
            const SizedBox(height: 20),
            const Padding(
              padding: EdgeInsets.only(left: 4, bottom: 10),
              child: Text(
                'Ürün Bilgileri',
                style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
              ),
            ),
            Card(
              child: Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Column(
                  children: [
                    _bilgiSatiri(
                      ikon: Icons.qr_code_2_rounded,
                      etiket: 'Ürün Barkod Kodu',
                      deger: _urun.urunKodu,
                    ),
                    const Divider(height: 1, indent: 56, endIndent: 16),
                    _bilgiSatiri(
                      ikon: Icons.label_outline_rounded,
                      etiket: 'Ürün Adı',
                      deger: _urun.ad,
                    ),
                    const Divider(height: 1, indent: 56, endIndent: 16),
                    _bilgiSatiri(
                      ikon: Icons.category_outlined,
                      etiket: 'Kategori',
                      deger: _urun.kategoriAdi.isEmpty
                          ? 'Belirtilmemiş'
                          : _urun.kategoriAdi,
                    ),
                    const Divider(height: 1, indent: 56, endIndent: 16),
                    _bilgiSatiri(
                      ikon: Icons.description_outlined,
                      etiket: 'Açıklama',
                      deger: aciklama.isEmpty
                          ? 'Açıklama bulunmuyor.'
                          : aciklama,
                      solukDeger: aciklama.isEmpty,
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _depoSatiri(UrunDepoStok depo) {
    final stokVar = depo.miktar > 0;
    final renk = stokVar ? const Color(0xFF0D9488) : Colors.grey;
    return ListTile(
      leading: Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: renk.withValues(alpha: 0.12),
          borderRadius: BorderRadius.circular(10),
        ),
        child: Icon(
          Icons.warehouse_rounded,
          color: renk,
          size: 20,
        ),
      ),
      title: Text(
        depo.depoAdi,
        style: TextStyle(
          fontWeight: FontWeight.w700,
          color: stokVar ? Colors.black87 : Colors.grey.shade600,
        ),
      ),
      subtitle: (depo.depoKonum ?? '').isNotEmpty
          ? Text(depo.depoKonum!)
          : null,
      trailing: Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
        decoration: BoxDecoration(
          color: renk.withValues(alpha: 0.12),
          borderRadius: BorderRadius.circular(16),
        ),
        child: Text(
          '${depo.miktar} adet',
          style: TextStyle(
            fontSize: 14,
            fontWeight: FontWeight.w800,
            color: renk,
          ),
        ),
      ),
    );
  }

  Widget _ustOzetKarti(Color anaRenk, Color durumRenk, bool kritik) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: anaRenk.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Icon(
                    Icons.inventory_2_rounded,
                    size: 32,
                    color: anaRenk,
                  ),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        _urun.ad,
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Wrap(
                        spacing: 8,
                        runSpacing: 6,
                        children: [
                          _rozet(
                            metin: _urun.urunKodu,
                            renk: anaRenk,
                            ikon: Icons.tag_rounded,
                          ),
                          if (_urun.kategoriAdi.isNotEmpty)
                            _rozet(
                              metin: _urun.kategoriAdi,
                              renk: Colors.teal,
                              ikon: Icons.folder_outlined,
                            ),
                          _rozet(
                            metin: kritik ? 'Kritik Stok' : 'Stok Yeterli',
                            renk: durumRenk,
                            ikon: kritik
                                ? Icons.warning_amber_rounded
                                : Icons.check_circle_outline_rounded,
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _rozet({
    required String metin,
    required Color renk,
    required IconData ikon,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: renk.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(ikon, size: 14, color: renk),
          const SizedBox(width: 4),
          Text(
            metin,
            style: TextStyle(
              color: renk,
              fontSize: 12,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    );
  }

  Widget _stokSayacKarti({
    required String baslik,
    required String deger,
    required String altYazi,
    required IconData ikon,
    required Color renk,
  }) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: renk.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(ikon, color: renk, size: 22),
            ),
            const SizedBox(height: 12),
            Text(
              deger,
              style: TextStyle(
                fontSize: 28,
                fontWeight: FontWeight.w800,
                color: renk,
              ),
            ),
            const SizedBox(height: 2),
            Text(
              baslik,
              style: const TextStyle(
                fontWeight: FontWeight.w700,
                fontSize: 13,
              ),
            ),
            Text(
              altYazi,
              style: TextStyle(
                color: Colors.grey.shade600,
                fontSize: 12,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _bilgiSatiri({
    required IconData ikon,
    required String etiket,
    required String deger,
    bool solukDeger = false,
  }) {
    final anaRenk = Theme.of(context).colorScheme.primary;
    return ListTile(
      leading: Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: anaRenk.withValues(alpha: 0.08),
          borderRadius: BorderRadius.circular(10),
        ),
        child: Icon(ikon, color: anaRenk, size: 20),
      ),
      title: Text(
        etiket,
        style: TextStyle(
          fontSize: 12,
          color: Colors.grey.shade600,
          fontWeight: FontWeight.w500,
        ),
      ),
      subtitle: Padding(
        padding: const EdgeInsets.only(top: 2),
        child: Text(
          deger,
          style: TextStyle(
            fontSize: 15,
            fontWeight: solukDeger ? FontWeight.w400 : FontWeight.w600,
            fontStyle: solukDeger ? FontStyle.italic : FontStyle.normal,
            color: solukDeger ? Colors.grey.shade500 : Colors.black87,
          ),
        ),
      ),
    );
  }
}
