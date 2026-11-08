import 'package:flutter/material.dart';
import 'screens/giris_ekrani.dart';
import 'tema/uygulama_tema.dart';
void main() {
  runApp(const StokTakipUyg());
}
class StokTakipUyg extends StatelessWidget {
  const StokTakipUyg({super.key});
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Stok Takip',
      debugShowCheckedModeBanner: false,
      theme: UygulamaTema.light,
      home: const GirisEkrani(),
    );
  }
}