// --- AYARLAR ---
const API_BASE_URL = "/api/Komut";
let aktifCihazID = null;
let gonderimInterval = null;
const GONDERIM_ARALIGI = 250;

// Komut objesi - Varsayilan degerler
let currentCommand = {
    komut_adi: "ROBOT_YON",
    deger: 0
};

// HTML Elementlerini Sec
const cihazSecimEkrani = document.getElementById('cihaz-secim-ekrani');
const kontrolPaneli = document.getElementById('kontrol-paneli');
const cihazListesiDiv = document.getElementById('cihaz-listesi');
const aktifCihazIDSpan = document.getElementById('aktif-cihaz-id');
const logConsole = document.getElementById('log-console');
const acknowledgementDiv = document.getElementById('acknowledgement');

// --- YARDIMCI FONKSİYONLAR ---

function logYaz(mesaj, tip = 'bilgi') {
    const now = new Date().toLocaleTimeString();
    let renk;
    if (tip === 'hata') renk = 'red';
    else if (tip === 'basari') renk = '#17e017';
    else renk = 'lightgray';

    logConsole.innerHTML += `<span style="color: ${renk}">[${now}] ${mesaj}</span><br>`;
    logConsole.scrollTop = logConsole.scrollHeight; // Scroll asagi
}

// Kullaniciya islem sonucunu gosteren fonksiyon
function acknowledgementGoster(basarili, mesaj) {
    acknowledgementDiv.classList.remove('d-none', 'alert-success', 'alert-danger');

    if (basarili) {
        acknowledgementDiv.classList.add('alert-success');
        acknowledgementDiv.innerHTML = `<h2>✓ ${mesaj}</h2>`;
    } else {
        acknowledgementDiv.classList.add('alert-danger');
        acknowledgementDiv.innerHTML = `<h2>❌ ${mesaj}</h2>`;
    }
}

// 1. Cihaz listesi veritabanindan cekiliyor
async function cihazlariYukle() {
    logYaz("Cihaz listesi getiriliyor...");

    try {
        const response = await fetch(`${API_BASE_URL}/cihazlar`);

        if (response.status === 200) {
            const cihazlar = await response.json();
            cihazListesiDiv.innerHTML = '';

            cihazlar.forEach(cihaz => {
                const button = document.createElement('button');
                button.className = 'list-group-item list-group-item-action list-group-item-primary mb-2';
                button.innerHTML = `<strong>Cihaz ID: ${cihaz.cihazID}</strong> - ${cihaz.cihazToken || 'Tanimlanmamis Cihaz'}`;

                button.onclick = () => cihazSecildi(cihaz.cihazID);

                cihazListesiDiv.appendChild(button);
            });
            logYaz(`${cihazlar.length} cihaz listelendi.`, 'basari');

        } else if (response.status === 204) {
            cihazListesiDiv.innerHTML = '<p class="text-danger">Kayitli cihaz bulunamadi.</p>';
            logYaz("Veritabani bos.", 'hata');
        } else {
            logYaz(`Hata kodu: ${response.status}`, 'hata');
        }
    } catch (error) {
        logYaz(`Baglanti hatasi: API'ye ulasilamiyor.`, 'hata');
        cihazListesiDiv.innerHTML = '<p class="text-danger">Baglanti kurulamadi.</p>';
    }
}

function cihazSecildi(cihazID) {
    aktifCihazID = cihazID;
    aktifCihazIDSpan.textContent = cihazID;
    logYaz(`Cihaz ${cihazID} secildi.`);

    cihazSecimEkrani.classList.add('d-none');
    kontrolPaneli.classList.remove('d-none');
}

// 2. Kontrol Modlari

function klavyeModuBaslat() {
    logYaz("Klavye kontrolu aktif. W/S tuşlari ile yonetebilirsiniz.");
    document.addEventListener('keydown', klavyeListener);
    document.addEventListener('keyup', klavyeListener);

    // Periyodik gonderme baslat
    gonderimInterval = setInterval(komutGonder, GONDERIM_ARALIGI);
}

function klavyeModuDurdur() {
    document.removeEventListener('keydown', klavyeListener);
    document.removeEventListener('keyup', klavyeListener);

    clearInterval(gonderimInterval);
    gonderimInterval = null;
    logYaz("Kontrol durduruldu.");
}

// Klavye tuslarini dinleyen fonksiyon
function klavyeListener(event) {
    if (event.type === 'keydown') {
        if (event.key.toLowerCase() === 'w') {
            currentCommand.deger = 100; // Ileri
        } else if (event.key.toLowerCase() === 's') {
            currentCommand.deger = -100; // Geri
        } else if (event.key === 'Escape') {
            // ESC: Acil durdurma ve cikis
            currentCommand.deger = 0;
            komutGonder(true);
            klavyeModuDurdur();

            cihazSecildi(aktifCihazID); // Panele don
        }
    } else if (event.type === 'keyup') {
        // Tus birakildiginda dur
        if (event.key.toLowerCase() === 'w' || event.key.toLowerCase() === 's') {
            currentCommand.deger = 0;
        }
    }
}

// 3. API'ye istek (POST) atma
async function komutGonder(force = false) {
    if (!aktifCihazID) return;

    const istekGovdesi = {
        cihazID: aktifCihazID,
        komut: JSON.stringify(currentCommand)
    };

    try {
        const response = await fetch(`${API_BASE_URL}/gonder`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(istekGovdesi)
        });

        if (response.status === 200) {
            const data = await response.json();
            acknowledgementGoster(true, `Komut Iletildi: ${currentCommand.deger}`);
            logYaz(`Gonderildi: ${currentCommand.deger} (ID: ${data.komutID})`, 'basari');
        } else {
            acknowledgementGoster(false, `Hata: ${response.status}`);
            logYaz(`Sunucu hatasi: ${response.status}`, 'hata');
        }
    } catch (error) {
        acknowledgementGoster(false, "Baglanti Hatasi");
        logYaz(`Sunucuya erisilemedi.`, 'hata');
    }
}

// --- BASLANGIC ---
document.addEventListener('DOMContentLoaded', () => {
    cihazlariYukle();

    document.getElementById('btn-klavye').onclick = () => {
        // Arayuz degisiklikleri
        document.getElementById('btn-klavye').classList.add('d-none');
        document.getElementById('btn-gamepad').classList.add('d-none');
        document.body.style.backgroundColor = '#2c3e50';

        klavyeModuBaslat();
    };

    document.getElementById('btn-gamepad').onclick = () => {
        alert("Bu ozellik henuz eklenmedi.");
    };

    document.getElementById('btn-geri').onclick = () => {
        logYaz("Menuye donuluyor.");
        aktifCihazID = null;
        klavyeModuDurdur();

        // Arayuz sifirlama
        document.body.style.backgroundColor = '#f8f9fa';
        acknowledgementDiv.classList.add('d-none');
        document.getElementById('btn-klavye').classList.remove('d-none');
        document.getElementById('btn-gamepad').classList.remove('d-none');

        kontrolPaneli.classList.add('d-none');
        cihazSecimEkrani.classList.remove('d-none');

        cihazlariYukle();
    };
});