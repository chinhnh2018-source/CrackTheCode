// ============================================================
// CRACK THE CODE - Hiệu ứng confetti (canvas thuần, không thư viện ngoài)
// ============================================================
function launchConfetti(opts = {}) {
    const count = opts.count || 130;
    const duration = opts.duration || 2600;

    let canvas = document.getElementById("confetti-canvas");
    if (!canvas) {
        canvas = document.createElement("canvas");
        canvas.id = "confetti-canvas";
        canvas.style.cssText =
            "position:fixed;inset:0;width:100%;height:100%;pointer-events:none;z-index:3000;";
        document.body.appendChild(canvas);
    }

    const ctx = canvas.getContext("2d");
    const dpr = window.devicePixelRatio || 1;
    const W = window.innerWidth, H = window.innerHeight;
    canvas.width = W * dpr;
    canvas.height = H * dpr;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    const colors = ["#4cc9f0", "#f72585", "#ffd166", "#06d6a0", "#ef476f", "#118ab2", "#fb8500"];
    const parts = [];
    for (let i = 0; i < count; i++) {
        parts.push({
            x: Math.random() * W,
            y: -20 - Math.random() * H * 0.4,
            vx: (Math.random() - 0.5) * 3.5,
            vy: 2 + Math.random() * 4,
            size: 5 + Math.random() * 8,
            color: colors[Math.floor(Math.random() * colors.length)],
            rot: Math.random() * Math.PI,
            vr: (Math.random() - 0.5) * 0.35,
            shape: Math.random() < 0.5 ? "rect" : "circle"
        });
    }

    const start = performance.now();
    function frame(now) {
        const t = now - start;
        ctx.clearRect(0, 0, W, H);
        for (const p of parts) {
            p.x += p.vx; p.y += p.vy; p.vy += 0.045; p.rot += p.vr;
            ctx.save();
            ctx.translate(p.x, p.y);
            ctx.rotate(p.rot);
            ctx.globalAlpha = Math.max(0, 1 - t / duration);
            ctx.fillStyle = p.color;
            if (p.shape === "rect") {
                ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size * 0.6);
            } else {
                ctx.beginPath();
                ctx.arc(0, 0, p.size / 2, 0, Math.PI * 2);
                ctx.fill();
            }
            ctx.restore();
        }
        if (t < duration) {
            requestAnimationFrame(frame);
        } else {
            ctx.clearRect(0, 0, W, H);
            canvas.remove();
        }
    }
    requestAnimationFrame(frame);
}
