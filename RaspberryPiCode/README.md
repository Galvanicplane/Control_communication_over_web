# Raspberry Pi Kurulum Notları

Bu klasördeki `robot_main.py` dosyası, Raspberry Pi 5 üzerinde çalışarak web sitesinden gelen komutları okur ve bağlı olan servo motoru hareket ettirir.

## Gereksinimler

Raspberry Pi üzerinde aşağıdaki paketlerin kurulu olması gerekir:

```bash
# Python kütüphanelerini yükleyin
pip install requests gpiozero lgpio
```

## Bağlantı Şeması (SG90 Servo)

*   **Kahverengi Kablo:** Raspberry Pi GND (Pin 6)
*   **Kırmızı Kablo:** Raspberry Pi 5V (Pin 2 veya 4)
*   **Turuncu(Data) Kablo:** Raspberry Pi GPIO 18 (Pin 12)

## Çalıştırma

Terminalden şu komutu vererek başlatabilirsiniz:

```bash
python robot_main.py
```

**Not:** Script, `https://erenarslan.online` adresine bağlanacak şekilde ayarlanmıştır. Eğer localde test ediyorsanız script içindeki `API_URL` satırını güncelleyin.
