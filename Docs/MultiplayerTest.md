# Aynı bilgisayarda iki oyuncu testi

Unity 6000.5.6f1 ile HarborPrototype'ı aç. Güncel macOS client build'i: `Builds/Stage4A/macOS/SalvageCrew.app`. Build almak bağlantı testinin yerine geçmez; iki pencere de çalışmalı.

## Aşama 4A — tek kişi, iki pencere

1. Editor Play → Host başlat; standalone → Client bağlan (`127.0.0.1:7777`). HOST/CLIENT etiketleri görünmeli. İki oyuncuyu ve üç hurdayı **kalkıştan önce** rampadan güverteye çıkar. Kabin önündeki küçük gri blok dümen noktasıdır.
2. Host dümen bloğuna bakıp E: HUD dümen/WASD/E göstermeli. W/S ileri/geri, A/D dönüş; normal yürüme ve Space kapalı. E ile bırakınca yürüme geri gelmeli. Client ile aynı işlemi dene.
3. Host düz sürüş/dönüş/yavaşlama/geri hareket yapar, Tab ile panel açıp client penceresine geçer. Client güvertede taşınmalı, yürüyüp zıplayıp inebilmeli. Sonra rolleri değiştir. Kamera titremesi, kayma ve geri çekilme gözlemini ayrı kaydet.
4. **Tek kişi sınırı:** Odak/panel kaybında motor girdisi sıfırlanır; pencere değişiminde tekne yalnızca ataletiyle süzülür. Bu elle tur, eşzamanlı sürekli sürüş/yürüme değildir. Sürekli sürüş + yolcu hareketi gerçek iki instance'ta development probe ile otomatik denendi; oyuna otomatik seyir eklenmedi.
5. Üç hurda güvertedeyken sür/dön/yavaşla; durunca konumlarını gözle. Serbest hurda dinamik kalır, güverteye sabitlenmez. Fiziksel yükler oyuncuyu engelleyebilir; dümen önünü kapatma.
6. Host sürerken client üç türü sırayla E ile tutup kısa yürür/bırakır; host'ta gözle. Rolleri ters çevir. Ağır motor koşuyu kapatmalı. Elde hurda varken dümeni hedefle/E: önce hurdayı bırakma açıklaması ve kullanım reddi beklenir.
7. Client dümeni kullanırken host almaya çalışır: meşgul açıklaması, sahip değişmemeli. Bu sıralı ret testi, eşzamanlı yarış değildir. Client Tab → Kes: driver boş, motor girdisi sıfır, tekne dirençle yavaşlamalı.
8. Hareketli teknede hurda tutarken R; sonra açık borda girişinden yürüyerek suya düşme. Hurda önce bırakılmalı; oyuncu **güncel kıç güvertesine** dönmeli. Dümen sahibinde R kontrolü bırakmalı. Güverteye yüklenmiş hurdayı denize düşür: yaklaşık 1,5 s sonra server güncel güvertede boş alana döndürmeli; eski dünya konumuna dönmemeli.
9. Kalkışta rampa ve iki kılavuz devre dışıdır; görünmez iskele–tekne collider köprüsü kalmamalı. Yanaşınca rampa otomatik açılmaz; yanaşma bu aşamada yoktur. Yeni oturum/offline dönüş rampayı açar. Geç katılan client kalkmış teknenin güvenli güvertesinde doğmalı.
10. Native odak: W basılıyken diğer pencereye geç; eski karakter/motor input'u durmalı, ağ ve simülasyon devam etmeli. **Bu gerçek OS odak testi native araç köprüsü hatası nedeniyle otomatik doğrulanmadı**; simüle callback testi yerine geçmez.
11. Normal 7777 turundan sonra aşağıdaki proxy ile 100 ms/yön ve %1 kayıp turunu yap; yalnız client portu 7778 olur. Sürüş/yolcu, zıplama, üç hurda, ayrılma ve kurtarmayı tekrarla. Yaklaşık 200 ms ek RTT'dir, 100 ms RTT değildir.
12. Host kapanışında client anlaşılır mesajla panel/SOLO'ya dönmeli. Üç bağlan/kes döngüsünde tek tekne, üç hurda, iki oyuncu beklenir. SOLO'da eski sabit tekne/rampa/hareket/taşıma/Escape/tıklama/R/deniz kurtarmasını tekrar et; Console ve standalone logunu kontrol et.

Güncel gerçek sonuçlar Progress ve `Verification/Stage4AResults.json` içinde; aşağıdaki liste Aşama 3 akışını da korur.

## Başlatma

1. Editor'da Play'e gir. Bağlantı panelinde adres `127.0.0.1`, port `7777`; **Host başlat**. Ekranın sağ üstünde HOST görünmeli.
2. macOS uygulamasını aç. Aynı adres/port ile **Client bağlan**. CLIENT görünmeli; iki pencerede de bir yerel kamera ve bir diğer oyuncu kapsülü olmalı.
3. **Oyuna / Solo devam** paneli kapatıp imleci kilitler. Tab paneli açar ve imleci bırakır. Escape yalnızca imleci serbest bırakır; panel kapalıyken Game view tıklaması tekrar kilitler ve eşya eylemi tetiklemez. Panel açıkken WASD/E/R çalışmaz. Pencere odağı kaybolunca hareket input'u sıfırlanır; ağ ve host fiziği arka planda devam eder.
4. İki pencereyi yan yana yerleştir. Rol yazıları sürekli görünür. Test için her seferinde yalnızca gözlenecek/kontrol edilecek pencereye odaklan.

## Tek kişinin sırayla yapacağı kontroller

1. Host'ta WASD/mouse/zıplama ile dolaş. Client'ta mavi kapsülün konumu ve bakış işaretini gözle. Client'ı hareket ettir; host'ta turuncu kapsülü gözle. Diğer pencereye geçince eski karakter yürümeyi sürdürmemeli.
2. Host ile 12 kg metal kasayı E ile tut, rampadan tekneye taşı. Tab ile paneli aç ve client'a geç. Aynı kasa aynı konumda tutuluyor görünmeli; client HUD'unda hedeflenince **Başka bir oyuncu taşıyor** yazmalı.
3. Client ile host'un tuttuğu kasayı almaya çalış. Kontrol client'a geçmemeli; host taşıması sürmeli. Bu, **meşgul eşya reddi** testidir; gerçek eşzamanlı yarış testi değildir.
4. Host kasayı güverteye bırakır. Client başka bir hurdayı E ile tutup taşır; host penceresine geçerek aynı eşyanın hareketini gözle. Hafif/orta/ağır eşyaları sırayla her iki oyuncuyla dene; ağır motor taşırken Shift koşusu kapalı olmalı.
5. Client eşya tutarken Tab → **Bağlantıyı kes**. Host'ta eşya serbest kalıp düşmeli ve client kapsülü kaldırılmalı. Host oyunu/simülasyonu sürmeli.
6. Client yeniden bağlansın. Tam iki oyuncu ve üç ortak hurda olmalı; güvertedeki kasa başlangıca dönmemeli. Host'un hâlen tuttuğu eşya varsa tutma durumu geç katılan client'ta doğru görünmeli.
7. Her oyuncu bir eşya tutarken R'yi dene; eşya önce serbest kalmalı, oyuncu ağ modunda güncel güvenli tekne güvertesine (SOLO'da iskele başlangıcına) dönmeli ve normal hareket geri gelmeli. Oyuncunun denize düşmesini de dene. Hurdayı denize bırak; yaklaşık 1,5 saniyede server, daha önce güverteye yüklenmişse güncel tekneye, yüklenmemişse iskele başlangıcına döndürmeli; iki pencerede sonuç aynı olmalı.
8. Client bağlıyken host **Bağlantıyı kes** yapar veya Editor Play'i durdurur. Client bağlantı kaybını anlaşılır mesajla göstermeli ve panel açık tek oyuncu moduna dönmeli. Host migration yoktur.
9. Host/client başlat → kes → yeniden bağlan döngüsünü üç kez yap. Bir oyuncu başına tek karakter, instance başına tek bağlantı paneli/yerel HUD ve tam üç ortak hurda beklenir.
10. Bağlantı başlatmadan **Oyuna / Solo devam** seçerek Aşama 1/2 yürüme, taşıma, Escape/tıklama, R ve kurtarma testlerini tekrar yap.

## Eşzamanlı yarış ve gecikme

Gerçek yarış için aynı boş eşyayı menzil/görüş içinden **aynı zaman aralığında** isteyen iki gerçek instance gerekir. Tek kişinin sırayla E basması bu kriteri doğrulamaz. Otomatik test sonucu Progress'te ayrıca belirtilir.

Development build'de opt-in `--salvage-probe <dizin>` yalnızca yerel doğrulama için command/state JSON köprüsünü açar; normal açılışta çalışmaz, release build'e dahil edilmez. Test probe hareketi gerçek CharacterController adımı, pickup ise gerçek NGO RPC üzerinden yapar; fizik simülasyonunu taklit etmez.

`Docs/Verification/LatencyProxy.py` isteğe bağlı, loopback UDP köprüsüdür. Port 7778'den client paketlerini host'un 7777 portuna iletir; iki yönde 100 ms gecikme ve %1 paket kaybı uygulanabilir. Bu proxy kullanılırken client portu 7778 olur. Normal testte doğrudan 7777 kullanılmalı. Test sonuçları ve doğrulanmamış sınırlamalar `Docs/Progress.md` içindedir.
