# Aynı bilgisayarda iki oyuncu testi

Unity 6000.5.6f1 ile HarborPrototype'ı aç. macOS client build'i: `Builds/Stage3/macOS/SalvageCrew.app`. Build almak bağlantı testinin yerine geçmez; iki pencere de çalışmalı.

## Başlatma

1. Editor'da Play'e gir. Bağlantı panelinde adres `127.0.0.1`, port `7777`; **Host başlat**. Ekranın sağ üstünde HOST görünmeli.
2. macOS uygulamasını aç. Aynı adres/port ile **Client bağlan**. CLIENT görünmeli; iki pencerede de bir yerel kamera ve bir diğer oyuncu kapsülü olmalı.
3. **Oyuna / Solo devam** paneli kapatıp imleci kilitler. Tab paneli açar ve imleci bırakır. Escape yalnızca imleci serbest bırakır; panel kapalıyken Game view tıklaması tekrar kilitler ve eşya eylemi tetiklemez. Panel açıkken WASD/E/R çalışmaz. Pencere odağı kaybolunca hareket input'u sıfırlanır; ağ ve host fiziği arka planda devam eder.
4. İki pencereyi yan yana yerleştir. Rol yazıları sürekli görünür. Test için her seferinde yalnızca gözlenecek/kontrol edilecek pencereye odaklan.

## Tek kişinin sırayla yapacağı kontroller

1. Host'ta WASD/mouse/zıplama ile dolaş. Client'ta mavi kapsülün konumu ve bakış işaretini gözle. Client'ı hareket ettir; host'ta turuncu kapsülü gözle. Diğer pencereye geçince eski karakter yürümeyi sürdürmemeli.
2. Host ile 12 kg metal kasayı E ile tut, rampadan tekneye taşı. Escape ile paneli aç ve client'a geç. Aynı kasa aynı konumda tutuluyor görünmeli; client HUD'unda hedeflenince **Başka bir oyuncu taşıyor** yazmalı.
3. Client ile host'un tuttuğu kasayı almaya çalış. Kontrol client'a geçmemeli; host taşıması sürmeli. Bu, **meşgul eşya reddi** testidir; gerçek eşzamanlı yarış testi değildir.
4. Host kasayı güverteye bırakır. Client başka bir hurdayı E ile tutup taşır; host penceresine geçerek aynı eşyanın hareketini gözle. Hafif/orta/ağır eşyaları sırayla her iki oyuncuyla dene; ağır motor taşırken Shift koşusu kapalı olmalı.
5. Client eşya tutarken Tab → **Bağlantıyı kes**. Host'ta eşya serbest kalıp düşmeli ve client kapsülü kaldırılmalı. Host oyunu/simülasyonu sürmeli.
6. Client yeniden bağlansın. Tam iki oyuncu ve üç ortak hurda olmalı; güvertedeki kasa başlangıca dönmemeli. Host'un hâlen tuttuğu eşya varsa tutma durumu geç katılan client'ta doğru görünmeli.
7. Her oyuncu bir eşya tutarken R'yi dene; eşya önce serbest kalmalı, oyuncu kendi başlangıcına dönmeli ve normal hareket geri gelmeli. Oyuncunun denize düşmesini de dene. Hurdayı denize bırak; yaklaşık 1,5 saniyede server tarafından başlangıca dönmeli, iki pencerede de sonuç aynı olmalı.
8. Client bağlıyken host **Bağlantıyı kes** yapar veya Editor Play'i durdurur. Client bağlantı kaybını anlaşılır mesajla göstermeli ve panel açık tek oyuncu moduna dönmeli. Host migration yoktur.
9. Host/client başlat → kes → yeniden bağlan döngüsünü üç kez yap. Bir oyuncu başına tek karakter, instance başına tek bağlantı paneli/yerel HUD ve tam üç ortak hurda beklenir.
10. Bağlantı başlatmadan **Oyuna / Solo devam** seçerek Aşama 1/2 yürüme, taşıma, Escape/tıklama, R ve kurtarma testlerini tekrar yap.

## Eşzamanlı yarış ve gecikme

Gerçek yarış için aynı boş eşyayı menzil/görüş içinden **aynı zaman aralığında** isteyen iki gerçek instance gerekir. Tek kişinin sırayla E basması bu kriteri doğrulamaz. Otomatik test sonucu Progress'te ayrıca belirtilir.

Development build'de opt-in `--salvage-probe <dizin>` yalnızca yerel doğrulama için command/state JSON köprüsünü açar; normal açılışta çalışmaz, release build'e dahil edilmez. Test probe hareketi gerçek CharacterController adımı, pickup ise gerçek NGO RPC üzerinden yapar; fizik simülasyonunu taklit etmez.

`Docs/Verification/LatencyProxy.py` isteğe bağlı, loopback UDP köprüsüdür. Port 7778'den client paketlerini host'un 7777 portuna iletir; iki yönde 100 ms gecikme ve %1 paket kaybı uygulanabilir. Bu proxy kullanılırken client portu 7778 olur. Normal testte doğrudan 7777 kullanılmalı. Test sonuçları ve doğrulanmamış sınırlamalar `Docs/Progress.md` içindedir.
