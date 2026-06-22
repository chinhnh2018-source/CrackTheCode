const KAKURO_PUZZLES = {"easy":{"board":[[{"t":"x"},{"t":"x","d":10},{"t":"x","d":29},{"t":"x"},{"t":"x","d":24},{"t":"x","d":12}],[{"t":"x","r":11},{"t":"w"},{"t":"w"},{"t":"x","r":11,"d":10},{"t":"w"},{"t":"w"}],[{"t":"x","r":20},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"}],[{"t":"x"},{"t":"x","r":20,"d":4},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"x","d":9}],[{"t":"x","r":10},{"t":"w"},{"t":"w"},{"t":"x","r":9},{"t":"w"},{"t":"w"}],[{"t":"x","r":11},{"t":"w"},{"t":"w"},{"t":"x","r":6},{"t":"w"},{"t":"w"}]]},"medium":{"board":[[{"t":"x"},{"t":"x"},{"t":"x"},{"t":"x","d":22},{"t":"x","d":15},{"t":"x"},{"t":"x"},{"t":"x"}],[{"t":"x"},{"t":"x"},{"t":"x","r":13},{"t":"w"},{"t":"w"},{"t":"x","d":26},{"t":"x","d":16},{"t":"x","d":9}],[{"t":"x"},{"t":"x","d":11},{"t":"x","r":23,"d":12},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"}],[{"t":"x","r":33},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"}],[{"t":"x","r":8},{"t":"w"},{"t":"w"},{"t":"x","d":9},{"t":"x","r":11,"d":15},{"t":"w"},{"t":"w"},{"t":"w"}],[{"t":"x"},{"t":"x","r":28},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"x"},{"t":"x"}],[{"t":"x"},{"t":"x","r":19},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"w"},{"t":"x"},{"t":"x"}]]}};

// ============================================================
// KAKURO (Cộng chéo) - đề sinh sẵn, kiểm tra theo luật. Phía client.
// ============================================================
let kkLevel = "easy";

function kkBoard() { return KAKURO_PUZZLES[kkLevel].board; }

function renderKakuro() {
    const board = kkBoard();
    const R = board.length, C = board[0].length;
    const grid = document.getElementById("kakuro-grid");
    grid.style.gridTemplateColumns = `repeat(${C}, 48px)`;
    grid.innerHTML = "";
    for (let r = 0; r < R; r++) for (let c = 0; c < C; c++) {
        const spec = board[r][c];
        const cell = document.createElement("div");
        cell.className = "num-cell";
        if (spec.t === "x") {
            if (spec.r != null || spec.d != null) {
                cell.classList.add("kk-clue");
                if (spec.d != null) { const cd = document.createElement("span"); cd.className = "cd"; cd.innerText = spec.d; cell.appendChild(cd); }
                if (spec.r != null) { const cr = document.createElement("span"); cr.className = "cr"; cr.innerText = spec.r; cell.appendChild(cr); }
            } else {
                cell.classList.add("kk-wall");
            }
        } else {
            const inp = document.createElement("input");
            inp.type = "text"; inp.inputMode = "numeric"; inp.maxLength = 1;
            inp.dataset.r = r; inp.dataset.c = c;
            inp.addEventListener("input", () => {
                inp.value = inp.value.replace(/[^1-9]/g, "").slice(0, 1);
                Sound.play("key");
            });
            cell.appendChild(inp);
        }
        grid.appendChild(cell);
    }
    document.getElementById("kakuro-feedback").classList.add("d-none");
}

function kkGetGrid() {
    const board = kkBoard();
    const R = board.length, C = board[0].length;
    const g = Array.from({ length: R }, () => Array(C).fill(0));
    document.querySelectorAll("#kakuro-grid input").forEach(inp => {
        g[+inp.dataset.r][+inp.dataset.c] = parseInt(inp.value) || 0;
    });
    return g;
}

function checkKakuro() {
    const board = kkBoard();
    const R = board.length, C = board[0].length;
    const g = kkGetGrid();
    // mọi ô trắng phải được điền
    for (let r = 0; r < R; r++) for (let c = 0; c < C; c++)
        if (board[r][c].t === "w" && !g[r][c]) return kkFb("Hãy điền đầy đủ tất cả các ô trắng.", "warning");

    const runs = [];
    for (let r = 0; r < R; r++) for (let c = 0; c < C; c++) {
        const spec = board[r][c];
        if (spec.t !== "x") continue;
        if (spec.r != null) {
            const cellsIn = []; let cc = c + 1;
            while (cc < C && board[r][cc].t === "w") { cellsIn.push(g[r][cc]); cc++; }
            runs.push({ sum: spec.r, vals: cellsIn });
        }
        if (spec.d != null) {
            const cellsIn = []; let rr = r + 1;
            while (rr < R && board[rr][c].t === "w") { cellsIn.push(g[rr][c]); rr++; }
            runs.push({ sum: spec.d, vals: cellsIn });
        }
    }
    for (const run of runs) {
        const total = run.vals.reduce((s, v) => s + v, 0);
        const uniq = new Set(run.vals);
        if (uniq.size !== run.vals.length) return kkFb("Có số bị lặp trong một dãy. Mỗi dãy không được trùng số.", "danger");
        if (total !== run.sum) return kkFb(`Một dãy có tổng ${total} nhưng cần là ${run.sum}. Thử lại nhé!`, "danger");
    }
    Sound.play("win"); launchConfetti();
    return kkFb("\uD83C\uDFC6 Tuyệt vời! Bạn đã giải đúng Kakuro!", "success");
}

function kkFb(msg, type) {
    if (type === "danger" || type === "warning") Sound.play("wrong");
    const box = document.getElementById("kakuro-feedback");
    box.className = `alert alert-${type} py-2 mt-3`;
    box.innerHTML = msg; box.classList.remove("d-none");
}

function initKakuro() {
    if (!initKakuro._wired) {
        document.getElementById("kakuro-new").addEventListener("click", renderKakuro);
        document.getElementById("kakuro-check").addEventListener("click", checkKakuro);
        document.getElementById("kakuro-level").addEventListener("change", (ev) => { kkLevel = ev.target.value; renderKakuro(); });
        initKakuro._wired = true;
    }
    kkLevel = document.getElementById("kakuro-level").value;
    renderKakuro();
}
window.initKakuro = initKakuro;
