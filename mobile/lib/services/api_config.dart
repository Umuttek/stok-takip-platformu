import 'package:flutter/foundation.dart';

class ApiConfig {
  static String get baseUrl {
    if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5237';
    }
    return 'http://localhost:5237';
  }
}
