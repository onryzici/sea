# İlk görsel geçiş — kaynak ve lisans kaydı

## Mürettebat ve yakın liman detayları — 2026-10-06

- [Kenney Pirate Kit 2.1](https://kenney.nl/assets/pirate-kit), üretici Kenney, **CC0**, atıf zorunlu değil. İndirilen paketin kendi `License.txt` dosyası kişisel/eğitsel/ticari kullanıma açıkça izin veriyor; projedeki `ThirdParty/Kenney/Pirate/License.txt` korunur. Bu tur mevcut doğrulanmış arşivden `structure`, `structure-roof`, `crate-bottles`, `boat-row-small`, `tool-paddle`, `flag-high`, `chest` FBX kaynakları eklendi. `Assets/_Game/Art/ThirdParty/Kenney/Pirate/Models/FBX format/` altında özgün kaynak ve Unity metaları. Kullanım: Harbor/CrewHarborDetails ikmal çatısı, kargo kümeleri, servis kayığı ve kıyı atölyesi. Mevcut CC0 crate/barrel ve palette materyali tekrar kullanılır.
- Mevcut lisanslı Loafbrr iskele/sütunları, KayKit kıyı evleri, Quaternius palmiye ve Rubberduck kayaları aynı aşağıdaki lisanslarla yeniden yerleştirildi. 40 detay kökü, 43 model Renderer + 2 dünya yazısı. Yeni görünür primitive veya özel dekor mesh'i üretilmedi. Tüm yeni modeller statiktir; Compound boat/cargo/network kökleri değiştirilmedi.
- [KayKit Character Adventures](https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0), **CC0**, atıf gerekmiyor; mevcut `KayKitCrew/License.txt`. `Rogue.fbx` içindeki Idle, Walking_A, Running_A kliplerinin döngülü kopyaları `Settings/CrewLocomotion.controller` içine bağlandı. Özgün FBX korunur. İsteğe bağlı silah çocukları gizlendi; atlas renklerini bozan tüm-model renk çarpımı kaldırıldı. Denizciye özgü yeni karakter/animasyon üretildiği iddia edilmez.

Kurulum: `SalvageCrew/Art/Install Crew Presence and Harbor Details`. Yalnız sahip olduğu detay, HUD ve animasyon bağlantılarını yerinde günceller. Eski ana sahne/Art kurulumu çalıştırılmaz.

## Özgün HUD ikonları — 2026-10-06

`Assets/_Game/Art/UI/Anchor.png`, `Cargo.png`, `Helm.png`: bu proje için yerleşik OpenAI image_gen ile üretilmiş özgün UI görselleri; indirilen üçüncü taraf model/ikon değildir, CC0 olarak etiketlenmez. Harici üretici/atıf dosyası yoktur. 1280² PNG kaynak ve gerçek alpha korunur, Sprite importu 512 px. Anchor bağlantı paneli/oyun başlığında, Cargo gerçek hurda etkileşiminde, Helm gerçek dümen durumunda kullanılır. Son promptlar ve üretim yöntemi `UIArtPrompts.md` içinde. Bu varlıklar oyun dünyasına yeni primitive/model eklemez.

## Sonraki stilize deniz/kıyı revizyonu — 2026-10-06

Bu bölüm aşağıdaki ilk-geçiş kullanım listesinin önüne geçer. Yeni görünüm Sea of Thieves referansından sanat yönü alır; oyunun dosyaları alınmadı/kopyalanmadı.

| Kaynak / üretici | Lisans ve atıf | Projedeki kullanım |
| --- | --- | --- |
| [Uber Stylized Water — MatrixRex](https://github.com/MatrixRex/Uber-Stylized-Water) | MIT, Copyright 2025 MatrixRex; `ThirdParty/MatrixRexWater/LICENSE.txt` dağıtımda korunmalı. Kaynak LICENSE ve special-thanks listesi incelendi | `Textures/Normal 1.png` → `Normal1.png`, 1024²: deniz yüzeyinde iki yönlü normal örnekleme. Normal2.png incelenen alternatif, aktif değil. Yalnız PNG/lisans indirildi; shader paketi, demo sahnesi, script veya bağımlılık kurulmadı |
| [Handpainted style rocks — rubberduck](https://opengameart.org/content/handpainted-style-rocks) | CC0; atıf gerekmez; ThirdParty/Rubberduck/License.txt | rock1/2/3 OBJ + özgün renk/normal dokuları; kayalık kümeler ve kum/kıyı; Harbor/ArtVisuals. İstenmeyen iki büyük deniz altı levhası kaldırıldı |
| [Modular Wooden Docks — loafbrr_1](https://opengameart.org/content/modular-wooden-docks) | CC0; atıf gerekmez; ThirdParty/Loafbrr/License.txt | Floor_Dock, Floor_Support, Dock_Bollard_1, Fence_Plane/Fence_Post; DockTrim ve alpha-clip WoodBeams dokuları; iskele/rampa/korkuluk. Collider'lar ayrı mevcut gameplay köklerinde |
| [Ultimate Stylized Nature — Quaternius](https://quaternius.com/packs/ultimatestylizednature.html) | CC0; atıf gerekmez; ThirdParty/Quaternius/License.txt | PalmTree_1/2, Bush FBX, yaprak/gövde dokuları; uzak ada bitkileri. Alpha-clip URP materyalleri; 1024 import sınırı; özgün gövde PNG LFS |
| [Medieval Hexagon — KayKit](https://github.com/KayKit-Game-Assets/KayKit-Medieval-Hexagon-Pack-1.0) | CC0; atıf gerekmez; ThirdParty/KayKit/LICENSE.txt | Küçük liman evleri/tavern ve özgün atlas. Dağ/tepe alternatifleri indirildi fakat son görünümde kullanılmıyor |
| [Character Adventures — KayKit](https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0) | CC0; atıf gerekmez; ThirdParty/KayKitCrew/License.txt | Rogue FBX/atlas, remote oyuncunun görsel çocuğu; denizci karakteri değildir, animasyon eklenmedi |
| Kullanıcının `Meshy_AI_Turquoise_Harbor_Tug_1006125812_texture.blend` dosyası | Kullanıcı sağladı; kullanım hakkı/üretim planı belgesi sağlanmadı. CC0 değildir diye de hüküm verilmedi; ticari dağıtımdan önce doğrulanmalı | Özgün Source/Art/TurquoiseHarborTug.blend korunur (LFS). İzinli borda geçidi düzenlemesi TurquoiseHarborTug_BoardingGate.blend; ThirdParty/UserBoat altında 95.569 yüzlü FBX ve 3 özgün doku. Blender autoexec kapalı; `EditBoatBoardingGate.py` yeniden aktarır |
| [AllSky Free — Richard Whitelock](https://github.com/rpgwhitelock/AllSkyFree_Godot) | MIT; copyright/lisans metni ThirdParty/AllSkyFree/LICENSE.txt içinde korunur, dağıtıma dahil edilmelidir | EpicBlueSunset.png: özgün 8192×4096 panorama, Unity max8192/HQ import; aktif gökyüzü ve deniz yansıması. Sadece doku alındı; Godot script/sahnesi çalıştırılmadı |
| `Generated/MaritimeSky.png` — imagegen | Proje için üretilmiş bitmap; harici oyundan alınmadı, CC0 kaynak diye sunulmaz | Önceki 1774×887 gökyüzü denemesi, artık aktif skybox değil. Üretim istemi `MaritimeSkyPrompt.md`. İkinci 4K istemi de 1774×887 döndü; 4K diye kullanılmadı |

Kenney hurda modelleri ve variller önceki lisanslarla kullanımdadır. Factory `pipe-large-valve.fbx` ayrıca sarı el çarklı dış dümen etkileşim işaretidir; özgün atlas kullanılır. Kenney palm/sand alternatifleri denendi; son adalarda Quaternius bitkileri kullanılır. Poly Haven ahşap/HDR alternatifleri indirildi ([wooden_planks](https://polyhaven.com/a/wooden_planks), [kloofendal_48d_partly_cloudy_puresky](https://polyhaven.com/a/kloofendal_48d_partly_cloudy_puresky)); CC0 kaynak kaydı klasöründe, son gökyüzü bu HDR değildir. Hiçbir harici script çalıştırılmadı, ücretli içerik satın alınmadı veya giriş engeli aşılmadı.

Güncel kurulum: `SalvageCrew/Apply Downloaded Stylized Assets`. Unity/URP paketleri değişmedi. Özgün ve düzenlenmiş `.blend`, büyük Quaternius gövde ve AllSky PNG'leri Git LFS'dedir; diğer seçilmiş modeller/dokular/lisanslar ve `.meta` dosyaları normal Git'tedir. Kullanıcının ek izniyle `TurquoiseHarborTug_Interior.blend` oluşturuldu: kapı/pencereler, yükseltilmiş kabin ve iç kaplama; 94.019 yüz. Kaynak `EditBoatInterior.py`, özgün BoardingGate dosyasından türetir; gövde/güverte su hattı korunur. Aşağıdaki ilk-geçiş notları **tarihseldir**; eski kurulum menüsü ve primitive modeller artık aktif görsel sahneyi tarif etmez.

## Tarihsel ilk görsel geçiş

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

### Enkaz seferi ve gerçek dümen — 2026-10-06

- **Ship's wheel — 3D Assets**: [kaynak](https://3dassets.dev/assets/pirate-port-and-tall-ships-ships-wheel-3a6878c2), [indirilen GLB](https://cdn.3dassets.dev/assets/30154/v1/model.glb). Sayfa CC0 1.0 Universal, ticari kullanım ve değişikliğe izin veriyor; atıf gerekmiyor. Üretici AI destekli model olduğunu belirtiyor. Kaynak `Source/ThirdParty/ShipWheel`; Unity FBX/doku/materyal/lisansı `Assets/_Game/Art/ThirdParty/ShipWheel`. Blender'da kaynaktaki yatık çember/aks eksenleri düzeltildi; gömülü otomatik dönüş kaldırıldı. Kullanım: offline tekne ve NetworkBoat/ShipWheelVisual; yalnız görsel çocuk, mevcut etkileşim kimliği korunur.
- **ship-wreck — Kenney Pirate Kit 2.1**: [resmi kaynak](https://kenney.nl/assets/pirate-kit). CC0; atıf gerekmez. Mevcut tam kaynak arşivinden FBX aktarıldı. `Assets/_Game/Art/ThirdParty/Kenney/Pirate/Models/FBX format/ship-wreck.fbx`; mevcut Pirate lisansı ve paleti korundu. Kullanım: HarborPrototypeGenerated/WreckExpedition/WreckSite_0 ve _1. Statik mesh collider, altı ayrı fizik yükü; orijinal model script içermez.
- BlendSwap ve Cad Crowd'da bulunan dümenler giriş istediğinden indirilmedi. Quaternius Pirate Kit'in Google Drive indirmesi kota mesajı verdi; HTML dosyası model olarak projeye alınmadı.
- Bu eklerin en büyük tek kaynak dosyası 1 MB'ın altında; mevcut LFS desenlerine dokunulmadı. Kaynak GLB/Blender ve Unity kaynakları saklanır; `.meta` dosyaları dahil edilir.

Enkaz/dümen kurulumu: `SalvageCrew/Expedition/Install Wreck Search`. Var olan tekne/oyuncu köklerini yeniden kurmaz; isimli yeni çocukları tekrar kullanır ve NetworkPrefabs kaydını çoğaltmaz. Eski ana liman/görsel yeniden kurulum araçlarını bu özellik için çalıştırma.

HarborPrototype açık ve Play kapalıyken `SalvageCrew/Apply Harbor Art Pass`. `HarborArtSetup` yalnız kendi `ArtVisuals` alt ağaçlarını yeniler; gameplay bileşenlerini eklemez/silmez veya collider ayarlarını değiştirmez. `ArtVisuals` içine elle yapılan düzenlemeler yeniden uygulamada kaybolur. Ana liman kurulumunu yeniden çalıştırmak görsel geçişi siler; kullanıcının sahne değişikliklerini korumak için önce incele.
