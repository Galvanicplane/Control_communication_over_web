import requests
import json
import pygame
import time

# --- AYARLAR ---
CIHAZ_ID = 1 
API_URL = "https://api.erenarslan.online/api/komut/gonder" 
HEADERS = {'Content-Type': 'application/json'}

# Sağ joystick 
SAG_JOYSTICK_Y_EKSENI_INDEKSI = 3
KOMUT_ADI = "JOYSTICK_SAG_Y" 

# Gönderme frekansı: 0.25 saniye
GONDERME_ARALIGI = 0.25 
# -----------------

# Global değişkenler
anlik_joystick_verisi = 0.0
son_gonderilen_deger = 999 

def pygame_baslat():
    
    global anlik_joystick_verisi #degeri degistirmek icin bunu yapiyoruz

    pygame.init()
    pygame.joystick.init()
    
    joystick_sayisi = pygame.joystick.get_count()
    if joystick_sayisi == 0:
        print("------------- Gamepad bagli degil ---------------------")
        return None
        
    joystick = pygame.joystick.Joystick(0)
    joystick.init()
    print(f"Gamepad bağlandı: {joystick.get_name()}")
    

    
    return joystick

def veriyi_guncelle(joystick):
    
    global anlik_joystick_verisi
    
    pygame.event.get()
    
    if joystick.get_numaxes() > SAG_JOYSTICK_Y_EKSENI_INDEKSI:
        #(-1.0 - 1.0 )
        anlik_joystick_verisi = joystick.get_axis(SAG_JOYSTICK_Y_EKSENI_INDEKSI)


def komut_gonder(deger):

    global son_gonderilen_deger
    
    # donusturme (-100-100)
    int_deger = int(deger * 100)

    
    if int_deger == son_gonderilen_deger:
        return
    
    data = {
        'cihazID': CIHAZ_ID,
        'komut': {
            'komut_adi': KOMUT_ADI,
            'deger': int_deger
        }
    }
    
    print(f"\rAPI  gonder  -> C: {KOMUT_ADI}, D: {int_deger} ", end="")
    
    try:
        response = requests.post(API_URL, headers=HEADERS, data=json.dumps(data), timeout=1) #gonderme istegi
        
        if response.status_code == 200: #200 = basarili
            son_gonderilen_deger = int_deger
            
            pass 
        else:
            #ag tamam ama gonderilemedi
            print(f"\n[Hata : {response.status_code}] Yanıt: {response.text[:50]}...", end="")
            
    except requests.exceptions.RequestException as e:
        #aga baglanilamadi
        print(f"\n[Ağ Hatası: {e.__class__.__name__}]", end="")

#------------------------------------------------------Ana program-----------------------------------------------------
gamepad = pygame_baslat()

if gamepad:
    
    try:
        while True:
            veriyi_guncelle(gamepad)
            
            komut_gonder(anlik_joystick_verisi)
            
            time.sleep(GONDERME_ARALIGI)
            
    except KeyboardInterrupt:
        print("\nExit")
    finally:
        pygame.quit()