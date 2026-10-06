# SalvageCrew

Geçici ad: SalvageCrew. Birinci şahıs bakış açılı, 1–4 oyunculu co-op hurda çıkarma oyunu. İlk hedef iki oyuncuyla çalışan küçük bir prototiptir.

## Temel oyun döngüsü

Oyuncular küçük bir tekneyle denize çıkar, denizden hurda çıkarır, vinçle güverteye yükler, limana dönerek satar ve kazançla ekipmanlarını geliştirir. Ekip iş birliği tekne, vinç ve yükün ortak kullanımına dayanır.

## İlk prototip hedefi

- İki oyuncu aynı oturuma katılır ve birinci şahıs olarak birlikte oynar.
- Küçük bir tekne, sınırlı bir deniz alanı, bir hurda çıkarma noktası ve bir liman bulunur.
- En az bir hurda türü vinçle güverteye yüklenir ve limanda satılır.
- Satış geliriyle en az bir basit ekipman geliştirmesi yapılır.
- Ortak tekne ve yük fiziğini host yönetir; istemciler sonucu tutarlı biçimde görür.
- Görseller basit şekiller ve geçici materyallerden oluşur.

Prototipin kabulü: iki oyuncunun denize çıkış → çıkarma → yükleme → limana dönüş → satış → geliştirme döngüsünü birlikte tamamlaması. Tek oyuncu ve 3–4 oyuncu desteği sonraki doğrulama hedefleridir.

## Hazırlık geçmişi

Proje incelemesi, talimatlar, belgeler, klasör yapısı ve `.gitignore` hazırlanır. Kullanıcının ek isteğiyle gerçek Unity projesi de resmi Universal 3D şablonundan oluşturulur, Editor bağlantısı ve başlangıç Play/build kontrolleri yapılır. Oyun sistemleri geliştirilmez. Şablon paketleri ve Editor araç bağlantısı için Pipeline paketi kullanılır; oyun prefabı oluşturulmaz.

## Aşama 1 — yürünebilir liman

Hazırlık sonrasında ilk oynanabilir aşama eklendi: HarborPrototype, kıyı/iskele, rampa, yaklaşık 10×4 metre sabit tekne ve yeniden kullanılabilir CharacterController oyuncu prefabı. WASD, mouse, Space, Left Shift, Escape/tıklama, R ve düşme sonrası kurtarma desteklenir. Deniz yalnızca görseldir. HUD nişangâh ve kontrol bilgisi gösterir. Eşya taşıma, vinç, tekne sürüşü, düşman, satış ve multiplayer bu aşamanın dışında kalır.

## Henüz kararlaştırılmayanlar

Başlangıç: Unity 6000.5.6f1, URP 17.5.0, masaüstü ve ilk doğrulama hedefi macOS. Bu sürüm kurulu Editor kullanılarak proje dosyalarına kaydedildi; LTS geçişi yapılmadı. Ticari hedef platformlar, ağ paketi/transport, oturuma katılım yöntemi ve ekipman geliştirmesinin ayrıntıları henüz belirlenmedi. Monetizasyon bu hazırlığın kapsamında değil.
