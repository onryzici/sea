# SalvageCrew proje talimatları

## Kapsam ve çalışma kuralları

- Her görevde yalnızca kullanıcının belirttiği aşamayı uygula. Sonraki aşamanın sistemlerini kendiliğinden geliştirme.
- Mevcut çalışan sistemleri ve kullanıcı değişikliklerini koru; ilgisiz dosyaları değiştirme veya silme.
- Oynanış kodu geliştirilen görevlerde gerekli sahne, prefab ve Inspector bağlantılarını da tamamla ve doğrula.
- Paket sürümlerini projenin `ProjectSettings/ProjectVersion.txt` dosyasında belirtilen Unity sürümüyle uyumlu seç. Makinede kurulu Editor sürümünü proje sürümü sanma.
- Test edemediğin davranışları test edilmiş diye raporlama. Dosya incelemesi, derleme, EditMode testi ve Play testi sonuçlarını ayrı belirt.
- Her görev sonunda değişiklikleri, kontrol sonuçlarını, kısa manuel test adımlarını ve kalan sorunları bildir. `Docs/Progress.md` durumunu güncel tut.
- Ortak tekne ve yük fiziğinde host otoritesini koru. Aşama 3 NGO 2.13.3 + Unity Transport (Unity 6.5 builtin 6.5.0) kullanır; Relay/Lobby/Steam veya host migration yoktur.
- İlk prototipte basit şekiller ve geçici materyaller kullan.
- Unity assetlerini ve klasörlerini `.meta` dosyalarıyla birlikte sürüm kontrolünde tut; GUID'leri koru. Üretilen önbellekleri sürüm kontrolüne alma.

## Unity Editor ve doğrulama

- Sahne veya prefab oluşturmadan/değiştirmeden önce Editor araçlarını ve bağlantısını kontrol et. Bağlantıyı aktif sahne adı ve gerçek Hierarchy nesnelerini okuyarak doğrula.
- Sahne, prefab ve Inspector bağlantıları için Unity Editor araçlarını kullan; ham `.unity` veya `.prefab` YAML yazarak bunları taklit etme.
- İlk incelemede Editor bağlantısı yoktu; proje oluşturulduktan sonra Unity CLI + Pipeline bağlantısı gerçek sahne ve nesneler okunarak doğrulandı. Her görevde erişimi yeniden kontrol et. Sahne ve prefab üretiminde Unity Editor araçlarını kullan; bağlantı yoksa erişim varmış gibi davranma ve bu sınırlamayı raporla.
- Build veya Play testini ancak gerçek Unity projesi, uygun Editor, lisans ve hedef modülleri doğrulandıktan sonra çalıştır; sonuçları gerçekten gözlemle.
- Özellikle NGO oturumu açıkken C# veya Editor kurulum scriptini düzenlemeden önce Play'i durdur. Canlı ağ oturumunda domain reload bu prototipte desteklenmez; Input System state assertion ve NGO shutdown hatası üretebilir. Temiz yeniden başlatma sonuçlarını hot-reload hatalarından ayrı raporla.

## Dizin düzeni

- `Docs/GameBrief.md`: oyun kapsamı ve prototip hedefi.
- `Docs/Progress.md`: aşamalar, inceleme bulguları, doğrulamalar ve bekleyen işler.
- `Assets/_Game/Scenes`, `Scripts`, `Prefabs`, `Materials`, `Audio`, `Settings`: oyuna ait içerik.

Hazırlık, Aşama 1 liman/birinci şahıs, Aşama 2 yerel taşıma, Aşama 3 iki oyunculu direct-IP ve Aşama 4A ağ üzerinden tekne sürüşü/hareketli güverte prototipi vardır. Proje Unity 6000.5.6f1, URP 17.5.0 ve masaüstü başlangıç hedefi kullanır. Vinç, satış ve düşman yoktur. Sonraki görevlerde yalnızca açıkça istenen aşama uygulanır.

Liman sahnesi: `Assets/_Game/Scenes/HarborPrototype.unity`. Yeniden kurulum: `SalvageCrew/Build Harbor Prototype`; yalnızca `HarborPrototypeGenerated` kökünü yeniden üretir. Bu kökte elle yapılan değişiklikler yeniden kurulumda kaybolur; kullanıcı düzenlemelerini korumak için bu komutu çalıştırmadan önce incele. `SampleScene` korunur. `LocalPlayerInput` yalnızca yerel input/imleç işlerini, `FirstPersonMotor` hareket ve kurtarmayı yönetir.

Aşama 2 ek kurulumu: `SalvageCrew/Setup Scrap Carry Prototype`; açık HarborPrototype sahnesine eksik bileşenleri, üç hurda instance'ını ve tek HUD alanını ekler. Var olan hurda instance'larını/prefab ayarlarını yeniden üretmez. `LocalCarryInteraction` yerel input örneğini komuta çevirir; `PhysicsCarry` input cihazı bilmeden hedefleme, sahiplik, kuvvet ve bırakmayı; `ScrapItem` eşya ayarları ve kurtarmayı yönetir. Tutarken parent/Transform ışınlama kullanılmaz.

Aşama 3 kurulumu: `SalvageCrew/Setup Multiplayer Prototype`; mevcut limanı yeniden üretmeden ağ prefabları, tek session/panel ve Inspector bağlantılarını tamamlar. Var olan prefab ayarlarını korur. `HarborSession` offline/network geçişini; `NetworkCrewPlayer` yerel CharacterController hareketi, server doğrulaması ve RPC komutlarını; `NetworkScrap` server fizik otoritesi ve tutan oyuncu bilgisini yönetir. Eşya kontrolünü client'a devretme. Offline prefabları bağımsız kalır. Aynı bilgisayar testi: Editor host + macOS client, 127.0.0.1:7777; Tab panel, Escape imleç. Arka planda ağ/simülasyon açık, odak kaybında input kapalıdır. Test listesi `Docs/MultiplayerTest.md`; development-only probe normal açılışta etkin değildir. Sıralı meşgul eşya reddini eşzamanlı yarış kanıtı sayma.

Aşama 4A ek kurulumu: `SalvageCrew/Setup Moving Boat Prototype`; mevcut görünümü yeniden üretmeden NetworkBoat prefabı/kayıtları, DeckPassenger ve session/rampa bağlantılarını tamamlar. Offline sabit tekne korunur. NetworkBoat tek server Rigidbody ve compound collider kullanır; client yalnız snapshot interpolasyonu yapar. Serbest hurdaları tekneye parent etme veya güverteye kinematic sabitleme. Yerel yolcuya platform delta'sı bir kez; remote ağ pose'una boat-local koordinatlardan mutlak dönüşüm bir kez uygulanır. Server hız kontrolü platform hareketini oyuncunun yürüyüşü saymaz. Dümen tek sahipli ve server doğrulamalıdır. Kalkış rampayı/kılavuzları kapatır; otomatik yeniden yanaşma yoktur. Ağ modunda R/düşme kurtarması güncel boş kıç güvertesine, offline kurtarma iskeleye yapılır. Native pencere odağı hâlâ manuel doğrulama gerektirir; simüle callback testi OS odak testi değildir.
