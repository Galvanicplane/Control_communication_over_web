let selectedRobotId = null;
let selectedRobotName = null;
let senderId = null;

// Tuş Durumu
const keys = { W: false, A: false, S: false, D: false };
let currentVector = { x: 0, y: 0 };
let targetVector = { x: 0, y: 0 };
let lastSentVector = { x: 0, y: 0 }; // Son Gönderilen Vektör

// Başlatma
document.addEventListener('DOMContentLoaded', () => {
    loadRobots();
    // Animasyon Döngüsü
    requestAnimationFrame(updateLoop);
});

async function loadRobots() {
    try {
        const res = await fetch('/api/sender/receivers');
        const robots = await res.json();
        const list = document.getElementById('robot-list');
        list.innerHTML = '';

        if (robots.length === 0) {
            list.innerHTML = '<p class="text-muted">Online robot bulunamadı.</p>';
        }

        robots.forEach(r => {
            list.innerHTML += `
                <div class="col-md-4">
                    <div class="card border-${r.isBusy ? 'danger' : 'success'} mb-3">
                        <div class="card-body">
                            <h5 class="card-title">${r.name}</h5>
                            <p class="card-text badge bg-secondary">${r.type}</p>
                            <p class="card-text">${r.isBusy ? 'Meşgul' : 'Müsait'}</p>
                            <button onclick="promptConnect(${r.id}, '${r.name}')" class="btn btn-outline-info btn-sm w-100" ${r.isBusy ? 'disabled' : ''}>Kontrol Et</button>
                        </div>
                    </div>
                </div>
            `;
        });
    } catch (e) { console.error(e); }
}

// Global scope function for HTML onclick access
window.loadRobots = loadRobots;
window.promptConnect = promptConnect;
window.submitConnect = submitConnect;
window.disconnect = disconnect;

function promptConnect(id, name) {
    selectedRobotId = id;
    selectedRobotName = name;
    document.getElementById('auth-overlay').classList.remove('d-none');
}

async function submitConnect() {
    const pass = document.getElementById('connect-pass').value;
    const res = await fetch('/api/sender/connect', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            receiverId: selectedRobotId,
            senderName: 'WebOperator',
            password: pass
        })
    });

    if (res.ok) {
        const data = await res.json();
        senderId = data.senderId;

        document.getElementById('auth-overlay').classList.add('d-none');
        document.getElementById('list-panel').classList.add('d-none');
        document.getElementById('control-panel').classList.remove('d-none');
        document.getElementById('target-name').innerText = selectedRobotName;

        Viz.init('vizCanvas');

        // Klavye Dinleyicileri (WASD)
        window.addEventListener('keydown', e => { if (keys.hasOwnProperty(e.key.toUpperCase())) keys[e.key.toUpperCase()] = true; });
        window.addEventListener('keyup', e => { if (keys.hasOwnProperty(e.key.toUpperCase())) keys[e.key.toUpperCase()] = false; });
    } else {
        alert("Hata: " + await res.text());
    }
}

async function disconnect() {
    if (!senderId || !selectedRobotId) return;

    await fetch('/api/sender/disconnect', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            senderId,
            receiverId: selectedRobotId
        })
    });

    location.reload();
}

function updateLoop() {
    if (!senderId) { requestAnimationFrame(updateLoop); return; }

    // 1. Hedef Vektör Hesabı
    let tx = 0, ty = 0;
    if (keys.W) ty -= 1;
    if (keys.S) ty += 1;
    if (keys.A) tx -= 1;
    if (keys.D) tx += 1;

    // Çapraz Normalizasyon
    if (tx !== 0 && ty !== 0) {
        const len = Math.sqrt(tx * tx + ty * ty);
        tx /= len;
        ty /= len;
    }
    targetVector = { x: tx, y: ty };

    currentVector = { ...targetVector };

    //  Çiz
    Viz.draw(currentVector.x, currentVector.y);

    
    const cmdStr = `V:${currentVector.x.toFixed(2)},${currentVector.y.toFixed(2)}`;

    // Değişiklik Kontrolü
    if (Math.abs(currentVector.x - lastSentVector.x) > 0.05 || Math.abs(currentVector.y - lastSentVector.y) > 0.05) {
        send(cmdStr);
        lastSentVector = { ...currentVector };
    }

    requestAnimationFrame(updateLoop);
}

async function send(code) {
    // Log Ekleme
    const logDiv = document.getElementById('sender-log');
    const p = document.createElement('div');
    p.className = 'log-entry';
    p.innerText = `> ${code}`;
    logDiv.insertBefore(p, logDiv.firstChild); // Prepend

    // Log Temizliği
    if (logDiv.children.length > 20) logDiv.lastChild.remove();

    // API Çağrısı
    try {
        await fetch('/api/sender/command', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                senderId,
                receiverId: selectedRobotId,
                code: code,
                param: ''
            })
        });
    } catch (e) { console.error("Send failed", e); }
}
