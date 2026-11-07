import 'package:flutter_test/flutter_test.dart';
import 'package:stok_takip/main.dart';
void main() {
  testWidgets('Ana ekran açılıyor', (tester) async {
    await tester.pumpWidget(const StokTakipApp());
    expect(find.text('Stok Takip Uygulaması'), findsOneWidget);
  });
}