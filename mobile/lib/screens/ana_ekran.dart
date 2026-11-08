import 'package:flutter/material.dart';
class AnaEkran extends StatelessWidget {
  const AnaEkran({super.key});
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Stok Takip')),
      body: const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.inventory_2_outlined, size: 64),
            SizedBox(height: 16),
            Text('Stok Takip Uygulaması'),
          ],
        ),
      ),
    );
  }
}