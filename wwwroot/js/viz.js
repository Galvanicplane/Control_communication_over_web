const Viz = {
    canvas: null,
    ctx: null,
    centerX: 0,
    centerY: 0,
    radius: 0,

    init: function (canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.resize();
        window.addEventListener('resize', () => this.resize());
        this.draw(0, 0);
    },

    resize: function () {
        // Make it square based on container width
        const size = Math.min(this.canvas.parentElement.offsetWidth, 300);
        this.canvas.width = size;
        this.canvas.height = size;
        this.centerX = size / 2;
        this.centerY = size / 2;
        this.radius = (size / 2) - 10;
        this.draw(0, 0);
    },

    draw: function (x, y) {
        // x and y are -1 to 1 representing input vector
        const { ctx, centerX, centerY, radius } = this;

        // Clear
        ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        // Draw Outer Circle (G-Meter Background)
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, 2 * Math.PI);
        ctx.fillStyle = '#222';
        ctx.fill();
        ctx.strokeStyle = '#444';
        ctx.lineWidth = 2;
        ctx.stroke();

        // Draw Crosshair
        ctx.beginPath();
        ctx.moveTo(centerX - radius, centerY);
        ctx.lineTo(centerX + radius, centerY);
        ctx.moveTo(centerX, centerY - radius);
        ctx.lineTo(centerX, centerY + radius);
        ctx.strokeStyle = '#333';
        ctx.stroke();

        // Calculate Puck Position
        // Limit magnitude to 1
        const mag = Math.sqrt(x * x + y * y);
        if (mag > 1) {
            x /= mag;
            y /= mag;
        }

        const puckX = centerX + (x * radius);
        const puckY = centerY + (y * radius); // Canvas Y is down, so +y is down

        // Draw Trail (Pseudo) - could implement history array if needed

        // Draw Puck
        ctx.beginPath();
        ctx.arc(puckX, puckY, 15, 0, 2 * Math.PI);

        // Color changes based on intensity
        const intensity = Math.min(Math.sqrt(x * x + y * y), 1);
        const r = Math.floor(intensity * 255);
        const g = Math.floor((1 - intensity) * 200 + 55);

        ctx.fillStyle = `rgb(${r}, ${g}, 0)`;
        ctx.fill();
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.stroke();

        // Draw Text
        ctx.fillStyle = '#fff';
        ctx.font = '12px monospace';
        ctx.textAlign = 'center';
        ctx.fillText(`X: ${x.toFixed(2)}`, centerX, centerY + radius + 15); // won't show if canvas tight
    }
};
