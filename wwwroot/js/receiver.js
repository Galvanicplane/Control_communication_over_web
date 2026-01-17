let isSpyMode = false;

// Global scope function for HTML onclick access
window.startRobot = startRobot;

// Başlatma
document.addEventListener('DOMContentLoaded', () => {
    // Gerekirse UI init
});

async function startRobot() {
    const name = document.getElementById('robot-name').value;
    const pass = document.getElementById('robot-pass').value;
    const type = document.getElementById('robot-type').value;

    if (!name || !pass) return alert("İsim ve şifre giriniz");

    const res = await fetch('/api/robot/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, password: pass, type })
    });

    if (res.ok) {
        const data = await res.json();
        robotId = data.id;

        // Otomatik Spy Algılama
        if (data.isSpyMode) {
            isSpyMode = true;

            // Kullanıcıya bilgi ver
            const badge = document.getElementById('status-display');
            if (badge) {
                badge.className = 'badge bg-warning text-dark';
                badge.innerText = 'Gözlemci Modu (Spy)';
            }
            alert("Bu robot zaten aktif! İzleyici moduna geçildi. (Motor kontrolü diğer cihazda)");
        }

        document.getElementById('setup-panel').classList.add('d-none');
        document.getElementById('dashboard').classList.remove('d-none');
        Viz.init('vizCanvas');

        setInterval(pollLoop, 500); // Polling (500ms)
    }
}

async function pollLoop() {
    try {
        // Eğer Spy Mode ise 'peek=true' ekle, değilse normal sorgula
        const url = isSpyMode
            ? '/api/robot/poll/' + robotId + '?peek=true'
            : '/api/robot/poll/' + robotId;

        const res = await fetch(url);
        if (res.ok) {
            const commands = await res.json();

            if (commands.length > 0) {
                const logList = document.getElementById('log-list');

                commands.forEach(cmd => {
                    let displayTxt = cmd.commandCode;

                    if (cmd.commandCode.startsWith('V:')) {
                        const parts = cmd.commandCode.substring(2).split(',');
                        const x = parseFloat(parts[0]);
                        const y = parseFloat(parts[1]);
                        Viz.draw(x, y);
                        displayTxt = `Vector(${x.toFixed(2)}, ${y.toFixed(2)})`;
                    } else {
                        
                        let x = 0, y = 0;
                        if (cmd.commandCode === 'W') y = -1;
                        if (cmd.commandCode === 'S') y = 1;
                        if (cmd.commandCode === 'A') x = -1;
                        if (cmd.commandCode === 'D') x = 1;
                        Viz.draw(x, y);
                    }

                    const div = document.createElement('div');
                    div.className = 'log-entry';
                    div.innerHTML = `<span style="color:#888">[${new Date().toLocaleTimeString()}]</span> <span style="color:#fff">${displayTxt}</span>`;
                    logList.insertBefore(div, logList.firstChild);
                });
            }
        }
    } catch (e) { console.error(e); }
}
