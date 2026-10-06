# SalvageCrew proje talimatları

## Kapsam ve çalışma kuralları

- Her görevde yalnızca kullanıcının belirttiği aşamayı uygula. Sonraki aşamanın sistemlerini kendiliğinden geliştirme.
- Mevcut çalışan sistemleri ve kullanıcı değişikliklerini koru; ilgisiz dosyaları değiştirme veya silme.
- Oynanış kodu geliştirilen görevlerde gerekli sahne, prefab ve Inspector bağlantılarını da tamamla ve doğrula.
- Paket sürümlerini projenin `ProjectSettings/ProjectVersion.txt` dosyasında belirtilen Unity sürümüyle uyumlu seç. Makinede kurulu Editor sürümünü proje sürümü sanma.
- Test edemediğin davranışları test edilmiş diye raporlama. Dosya incelemesi, derleme, EditMode testi ve Play testi sonuçlarını ayrı belirt.
- Her görev sonunda değişiklikleri, kontrol sonuçlarını, kısa manuel test adımlarını ve kalan sorunları bildir. `Docs/Progress.md` durumunu güncel tut.
- Ortak tekne ve yük fiziğinde host otoritesi hedefle. Ağ çözümü ve paketleri henüz seçilmedi.
- İlk prototipte basit şekiller ve geçici materyaller kullan.
- Unity assetlerini ve klasörlerini `.meta` dosyalarıyla birlikte sürüm kontrolünde tut; GUID'leri koru. Üretilen önbellekleri sürüm kontrolüne alma.

## Unity Editor ve doğrulama

- Sahne veya prefab oluşturmadan/değiştirmeden önce Editor araçlarını ve bağlantısını kontrol et. Bağlantıyı aktif sahne adı ve gerçek Hierarchy nesnelerini okuyarak doğrula.
- Sahne, prefab ve Inspector bağlantıları için Unity Editor araçlarını kullan; ham `.unity` veya `.prefab` YAML yazarak bunları taklit etme.
- İlk incelemede Editor bağlantısı yoktu; proje oluşturulduktan sonra Unity CLI + Pipeline bağlantısı gerçek sahne ve nesneler okunarak doğrulandı. Her görevde erişimi yeniden kontrol et. Sahne ve prefab üretiminde Unity Editor araçlarını kullan; bağlantı yoksa erişim varmış gibi davranma ve bu sınırlamayı raporla.
- Build veya Play testini ancak gerçek Unity projesi, uygun Editor, lisans ve hedef modülleri doğrulandıktan sonra çalıştır; sonuçları gerçekten gözlemle.

## Dizin düzeni

- `Docs/GameBrief.md`: oyun kapsamı ve prototip hedefi.
- `Docs/Progress.md`: aşamalar, inceleme bulguları, doğrulamalar ve bekleyen işler.
- `Assets/_Game/Scenes`, `Scripts`, `Prefabs`, `Materials`, `Audio`, `Settings`: oyuna ait içerik.

Hazırlık, Aşama 1 liman/birinci şahıs ve Aşama 2 fizik tabanlı hurda taşıma prototipi vardır. Proje Unity 6000.5.6f1, URP 17.5.0 ve masaüstü başlangıç hedefi kullanır. Yerel oyuncu, sabit tekne maketi ve üç hurda prefabı vardır; vinç, tekne sürüşü ve multiplayer henüz yoktur. Sonraki görevlerde yalnızca açıkça istenen aşama uygulanır.

Liman sahnesi: `Assets/_Game/Scenes/HarborPrototype.unity`. Yeniden kurulum: `SalvageCrew/Build Harbor Prototype`; yalnızca `HarborPrototypeGenerated` kökünü yeniden üretir. Bu kökte elle yapılan değişiklikler yeniden kurulumda kaybolur; kullanıcı düzenlemelerini korumak için bu komutu çalıştırmadan önce incele. `SampleScene` korunur. `LocalPlayerInput` yalnızca yerel input/imleç işlerini, `FirstPersonMotor` hareket ve kurtarmayı yönetir.

Aşama 2 ek kurulumu: `SalvageCrew/Setup Scrap Carry Prototype`; açık HarborPrototype sahnesine eksik bileşenleri, üç hurda instance'ını ve tek HUD alanını ekler. Var olan hurda instance'larını/prefab ayarlarını yeniden üretmez. `LocalCarryInteraction` yerel input örneğini komuta çevirir; `PhysicsCarry` input cihazı bilmeden hedefleme, sahiplik, kuvvet ve bırakmayı; `ScrapItem` eşya ayarları ve kurtarmayı yönetir. Tutarken parent/Transform ışınlama kullanılmaz. Gelecekte ortak eşya komutlarını ve fizik adımlarını host'a taşı; mevcut yerel yapıyı multiplayer yapılmış sayma.
