# İlk görsel geçiş — kaynak ve lisans kaydı

6 Ekim 2026. Sanat yönü: kullanıcının sağladığı iki konsept görselindeki turkuaz çalışma teknesi, sıcak ahşap, sarı güvenlik/ekipman vurguları, beyaz–terracotta kıyı ve turkuaz deniz. Referans görseller oyun asseti olarak dağıtılmadı. Vinç, düşman, para/can ve görev HUD'u eklenmedi.

## Kullanılan dış kaynaklar

**Aşağıdaki tüm modellerin üreticisi Kenney'dir; lisansları CC0 1.0'dır.** Her paketin kendi indirilen `License.txt` dosyası ticari kullanıma açıkça izin veriyor; atıf zorunlu değil. İsteğe bağlı kredi: “Kenney — www.kenney.nl”. Paketin web sayfası ve arşivin lisansı ayrı ayrı incelendi. Yalnız seçilen FBX ve özgün palette PNG'leri aktarıldı; üçüncü taraf script, shader, collider veya paket eklenmedi.

Kaynak dosyaları `Assets/_Game/Art/ThirdParty/Kenney/<Paket>/Models/FBX format/` altında, lisanslar aynı paket klasörünün `License.txt` dosyasında. Her model aşağıdaki kaynak sayfasından indirildi:

| Model / kaynak | Paket, sürüm | Kullanım |
| --- | --- | --- |
| [crate.fbx](https://kenney.nl/assets/pirate-kit) | Pirate 2.1 | LightBox + NetworkLightBox / ArtVisuals; 3 kg kutu |
| [barrel.fbx](https://kenney.nl/assets/pirate-kit) | Pirate 2.1 | Harbor / ArtVisuals; kıyı varili |
| [rocks-a.fbx](https://kenney.nl/assets/pirate-kit) | Pirate 2.1 | Harbor / ArtVisuals; uzak kıyı kayaları |
| [rocks-b.fbx](https://kenney.nl/assets/pirate-kit) | Pirate 2.1 | Harbor / ArtVisuals; uzak kıyı kayaları |
| [structure-platform-dock-small.fbx](https://kenney.nl/assets/pirate-kit) | Pirate 2.1 | Harbor / ArtVisuals; iskele kaplaması, 6 görsel modül |
| [box-large.fbx](https://kenney.nl/assets/factory-kit) | Factory 3.0 | MetalCrate + NetworkMetalCrate / ArtVisuals; 12 kg metal kasa, ochre materyal |
| [machine.fbx](https://kenney.nl/assets/factory-kit) | Factory 3.0 | ScrapEngine + NetworkScrapEngine / ArtVisuals; 35 kg motor maketi |
| [pipe-large-valve.fbx](https://kenney.nl/assets/factory-kit) | Factory 3.0 | Aynı motor görselindeki paslı üst mekanizma |
| [building-type-a.fbx](https://kenney.nl/assets/city-kit-suburban) | Suburban 2.0 | Harbor / ArtVisuals; küçük kıyı binaları |
| [building-type-c.fbx](https://kenney.nl/assets/city-kit-suburban) | Suburban 2.0 | Harbor / ArtVisuals; küçük kıyı binaları |
| [boat-fishing-small.fbx](https://kenney.nl/assets/watercraft-kit) | Watercraft 2.1 | Harbor / ArtVisuals; uzaktaki küçük balıkçı teknesi, yalnız görsel |
| [buoy.fbx](https://kenney.nl/assets/watercraft-kit) | Watercraft 2.1 | Harbor / ArtVisuals; görsel şamandıra |

Her paketin `Textures/colormap.png` dosyası aynı CC0 lisansı altındadır ve `<Paket>Palette.mat` ile URP/Lit kullanır. Binaların yeşil palette renkleri, `MediterraneanPalette.asset` türevinde terracotta tonlarına dönüştürüldü; özgün PNG korunur. Motor, kasa, iskele ve kayalarda ortak renk materyalleri ile stil uyumu sağlandı.

İndirilen arşiv SHA-256 kayıtları (arşivler geçici dizinde; kullanılan özgün FBX/PNG ve lisanslar Git'te):

```text
Pirate    667ed2caf92954ddb98f7b7cede831fe99ab75063c26b25e23d32715bee9c943
Factory   7e31fb2308e90304672bd15cd18fa9d9f02c03731a8cbc57a8e3e1c181dfb0a7
Suburban  5869c35cf30b1c87bdb2d197b6d325eebadd2ef08ea27f04797e8e08d77a9a39
Watercraft cd1470c1cf441c7f46d0944ae6d0d897242365dc97677c5079b3238965d659f3
```

## Projeye özgü görseller

- Oyuncunun kullandığı mevcut 10 × 4 m tekne için uygun yürünebilir güverte/kabin geometrisini birebir karşılayan ücretsiz model bulunmadı. Küçük balıkçı FBX'i oynanış teknesinin yerine geçirilmedi. Mevcut tekne, `WorkboatHull.asset` görsel gövdesi, renkli materyaller, güverte tahtaları, mast/baca ve boyası aşınmış küçük yüzeylerle iyileştirildi. Compound collider ve fizik kökü aynı kaldı.
- `Ring.asset`: proje içinde üretilen ortak torus mesh; lastik tampon, can simidi, halat halkaları ve dümen görseli. Collider yok. Mevcut dümen mekanizmasını değiştirmez.
- `CalmHarborWater.shader` ve materyali: bu görev için yazılan hafif URP shader. Dünya XZ koordinatlarında iki sinüzoidal yüzey deseni, hafif Fresnel/parıltı ve ince ripple köpüğü. Vertex displacement, su fiziği, depth/reflection capture, Renderer Feature ve ilave paket yok. [Unity'nin URP shader uyumluluk belgesi](https://docs.unity.com/en-us/engine/6000.0/manual/render-pipelines/universal-render-pipeline/introduction/installing-and-configuring-urp/upgrading-from-birp/upgrade-shaders/birp-urp-custom-shader-upgrade-guide) temel include/CBUFFER düzeni için kontrol edildi.
- Deniz feneri, kıyı adası/teras, mast ve ahşap plank görselleri basit Unity şekilleri; harici lisans/atıf yok. Uzak dekorlar collider içermez ve yeni oynanabilir ada değildir. Rampa ve yolların fizik düzeni korunur.

## İncelenen ancak kullanılmayan kaynaklar

- [Quaternius Ships Pack](https://quaternius.com/packs/ships.html): CC0; gemi türleri mevcut küçük çalışma teknesi/güverte düzenine uygun bulunmadı. İndirilmedi.
- [Poly Haven Wooden Planks](https://polyhaven.com/a/wooden_planks): CC0; fotoğrafik doku bu ilk düz renkli stilize geçişte kullanılmadı.
- [itch.io Fishing Boat](https://hankgreenburg.itch.io/simple-fishing-boat): sayfada açık ticari lisans saptanmadı; indirilmedi/kullanılmadı.
- [Sketchfab Stylized Fishing Boat](https://sketchfab.com/3d-models/stylized-fishing-boat-eb459efb50d643a2b5112806e5530bef): atıf lisansı yanında NoAI kısıtı görüldü; bu çalışma için kullanılmadı. Giriş gerektiren indirme aşılmadı.
- [BitGem URP Stylized Water](https://assetstore-fallback.unity.com/packages/vfx/shaders/urp-stylized-water-shader-proto-series-187485): eski Unity hedefi ve Asset Store/My Assets indirme akışı; mevcut URP 17.5 uyumluluğu doğrulanmadığından kullanılmadı. Satın alma/giriş veya paket sürümü değişikliği yapılmadı.

## Git ve ölçek

Git LFS 3.7.1 ve yerel filtre kurulumu var; projede `.gitattributes`/mevcut LFS takip deseni yok. Bu geçişin tüm Art dizini yaklaşık 2,8 MB; FBX'ler küçük, en büyük dosya türetilmiş palette assetidir. LFS geçmiş migrasyonu veya yeni filtre dayatılmadı. Kaynaklar, lisanslar, Unity materyal/mesh/texture assetleri ve tüm `.meta` dosyaları normal Git ile dahil edilir; build, Library, log ve indirilen tam ZIP önbellekleri dahil edilmez.

## Tekrarlanabilir kurulum

HarborPrototype açık ve Play kapalıyken `SalvageCrew/Apply Harbor Art Pass`. `HarborArtSetup` yalnız kendi `ArtVisuals` alt ağaçlarını yeniler; gameplay bileşenlerini eklemez/silmez veya collider ayarlarını değiştirmez. `ArtVisuals` içine elle yapılan düzenlemeler yeniden uygulamada kaybolur. Ana liman kurulumunu yeniden çalıştırmak görsel geçişi siler; kullanıcının sahne değişikliklerini korumak için önce incele.
