// ============================================================
// KENKEN - lưới Latin + phép toán theo cụm (cage). Phía client.
// ============================================================
const KenKen = (() => {
    let N, sol, cageId, cages, level = "4";
    const SIZES = { "4": 4, "5": 5, "6": 6 };

    function latinSquare(n) {
        // hàng cơ sở hoán vị, dịch vòng, rồi xáo trộn hàng/cột + đổi nhãn số
        let base = [];
        for (let r = 0; r < n; r++) {
            const row = [];
            for (let c = 0; c < n; c++) row.push(((r + c) % n) + 1);
            base.push(row);
        }
        const shuffle = a => { for (let i = a.length - 1; i > 0; i--) { const j = Math.floor(Math.random() * (i + 1));[a[i], a[j]] = [a[j], a[i]]; } return a; };
        const rp = shuffle([...Array(n).keys()]), cp = shuffle([...Array(n).keys()]);
        const lbl = shuffle([...Array(n).keys()].map(x => x + 1));
        const g = [];
        for (let r = 0; r < n; r++) { const row = []; for (let c = 0; c < n; c++) row.push(lbl[base[rp[r]][cp[c]] - 1]); g.push(row); }
        return g;
    }

    function makeCages(n) {
        const id = Array.from({ length: n }, () => Array(n).fill(-1));
        let cid = 0;
        const order = [];
        for (let r = 0; r < n; r++) for (let c = 0; c < n; c++) order.push([r, c]);
        // xáo trộn nhẹ thứ tự bắt đầu
        for (let r = 0; r < n; r++) for (let c = 0; c < n; c++) {
            if (id[r][c] !== -1) continue;
            const size = Math.random() < 0.45 ? 2 : (Math.random() < 0.7 ? 3 : (Math.random() < 0.5 ? 1 : 2));
            const cellsIn = [[r, c]]; id[r][c] = cid;
            while (cellsIn.length < size) {
                const opts = [];
                for (const [cr, cc] of cellsIn) {
                    for (const [dr, dc] of [[-1, 0], [1, 0], [0, -1], [0, 1]]) {
                        const nr = cr + dr, nc = cc + dc;
                        if (nr >= 0 && nr < n && nc >= 0 && nc < n && id[nr][nc] === -1) opts.push([nr, nc]);
                    }
                }
                if (!opts.length) break;
                const [pr, pc] = opts[Math.floor(Math.random() * opts.length)];
                id[pr][pc] = cid; cellsIn.push([pr, pc]);
            }
            cid++;
        }
        return id;
    }

    function buildCages() {
        const groups = {};
        for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) {
            (groups[cageId[r][c]] = groups[cageId[r][c]] || []).push([r, c]);
        }
        cages = {};
        for (const k in groups) {
            const cellsIn = groups[k];
            const vals = cellsIn.map(([r, c]) => sol[r][c]);
            let op = "", target = vals[0];
            if (cellsIn.length === 1) { op = ""; target = vals[0]; }
            else if (cellsIn.length === 2) {
                const [a, b] = vals, hi = Math.max(a, b), lo = Math.min(a, b);
                const choices = [["+", a + b], ["\u00D7", a * b], ["\u2212", hi - lo]];
                if (hi % lo === 0) choices.push(["\u00F7", hi / lo]);
                [op, target] = choices[Math.floor(Math.random() * choices.length)];
            } else {
                if (Math.random() < 0.5) { op = "+"; target = vals.reduce((s, v) => s + v, 0); }
                else { op = "\u00D7"; target = vals.reduce((s, v) => s * v, 1); }
            }
            // ô gắn nhãn = ô trên-trái nhất
            const anchor = cellsIn.slice().sort((p, q) => p[0] - q[0] || p[1] - q[1])[0];
            cages[k] = { cells: cellsIn, op, target, anchor };
        }
    }

    function reset(lv) {
        level = lv || level; N = SIZES[level];
        sol = latinSquare(N);
        cageId = makeCages(N);
        buildCages();
        render();
        document.getElementById("kenken-feedback").classList.add("d-none");
    }

    function render() {
        const grid = document.getElementById("kenken-grid");
        grid.style.gridTemplateColumns = `repeat(${N}, 64px)`;
        grid.innerHTML = "";
        for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) {
            const cell = document.createElement("div");
            cell.className = "num-cell";
            const cur = cageId[r][c];
            if (r === 0 || cageId[r - 1][c] !== cur) cell.classList.add("bt-thick");
            if (c === 0 || cageId[r][c - 1] !== cur) cell.classList.add("bl-thick");
            if (r === N - 1 || cageId[r + 1][c] !== cur) cell.classList.add("bb-thick");
            if (c === N - 1 || cageId[r][c + 1] !== cur) cell.classList.add("br-thick");
            const cg = cages[cur];
            if (cg.anchor[0] === r && cg.anchor[1] === c) {
                const lab = document.createElement("span");
                lab.className = "cage-label";
                lab.innerText = cg.target + cg.op;
                cell.appendChild(lab);
            }
            const inp = document.createElement("input");
            inp.type = "text"; inp.inputMode = "numeric"; inp.maxLength = 1;
            inp.dataset.r = r; inp.dataset.c = c;
            inp.addEventListener("input", () => {
                inp.value = inp.value.replace(/[^1-9]/g, "").slice(0, 1);
                Sound.play("key");
            });
            cell.appendChild(inp);
            grid.appendChild(cell);
        }
    }

    function getGrid() {
        const g = Array.from({ length: N }, () => Array(N).fill(0));
        document.querySelectorAll("#kenken-grid input").forEach(inp => {
            g[+inp.dataset.r][+inp.dataset.c] = parseInt(inp.value) || 0;
        });
        return g;
    }

    function check() {
        const g = getGrid();
        for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) if (!g[r][c]) { return fb("Hãy điền đầy đủ tất cả các ô trước.", "warning"); }
        for (let i = 0; i < N; i++) {
            const row = new Set(), col = new Set();
            for (let j = 0; j < N; j++) { row.add(g[i][j]); col.add(g[j][i]); }
            if (row.size !== N || col.size !== N) return fb("Mỗi hàng và cột phải chứa đủ các số 1.." + N + " không lặp.", "danger");
        }
        for (const k in cages) {
            const cg = cages[k], vals = cg.cells.map(([r, c]) => g[r][c]);
            let ok;
            if (cg.op === "") ok = vals[0] === cg.target;
            else if (cg.op === "+") ok = vals.reduce((s, v) => s + v, 0) === cg.target;
            else if (cg.op === "\u00D7") ok = vals.reduce((s, v) => s * v, 1) === cg.target;
            else if (cg.op === "\u2212") ok = Math.abs(vals[0] - vals[1]) === cg.target;
            else if (cg.op === "\u00F7") { const hi = Math.max(...vals), lo = Math.min(...vals); ok = hi / lo === cg.target; }
            if (!ok) return fb(`Cụm ${cg.target}${cg.op} chưa thỏa mãn. Kiểm tra lại nhé!`, "danger");
        }
        Sound.play("win"); launchConfetti();
        return fb("\uD83C\uDFC6 Chính xác! Bạn đã giải xong KenKen!", "success");
    }

    function fb(msg, type) {
        if (type === "danger" || type === "warning") Sound.play("wrong");
        const box = document.getElementById("kenken-feedback");
        box.className = `alert alert-${type} py-2 mt-3`;
        box.innerHTML = msg; box.classList.remove("d-none");
    }

    return { start: (lv) => reset(lv), check };
})();

function initKenKen() {
    if (!KenKen._wired) {
        document.getElementById("kenken-new").addEventListener("click", () => KenKen.start());
        document.getElementById("kenken-check").addEventListener("click", () => KenKen.check());
        document.getElementById("kenken-level").addEventListener("change", (e) => KenKen.start(e.target.value));
        KenKen._wired = true;
    }
    KenKen.start(document.getElementById("kenken-level").value);
}
window.initKenKen = initKenKen;
