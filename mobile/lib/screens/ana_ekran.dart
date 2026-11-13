import 'package:flutter/material.dart';
import '../models/ozet_bilgi.dart';
import '../models/oturum.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import 'giris_ekrani.dart';
import 'kullanici_ekle_ekrani.dart';
import 'urun_listesi_ekrani.dart';
class _Islem {
  final IconData ikon;
  final String baslik;
  final String aciklama;
  final Color renk;
  /// Null ise ekran henüz yazılmamıştır ("yakında" mesajı gösterilir).
  final Widget Function()? ekran;
  const _Islem(this.ikon, this.baslik, this.aciklama, this.renk, {this.ekran});
}

Widget _kullaniciEkleEkrani() => const KullaniciEkleEkrani();
Widget _urunListesiEkrani() => const UrunListesiEkrani();
class _Istatistik {
  final IconData ikon;
  final String baslik;
  final Color renk;
  final int Function(OzetBilgi) deger;
  final bool uyari;
  const _Istatistik(this.ikon, this.baslik, this.renk, this.deger,
      {this.uyari = false});
}

int _urunCesidi(OzetBilgi o) => o.urunCesidi;
int _toplamStok(OzetBilgi o) => o.toplamStok;
int _kritikSayisi(OzetBilgi o) => o.kritikStokUrunSayisi;

const _yoneticiIstatistikleri = [
  _Istatistik(Icons.category_rounded, 'Ürün Çeşidi', Color(0xFF4F46E5),
      _urunCesidi),
  _Istatistik(Icons.inventory_2_rounded, 'Stoktaki Ürün', Color(0xFF0D9488),
      _toplamStok),
  _Istatistik(Icons.warning_amber_rounded, 'Kritik Stoktaki Ürün',
      Color(0xFFF59E0B), _kritikSayisi,
      uyari: true),
];

const _calisanIstatistikleri = [
  _Istatistik(Icons.warning_amber_rounded, 'Kritik Stoktaki Ürün',
      Color(0xFFF59E0B), _kritikSayisi,
      uyari: true),
];

const _yoneticiIslemleri = [
  _Islem(Icons.inventory_2_rounded, 'Ürünler', 'Ürünleri listele ve ara',
      Color(0xFF0EA5E9),
      ekran: _urunListesiEkrani),
  _Islem(Icons.add_box_rounded, 'Ürün Ekle', 'Yeni ürün kaydı oluştur',
      Color(0xFF4F46E5)),
  _Islem(Icons.edit_rounded, 'Ürün Düzenle', 'Ürün bilgilerini güncelle',
      Color(0xFF0D9488)),
  _Islem(Icons.person_add_alt_1_rounded, 'Yeni Kullanıcı Ekle',
      'Yönetici veya çalışan hesabı aç', Color(0xFF7C3AED),
      ekran: _kullaniciEkleEkrani),
];

const _calisanIslemleri = [
  _Islem(Icons.point_of_sale_rounded, 'Satış Yap', 'Stoktan satış kaydı gir',
      Color(0xFF4F46E5)),
  _Islem(Icons.search_rounded, 'Stok Sorgula', 'Ürün stok durumunu gör',
      Color(0xFF0D9488),
      ekran: _urunListesiEkrani),
];

/// Role göre doğru ana ekranı seçer.
Widget anaEkranSec(Oturum oturum) => oturum.yoneticiMi
    ? YoneticiAnaEkran(oturum: oturum)
    : CalisanAnaEkran(oturum: oturum);

class YoneticiAnaEkran extends StatelessWidget {
  final Oturum oturum;
  const YoneticiAnaEkran({super.key, required this.oturum});

  @override
  Widget build(BuildContext context) => _AnaIskelet(
        oturum: oturum,
        istatistikler: _yoneticiIstatistikleri,
        islemler: _yoneticiIslemleri,
      );
}

class CalisanAnaEkran extends StatelessWidget {
  final Oturum oturum;
  const CalisanAnaEkran({super.key, required this.oturum});

  @override
  Widget build(BuildContext context) => _AnaIskelet(
        oturum: oturum,
        istatistikler: _calisanIstatistikleri,
        islemler: _calisanIslemleri,
      );
}

class _AnaIskelet extends StatefulWidget {
  final Oturum oturum;
  final List<_Istatistik> istatistikler;
  final List<_Islem> islemler;
  const _AnaIskelet({
    required this.oturum,
    required this.istatistikler,
    required this.islemler,
  });

  @override
  State<_AnaIskelet> createState() => _AnaIskeletDurum();
}

class _AnaIskeletDurum extends State<_AnaIskelet> {
  OzetBilgi? _ozet;
  String? _hata;
  bool _yukleniyor = true;

  String get _rolMetni => widget.oturum.yoneticiMi ? 'Yönetici' : 'Çalışan';

  String get _gorunenAd {
    final ad = widget.oturum.adSoyad.trim();
    return ad.isEmpty ? widget.oturum.eposta : ad;
  }

  @override
  void initState() {
    super.initState();
    _ozetiYukle();
  }

  Future<void> _ozetiYukle() async {
    setState(() {
      _yukleniyor = true;
      _hata = null;
    });
    try {
      final cevap = await ApiService().get('/api/ozet');
      if (!mounted) return;
      setState(() {
        _ozet = OzetBilgi.fromJson(cevap as Map<String, dynamic>);
        _yukleniyor = false;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      if (e.durum == 401) {
        await _cikisYap();
        return;
      }
      setState(() {
        _hata = e.mesaj;
        _yukleniyor = false;
      });
    }
  }

  void _islemAc(_Islem islem) {
    final ekran = islem.ekran;
    if (ekran == null) {
      _yakinda(islem.baslik);
      return;
    }
    Navigator.of(context).push(MaterialPageRoute(builder: (_) => ekran()));
  }

  void _yakinda(String ad) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('$ad ekranı yakında eklenecek.')),
    );
  }

  Future<void> _cikisYap() async {
    await AuthService.instance.cikisYap();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const GirisEkrani()),
      (_) => false,
    );
  }

  Widget _karsilamaKarti() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF4F46E5), Color(0xFF7C3AED)],
        ),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF4F46E5).withValues(alpha: 0.3),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 28,
            backgroundColor: Colors.white.withValues(alpha: 0.2),
            child: Text(
              _gorunenAd.characters.first.toUpperCase(),
              style: const TextStyle(
                  color: Colors.white,
                  fontSize: 24,
                  fontWeight: FontWeight.w700),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Hoş geldiniz,',
                    style: TextStyle(
                        color: Colors.white.withValues(alpha: 0.85))),
                const SizedBox(height: 2),
                Text(
                  _gorunenAd,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                      color: Colors.white,
                      fontSize: 22,
                      fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 8),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 3),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.2),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(_rolMetni,
                      style: const TextStyle(
                          color: Colors.white,
                          fontSize: 12,
                          fontWeight: FontWeight.w600)),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _istatistikKarti(_Istatistik ist) {
    final deger = _ozet == null ? null : ist.deger(_ozet!);
    final renk = ist.uyari && (deger ?? 0) == 0 ? const Color(0xFF16A34A) : ist.renk;
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
              child: Icon(ist.ikon, color: renk, size: 24),
            ),
            const SizedBox(height: 14),
            deger == null
                ? SizedBox(
                    height: 32,
                    child: Align(
                      alignment: Alignment.centerLeft,
                      child: SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(
                            strokeWidth: 2.4, color: renk),
                      ),
                    ),
                  )
                : Text(
                    '$deger',
                    style: TextStyle(
                        fontSize: 30,
                        fontWeight: FontWeight.w800,
                        color: renk),
                  ),
            const SizedBox(height: 2),
            Text(ist.baslik,
                style: TextStyle(color: Colors.grey.shade700, fontSize: 13)),
          ],
        ),
      ),
    );
  }

  /// İstatistik kartlarını ikili satırlara dizer; tek kalan kart tam genişlik olur.
  Widget _istatistikler() {
    final liste = widget.istatistikler;
    final satirlar = <Widget>[];
    for (var i = 0; i < liste.length; i += 2) {
      final ikili = liste.skip(i).take(2).toList();
      satirlar.add(IntrinsicHeight(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            for (var j = 0; j < ikili.length; j++) ...[
              if (j > 0) const SizedBox(width: 12),
              Expanded(child: _istatistikKarti(ikili[j])),
            ],
          ],
        ),
      ));
      if (i + 2 < liste.length) satirlar.add(const SizedBox(height: 12));
    }
    return Column(children: satirlar);
  }

  Widget _islemKarti(_Islem islem) {
    return Card(
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: () => _islemAc(islem),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: islem.renk.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(islem.ikon, color: islem.renk, size: 26),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(islem.baslik,
                        style: const TextStyle(
                            fontSize: 16, fontWeight: FontWeight.w700)),
                    const SizedBox(height: 2),
                    Text(islem.aciklama,
                        style: TextStyle(
                            color: Colors.grey.shade600, fontSize: 13)),
                  ],
                ),
              ),
              Icon(Icons.chevron_right_rounded, color: Colors.grey.shade400),
            ],
          ),
        ),
      ),
    );
  }

  Widget _baslik(String metin) => Padding(
        padding: const EdgeInsets.only(top: 24, bottom: 12, left: 4),
        child: Text(metin,
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w800)),
      );

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Stok Takip')),
      drawer: Drawer(
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            Container(
              padding: EdgeInsets.fromLTRB(
                  20, MediaQuery.of(context).padding.top + 24, 20, 20),
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [Color(0xFF4F46E5), Color(0xFF7C3AED)],
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  CircleAvatar(
                    radius: 30,
                    backgroundColor: Colors.white.withValues(alpha: 0.2),
                    child: Text(
                      _gorunenAd.characters.first.toUpperCase(),
                      style: const TextStyle(
                          color: Colors.white,
                          fontSize: 26,
                          fontWeight: FontWeight.w700),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Text(_gorunenAd,
                      style: const TextStyle(
                          color: Colors.white,
                          fontSize: 18,
                          fontWeight: FontWeight.w700)),
                  const SizedBox(height: 2),
                  Text('${widget.oturum.eposta} • $_rolMetni',
                      style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.85),
                          fontSize: 13)),
                ],
              ),
            ),
            ListTile(
              leading: const Icon(Icons.home_rounded),
              title: const Text('Ana Sayfa'),
              onTap: () => Navigator.of(context).pop(),
            ),
            for (final islem in widget.islemler)
              ListTile(
                leading: Icon(islem.ikon),
                title: Text(islem.baslik),
                onTap: () {
                  Navigator.of(context).pop();
                  _islemAc(islem);
                },
              ),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.logout_rounded),
              title: const Text('Çıkış Yap'),
              onTap: _cikisYap,
            ),
          ],
        ),
      ),
      body: RefreshIndicator(
        onRefresh: _ozetiYukle,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
          children: [
            _karsilamaKarti(),
            if (_hata != null) ...[
              const SizedBox(height: 16),
              Card(
                color: Theme.of(context).colorScheme.errorContainer,
                child: ListTile(
                  leading: const Icon(Icons.error_outline),
                  title: Text(_hata!),
                  trailing: IconButton(
                    icon: const Icon(Icons.refresh),
                    onPressed: _yukleniyor ? null : _ozetiYukle,
                  ),
                ),
              ),
            ],
            _baslik('Genel Durum'),
            _istatistikler(),
            _baslik('İşlemler'),
            for (final islem in widget.islemler) ...[
              _islemKarti(islem),
              const SizedBox(height: 12),
            ],
          ],
        ),
      ),
    );
  }
}
