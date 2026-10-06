# SalvageCrew ilerleme

## Mevcut durum — 2026-10-06

Hazırlık, gerçek Unity proje başlangıcı ve kullanıcının Aşama 1 olarak istediği birinci şahıs/yürünebilir liman prototipi tamamlandı. Kök: `/Users/trexoinnovation/salvage`. İlk incelemede klasör tamamen boştu; resmi Universal 3D şablonundan oluşturulan proje korunarak geliştirildi. Görev başlangıcında Git temizdi; kullanıcı değişikliği silinmedi.

## Aşama 1 — birinci şahıs ve yürünebilir liman

- Sahne: `Assets/_Game/Scenes/HarborPrototype.unity`. Kıyı, iskele, hafif eğimli rampa, 10×4 metre sabit tekne, yürünebilir güverte, alçak korkuluklar ve kabin maketi. Deniz görsel bir yüzeydir, collider ve su fiziği içermez.
- Prefab: `Assets/_Game/Prefabs/FirstPersonPlayer.prefab`. CharacterController, 1,8 metre boy/0,3 metre yarıçap, kamera pivotu, AudioListener, URP kamera bileşeni ve iki oyun scripti bağlıdır. Sahne instance'ında PierSpawn bağlantısı ayrıca doğrulandı.
- `LocalPlayerInput`: Input System 1.20.0 action asset'ini oyuncuya özel kopyalar; WASD/mouse/Space/Left Shift/R/Escape/sol tıklama bağlantıları hazırdır. Kısa basışlar normal Update tarafından okunana kadar saklanır; focus kaybında temizlenir.
- `FirstPersonMotor`: 4 m/s yürüme, 6,5 m/s koşu, 1 metre zıplama, yerçekimi, zemin/baş çarpışması, normalize çapraz hareket ve ±80 derece bakış sınırı. Inspector'da hız, zıplama ve mouse hassasiyeti ayarlanabilir. R veya oyuncu ayağının y<0,2 olması spawn'a döndürür; düşme hızı ve bakış sıfırlanır.
- uGUI/TMP HUD: merkez nişangâhı ve küçük kontrol alanı. TMP Essential Resources, paket eklenmeden kurulu uGUI paketinden non-interactive import edildi.
- Editor kurulum komutu: `SalvageCrew/Build Harbor Prototype`. Yalnızca kendi `HarborPrototypeGenerated` kökünü yeniden üretir; diğer kökler korunur. Bu kökteki manuel düzenlemeler yeniden kurulumda kaybolur. İki çalıştırma sonrası 53 Transform → 53 Transform ve tek oyuncu doğrulandı.
- HarborPrototype build listesinin başına eklendi. SampleScene, eski input asset'i, Unity sürümü, URP pipeline assetleri ve paket manifest/lock dosyaları korundu.

### Gerçek doğrulama sonuçları

| Kontrol | Sonuç |
| --- | --- |
| Başlangıç bağlantısı | Aktif SampleScene, üç gerçek kök nesne ve Console okundu; Play kapalıydı |
| C# derleme | Unity recompile geçti; 0 compile hatası/uyarısı |
| Canlı Play fizik kontrolleri | Gerçek CharacterController ve sahne collider'larıyla 12 kontrol geçti |
| FPS ve çapraz hız | 30/120 adımda 4/4 metre; düz/çapraz 4/4 metre |
| İskele → rampa → güverte | Oyuncu (6,67; 1,53; 7,50) konumuna yürüyerek geçti; yerde |
| Korkuluk ve kabin | Oyuncu korkulukta x=8,49, kabinde z=10,87 noktasında durdu; içinden geçmedi |
| Zıplama | İlk hız 5,99 m/s; havada ikinci basış hız artırmadı; tepe yaklaşık 0,947 metre ve tekrar iniş başarılı |
| Bakış sınırı | Pitch mutlak değeri 80 dereceyi aşmadı |
| Gerçek input olayları | WASD yönleri, Space zıplama, Left Shift koşma, R dönüş, Escape bırakma ve tıklama kilitleme geçti |
| Mouse olayı | 80/20 piksel delta sonrası yaw 58→67,6; pitch 0→−2,4 derece |
| Canlı Update kurtarma | Oyuncu harita altına taşındıktan sonra (0; 1,23; 4) iskele spawn'ına otomatik döndü |
| Build | HarborPrototype için StandaloneOSX başarılı, 36,96 saniye, 0 hata/1 bilinen uyarı |
| Proje bütünlüğü | Strict verify: 199 dosya, 0 hata, 0 uyarı; meta/GUID/manifest kontrolleri geçti |
| Son durum | HarborPrototype açık, Play kapalı; geçici runInBackground ayarı false'a geri alındı |

Fizik kontrolleri motorun gerçek hareket adımını farklı deltaTime değerleriyle çağırır. Input kontrolleri ayrıca Input System'e keyboard/mouse olayları göndererek oyuncunun normal Update döngüsünü çalıştırdı. Fizik ve input doğrulama scriptleri ile sonuç JSON dosyaları `Docs/Verification` altında tekrar çalıştırılabilir şekilde tutulur; NUnit EditMode test süiti çalıştırılmadı. Fiziksel klavye/mouse ile insan eliyle baştan sona yürüyüş ve standalone uygulama çalıştırma testi yapılmadı.

Build: `Builds/HarborPrototype/macOS/SalvageCrew.app` (Git dışında). [İskele başlangıcından ekran görüntüsü](Screenshots/HarborPrototype.png) gerçek Game view ve HUD'dan alındı, görsel olarak incelendi. Yakalama aracının Assets altında oluşturduğu geçici kopya silindi; kalıcı görüntü Docs/Screenshots altında korundu.

### Önceki uyarılar ve bu görevin sorunları

Başlangıç Console geçmişinde önceki Pipeline 5 saniye timeout hatası, immutable URP paket asset uyarısı ve RuntimePipelineConfig eksikliği vardı. Bu görevde ilk sahne kurulum çağrısı da 5 saniyeyi aştı; sahnenin gerçekten oluşturulduğu okunarak doğrulandı, ikinci kurulum daha geniş timeout ile geçti. Domain reload sırasında iki bağlantı çağrısı başarısız oldu; Editor yeniden hazır olduğunda kontroller tamamlandı. Bir geçici Inspector sorgusu olmayan bir API'yi kullandı; sorgu düzeltildi, proje kaynaklarında derleme hatası oluşmadı.

İlk input denemesinde kısa tuş basışları birden fazla input güncellemesi arasında kayboldu; event saklama ile düzeltildi ve R/Escape/Space tekrar geçti. Async sprint testi ilk denemede duvar saati üzerinden karşılaştırma yaptığı için başarısız oldu; oyun zamanı üzerinden hız ölçümüyle yürüme≈4, koşu≈6,5 m/s doğrulandı. Son test aralığında oyun kaynaklı yeni error/exception yok. Build'in tek uyarısı eski RuntimePipelineConfig eksikliği; paket/pipeline değişikliği yapılmadı. Immutable package uyarısı yeniden üretilmedi ve kapsam dışında bırakıldı.

### Değiştirilen dosya grupları

`Assets/_Game/Scenes/HarborPrototype.unity`, oyuncu prefabı, iki runtime scripti, Editor kurulum scripti, `PlayerControls.inputactions`, dokuz URP materyali ve bunların `.meta` dosyaları; gerekli TMP kaynakları; `ProjectSettings/EditorBuildSettings.asset` ve Unity'nin yeni sahne oluştururken ürettiği `SceneTemplateSettings.json`; AGENTS ve Docs belgeleri, doğrulama scriptleri/sonuçları ve screenshot. SampleScene ile render/paket yapılandırmasında diff yok.

## Proje ve araçlar

| Konu | Doğrulanan durum |
| --- | --- |
| Unity | 6000.5.6f1 (0e0577a1a2ac), arm64; ProjectVersion.txt ile doğrulandı |
| Pipeline | URP 17.5.0; canlı Editor'da etkin asset PC_RPAsset |
| Hedef | Masaüstü başlangıcı, ilk build kontrolü macOS; PC kalite seviyesi |
| Lisans | Unity CLI aktif Unity Personal lisansı ve oturum bildirdi |
| Editor araçları | Unity CLI 1.0.0-beta.12 ve com.unity.pipeline 0.8.0-exp.1; localhost bağlantısı hazır |
| Sahne | Assets/_Game/Scenes/HarborPrototype.unity; build listesinde ilk sırada. SampleScene korundu |
| Gerçek Hierarchy | HarborPrototypeGenerated altında Harbor, StaticBoat_10m_x_4m, Daylight, GlobalVolume, PierSpawn, FirstPersonPlayer, HarborHUD |
| Git | Yerel main deposu; hazırlık ve proje başlangıcı ilk commit'e kaydedildi (e138bde); uzak depo oluşturulmadı |
| Asset düzeni | Assets/_Game altında Scenes, Scripts, Prefabs, Materials, Audio ve Settings; Editor'ın ürettiği .meta dosyaları |
| Unity VCS ayarları | Force Text ve Visible Meta Files |

Sahne adı ve nesneler dosya tahminiyle değil, çalışan Editor üzerinden okundu. Gelecekte sahne/prefab ve Inspector bağlantıları Unity Editor araçlarıyla hazırlanacak; her görevde bağlantı yeniden doğrulanmalı.

## Kurulu paketler

Manifest ve lock dosyaları kaynak kontrolüne dahildir. Unity ile gelen yerel şablonun sürümleri kullanıldı; eklenen tek araç paketi Pipeline'dır.

| Paket | Sürüm |
| --- | --- |
| Universal RP | 17.5.0 |
| Input System | 1.20.0 |
| Test Framework | 1.7.0 |
| Unity UI (uGUI) | 2.5.0 |
| Timeline | 1.8.12 |
| AI Navigation | 2.0.14 |
| Visual Scripting | 1.9.11 |
| Multiplayer Center | 1.0.1 |
| Collab Proxy | 2.13.3 |
| Rider Editor | 3.0.38 |
| Visual Studio Editor | 2.0.26 |
| Pipeline | 0.8.0-exp.1 |

Unity modülleri manifestte listelenir. Multiplayer Center şablonun araç paketidir; bir networking çözümü veya uygulanmış multiplayer sistemi değildir. Ağ paketi/transport seçimi bekliyor.

## Hazırlık aşamasının geçmiş kontrolleri

- Proje bütünlüğü: `unity projects verify --strict --expect-editor 6000.5.6f1` geçti; 0 hata, 0 uyarı. Eksik/orphan meta, yinelenen GUID, conflict marker ve manifest hatası bulunmadı.
- Derleme durumu: canlı Console `compilationFailed=false`, `compiling=false` bildirdi.
- Play: SampleScene'de gerçek Play testi yapıldı. `Time.frameCount` 1'den 8'e ilerledi; `wait_for` sonucu `met=true`, `timedOut=false`. Test aralığında yeni hata/exception yok. Play durduruldu.
- Test sırasında arka planda çalıştırma geçici olarak açıldı; önceki `false` değeri geri yüklendi. Bu, oynanış testi değildir.
- macOS build: başarılı (Succeeded), 0 hata, 1 uyarı; yaklaşık 76 saniye, 120.782.750 byte. Çıktı: `Builds/macOS/SalvageCrew.app`. Uyarı: RuntimePipelineConfig yok; Pipeline araç sunucusu Player build'inde devre dışı. Runtime araç erişimi bu hazırlıkta gerekli olmadığından etkinleştirilmedi.
- EditMode/otomatik oyun testleri: çalıştırılmadı; oyun kodu ve test süiti henüz yok.
- Ignore: Library, Temp, Logs ve Builds dışlanıyor; SampleScene ve .meta dosyaları dışlanmıyor.
- Git whitespace kontrolü: Unity'nin ürettiği YAML/.meta dosyalarında trailing whitespace bildirdi. Unity serializer çıktısı korunarak bu dosyalar elle yeniden biçimlendirilmedi; belgelerin whitespace kontrolü temiz. Üretilen klasörler commit'e dahil edilmedi.

## Aşamalar

| Aşama | Kapsam | Durum |
| --- | --- | --- |
| 0 — Hazırlık | Talimatlar, kapsam, ilerleme, klasörler, ignore | Tamamlandı |
| Hazırlık — Unity başlangıcı | Gerçek proje, pipeline, Editor bağlantısı, Play/build kontrolü | Tamamlandı |
| 1 — Birinci şahıs liman | Oyuncu, kamera, iskele/rampa/sabit tekne ve HUD | Tamamlandı |
| 2 — İki oyuncu temeli | Ağ paketi/transport, host/client ve oyuncu eşleme | Başlanmadı |
| 3 — Ortak tekne ve yük | Host otoriteli fizik | Başlanmadı |
| 4 — Hurda ve vinç | Çıkarma ve güverteye yükleme | Başlanmadı |
| 5 — Liman ve ekonomi | Satış ve basit ekipman geliştirmesi | Başlanmadı |
| 6 — Doğrulama | İki oyuncuyla tam döngü, ardından 1–4 oyuncu | Başlanmadı |

Her görev yalnızca açıkça istenen aşamayı uygular. Aşama 1'de tekne statik makettir; su/tekne fiziği, eşya taşıma, vinç, satış, düşman ve multiplayer eklenmedi. Şablonun SampleScene, render ayarları ve Readme araç kodu korundu.

## Kısa manuel test

1. Unity Hub'dan bu proje kökünü Unity 6000.5.6f1 ile aç.
2. Assets/_Game/Scenes/HarborPrototype.unity sahnesini aç, Play'e gir ve Game view'a tıkla.
3. WASD/Left Shift/Space ile iskelede yürü, koş ve zıpla; rampadan güverteye geç. Korkuluk ve kabine yürüyerek çarpışmayı kontrol et.
4. Mouse ile bak; yukarı/aşağı sınırını dene. Escape ile imleci bırak, Game view'a tıklayıp yeniden kilitle. R ile iskeleye dön; iskele kenarından denize düşüp otomatik kurtarmayı kontrol et.
5. HUD ve Console'u kontrol et; Play'den çık. İsteğe bağlı olarak Builds/HarborPrototype/macOS/SalvageCrew.app çıktısını aç; standalone çalıştırma ayrıca doğrulanmadı.

## Kalanlar ve sonraki aşama

Sonraki planlanan aşama iki oyuncu temeli; kapsamı ayrıca istenmeli. Ticari hedef platformlar ve networking tercihi henüz belirlenmedi. Kurulu 6000.5.6f1 korundu; LTS sürümüne geçiş yapılmadı. Windows/Linux build ve iki oyunculu test yapılmadı. Pipeline paketi experimental sürümdür. Önceden belgelenen araç/paket uyarıları dışında Aşama 1 için bilinen oyun engeli yok.

Console geçmişinde URP Core paketinin `RuntimeDebugWindow_PanelSettings.asset` dosyasının immutable package içinde değiştiği uyarısı da görüldü. Paket kaynakları elle değiştirilmedi; bu uyarı package cache/import sırasında ortaya çıktı. Kaynak kontrolüne dahil olmayan Library/PackageCache içindedir; kök nedeni bu hazırlıkta giderilmedi. Build başarılı ve son Console kontrolünde compile hatası yok; ileride paket importunda yeniden kontrol edilmeli.

İlk CLI ile GUI açılışı EPIPE ile sonlandı; Editor doğrudan resmi executable üzerinden açıldı ve bağlantı kuruldu. İlk import sırasında bir Pipeline çağrısı 5 saniyede timeout verdi; sonrasında auto-tick/odak ve import tamamlanmasıyla canlı okuma ve Play testi başarılı oldu. Bu eski araç hatası yeni runtime hatası diye değerlendirilmedi.

Geçici şablon oluşturma kopyası /tmp/salvagecrew-bootstrap.eezB7t/SalvageCrew altında durur; asıl proje kökü değildir. Unity Hub kaydı asıl köke taşındı; Library önbelleği yeniden import maliyetini azaltmak için asıl köke aktarıldı. Geçici kopya otomatik silinmedi.
