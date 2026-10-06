# SalvageCrew ilerleme

## Mevcut durum — 2026-10-06

Hazırlık ve gerçek Unity proje başlangıcı tamamlandı. Kök: `/Users/trexoinnovation/salvage`. İlk incelemede klasör tamamen boştu; Unity projesi, paket manifesti, sahne, Git deposu ve mevcut talimatlar yoktu. Kullanıcının ek isteğiyle resmi Universal 3D şablonundan gerçek proje oluşturuldu. Hazırlık belgeleri korundu; kullanıcı değişikliği silinmedi.

## Proje ve araçlar

| Konu | Doğrulanan durum |
| --- | --- |
| Unity | 6000.5.6f1 (0e0577a1a2ac), arm64; ProjectVersion.txt ile doğrulandı |
| Pipeline | URP 17.5.0; canlı Editor'da etkin asset PC_RPAsset |
| Hedef | Masaüstü başlangıcı, ilk build kontrolü macOS; PC kalite seviyesi |
| Lisans | Unity CLI aktif Unity Personal lisansı ve oturum bildirdi |
| Editor araçları | Unity CLI 1.0.0-beta.12 ve com.unity.pipeline 0.8.0-exp.1; localhost bağlantısı hazır |
| Sahne | Assets/Scenes/SampleScene.unity; build listesinde etkin |
| Gerçek Hierarchy | Main Camera (Camera, AudioListener, UniversalAdditionalCameraData), Directional Light (Light, UniversalAdditionalLightData), Global Volume (Volume) |
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

## Kontroller

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
| 1 — Unity başlangıcı | Gerçek proje, pipeline, Editor bağlantısı, Play/build kontrolü | Tamamlandı |
| 2 — Birinci şahıs temel | Oyuncu, kamera, etkileşim ve test sahnesi | Başlanmadı |
| 3 — İki oyuncu temeli | Ağ paketi/transport, host/client ve oyuncu eşleme | Başlanmadı |
| 4 — Ortak tekne ve yük | Host otoriteli fizik | Başlanmadı |
| 5 — Hurda ve vinç | Çıkarma ve güverteye yükleme | Başlanmadı |
| 6 — Liman ve ekonomi | Satış ve basit ekipman geliştirmesi | Başlanmadı |
| 7 — Doğrulama | İki oyuncuyla tam döngü, ardından 1–4 oyuncu | Başlanmadı |

Her görev yalnızca açıkça istenen aşamayı uygular. Bu görevde oyuncu, tekne, vinç, oyun prefabı veya multiplayer sistemi oluşturulmadı. Şablonun sahne, render ayarları ve Readme araç kodu korundu.

## Kısa manuel test

1. Unity Hub'dan bu proje kökünü Unity 6000.5.6f1 ile aç.
2. Assets/Scenes/SampleScene.unity sahnesini aç; Hierarchy'de Main Camera, Directional Light ve Global Volume'ü kontrol et.
3. Play'e gir; Game görünümünün render ettiğini ve Console'da yeni hata olmadığını kontrol et; Play'den çık.
4. Assets/_Game altındaki altı klasörü ve .meta dosyalarını kontrol et.
5. Builds/macOS/SalvageCrew.app uygulamasını aç; başlangıç sahnesini kontrol et. Build başarıyla alındı; standalone uygulama ayrıca çalıştırılmadı.

## Kalanlar ve sonraki aşama

Sonraki uygulama aşaması birinci şahıs temel prototiptir; kapsamı ayrıca istenmeli. Ticari hedef platformlar ve networking tercihi henüz belirlenmedi. Kurulu 6000.5.6f1 kullanıldı; LTS sürümüne geçiş yapılmadı. Windows/Linux build ve iki oyunculu test yapılmadı. Pipeline paketi experimental sürümdür.

Console geçmişinde URP Core paketinin `RuntimeDebugWindow_PanelSettings.asset` dosyasının immutable package içinde değiştiği uyarısı da görüldü. Paket kaynakları elle değiştirilmedi; bu uyarı package cache/import sırasında ortaya çıktı. Kaynak kontrolüne dahil olmayan Library/PackageCache içindedir; kök nedeni bu hazırlıkta giderilmedi. Build başarılı ve son Console kontrolünde compile hatası yok; ileride paket importunda yeniden kontrol edilmeli.

İlk CLI ile GUI açılışı EPIPE ile sonlandı; Editor doğrudan resmi executable üzerinden açıldı ve bağlantı kuruldu. İlk import sırasında bir Pipeline çağrısı 5 saniyede timeout verdi; sonrasında auto-tick/odak ve import tamamlanmasıyla canlı okuma ve Play testi başarılı oldu. Bu eski araç hatası yeni runtime hatası diye değerlendirilmedi.

Geçici şablon oluşturma kopyası /tmp/salvagecrew-bootstrap.eezB7t/SalvageCrew altında durur; asıl proje kökü değildir. Unity Hub kaydı asıl köke taşındı; Library önbelleği yeniden import maliyetini azaltmak için asıl köke aktarıldı. Geçici kopya otomatik silinmedi.
