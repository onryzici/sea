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

## Aşama 2 — yerel fizik tabanlı hurda taşıma

İskelede üç hurda: 3 kg hafif kutu, 12 kg metal kasa ve 35 kg motor. Kamera menzili 2,5 metre; E tutar/bırakır. Eşya Rigidbody kuvvetleriyle takip eder, çevreyle çarpışır ve ağır eşya daha yavaş takip eder. Taşıma yürüme hızını azaltır; motor taşırken koşulamaz. HUD hedefi/tutulan eşyayı gösterir. R ve oyuncu kurtarması tutmayı kaldırır; görsel denize düşen hurda kısa gecikmeyle kendi başlangıcına döner. Yerel input ve taşıma fiziği ayrıdır; henüz networking/host otoritesi uygulanmadı. Vinç, fırlatma, satış, düşman ve hareketli tekne kapsam dışındadır.

## Aşama 3 — iki oyunculu ortak hurda

Direct-IP host/client (127.0.0.1:7777 varsayılan), NGO 2.13.3 ve Unity Transport ile aynı limanda iki oyuncu ve üç ortak hurda bulunur. Hurda Rigidbody/kuvvet/kurtarma yalnızca server'da çalışır. Client tutma/bırakma isteği gönderir; server kimlik, menzil, görüş ve tek sahiplik doğrular. Oyuncu CharacterController'ı yerelde tepkili çalışır; server hız/alan/sıklık sınırları ve remote collider hareketiyle pozları doğrulayıp yayınlar. Bu sınırlı co-op modeli tam tahmin/reconciliation veya anti-cheat değildir. Offline oyun bağımsız korunur; bağlantı paneli ve pencere odağı tek bilgisayarda sırayla testi destekler. Steam, Relay, Lobby, hesap ve host migration yoktur. Gerçek doğrulama sonuçları Progress'tedir.

## Aşama 4A — tekne sürüşü ve hareketli güverte

Ağ modunda aynı 10×4 m tekneyi host veya client E ile tek sahipli dümen noktasından kullanabilir. Server sakin su yüzdürmesi/direnci ve düşük hızlı ileri/geri/dönüş fiziğini simüle eder; client tekneyi interpolasyonla gösterir. Pitch/roll prototipte kilitlidir; dalga ve yük dengesi yoktur. CharacterController güverte ötelemesi/dönüşünü izler; zıplama platform hızını alır. Hurdalar server'da dinamik ve sürtünmeli kalır; taşıma kuvvetleri platforma göre hesaplanır. Kalkışta rampa kapanır. Ağ kurtarması güncel güvenli güverteyi kullanır; offline sabit tekne ve iskele kurtarması korunur. Yanaşma/otomatik rampa açılması, vinç, satış ve düşman bu aşamanın dışında kalır. Gerçek iki instance, gecikme ve kalan manuel kontroller Progress ve MultiplayerTest'tedir.

## Henüz kararlaştırılmayanlar

Başlangıç: Unity 6000.5.6f1, URP 17.5.0, masaüstü ve ilk doğrulama hedefi macOS. LTS geçişi yapılmadı. Ticari hedef platformlar, internette oturum keşfi/katılımı ve ekipman geliştirmesinin ayrıntıları henüz belirlenmedi. Yerel prototip NGO/UTP direct-IP kullanır; monetizasyon kapsam dışıdır.
