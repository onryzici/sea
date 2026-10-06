# SalvageCrew ilerleme

## Mevcut durum — 2026-10-06

Hazırlık, Aşama 1/2/3 ve Aşama 4A tekne sürüşü/hareketli güverte uygulanmıştır. Gerçek Editor host + macOS standalone client çekirdek testleri normal ağ ve 100 ms/yön + %1 kayıpta geçti. Native pencere odağı ve fiziksel klavye/mouse ile tam iki pencere turu hâlâ manuel kontroldür. Kök: `/Users/trexoinnovation/salvage`. Bu görevin başlangıcı 877d2ab ve Git temizdi; kullanıcı değişikliği silinmedi. Aşağıdaki eski aşama bölümleri tarihsel sonuçlardır; güncel 4A sonuçları aşağıdadır.

## Aşama 4A — ağ üzerinden tekne sürüşü ve hareketli güverte

### Uygulama ve ön kontrol

- HarborPrototype adı ve gerçek Hierarchy (Harbor/BoardingRamp, tekne Hull/Deck/korkuluk/Cabin, oyuncu, üç hurda, session/panel) canlı Editor üzerinden okundu; başlangıçta Play kapalıydı. Console ground truth derleme hatası göstermedi; geçmiş hata/uyarılar temizlenmeden ayrıldı. Editor 6000.5.6f1, URP 17.5.0, NGO 2.13.3 ve kurulu Transport/Input System değişmedi. Aktif lisans ve StandaloneOSX hedef desteği doğrulandı.
- `NetworkBoat`: server-owned tek 2500 kg Rigidbody, mevcut görünümden Editor ile üretilmiş compound collider prefabı; client snapshot'ları 20 Hz alıp interpolasyon + sınırlı extrapolation ile gösterir. Başlangıç iskele kelepçesi kinematic'tir; ilk motor girdisi rampayı ve iki kılavuzu kapatıp server body'yi dinamik yapar. **Bu yalnız ilk demirleme içindir; serbest hurda sabitlenmez.** Sakin su düşey yayı/sönümü, su ve yan direnç, sınırlı ivme/yaw uygulanır; pitch/roll kilitlidir. Varsayılan gerçek ileri denge hızı≈1,2 m/s; Inspector maksimumu 2,5 m/s, ivme 0,6 m/s² ve direnç 0,5/s olduğundan hedef hızın tamamına ulaşmaz. Bu bilinçli düşük hızlı ilk prototiptir.
- Dümen kabin önündeki gri bloktur. E kullan/bırak; WASD sürüş, yürüme/zıplama kilitli ama bakış açık. Server sender, güncel pose, 2,5 m kamera menzili/görüş, boş sahiplik ve elde hurda olmamasını doğrular. Girdi normalize/rate-limit edilir; 0,35 s timeout/panel/odak kaybında sıfırlanır. Ayrılma/R kontrolü serbest bırakır. Motorun girdi olmadığında tekneyi aniden durdurması yerine su direnci yavaşlatır.
- `DeckPassenger`: parent olmadan yerel CC'ye bir platform öteleme/yaw delta'sı, ardından normal yürüyüş; zıplamada platform nokta hızı, inişte yeniden destek. Server/remote pozları boat-local konum/yaw ile mutlak dönüştürülür; client'ta ikinci platform delta'sı uygulanmaz. Server hız doğrulaması platform hareketini yürüme saymaz, alan/güncellik/sıklık/epoch sınırlarını korur. Model hâlâ yerel CC tahmini + sınırlı server doğrulamasıdır; tam reconciliation/anti-cheat değildir.
- `PhysicsCarry` server'da mevcut sınırlandırılmış PD kuvvetini platforma göre sönümler/hız sınırlar; bırakma platform hızını korur. Üç network hurdanın sürtünmesi ve solver ayarları güverteye uygun; serbestken dinamik, parent yok, kinematic deck-lock yok. Oyuncu–tutulan eşya ignore pair restorasyonu ve ağırlık kısıtları korunur.
- Ağ modunda R/düşme: önce tutma/dümen temizlenir, güncel kıç güvertesinde collider'larla boşluğu kontrol edilen bir nokta seçilir; client epoch/boat-local ack ile aynı güncel tekneye döner. Güverteye yüklenmiş hurdalar deniz kurtarmasında server tarafından güncel tekne boş slotuna, tekne hızıyla döner. Yüklenmemiş pier hurdaları eski başlangıcını kullanır. Kalkmış tekneye geç katılım güncel güvenli güverteden olur. Offline sabit tekne, rampa ve eski iskele kurtarması korunur.
- `Setup Moving Boat Prototype` tekrar tekrar çalıştırıldı: bir session/EventSystem, beş prefab kaydı, tek boat prefabı ve üç rampa bağlantısı; çoğalma yok. Sahne değişikliği yalnız altı serialized bağlantı satırıdır; mevcut liman Transform'ları/tekne görünümü değişmedi. Offline prefablar, SampleScene, paketler ve render ayarları korundu.

### Gerçek Play / iki instance sonuçları

| Kontrol | Gerçek sonuç |
| --- | --- |
| Normal iki instance | Canlı Editor HOST + çalıştırılmış macOS CLIENT, 127.0.0.1:7777; bir ağ teknesi, iki oyuncu, üç ortak hurda |
| Güverteye geçiş | Her iki oyuncu kalkıştan önce gerçek motor adımıyla iskele/rampadan güverteye çıktı; kalkışta rampalar kapandı |
| Host sürer / client yolcu | Düz sürüş, dönüş, geri ve süzülme; client güvertede durdu/yürüdü; owner düzeltme sayısı 0 |
| Client sürer / host yolcu | Aynı sürüş ve yolcu hareketi; host güvertede durdu/yürüdü; 0 owner düzeltmesi |
| Zıplama / iniş | Her iki rolde hareketli güvertede destek kesildi, yükseklik arttı ve inişte destek geri geldi; client tekne-local y≈2,33→1,53 |
| Üç dinamik hurda | Tekrarlanabilir server fixture'ıyla üçü kıç güverteye yerleştirildi; gerçek Rigidbody/FixedUpdate sürüş/dönüşte güvertede kaldı. Bu fixture, üç hurdanın bu aşamada rampadan elle yüklendiği kanıtı değildir |
| Host ve client taşıma | Her iki oyuncu 3/12/35 kg eşyayı karşı oyuncu aktif sürerken gerçek RPC ile tuttu, kısa yürüdü, gerçek kuvvetle taşıdı/bıraktı; holder/kütle/aktif driver birlikte kontrol edildi |
| Ağırlık kısıtları | 0,95 / 0,75 / 0,50 hareket katsayısı; 35 kg'da koşu false; drop/R sonrası 1/true |
| Dümen tek sahiplik | Client sahibiyken host isteği meşgul açıklamasıyla reddedildi. Elde hurda ile dümen reddi ve açıklaması final build'de geçti. Gerçek eşzamanlı dümen yarışı bu aşamada otomatik denenmedi; sıralı test yarış diye sayılmadı |
| Sürücü ayrılması | Normal ve gecikmeli ağda client ayrılınca Driver boş, motor girdisi 0, host yolcusu güvertede; su direnci hızı azalttı (gecikmeli son gözlem≈1,11→0,065 m/s) |
| R / düşme / hurda kurtarma | Host/client R tutma ve dümeni kaldırdı. Client açık bordadan gerçekten yürüyerek suya düştü ve güncel güverteye döndü. Server deniz-altı fixture'ına alınan motor gerçek 1,5 s kurtarma ile güncel tekneye döndü; client aynı sonucu gördü |
| Geç katılma / yeniden bağlanma | Ayrılıp tekrar bağlanan final client kalkmış teknenin güncel güvenli güvertesinde doğdu; mevcut üç hurda durumları korundu, oyuncu/tekne çoğalmadı |
| Gecikme / kayıp | Gerçek loopback UDP proxy: her yönde 100 ms, yaklaşık 200 ms ek RTT, %1 kayıp. İki sürücü yönü, yolcu yürüyüş/zıplama, her oyuncuda üç hurda taşı/bırak, R ve hurda kurtarma ile sürücü ayrılması geçti; 0 owner correction / probe exception. Gecikmeli ayrı geri-sürüş, gerçek suya yürüme ve geç-katılma turu tekrar edilmedi; normal ağ sonuçları bunların yerine gecikmeli kanıt sayılmaz |
| Odak / arka plan | Gerçek Input System W/Space ile 5 BoatInputChecks geçti: dümen girdisi, yürüme/zıplama kilidi, simüle focus-loss sıfırlama, panel input engeli, kare ilerlemesi/runInBackground. **Native OS odak değişimi doğrulanmadı** |

Network test probe development-only/opt-in'dir. Motor adımı ve komut adaptörü cihaz input'unu atlayabilir; tutma/dümen gerçek NGO RPC, fizik gerçek server FixedUpdate'tır. İki instance ağ simülasyonu taklit edilmedi. Fiziksel klavye/mouse ve gözle sürekli kamera titremesi değerlendirmesi yerine geçmez. Sonuç snapshot'ları [Stage4AResults.json](Verification/Stage4AResults.json), tek kişinin uygulayacağı liste [MultiplayerTest.md](MultiplayerTest.md), gerçek Game view [hareketli güverte screenshot'ı](Screenshots/Stage4AMovingDeck.png). Screenshot probe ile konumlandırılmış gerçek sahneyi gösterir; native klavye test kanıtı değildir.

### Denemeler ve kalan fizik sınırları

- İlk dinamik demirleme turunda rampanın çarpışması tekneyi girdisiz kaydırdı; ilk demirleme kelepçesi ve hull inertia düzeltmesiyle dock konumu sabit kaldı. İlk hareketli güverte zıplaması platform Move'unun grounded durumunu temizlemesi nedeniyle atlamadı; motor grounded durumunu platform adımından önce kaydedince gerçek havalanma/iniş tekrar geçti.
- Bazı erken taşıma snapshot adları “client drives” dese de sürüş süresi bitmişti veya yanlış eşya raycast'i kabul edilmişti; bu turlar başarı sayılmadı. Sonraki sıkı kontroller holder, tam kütle, aktif driver ve tekne hızını birlikte doğruladı. JSON'da başarısız/eksik fixture kayıtları da tutulur.
- Güverte/dümen önüne yığılan hurda CC'yi engelleyebilir veya sürücüyü menzil dışına itebilir; güvenli server menzil reddi kontrolü bırakır. Dinamik yük üstünde destek takibi eklendi, fakat büyük yığın/uzun stres/iki oyuncu sıkışması test edilmedi. Düşük hız, yüksek sürtünme ve kilitli pitch/roll sakin su içindir; sert dönüş/hız yükseltme, dalga, yükten batma/yatma yoktur. Son numerik örneklerde pitch/roll küçük solver sapması≈0,001–0,003°; serbest yükler hâlâ fiziksel olarak kayabilir.
- Native UI köprüsü `Sky Computer Use native pipe startup failed` verdi; gerçek iki pencere W basılı odak değişimi ve insan eliyle kamera titremesi değerlendirmesi manuel listededir. Callback/input simülasyonu OS testi diye sunulmadı. NUnit EditMode süiti çalıştırılmadı; canlı Play kontrolleri ayrıdır. Internet/WAN, Windows/Linux ve uzun süreli stres yoktur.
- Rampalar kalkışta gizlenir ve collider'ları kapanır; geri yanaşınca otomatik açılmaz. Yeni oturum/offline dönüş açar. Mevcut deniz görseli sınırlıdır; tekneye dünya ±1000 m sınırı dışında serbest seyir veya yeni açık dünya eklenmedi.
- Bilinen Pipeline RuntimePipelineConfig build uyarısı korundu. Import/domain reload sırasında geçici CLI bağlantı/queue hataları ve HUD son düzeltmesinde kesilen ilk offline taşıma testi başarı sayılmadı; hazır Editor/temiz Play'de yeniden kontroller aşağıda belirtilir. Pipeline/URP uyarıları için ilgisiz paket değişikliği yapılmadı.
- HUD Editor scriptini Play sırasında değiştirmem canlı oturumda domain reload oluşturdu: Console seq 95–98 dört Input System map assertion'ı; 99–100 iki NGO NetworkSceneManager.Dispose NullReferenceException kaydetti. **Bunlar gerçek hatalardır, eski uyarı veya yalnız araç hatası değildir.** Oturum temiz yeniden başlatıldı; offline testler ve son üç bağlantı/kesme/host kapanışı 0 yeni hata ile geçti. Bu prototipte canlı NGO hot-reload desteklenmez; sonraki script düzenlemesinden önce Play durdurma kuralı AGENTS'e eklendi. Paket kaynakları değiştirilmedi.
- İlk güvenlik/input regresyonu bağlantı paneli açıkken çalıştı; doğru biçimde engellenen input nedeniyle sekiz assertion false döndü. Başarı sayılmadı. Panel kapatılıp simüle focus true ile tekrarlanan 18 kontrolün tamamı geçti; ilk sonuç da regresyon JSON'unda korunur.

### Son teslim kontrolleri

- Son macOS Development build **Succeeded**, 7,576 s, **0 hata / 1 bilinen RuntimePipelineConfig uyarısı**. Çıktı `Builds/Stage4A/macOS/SalvageCrew.app` (Git dışında). Build'den sonra gerçek executable yeniden çalıştırılıp Editor host'a 7777 üzerinden bağlandı; client dümen/ileri-dönüş sürerken host 12 kg kasayı gerçek RPC ile tuttu; iki probe 0 error/exception, owner correction 0.
- Son build'de üç client bağlantı/kesme döngüsü: iki oyuncu/tek tekne/üç hurda; kalkmış tekneye geç katılan client güncel güvertede doğdu. Host kapanışında client “Host bağlantısı kapandı… host shutting down” mesajıyla açık panel/SOLO'ya döndü. Restore kontrolü: ağ teknesi 0, ağ oyuncusu 0, offline hurda 3, eski sabit tekne/rampa açık.
- Offline canlı Play regresyonu: **12 motor/collider, 24 taşıma, 18 güvenlik/input, 7 yürüyüş/koşu, 5 panel/simüle odak** geçti. [Gerçek sonuçlar ve başarısız ilk panel-açık deneme](Verification/Stage4ARegressionResults.json). Son build'de 5 BoatInputChecks tekrar geçti. Native OS odağı doğrulanmış değildir.
- C# derleme/Console ground truth: compilationFailed=false, compiling=false, ConsoleErrors=0. Son temiz ağ oturumu ve kapanıştan sonra **cursor 105 sonrası yeni error/warning yok**; önceki 95–100 hot-reload hataları geçmişte korunur. Standalone teslim logunda error/exception eşleşmesi yok. NUnit EditMode süiti çalıştırılmadı.
- Strict proje kontrolü: **257 asset dosyası, 0 hata / 0 uyarı**, eksik/orphan meta, yinelenen GUID, conflict marker, manifest/editor drift yok. Kod/belge whitespace kontrolü temiz. SampleScene/offline prefab/paket/ProjectSettings/render değişikliği yok. Unity assetleri meta'larıyla korunur; fontun geçici glyph/cache farkı teslim diff'ine girmedi.
- Değişenler: yeni NetworkBoat prefabı, DeckGrip physics materyali, NetworkBoat/DeckPassenger runtime ve BoatPrototypeSetup Editor scriptleri + meta'ları; network oyuncu/üç hurda prefabı ve iki kayıt listesi; HarborPrototype yalnız session bağlantıları; motor/crew/session/carry/item/HUD/probe adaptörleri ve multiplayer setup'ın 4A yeniden bağlantısı; AGENTS/GameBrief/Progress/MultiplayerTest, BoatInputChecks, iki sonuç JSON'u ve screenshot. Tam dosya listesi yerel commit'tedir.
- Unity CLI ve uGUI becerileri mevcut Editor üzerinden idempotent asset/Inspector kurulumunu ve mevcut Canvas HUD'unu korumayı yönlendirdi; Multiplayer Services kurulumu direct-IP kapsamını genişletmemek için uygulanmadı.
- Teslimde HarborPrototype açık, **Play kapalı**; test client/proxy süreçleri durduruldu. Arka plan ağ/simülasyon ayarı runInBackground=true korundu. Yalnız bu görevin geçici Assets screenshot kopyası Editor üzerinden silindi; kalıcı Docs PNG korunur. Vinç/dalga/yük dengesi/satış/düşman/görsel iyileştirme eklenmedi. Sonraki adım [tek kişilik iki pencere manuel turu](MultiplayerTest.md); özellikle native W-basılı odak değişimi ve görsel titreme değerlendirilmeli.

## Aşama 3 — iki oyuncu ve ortak hurda

### Uygulama ve otorite

- Canlı Editor bağlantısı yeniden doğrulandı: HarborPrototype, gerçek HarborPrototypeGenerated/Harbor/StaticBoat, oyuncu ve üç hurda okundu; Console başlangıç durumu ayrıldı. Sahne/prefab/Inspector bağlantıları Editor üzerinden üretildi; ham YAML yazılmadı. SampleScene, mevcut liman geometrisi, offline oyuncu ve hurda prefablarının GUID'leri korundu.
- NGO 2.13.3 + Unity Transport kullanılıyor. UPM manifest isteği Transport 2.6.0; Unity 6000.5.6f1 bunu builtin 6.5.0 olarak çözüyor (lock ve canlı paket listesi). Unity/URP 17.5.0 ve ilgisiz paket sürümleri değiştirilmedi.
- Bağlantı paneli Host/Client/Kes/Devam, IPv4/port ve durum/hata mesajı içerir; varsayılan 127.0.0.1:7777. Rol sürekli görünür. Tab paneli açar ve imleci bırakır; Escape yalnızca imleci bırakır, panel kapalıyken tıklama tekrar kilitler. Panel açıkken oyun input'u kapalıdır. runInBackground=true ve 60 FPS hedefi iki pencere testini destekler. Odak kaybı action map'i kapatır, bekleyen hareket/eylem isteklerini temizler; ağ ve fizik durmaz.
- Network oyuncusu bağlantı onayıyla oluşturulur; en çok iki oyuncu ve farklı spawn'lar. Ağ prefabında motor/input/camera/AudioListener başlangıçta kapalıdır; yalnız owner spawn'ında açılır, remote oluşturulması yerel imleci etkilemez. Yerel HUD yalnızca owner'da; diğer oyuncu renkli kapsül ve bakış işaretidir. Offline akış bağlantı kurulana kadar bağımsız çalışır; kesilince tek oyuncu/üç yerel hurda geri açılır.
- Oyuncu hareketi **yerel CharacterController tahmini + server doğrulamalı pose yayınlama** modelidir. Konum/yaw/pitch en çok 20 Hz gönderilir; sender/ownership, finite değerler, alan/hız/delta sınırları ve remote server CharacterController çarpışması doğrulanır. Server pose'u diğer oyunculara interpolasyonla gösterilir. Tutma hedefi client tarafından ayrı bir eşya konumu olarak gönderilmez: kabul edilen oyuncu/kamera pose'undan server hold point üretir. Tam reconciliation, lag compensation ve güçlü anti-cheat değildir; host güvenilir kabul edilir, diğer oyuncuyla temas veya yüksek gecikmede düzeltme sıçraması olabilir.
- Üç NetworkScrap daima server ownership'inde kalır. Yalnız server Rigidbody/PhysicsCarry kuvveti ve ScrapItem kurtarması çalışır; client NetworkRigidbody kopyaları kinematic, kurtarma kapalıdır. Tut/bırak RPC'si sender, 2,5 m raycast/görüş, güncel pose, tek eşya/tek taşıyıcı ve 0,1 s pickup sıklığı kontrol eder. Pose güncellemeleri rate/hız/mesafe açısından sınırlıdır. Client konumu hurdaya doğrudan uygulanmaz. Accept sonrası NetworkVariable tutma durumu hareket katsayısını ve ağır koşu yasağını owner'a uygular; ret/drop/despawn geri yükler.
- Taşıyan oyuncu–eşya collision pair'i hem server'da hem owner client kopyasında geçici ignore edilir, önceki değer bırakma/despawn'da geri yüklenir. Çevre çarpışmaları açık kalır. R/düşmede server önce bırakır, sonra epoch'lu rescue ack ile oyuncuyu kendi spawn'ına döndürür; eski pose paketleri reddedilir. Client ayrılınca taşıma kaldırılır. Host migration yoktur.
- `SalvageCrew/Setup Multiplayer Prototype` iki kez çalıştırıldı: bir session, bir EventSystem, üç offline hurda, dört network prefab kaydı. Geometriyi yeniden kurmaz ve mevcut prefab ayarlarını korur. Tam Harbor kurulum aracı da ağ bağlantılarını yeniden bağlar; bu tam aracın kendi generated kökünü değiştirme uyarısı hâlâ geçerlidir.

### Gerçek doğrulama sonuçları

| Kontrol | Sonuç |
| --- | --- |
| C# derleme | Son recompile: 0 hata / 0 uyarı |
| macOS build | Development standalone başarılı; son build 11,777 s, 0 hata / 1 bilinen Pipeline uyarısı |
| İki gerçek instance | Editor HOST + çalıştırılan macOS CLIENT, 127.0.0.1; iki oyuncu/üç ortak NetworkObject, owner başına tek aktif kamera/input/motor |
| Hareket/bakış | Client yürüyüşü ve host bakışı karşı tarafta okundu; kamera/input remote'da kapalı |
| Hurda otoritesi | Host dynamic/recovery açık; client kinematic/recovery kapalı; bütün hurda ownership=0 |
| Host taşıması | Kasa iskele → rampa → güverte; client'taki konum aynı sonucu gösterdi; bırakılan kasa hız 0 ile yerleşti |
| Client taşıması | Hafif kutu aynı rotada host tarafından görüldü; motor/kasa kabul edildi; her iki oyuncu üç türü ayrı ayrı tuttu |
| Ağırlık | Kabulden sonra 0,95 / 0,75 / 0,50; motor koşusu false; bırakma/R sonrası 1/true |
| Meşgul eşya reddi | Host'un tuttuğu kasayı client istedi: “Başka bir oyuncu taşıyor”, kontrol değişmedi; bu test yarış diye sayılmadı |
| Gerçek eşzamanlı yarış | İki gerçek instance aynı boş kasaya planlanmış UTC ile RPC gönderdi. İlk tur 3 ms fark: host kazandı. Çarpışma düzeltmeli tur 8 ms fark: client kazandı, host reddedildi. Her tur tek taşıyıcı, server ownership korundu. Prefab başlangıç ayarı sonrası son smoke turu 74 ms farkla tek client taşıyıcı üretti; daha yakın yarış kanıtı ilk iki turdur |
| Taşıyan client ayrılması | Motor ve son build'de kasa serbest kaldı; client oyuncusu kaldırıldı, host simülasyonu devam etti |
| Geç katılma | Güvertedeki eşya konumları korundu; üç son reconnect turunda host'un tuttuğu kasa holder=0 doğru görüldü |
| Yeniden bağlanma | Üç son döngüde tam iki oyuncu/üç hurda; shutdown sonrası bir offline oyuncu/üç yerel hurda/tek session/tek EventSystem, network oyuncusu 0 |
| R ve oyuncu düşmesi | Client R tutmayı kaldırıp spawn'a döndü; host R motor/kasa tutmayı kaldırdı. Son client kasayı tutarken iskele kenarından gerçekten yürüyerek denize düştü; spawn (−0,9; 1,23; 4,9), tutma yok, normal hareket |
| Hurda deniz kurtarması | Server motoru deniz altı test fixture'ına aldı; gerçek FixedUpdate/1,5 s kurtarma sonrası başlangıca döndü, hız 0; client sonucu gördü |
| Host kapanışı | Client açık panel/SOLO moduna “Host bağlantısı kapandı… host shutting down” mesajıyla döndü; bilinçli kesme ayrı mesaj |
| Gecikme/paket kaybı | Gerçek loopback UDP proxy: **her yönde 100 ms (yaklaşık 200 ms ek RTT), %1 kayıp**. Son build client motoru rampadan güverteye taşıdı, 0 exception. Hareket sırasında gecikme görünür; durunca replica farkı≈0,0044 m |
| Offline regresyon | 12 motor/collider, 24 taşıma, 18 güvenlik/input, 7 WASD/Shift/düşme ve 5 panel/odak kontrolü geçti |
| Console | Son oyun oturumlarında iki probe da 0 error/exception; son cursor 73 sonrası yeni error/warning yok. Son derleme tek eski RuntimePipelineConfig uyarısıyla geçti; aradaki araç hataları aşağıda ayrı belirtilir |

Build: `Builds/Stage3/macOS/SalvageCrew.app` (Git dışında). [Tek kişilik test listesi](MultiplayerTest.md), [ölçümler ve iki instance snapshot'ları](Verification/Stage3Results.json), [son prefab/build smoke sonuçları](Verification/Stage3FinalSmoke.json), [host güvertede kasa](Screenshots/Stage3HostCarry.png). Son prefab ayarından sonra gerçek iki instance ile client motoru tekrar rampadan güverteye taşıdı, R/ayrılma/yeniden bağlantı/host kapanışı tekrar geçti; her iki hata sayısı 0. Eski sahne GameObject adları ve Transform konum/rotasyon/ölçek/parent alanları d6155e6 ile karşılaştırıldı: değişiklik yok. Strict proje/meta/GUID kontrolü 247 assette 0 hata/uyarı; prefab ve session Inspector referansları tamdır. Editor HarborPrototype açık/Play kapalı bırakıldı; test client/proxy süreçleri kapatıldı, runInBackground=true istenen proje ayarı olarak korundu. Ekran görüntüsü gerçek Game view/HUD'dan alındı ve incelendi; geçici Assets kopyası kaldırıldı. Host/client rol paneli native ekranlarda da görüldü. Test probe development-only ve opt-in'dir; normal açılışta etkinleşmez, release build'de bulunmaz. Hareket testleri gerçek motor adımını, tutma testleri gerçek RPC ve Rigidbody/FixedUpdate'ı kullanır; ağ simülasyonu taklit edilmedi.

### Denemeler, uyarılar ve sınırlar

- İlk NGO 2.7.0 denemesi bu Unity sürümünün EntityId API'siyle paket içinde 12 CS0619 ve geçici Burst assembly çözüm hatası üretti. Paket kaynaklarına yama yapılmadı; UPM'nin bu Editor için çözdüğü 2.13.3 seçildi ve yeniden derleme temiz geçti. Bu deneme hataları mevcut gameplay hatası diye gizlenmedi. En yeni registry sürümü değil, uyumlu çözülen sürüm kullanıldı.
- İlk standalone bağlantı denemesinde Editor kareleri ilerlemedi ve bağlantı timeout verdi. Auto-tick/gerçek kare ilerlemesi tekrar doğrulanıp düşük çözünürlüklü client ile test tamamlandı; ilk deneme başarı sayılmadı. Import/domain reload sırasında bazı CLI çağrıları geçici bağlantı hatası verdi; hazır durumdan sonra tekrarlandı. Son prefab kaydı/recompile kuyruğunda Pipeline 5/30 s timeout, HTTP connection reset ve headers-sent araç hataları da kaydedildi. Bu aralıktaki build “uncompiled code changes” uyarısı verdi; son teslim build'i için derleme up_to_date/Console temizliği doğrulanıp build tekrarlandı. Bunlar oyun exception'ı veya temiz test diye raporlanmadı.
- Gecikmeli ilk ağır taşıma turu başarısızdı: client oyuncu–kinematic hurda çarpışması kapalı değildi. Pair bazlı ignore/restorasyon eklendi; yeniden build ve aynı ağır rota geçti. Genel layer/pipeline ayarları değiştirilmedi.
- Server'daki remote oyuncuyu doğrudan deniz altına ışınlayan fixture, gelen client pose'u ile yarıştığı için spawn doğrulaması için yeterli değildi; başarı sayılmadı. Yerine client'ın gerçek motorla iskeleden yürüyerek düşmesi doğrulandı.
- Odak kaybı testi OnApplicationFocus callback'ini simüle edip gerçek Input System W state'iyle hareketin durduğunu ve panel engelini ölçtü. **Native pencere değişiminde fiziksel W basılı tutma ve fiziksel klavye/mouse ile tam network turu tamamlanmadı.** Ek network keyboard injection denemeleri unfocused Editor'da ilk pickup assertion'ını geçmedi; başarı sayılmadı. Native UI köprüsü sonradan “Sky Computer Use native pipe startup failed” verdi. Offline gerçek Input System E/Escape/click/R regresyonu geçti; kalan insan testi MultiplayerTest'tedir.
- NUnit EditMode süiti çalıştırılmadı. Gerçek ağ testleri canlı Play/standalone ve opt-in probe ile yapıldı. Yarış iki örnekle geçti; uzun süreli/stres, internet/WAN, Windows/Linux ve kötü niyetli client testi yapılmadı. Hareket doğrulaması prototip düzeyindedir; tam reconciliation/anti-cheat değildir.
- Pipeline RuntimePipelineConfig eksikliği eski araç uyarısıdır; Player'da Pipeline sunucusu bilerek kapalı kalır. Önceki immutable URP package uyarısına müdahale edilmedi. URP ve render assetlerinde değişiklik yok. Steam/Relay/Lobby/hesap, vinç, tekne hareketi, satış, düşman ve görsel iyileştirme eklenmedi.

Değişiklik grupları: HarborPrototype sahne bağlantıları/panel, dört network prefab + kayıt listesi ve geçici kapsül materyali, beş network/session/HUD/probe scripti ve Editor kurulum aracı; motor/input/carry/item'e sınırlı adaptörler; NGO/UTP manifest/lock; PlayerSettings pencere/arka plan; AGENTS/GameBrief/Progress ve test/kanıt belgeleri. Bütün Unity assetleri meta'larıyla tutulur; cache/build/log/probe çalışma dosyaları Git dışındadır. Sonraki aşamanın kapsamı kullanıcı tarafından ayrıca belirlenmelidir; önce manuel iki pencere listesini uygulamak önerilir.

## Aşama 2 — fizik tabanlı tutma, taşıma ve bırakma

- Canlı Editor bağlantısı yeniden doğrulandı: HarborPrototype açık, Play kapalı; Pier, BoardingRamp, Deck, Cabin, FirstPersonPlayer ve mevcut HUD gerçek Hierarchy'den okundu. Oyuncuda CharacterController, LocalPlayerInput ve FirstPersonMotor vardı. Unity 6000.5.6f1, URP 17.5.0 ve Input System 1.20.0 korundu; paket/pipeline ayarı değiştirilmedi.
- Prefablar: `LightBox` (3 kg), `MetalCrate` (12 kg), `ScrapEngine` (35 kg). Birer dinamik Rigidbody; basit kutu/compound collider'lar, sürekli çarpışma ve interpolasyon. İsim, kütle, yay/sönüm/kuvvet/hız sınırları, tutma mesafesi, hareket katsayısı, koşu ve kurtarma eşikleri Inspector'da düzenlenebilir. İskelede x=1,55; z=2/4/6; orta geçiş yolu ve rampa girişini kapatmazlar.
- `PhysicsCarry`: 2,5 m kamera raycast'i, tek eşya sahipliği, FixedUpdate'ta sınırlandırılmış PD kuvveti. Parent/Transform ışınlama veya kinematic taşıma yok. Yerçekimi desteği kuvvetle sağlanır; çevre collider'ları aktif kalır. Oyuncu/eşya collision pair'lerinin önceki ignore değerleri kaydedilip bırakınca geri yüklenir. Interpolation/collision detection ayarları da geri yüklenir; bırakma hızı en çok 2 m/s. Çok uzak eşya (3,2 m) veya 0,45 s boyunca engel arkasında kalan eşya bırakılır.
- `LocalCarryInteraction` yalnızca yerel örneği tut/bırak komutuna çevirir; PhysicsCarry Input System cihazı bilmez. E action'ı mevcut asset'e eklendi; imleç serbestken E saklanmaz/çalışmaz, yeniden kilitleyen tıklama etkileşimi tetiklemez. HUD hedef adı/kg ve E — Tut/Bırak gösterir.
- Motorun temel Inspector hızları değiştirilmedi. Geçici hareket katsayıları 0,95 / 0,75 / 0,50; yürüme 3,8 / 3,0 / 2,0 m/s. Ağır motor taşırken koşu kapalıdır. Drop, disable, R ve düşme kurtarması hareketi geri yükler. Oyuncu dönüşü öncesi tutma kaldırılır.
- `ScrapItem`: y<0,15 veya kendi başlangıcından >100 m uzaklıkta 1,5 s sonra kurtarır; tutmayı kaldırır, başlangıç konum/rotasyonuna döner, doğrusal/açısal hızı sıfırlar ve yerleşmek için fiziği uyandırır. Deniz hâlâ yalnızca görseldir.
- `SalvageCrew/Setup Scrap Carry Prototype` mevcut limanı yeniden yapmadan eksikleri ekler; iki çalıştırma sonrası üç eşya ve bir etkileşim HUD'u doğrulandı. Var olan hurda ayarları/instance'ları korunur. Tam Harbor kurulum komutu da bu ek kurulumu çağırır; tam komutun kendi kökünü yeniden üretme uyarısı geçerlidir.

### Aşama 2 gerçek doğrulama sonuçları

| Kontrol | Sonuç |
| --- | --- |
| Unity C# recompile | 0 hata, 0 uyarı |
| Inspector / prefab / input | Kamera, HUD, spawn bağlantıları; prefab kütleleri ve E binding'i okundu |
| Gerçek Play taşıma | 24 kontrol geçti; üç hurda iskele → rampa → güverte rotasından geçti |
| Bırakma / yerleşme | Üçü güvertede veya güvertedeki diğer hurda üzerinde kararlı durdu; hız 0 |
| Güvenlik ve input | 18 kontrol geçti: gerçek E olayları, hızlı E, HUD, Escape/tıklama, R ve oyuncu düşmesi |
| Ağırlık / ağır koşu | Son tur takip ilerlemesi 0,428 / 0,192 / 0,113 m; ağır motor + Shift gerçek hız≈2,0 m/s |
| Kabine bastırma | Kasa merkezinin en ileri z≈10,844; kabin önü 11,2; en yüksek hız≈2,33 m/s; oyuncu fırlamadı |
| Bırakma restorasyonu | Ignore collision ve farklı başlangıç interpolation/detection değerleri geri yüklendi |
| Güvenli bırakma / deniz | Uzaklık ve engel bırakması geçti; denize düşen motor başlangıca döndü ve hız≈0 |
| Aşama 1 regresyonu | 12 motor/fizik kontrolü ve WASD/Shift/otomatik kurtarma input kontrolleri tekrar geçti |
| Canlı kare ilerlemesi | wait_for: frame 4274→4281, met=true, timedOut=false |
| Console | Play aralığında yeni error/warning yok; build'de yalnızca bilinen RuntimePipelineConfig uyarısı tekrarlandı; compilationFailed=false |
| macOS build | Succeeded, 17,32 s, 0 hata / 1 bilinen araç uyarısı; `Builds/Stage2/macOS/SalvageCrew.app` |
| Proje bütünlüğü | Strict verify: 221 dosya, 0 hata / 0 uyarı; meta/GUID/manifest kontrolleri geçti |
| Son Editor durumu | HarborPrototype açık, Play kapalı, runInBackground=false |

Doğrulamalar canlı Unity Play'de gerçek Rigidbody/FixedUpdate, CharacterController ve sahne collider'larıyla yapıldı. Input testleri Input System'e gerçek keyboard/mouse state olayları gönderir; rota testleri normal motor adımını çağırır. Duvar, uzaklık, engel ve düşme uç durumlarında test fixture konumları oluşturulur; normal taşıma kodu eşyayı ışınlamaz. NUnit EditMode süiti, fiziksel klavye/mouse ile insan eliyle tam tur ve standalone uygulama çalıştırma testi yapılmadı. Sonuçlar ve tekrar çalıştırılabilir scriptler `Docs/Verification/Carry*` ve `Stage2RegressionResults.json` içindedir.

[Güvertede tutulan eşya ekran görüntüsü](Screenshots/CarryOnDeck.png) gerçek Game view/HUD'dan alındı ve görsel olarak incelendi. Yakalama aracının Assets altında oluşturduğu yalnızca bu göreve ait geçici kopya Editor AssetDatabase üzerinden silindi; Docs kopyası korunur.

### Aşama 2 deneme notları ve kalan kontroller

İlk taşıma turu geçti. Bir tekrar turunda test scriptinin ağır motor hedeflemesi başarısız oldu; sabit test fixture'ının yerleşmesi için bekleme eklendi, temiz Play oturumunda 24 kontrolün tamamı geçti. Bu aralıkta oyun Console exception'ı oluşmadı; test aracının başarısız assertion'ı başarı diye raporlanmadı. Kurtarma sırasında yüksek spawn'da uyuyan eşya olasılığı görülüp WakeUp ile giderildi ve deniz kurtarması tekrar geçti. Araç kuyruğu arkasında eşzamanlı Inspector sorgusu timeout verdi; script sonucu tamamlandıktan sonra seri sorgular kullanıldı. Recompile/import sırasında Play kapandığı için bir test çağrısı Play required döndü; hazır ve gerçekten Playing durumu doğrulanıp tekrar çalıştırıldı.

Değişiklikler: oyuncu prefabı/sahne/input asset'i, iki mevcut runtime scripti, beş yeni runtime scripti, üç hurda prefabı ve üç URP materyali, Editor ek kurulum scripti ve ana kurulum entegrasyonu; assetlerin meta'ları; AGENTS/GameBrief/Progress; doğrulama kodu/JSON ve screenshot. SampleScene, paket manifest/lock, URP pipeline assetleri ve mevcut liman geometrisi korunur. Unity sahneyi kaydederken Daylight'a varsayılan URP AdditionalLightData ekledi; ışık/renk/geometri ayarları değiştirilmedi. Türkçe HUD fallback'i kullanıldıktan sonra Unity/TMP mevcut LiberationSans SDF - Fallback asset'ini güncel serializer biçiminde kaydetti; font kaynağı ve fallback bağlantıları değiştirilmedi. Build çıktıları Git dışında, kaynak/meta ve Docs artefaktları commit kapsamındadır.

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

Manifest ve lock kaynak kontrolündedir. Şablon sürümleri korundu; Pipeline araç paketi ve Aşama 3 NGO/Transport eklendi.

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
| Netcode for GameObjects | 2.13.3 |
| Unity Transport | 6.5.0 builtin (manifest isteği 2.6.0) |

Unity modülleri manifestte listelenir. Multiplayer Center yalnız şablon araç paketidir; uygulanan ağ çözümü Aşama 3 NGO/Transport direct-IP'dir.

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
| 2 — Fizik tabanlı hurda taşıma | Yerel tutma, taşıma, bırakma ve kurtarma | Tamamlandı |
| 3 — İki oyuncu ve ortak hurda | Direct-IP host/client, oyuncu eşleme ve host fiziği | Uygulandı; gerçek iki instance çekirdek testleri geçti, manuel kullanıcı turu bekliyor |
| 4A — Ortak tekne ve hareketli güverte | Host otoriteli sürüş, yolcu, yük ve kurtarma | Uygulandı; gerçek iki instance normal/gecikmeli çekirdek testler geçti, native manuel tur bekliyor |
| Sonraki — Hurda ve vinç | Çıkarma ve güverteye yükleme | Başlanmadı |
| Sonraki — Liman ve ekonomi | Satış ve basit ekipman geliştirmesi | Başlanmadı |
| Sonraki — Doğrulama | İki oyuncuyla tam döngü, ardından 1–4 oyuncu | Başlanmadı |

Her görev yalnızca açıkça istenen aşamayı uygular. Aşama 1'de tekne statik makettir; su/tekne fiziği, eşya taşıma, vinç, satış, düşman ve multiplayer eklenmedi. Şablonun SampleScene, render ayarları ve Readme araç kodu korundu.

## Kısa manuel test

1. Unity Hub'dan bu proje kökünü Unity 6000.5.6f1 ile aç.
2. Assets/_Game/Scenes/HarborPrototype.unity sahnesini aç, Play'e gir ve Game view'a tıkla.
3. WASD/Left Shift/Space ile iskelede yürü, koş ve zıpla; rampadan güverteye geç. Korkuluk ve kabine yürüyerek çarpışmayı kontrol et.
4. Mouse ile bak; yukarı/aşağı sınırını dene. Escape ile imleci bırak, Game view'a tıklayıp yeniden kilitle. R ile iskeleye dön; iskele kenarından denize düşüp otomatik kurtarmayı kontrol et.
5. HUD ve Console'u kontrol et; Play'den çık. İsteğe bağlı olarak Builds/HarborPrototype/macOS/SalvageCrew.app çıktısını aç; standalone çalıştırma ayrıca doğrulanmadı.
6. İskelede sağ taraftaki üç hurdaya yaklaş/bak. E ile sırayla tut; rampadan güverteye taşı ve E ile bırak. İsim/kg ve Tut/Bırak bilgisi, ağırlık farkı ve motor taşırken koşmanın kapanmasını kontrol et.
7. Kasayı kabine/korkuluğa bastır; geçmediğini ve fırlamadığını kontrol et. E'ye hızlı bas; tutarken Escape, serbest imleçte E, tıklama, R ve denize düşmeyi dene. Bırakınca normal hız geri gelmeli.
8. Hurdayı denize düşür; yaklaşık 1,5 s sonra iskelede kendi başlangıcına dönmesini kontrol et. Console'da yeni error/exception olmamalı.

## Kalanlar ve sonraki aşama

Güncel durum: Aşama 4A tekne sürüşü/hareketli güverte ve gerçek iki instance sonuçları yukarıdadır; kapsam genişletmeden önce manuel iki pencere turu önerilir. Ticari platformlar ve internet oturum keşfi henüz belirlenmedi. 6000.5.6f1 korundu; LTS geçişi ve Windows/Linux build yapılmadı. Pipeline experimental'dır. Vinç, fırlatma, satış, düşman eklenmedi.

Console geçmişinde URP Core paketinin `RuntimeDebugWindow_PanelSettings.asset` dosyasının immutable package içinde değiştiği uyarısı da görüldü. Paket kaynakları elle değiştirilmedi; bu uyarı package cache/import sırasında ortaya çıktı. Kaynak kontrolüne dahil olmayan Library/PackageCache içindedir; kök nedeni bu hazırlıkta giderilmedi. Build başarılı ve son Console kontrolünde compile hatası yok; ileride paket importunda yeniden kontrol edilmeli.

İlk CLI ile GUI açılışı EPIPE ile sonlandı; Editor doğrudan resmi executable üzerinden açıldı ve bağlantı kuruldu. İlk import sırasında bir Pipeline çağrısı 5 saniyede timeout verdi; sonrasında auto-tick/odak ve import tamamlanmasıyla canlı okuma ve Play testi başarılı oldu. Bu eski araç hatası yeni runtime hatası diye değerlendirilmedi.

Geçici şablon oluşturma kopyası /tmp/salvagecrew-bootstrap.eezB7t/SalvageCrew altında durur; asıl proje kökü değildir. Unity Hub kaydı asıl köke taşındı; Library önbelleği yeniden import maliyetini azaltmak için asıl köke aktarıldı. Geçici kopya otomatik silinmedi.
