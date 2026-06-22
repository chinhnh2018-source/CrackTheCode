// ============================================================
// MINESWEEPER (Dò Mìn) - hoàn toàn phía client
// ============================================================
const Minesweeper = (() => {
    const LEVELS = {
        easy:   { r: 9,  c: 9,  m: 10, label: "Dễ (9×9)" },
        medium: { r: 12, c: 14, m: 30, label: "Vừa (12×14)" },
        hard:   { r: 14, c: 18, m: 55, label: "Khó (14×18)" }
    };
    let R, C, M, cells, started, over, flags, revealed, flagMode, timer, secs, level = "easy";

    function reset(lv) {
        level = lv || level;
        const cfg = LEVELS[level];
        R = cfg.r; C = cfg.c; M = cfg.m;
        cells = [];
        for (let r = 0; r < R; r++) {
            const row = [];
            for (let c = 0; c < C; c++) row.push({ mine: false, rev: false, flag: false, adj: 0 });
            cells.push(row);
        }
        started = false; over = false; flags = 0; revealed = 0; flagMode = false;
        secs = 0; clearInterval(timer); timer = null;
        render(); updateStatus();
    }

    function placeMines(sr, sc) {
        let placed = 0;
        while (placed < M) {
            const r = Math.floor(Math.random() * R), c = Math.floor(Math.random() * C);
            if (cells[r][c].mine) continue;
            if (Math.abs(r - sr) <= 1 && Math.abs(c - sc) <= 1) continue; // ô đầu an toàn
            cells[r][c].mine = true; placed++;
        }
        for (let r = 0; r < R; r++) for (let c = 0; c < C; c++) {
            if (cells[r][c].mine) continue;
            let n = 0;
            for (let dr = -1; dr <= 1; dr++) for (let dc = -1; dc <= 1; dc++) {
                const nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < R && nc >= 0 && nc < C && cells[nr][nc].mine) n++;
            }
            cells[r][c].adj = n;
        }
    }

    function startTimer() {
        timer = setInterval(() => { secs++; updateStatus(); }, 1000);
    }

    function reveal(r, c) {
        const cell = cells[r][c];
        if (cell.rev || cell.flag) return;
        cell.rev = true; revealed++;
        if (cell.adj === 0 && !cell.mine) {
            for (let dr = -1; dr <= 1; dr++) for (let dc = -1; dc <= 1; dc++) {
                const nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < R && nc >= 0 && nc < C && !cells[nr][nc].rev) reveal(nr, nc);
            }
        }
    }

    function onCell(r, c) {
        if (over) return;
        const cell = cells[r][c];
        if (flagMode) { toggleFlag(r, c); return; }
        if (cell.flag) return;
        if (!started) { placeMines(r, c); started = true; startTimer(); }
        if (cell.mine) { explode(r, c); return; }
        Sound.play("key");
        reveal(r, c);
        render(); updateStatus();
        if (revealed === R * C - M) win();
    }

    function toggleFlag(r, c) {
        if (over) return;
        const cell = cells[r][c];
        if (cell.rev) return;
        cell.flag = !cell.flag;
        flags += cell.flag ? 1 : -1;
        Sound.play("click");
        render(); updateStatus();
    }

    function explode(r, c) {
        over = true; clearInterval(timer);
        cells[r][c].exploded = true;
        for (let i = 0; i < R; i++) for (let j = 0; j < C; j++) if (cells[i][j].mine) cells[i][j].rev = true;
        Sound.play("lose");
        render();
        feedback("\uD83D\uDCA5 Bùm! Bạn đã chạm phải mìn. Thử lại nhé!", "danger");
    }

    function win() {
        over = true; clearInterval(timer);
        Sound.play("win"); launchConfetti();
        feedback(`\uD83C\uDFC6 Xuất sắc! Bạn đã dò sạch mìn trong ${secs} giây!`, "success");
    }

    function feedback(msg, type) {
        const box = document.getElementById("ms-feedback");
        box.className = `alert alert-${type} py-2 mt-3`;
        box.innerHTML = msg;
        box.classList.remove("d-none");
    }

    function updateStatus() {
        document.getElementById("ms-mines").innerText = M - flags;
        document.getElementById("ms-time").innerText = secs;
        const fb = document.getElementById("ms-flagmode");
        if (fb) fb.classList.toggle("active", flagMode);
    }

    function render() {
        const grid = document.getElementById("ms-grid");
        grid.style.gridTemplateColumns = `repeat(${C}, 30px)`;
        grid.innerHTML = "";
        for (let r = 0; r < R; r++) for (let c = 0; c < C; c++) {
            const cell = cells[r][c];
            const d = document.createElement("div");
            d.className = "ms-cell";
            if (cell.rev) {
                d.classList.add("revealed");
                if (cell.mine) { d.classList.add(cell.exploded ? "exploded" : "mine"); d.innerHTML = "\uD83D\uDCA3"; }
                else if (cell.adj > 0) { d.classList.add("ms-n" + cell.adj); d.innerText = cell.adj; }
            } else if (cell.flag) {
                d.classList.add("flag"); d.innerHTML = "\uD83D\uDEA9";
            }
            d.addEventListener("click", () => onCell(r, c));
            d.addEventListener("contextmenu", (e) => { e.preventDefault(); toggleFlag(r, c); });
            grid.appendChild(d);
        }
    }

    return {
        start: (lv) => { document.getElementById("ms-feedback").classList.add("d-none"); reset(lv); },
        setLevel: (lv) => { document.getElementById("ms-feedback").classList.add("d-none"); reset(lv); },
        toggleFlagMode: () => { flagMode = !flagMode; updateStatus(); }
    };
})();

function initMinesweeper() {
    if (!Minesweeper._wired) {
        document.getElementById("ms-new").addEventListener("click", () => Minesweeper.start());
        document.getElementById("ms-flagmode").addEventListener("click", () => Minesweeper.toggleFlagMode());
        document.getElementById("ms-level").addEventListener("change", (e) => Minesweeper.setLevel(e.target.value));
        Minesweeper._wired = true;
    }
    Minesweeper.start(document.getElementById("ms-level").value);
}
window.initMinesweeper = initMinesweeper;
