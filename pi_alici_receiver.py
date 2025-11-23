import requests
import json
import time

# --- AYARLAR ---
KENDI_CIHAZ_ID = 1 
API_BASE_URL = "https://api.erenarslan.online/api/Komut" 
HEADERS = {'Content-Type': 'application/json'}

# Çekme frekansı: 0.25 saniye (Saniyede 4 kez)
CEKME_ARALIGI = 0.25 
# -----------------

def komutlari_cek_ve_isaretle():
    
    
   
    GET_URL = f"{API_BASE_URL}/al/{KENDI_CIHAZ_ID}"
    
    print(f"\rKomut çekiliyor ", end="")
    
    try:
        response = requests.get(GET_URL, timeout=5)   #ana get kodu
        
        if response.status_code == 200:     #basarili

            gelen_komut = response.json()
            
            komut_id = gelen_komut.get('komutID', 'Yok')
            
            komut_icerigi_str = gelen_komut.get('komutIcerigi', '{}')
            
            try:
                komut_icerigi_data = json.loads(komut_icerigi_str)
                
                komut_adi = komut_icerigi_data.get('komut_adi', 'HATA_ADI') 
                deger = komut_icerigi_data.get('deger', 'HATA_DEGER') 
                
            except json.JSONDecodeError:
                komut_adi = "HATA (JSON decode)"
                deger = komut_icerigi_str
            except Exception:
                komut_adi = "HATA (JSON exception)"
                deger = komut_icerigi_str
                
            print("\n--- YENİ KOMUT ALINDI! ---")
            print(f"|-> [ID: {komut_id}] Komut: {komut_adi}, Değer: {deger}") 
            print("------------------------------")
            
        elif response.status_code == 204: 
            print(f"\rYeni komut bekleniyor...", end="")

        else:
            print(f"\n[Sunucu Hatası Kodu: {response.status_code}] Yanıt: {response.text[:50]}...", end="")
            
    except requests.exceptions.RequestException as e:
        print(f"\n[BAĞLANTI HATASI]: {e.__class__.__name__}", end="")


# Ana program döngüsü
print(f"Pi 5 Alıcı Başlatılıyor. Hedef API: {API_BASE_URL}")

try:
    while True:
        komutlari_cek_ve_isaretle()
        time.sleep(CEKME_ARALIGI)
        
except KeyboardInterrupt:
    print("\nAlıcı sonlandırıldı.")
    time.sleep(1)