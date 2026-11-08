import 'package:flutter_test/flutter_test.dart';
import 'package:stok_takip/main.dart';
void main() {
  testWidgets('Boş formda doğrulama mesajları görünüyorr', (tester) async {
    await tester.pumpWidget(const StokTakipUyg());
    await tester.tap(find.text('Giriş Yap'));
    await tester.pump();
    expect(find.text('E-posta boş bırakılamaz.'), findsOneWidget);
    expect(find.text('Şifre boş bırakılamaz.'), findsOneWidget);
  });
}