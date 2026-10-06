# Enkaz seferi — tek kişi / iki pencere

## Oynama

1. `HarborPrototype` → Play. Panelde **Oyuna devam et**, iskeleden tekneye geç.
2. Kabindeki gerçek ahşap dümeni hedefle: **E** kullan/bırak; **W/S** ileri/geri, **A/D** dönüş. Hurda tutarken dümen reddedilir. Tekne kalkınca liman rampası kapanır.
3. Teknedeyken **F** sonar. Menzil 60 m, tekrar tarama 5 sn. Ekrandaki yön **teknenin burnuna göredir**, kameraya göre değil. İskelede, açık panelde veya serbest imleçte F etkileşimi yok.
4. Kırık Direk Enkazı'na yaklaş. Yavaşla; teknenin **sol tarafındaki açık arka güverteyi** enkazın yüklerine paralel getir. İlk enkaz için örnek tekne merkezi x≈24.5,z≈43.6, burun +Z; bunlar oynanış gereksinimi değil test hizasıdır. Kabini/kıç korkuluğunu yükün karşısına getirme.
5. Dümeni bırak. Yüke 2.5 m içinde bak, **E** tut. Bakışı biraz kaldırarak bordayı aşır, güverte merkezine geri adım at, **E** bırak. 3/12/35 kg farkını kontrol et. Sıkışınca güvenli bırakma çalışır; yeniden hizalanıp al.
6. Üç **enkaz yükünü** arka güvertede bırakıp 2 sn bekle. İskele eğitim hurdaları sayılmaz. Sayaç 3/3 ve limana dönüş yönü görünmeli. Elde tutulan, borda dışında veya yeniden düşen yük sayılmaz.
7. Liman dönüş işaretine 12 m içinde yaklaş; düşük hızda dur. Yükler hâlâ güvertedeyse **Sefer tamamlandı**. Ödeme/satış sistemi yok; sefer oturumluk, yeni Play/oturumda sıfırlanır.
8. Henüz alınmamış bir yükü denize düşür: kendi enkazına dönmeli. Tekneye taşındıktan sonra suya düşen yük mevcut tekne kurtarmasını kullanır. R oyuncuyu kurtarır ve elindeki yükü bırakır.

## Editor host + macOS client

Build: `Builds/WreckSearch/macOS/SalvageCrew.app` (Development). Editor'da Host, uygulamada `127.0.0.1:7777` Client. TAB paneli açarak pencereler arasında geç. Arka planda simülasyon çalışır.

1. Host teknede F ile tara; client'ta aynı keşfi gör. Host taradıktan **sonra** client bağlayarak geç katılmayı da dene.
2. Client dümeni kullanıp sürsün; host'ta tekne/dümen dönüşünü ve güverte takibini gözle. E ile bırakıp rolleri değiştir.
3. Host enkaz kutusunu tutsun, client'ta hareketini gözle; bırakınca client metal kasayı alsın ve host'ta gözle.
4. Host'un tuttuğu aynı yükü client almaya çalışsın; çift kontrol olmamalı. Bu sırayla test gerçek eşzamanlı yarış değildir.
5. Client yük tutarken TAB → Bağlantıyı kes. Host'ta yük serbest kalmalı. Yeniden bağlan; ekstra tekne/yük/HUD olmamalı, keşif kaybolmamalı.
6. Üç yükü güverteye bırak; iki ekranda da sayaç ve limana dönünce sefer tamamlandı durumu aynı olmalı.
7. Host kapansın; client anlaşılır bağlantı mesajıyla offline panele dönmeli.

## Bu görevde yapılan / yapılmayan

- Derleme ve gerçek macOS build başarılı. Gerçek ilerleyen Editor Play'de kural testleri ve ayrı fizik taşıma/sürüş kontrolleri yapıldı.
- İki gerçek process arasında keşif, client tarama, server fizik/client kinematic, client taşıma, ayrılma ve yeniden bağlantı testleri yapıldı. `Verification/WreckNetworkResults.json` son gerçek sonuçtur.
- Otomasyon bazı konumları hazırlar; network taşıma testi hedefi deterministik seçmek için test oturumunda diğer yükleri kaldırır. Bu, teslim edilen sahneden içerik silmez ve baştan sona native oynanış kanıtı değildir.
- Native odak, elle F/E tıklama hissi, uzun sefer, yeni gecikme/paket kaybı ve eşzamanlı yarış bu turda ayrıca doğrulanmadı.
- Henüz sualtına yüzme/dalış, enkaz içine girme, vinç, satış, kayıt veya rastgele enkaz üretimi yok.
