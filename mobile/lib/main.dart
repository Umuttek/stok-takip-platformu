import 'package:flutter/material.dart';
import 'screens/ana_ekran.dart';

void main() {
  runApp(const StokTakipApp());
}

class StokTakipApp extends StatelessWidget {
  const StokTakipApp({super.key});

  @override
  Widget build(BuildContext context) {
    return const MaterialApp(
      title: 'Stok Takip',
      debugShowCheckedModeBanner: false,
      home: AnaEkran(),
    );
  }
}