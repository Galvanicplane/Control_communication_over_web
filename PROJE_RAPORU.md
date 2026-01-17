# ControlOverWeb: Web Tabanlı Gerçek Zamanlı Robotik Kontrol Sistemi
**IEEE Standartlarına Uygun Teknik Proje Raporu**

**Proje Sahibi:** Eren Arslan
**Tarih:** 17 Ocak 2026
**Sürüm:** 2.0

---

## 1. Özet (Abstract)
ControlOverWeb, coğrafi sınırları ortadan kaldırarak robotik sistemlerin internet üzerinden düşük gecikme (low-latency) ile kontrol edilmesini sağlayan bir IoT (Nesnelerin İnterneti) platformudur. Bu proje, ASP.NET Core tabanlı sağlam bir arka uç (backend) mimarisi ile modern web teknolojilerini birleştirerek, operatörlerin (Sender) ve robotların (Receiver/Robot) güvenli bir şekilde haberleşmesini sağlar. Sistem; kullanıcı doğrulama, gerçek zamanlı komut iletimi, detaylı loglama ve istatistiksel veri analizi yeteneklerine sahiptir.

## 2. Giriş (Introduction)
Geleneksel uzaktan kontrol sistemleri (RC), radyo frekanslarının fiziksel menzili ile sınırlıdır. Endüstri 4.0 ve IoT devrimi ile birlikte, bu sistemlerin internet üzerinden global ölçekte yönetilmesi bir gereklilik haline gelmiştir. Bu proje, HTTP/REST protokolleri üzerinden güvenli ve ölçeklenebilir bir komuta-kontrol altyapısı sunarak bu sorunu çözer. Proje ayrıca, operatörlere anlık görsel geri bildirim (G-Force Visualization) sunarak uzaktan kontrol deneyimini (UX) iyileştirmeyi hedefler.

---

## 3. Sistem Mimarisi ve Dosya Yapısı (System Architecture)

Proje, Katmanlı Mimari (Layered Architecture) prensiplerine uygun olarak geliştirilmiştir. Aşağıda projenin temel bileşenleri ve dosya yapısının detaylı analizi yer almaktadır.

### 3.1. Arka Uç (Backend: Controllers)
Sunucu tarafı mantığı `Controllers` klasörü altında yönetilir. Bu katman, istemcilerden gelen istekleri işler, veritabanı ile konuşur ve yanıt döner.

#### **3.1.1. SenderController.cs (Operatör Yönetimi)**
Operatörlerin (insan kullanıcıların) sistemle etkileşimini yöneten ana denetleyicidir.
*   `GetReceivers()`: Aktif ve çevrimiçi robotları listeler. Sistem kaynaklarını korumak için, son 2 dakika içinde sinyal göndermeyen (heartbeat almayan) "ölü" robotları otomatik olarak veritabanından temizler.
*   `Connect(ConnectRequest)`: Operatörün bir robota bağlanma isteğini işler. Robotun belirlediği oturum şifresini doğrular ve güvenli bir oturum başlatarak `ConnectionHistory` tablosuna kayıt açar.
*   `SendCommand(CommandRequest)`: Operatörden gelen hareket komutlarını (W, A, S, D veya Vektör verisi) alır, veritabanına yazar ve istatistik servisini tetikler.
*   `Disconnect(DisconnectRequest)`: Bağlantıyı güvenli bir şekilde sonlandırır ve oturum süresini veritabanına işler.

#### **3.1.2. RobotController.cs (Robot/Alıcı Yönetimi)**
Sahadaki fiziksel cihazların (Raspberry Pi, ESP32 vb.) veya simülasyonların API ile konuştuğu noktadır.
*   `Register(RegisterRequest)`: Robotun sisteme giriş yapmasını sağlar (Handshake). Eğer robot zaten kayıtlıysa, oturum bilgilerini günceller ve durumu "Online" yapar.
*   `PollCommands(int id)`: Robotun sürekli olarak sunucuya "Bana yeni bir emir var mı?" diye sorduğu (Polling) fonksiyondur. Yeni emir varsa bunları JSON formatında robota iletir ve `IsExecuted` (Uygulandı) olarak işaretler. Bu fonksiyon aynı zamanda robotun hayatta olduğunu (Heartbeat) sisteme bildirir.

#### **3.1.3. StatsController.cs (İstatistik Servisi)**
Sistemin genel sağlığı ve kullanım verilerini izler.
*   `GetStats()`: Toplam ziyaretçi sayısını, bugüne kadar gönderilen toplam komut sayısını ve anlık aktif robot sayısını raporlar.
*   `RecordVisit()`: Siteye yapılan her ziyareti sayaçta bir artırır.

### 3.2. Veri Modelleri (Models)
Veritabanı şemasının nesne tabanlı karşılıklarıdır (ORM - Entity Framework).

*   **`Command.cs`**: Sistemin en kritik veri yapıtaşıdır. Bir komutun kimden, kime, ne zaman ve hangi kodla (örn: "W", "V:0.5,1.0") gittiğini tutar.
*   **`Receiver.cs`**: Robot kimlik kartıdır. Robotun adı, türü (Drone, Rover vb.), anlık durumu ve oturum şifresini saklar.
*   **`Sender.cs`**: Sistemi kullanan operatörlerin geçici kimliklerini tutar.
*   **`ConnectionHistory.cs`**: Denetim izi (Audit Trail) için kullanılır. Hangi operatörün hangi robotu ne kadar süre yönettiğini saniye hassasiyetinde saklar.
*   **`SiteStats.cs`**: Singleton (tek kayıt) mantığıyla çalışan, sistemin kümülatif sayaçlarını tutan tablodur.

### 3.3. Ön Yüz (Frontend)
Kullanıcı deneyimi `wwwroot` klasörü altındaki statik dosyalarla sağlanır.

*   **`sender.html` (Kontrol Kokpiti):** Operatörün kullandığı ana arayüzdür.
    *   *İşlevi:* Aktif robotları listeler, şifreli bağlantı arayüzü sunar ve klavye/joystick girdilerini yakalayarak API'ye iletir.
    *   *Görselleştirme:* `viz.js` kütüphanesini kullanarak gönderilen komutun şiddetini ve yönünü grafiksel olarak gösterir.
*   **`receiver.html` (Robot Simülatörü):** Fiziksel bir robotun davranışını tarayıcıda taklit eder.
    *   *İşlevi:* Sunucudan gelen komutları anlık olarak dinler ve ekrana yansıtır. Geliştirme ve test aşamalarında fiziksel donanım gereksinimini ortadan kaldırır.

---

## 4. Veritabanı Mimarisi (Database Design)
Sistem, veri tutarlılığını (ACID prensipleri) sağlamak amacıyla İlişkisel Veritabanı Yönetim Sistemi (RDBMS) üzerine kurulmuştur. Toplam 7 adet normalize edilmiş tablo bulunur:

1.  **Receivers:** Robot envanteri ve durum bilgisi.
2.  **Senders:** Aktif operatör oturumları.
3.  **Commands:** İşlem kuyruğu ve geçmiş komut arşivi.
4.  **Logs:** Sistem olayları ve hata kayıtları.
5.  **AdminUsers:** Yönetim paneli yetkilendirmesi.
6.  **SiteStats:** Kümülatif sistem metrikleri.
7.  **ConnectionHistory:** Bağlantı ve oturum logları.

---

## 5. Çalışma Prensibi ve Veri Akışı
1.  **Başlatma (Initialization):** Robot (fiziksel veya sanal), `/api/robot/register` adresine POST isteği atarak oturum açar.
2.  **Keşif (Discovery):** Operatör web arayüzüne girer, `/api/sender/receivers` isteği ile online robotları görür.
3.  **Bağlantı (Handshake):** Operatör bir robot seçer ve şifreyi girer. Doğrulama başarılı ise sunucu iki tarafı eşleştirir.
4.  **Kontrol Döngüsü (Control Loop):**
    *   Operatör 'W' tuşuna basar.
    *   JavaScript, bu girdiyi yakalar ve `/api/sender/command` adresine POST eder.
    *   Sunucu komutu veritabanına yazar.
    *   Robot, `/api/robot/poll` isteği ile bekleyen komutu çeker.
    *   Robot motorlarını hareket ettirir.

---

## 6. Sonuç
Bu proje, modern web mimarilerinin robotik kontrol sistemlerine entegrasyonu konusunda başarılı bir örnek teşkil etmektedir. Özellikle 7 tablolu veritabanı yapısı, ayrık (decoupled) servis mimarisi ve kullanıcı dostu arayüzü ile hem eğitim hem de endüstriyel prototipleme alanlarında kullanılabilir bir altyapı sunmaktadır.
