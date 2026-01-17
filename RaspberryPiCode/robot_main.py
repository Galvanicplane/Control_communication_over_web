import time
import json
import requests
from gpiozero import Servo
from gpiozero.pins.pigpio import PiGPIOFactory

# --- AYARLAR ---
API_URL = "https://erenarslan.online/api/robot"  # Web API adresi
ROBOT_NAME = "PiRobot1"                          # Bu robotun adı
ROBOT_PASS = "1234"                              # Oturum şifresi
SERVO_PIN = 18                                   # Servonun bağlı olduğu GPIO Pini (BCM)

# --- DONANIM KURULUMU ---
# Raspberry Pi 5'te donanımsal PWM için PiGPIOFactory kullanmak gerekebilir
# Eğer hata alırsanız: sudo systemctl start pigpiod
factory = PiGPIOFactory()
servo = Servo(SERVO_PIN, pin_factory=factory)

# Robot 'ID'sini script başladığında sunucudan alacağız
robot_id = None

def register_robot():
    """Robotu sisteme kaydeder ve ID alır."""
    global robot_id
    print(f"[*] Sunucuya bağlanılıyor: {API_URL}")
    
    payload = {
        "name": ROBOT_NAME,
        "password": ROBOT_PASS,
        "type": "ServoArm"
    }
    
    try:
        response = requests.post(f"{API_URL}/register", json=payload, timeout=5)
        if response.status_code == 200:
            data = response.json()
            robot_id = data['id']
            print(f"[+] Kayıt Başarılı! Robot ID: {robot_id}")
            return True
        else:
            print(f"[-] Kayıt Hatası: {response.text}")
            return False
    except Exception as e:
        print(f"[!] Bağlantı Hatası: {e}")
        return False

def poll_server():
    """Sunucudan emirleri sorar ve servoyu çalıştırır."""
    if not robot_id:
        return

    try:
        # Polling isteği
        response = requests.get(f"{API_URL}/poll/{robot_id}", timeout=2)
        if response.status_code == 200:
            commands = response.json()
            
            # Eğer emir varsa işle
            for cmd in commands:
                process_command(cmd)
        else:
            print(f"[?] Sunucu Yanıtı: {response.status_code}")

    except Exception as e:
        print(f"[!] Polling Hatası: {e}")

def process_command(cmd):
    """Gelen komutu servoya uygular."""
    code = cmd.get('commandCode', '')
    print(f"[>] Gelen Komut: {code}")

    # Vektör verisi mi? (Örn: V:0.5,-1.0)
    if code.startswith("V:"):
        parts = code[2:].split(',')
        try:
            x_force = float(parts[0]) # Sağ/Sol (-1 ila 1)
            # Servoyu X eksenine göre hareket ettirelim.
            # -1 -> min, 0 -> orta, 1 -> max
            
            # Gelen veri -1 ile 1 arasında. Servo değeri de -1 ile 1 ister.
            # Direkt eşleştirebiliriz.
            servo.value = x_force
            print(f"    -> Servo Pozisyonu: {x_force}")
            
        except ValueError:
            print("    -> Vektör hatası")

    # Klavye tuşu mu?
    elif code == "A":
        servo.min() # Sola dön
        print("    -> Servo MIN")
    elif code == "D":
        servo.max() # Sağa dön
        print("    -> Servo MAX")
    elif code == "S" or code == "W":
        servo.mid() # Ortala
        print("    -> Servo MID")

# --- ANA DÖNGÜ ---
if __name__ == "__main__":
    print("--- CONTROL OVER WEB: PYTHON CLIENT ---")
    
    # Başarılı olana kadar kayıt dene
    while not register_robot():
        time.sleep(5)

    print("[*] Dinleme Moduna Geçildi...")
    
    while True:
        poll_server()
        time.sleep(0.1) # 100ms bekle (Çok sık sorgulayıp sunucuyu yorma)
