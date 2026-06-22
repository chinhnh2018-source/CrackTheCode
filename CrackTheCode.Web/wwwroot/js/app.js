// ============================================================
// CRACK THE CODE - SPA Client v2.0 (Multi-Player Auth)
// ============================================================

// GLOBAL STATE
let gameState = {
    sessionId: null,
    digitsCount: 3,
    activeDigitIndex: 0,
    mode: "Classic",
    difficulty: "Normal",
    timeRemaining: 0,
    timerInterval: null,
    challengeStage: 1,
    currentClues: [],
    eliminatedDigits: [],
    theme: "dark"
};

// Auth State (persisted to localStorage)
let authState = {
    userId: null,
    username: null
};

// ============================================================
// INITIALIZATION
// ============================================================
document.addEventListener("DOMContentLoaded", () => {
    const savedTheme = localStorage.getItem("theme") || "dark";
    setTheme(savedTheme);

    buildKeypad();
    registerEventListeners();
    loadAuthFromStorage();
});

function registerEventListeners() {
    document.getElementById("theme-toggle-btn").addEventListener("click", toggleTheme);
    document.getElementById("start-game-btn").addEventListener("click", handleStartGame);
    document.getElementById("daily-puzzle-btn").addEventListener("click", startDailyPuzzle);
    document.getElementById("back-to-menu-btn").addEventListener("click", exitToMenu);
    document.getElementById("submit-guess-btn").addEventListener("click", submitGuess);
    document.getElementById("show-answer-btn").addEventListener("click", forfeitAndShowAnswer);
    document.getElementById("restart-game-btn").addEventListener("click", restartCurrentGame);
    document.getElementById("hint-btn").addEventListener("click", requestHint);
    document.getElementById("game-mode-select").addEventListener("change", handleGameModeChange);
    document.getElementById("stats-nav-btn").addEventListener("click", fetchAndShowStats);
    document.getElementById("logout-btn").addEventListener("click", logout);
    document.getElementById("auth-login-btn").addEventListener("click", handleLogin);
    document.getElementById("auth-register-btn").addEventListener("click", handleRegister);
    document.addEventListener("keydown", handlePhysicalKeyboard);
}

// ============================================================
// AUTH MANAGEMENT
// ============================================================
function loadAuthFromStorage() {
    const savedUserId = localStorage.getItem("userId");
    const savedUsername = localStorage.getItem("username");
    if (savedUserId && savedUsername) {
        authState.userId = savedUserId;
        authState.username = savedUsername;
        applyLoggedInUI();
    } else {
        applyLoggedOutUI();
    }
}

function applyLoggedInUI() {
    document.getElementById("user-header-section").classList.remove("d-none");
    document.getElementById("user-header-section").classList.add("d-flex");
    document.getElementById("header-username").innerText = authState.username;
    document.getElementById("login-nav-btn").classList.add("d-none");
    document.getElementById("welcome-prompt-banner").classList.add("d-none");
}

function applyLoggedOutUI() {
    document.getElementById("user-header-section").classList.add("d-none");
    document.getElementById("user-header-section").classList.remove("d-flex");
    document.getElementById("login-nav-btn").classList.remove("d-none");
    document.getElementById("welcome-prompt-banner").classList.remove("d-none");
    authState.userId = null;
    authState.username = null;
}

async function handleLogin() {
    const username = document.getElementById("auth-username").value.trim();
    const password = document.getElementById("auth-password").value;

    if (!username || !password) {
        showAuthAlert("Vui lòng nhập đầy đủ tên tài khoản và mật khẩu!", "danger");
        return;
    }

    try {
        const res = await fetch("/api/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });
        const data = await res.json();

        if (!res.ok) {
            showAuthAlert(data.error || "Đăng nhập thất bại!", "danger");
            return;
        }

        authState.userId = data.userId;
        authState.username = data.username;
        localStorage.setItem("userId", data.userId);
        localStorage.setItem("username", data.username);

        // Close modal
        bootstrap.Modal.getInstance(document.getElementById("authModal"))?.hide();
        applyLoggedInUI();
        showToast(`Chào mừng trở lại, <strong>${data.username}</strong>! 🎉`, "success");

    } catch (err) {
        showAuthAlert("Lỗi kết nối máy chủ. Vui lòng thử lại.", "danger");
    }
}

async function handleRegister() {
    const username = document.getElementById("auth-username").value.trim();
    const password = document.getElementById("auth-password").value;

    if (!username || !password) {
        showAuthAlert("Vui lòng nhập đầy đủ tên tài khoản và mật khẩu!", "danger");
        return;
    }

    if (password.length < 4) {
        showAuthAlert("Mật khẩu phải có ít nhất 4 ký tự!", "warning");
        return;
    }

    try {
        const res = await fetch("/api/auth/register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });
        const data = await res.json();

        if (!res.ok) {
            showAuthAlert(data.error || "Đăng ký thất bại!", "danger");
            return;
        }

        authState.userId = data.userId;
        authState.username = data.username;
        localStorage.setItem("userId", data.userId);
        localStorage.setItem("username", data.username);

        bootstrap.Modal.getInstance(document.getElementById("authModal"))?.hide();
        applyLoggedInUI();
        showToast(`Đăng ký thành công! Chào mừng <strong>${data.username}</strong> đến với Crack The Code! 🔐`, "success");

    } catch (err) {
        showAuthAlert("Lỗi kết nối máy chủ. Vui lòng thử lại.", "danger");
    }
}

function logout() {
    localStorage.removeItem("userId");
    localStorage.removeItem("username");
    applyLoggedOutUI();

    // If playing, go back to menu
    document.getElementById("play-screen").classList.add("d-none");
    document.getElementById("menu-screen").classList.remove("d-none");

    if (gameState.timerInterval) {
        clearInterval(gameState.timerInterval);
    }

    showToast("Đã đăng xuất thành công.", "secondary");
}

function showAuthAlert(msg, type) {
    const alertBox = document.getElementById("auth-alert");
    alertBox.className = `alert alert-${type} py-2 d-block`;
    alertBox.innerHTML = msg;
}

function getAuthHeaders() {
    return {
        "Content-Type": "application/json",
        "X-User-Id": authState.userId || ""
    };
}

// ============================================================
// TOAST NOTIFICATIONS
// ============================================================
function showToast(msg, type = "primary") {
    const existing = document.getElementById("app-toast");
    if (existing) existing.remove();

    const toastEl = document.createElement("div");
    toastEl.id = "app-toast";
    toastEl.className = `position-fixed bottom-0 end-0 m-4 p-3 rounded shadow-lg bg-${type === "secondary" ? "dark" : type} text-white fw-semibold`;
    toastEl.style.zIndex = 9999;
    toastEl.style.maxWidth = "340px";
    toastEl.style.fontSize = "0.95rem";
    toastEl.innerHTML = `<i class="fa-solid fa-circle-check me-2"></i>${msg}`;
    document.body.appendChild(toastEl);

    setTimeout(() => {
        toastEl.style.transition = "opacity 0.5s";
        toastEl.style.opacity = "0";
        setTimeout(() => toastEl.remove(), 500);
    }, 3500);
}

// ============================================================
// THEME MANAGEMENT
// ============================================================
function setTheme(theme) {
    gameState.theme = theme;
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem("theme", theme);
    const icon = document.getElementById("theme-icon");
    icon.className = theme === "dark" ? "fa-solid fa-sun" : "fa-solid fa-moon";
}

function toggleTheme() {
    setTheme(gameState.theme === "dark" ? "light" : "dark");
}

// ============================================================
// KEYPAD GENERATION
// ============================================================
function buildKeypad() {
    const keypad = document.getElementById("numeric-keypad");
    keypad.innerHTML = "";

    for (let i = 1; i <= 9; i++) {
        const btn = document.createElement("button");
        btn.className = "keypad-btn btn-key";
        btn.innerText = i;
        btn.dataset.val = i;
        btn.addEventListener("click", () => enterDigit(i));
        keypad.appendChild(btn);
    }

    const backBtn = document.createElement("button");
    backBtn.className = "keypad-btn btn-key text-warning";
    backBtn.innerHTML = "<i class='fa-solid fa-backspace'></i>";
    backBtn.addEventListener("click", backspaceDigit);
    keypad.appendChild(backBtn);

    const zeroBtn = document.createElement("button");
    zeroBtn.className = "keypad-btn btn-key";
    zeroBtn.innerText = 0;
    zeroBtn.dataset.val = 0;
    zeroBtn.addEventListener("click", () => enterDigit(0));
    keypad.appendChild(zeroBtn);

    const clearBtn = document.createElement("button");
    clearBtn.className = "keypad-btn btn-key text-danger fw-bold";
    clearBtn.innerHTML = "C";
    clearBtn.addEventListener("click", clearAllDigits);
    keypad.appendChild(clearBtn);
}

// ============================================================
// DIGIT BOX MANAGEMENT
// ============================================================
function generateDigitBoxes(count) {
    const container = document.getElementById("digit-boxes-container");
    container.innerHTML = "";
    for (let i = 0; i < count; i++) {
        const box = document.createElement("input");
        box.type = "text";
        box.className = "digit-box";
        box.maxLength = 1;
        box.readOnly = true;
        box.dataset.index = i;
        box.addEventListener("click", () => {
            gameState.activeDigitIndex = i;
            highlightActiveBox();
        });
        container.appendChild(box);
    }
    gameState.activeDigitIndex = 0;
    highlightActiveBox();
}

function highlightActiveBox() {
    document.querySelectorAll(".digit-box").forEach((box, i) => {
        box.classList.toggle("border-primary", i === gameState.activeDigitIndex);
        box.classList.toggle("shadow-sm", i === gameState.activeDigitIndex);
        if (i === gameState.activeDigitIndex) box.focus();
    });
}

function enterDigit(num) {
    const boxes = document.querySelectorAll(".digit-box");
    if (gameState.activeDigitIndex < gameState.digitsCount) {
        boxes[gameState.activeDigitIndex].value = num;
        if (gameState.activeDigitIndex < gameState.digitsCount - 1) {
            gameState.activeDigitIndex++;
        }
        highlightActiveBox();
    }
}

function backspaceDigit() {
    const boxes = document.querySelectorAll(".digit-box");
    if (boxes[gameState.activeDigitIndex].value !== "") {
        boxes[gameState.activeDigitIndex].value = "";
    } else if (gameState.activeDigitIndex > 0) {
        gameState.activeDigitIndex--;
        boxes[gameState.activeDigitIndex].value = "";
    }
    highlightActiveBox();
}

function clearAllDigits() {
    document.querySelectorAll(".digit-box").forEach(box => box.value = "");
    gameState.activeDigitIndex = 0;
    highlightActiveBox();
}

function handlePhysicalKeyboard(e) {
    if (document.getElementById("play-screen").classList.contains("d-none")) return;
    if (e.key >= "0" && e.key <= "9") {
        const d = parseInt(e.key);
        if (!gameState.eliminatedDigits.includes(d)) enterDigit(d);
    } else if (e.key === "Backspace") {
        backspaceDigit();
    } else if (e.key === "Escape") {
        clearAllDigits();
    } else if (e.key === "Enter") {
        submitGuess();
    }
}

// ============================================================
// GAME FLOW
// ============================================================
function handleGameModeChange() {
    const mode = document.getElementById("game-mode-select").value;
    document.getElementById("time-attack-config").classList.toggle("d-none", mode !== "TimeAttack");

    const digitsSelect = document.getElementById("digits-count-select");
    const diffSelect = document.getElementById("difficulty-select");
    const duplicatesSelect = document.getElementById("duplicates-select");

    if (mode === "Daily") {
        digitsSelect.value = "4"; diffSelect.value = "Normal"; duplicatesSelect.value = "true";
        digitsSelect.disabled = diffSelect.disabled = duplicatesSelect.disabled = true;
    } else if (mode === "Challenge") {
        digitsSelect.disabled = diffSelect.disabled = duplicatesSelect.disabled = true;
    } else {
        digitsSelect.disabled = diffSelect.disabled = duplicatesSelect.disabled = false;
    }
}

function handleStartGame() {
    if (!authState.userId) {
        bootstrap.Modal.getOrCreateInstance(document.getElementById("authModal")).show();
        showToast("Bạn cần đăng nhập trước để bắt đầu chơi!", "warning");
        return;
    }

    const mode = document.getElementById("game-mode-select").value;
    gameState.mode = mode;
    gameState.challengeStage = 1;

    if (mode === "Daily") {
        startDailyPuzzle();
    } else if (mode === "Challenge") {
        startChallengeStage(1);
    } else {
        const difficulty = document.getElementById("difficulty-select").value;
        const digitsCount = parseInt(document.getElementById("digits-count-select").value);
        const timeLimit = parseInt(document.getElementById("time-limit-select").value);
        const allowDuplicates = document.getElementById("duplicates-select").value === "true";
        const range = document.getElementById("range-select").value;
        const minDigit = range === "1-9" ? 1 : 0;
        initiateNewGame(difficulty, digitsCount, mode, timeLimit, allowDuplicates, minDigit, 9);
    }
}

function startChallengeStage(stageNum) {
    gameState.challengeStage = stageNum;
    const stages = [
        { digits: 3, diff: "Easy",      dupes: false },
        { digits: 3, diff: "Normal",    dupes: false },
        { digits: 3, diff: "Hard",      dupes: true  },
        { digits: 4, diff: "Easy",      dupes: false },
        { digits: 4, diff: "Normal",    dupes: true  },
        { digits: 4, diff: "Hard",      dupes: true  },
        { digits: 4, diff: "Expert",    dupes: true  },
        { digits: 5, diff: "Normal",    dupes: false },
        { digits: 5, diff: "Hard",      dupes: true  },
        { digits: 5, diff: "Nightmare", dupes: true  }
    ];
    const s = stages[Math.min(stageNum - 1, 9)];
    initiateNewGame(s.diff, s.digits, "Challenge", 0, s.dupes, 0, 9);
}

function startDailyPuzzle() {
    if (!authState.userId) {
        bootstrap.Modal.getOrCreateInstance(document.getElementById("authModal")).show();
        return;
    }
    gameState.mode = "Daily";
    resetPlayUI();

    fetch("/api/dailypuzzle", { headers: getAuthHeaders() })
        .then(res => res.json())
        .then(data => {
            if (data.error) { showToast(data.error, "danger"); return; }
            loadGameData(data);
        })
        .catch(() => showToast("Lỗi tải Daily Puzzle!", "danger"));
}

function initiateNewGame(difficulty, digitsCount, mode, timeLimit, allowDuplicates, minDigit, maxDigit) {
    resetPlayUI();
    const url = `/api/newgame?difficulty=${difficulty}&digitsCount=${digitsCount}&mode=${mode}&timeLimit=${timeLimit}&allowDuplicates=${allowDuplicates}&minDigit=${minDigit}&maxDigit=${maxDigit}`;

    fetch(url, { headers: getAuthHeaders() })
        .then(async res => {
            // Stale identity (e.g. localStorage userId after the DB was reset):
            // the server returns 401 instead of crashing on a FK violation.
            if (res.status === 401) {
                const body = await res.json().catch(() => ({}));
                handleInvalidSession(body.error);
                return null;
            }
            return res.json();
        })
        .then(data => {
            if (!data) return;
            if (data.error) { showToast(data.error, "danger"); return; }
            loadGameData(data);
        })
        .catch(() => showToast("Không thể kết nối đến máy chủ!", "danger"));
}

// Clears a stale/invalid local session and sends the user back to login.
function handleInvalidSession(msg) {
    localStorage.removeItem("userId");
    localStorage.removeItem("username");
    authState.userId = null;
    authState.username = null;
    applyLoggedOutUI();
    document.getElementById("play-screen").classList.add("d-none");
    document.getElementById("menu-screen").classList.remove("d-none");
    if (gameState.timerInterval) {
        clearInterval(gameState.timerInterval);
    }
    showToast(msg || "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", "danger");
}

function loadGameData(data) {
    gameState.sessionId = data.sessionId;
    gameState.digitsCount = data.digitsCount;
    gameState.difficulty = data.difficulty;
    gameState.mode = data.mode;
    gameState.eliminatedDigits = [];

    document.getElementById("game-title").innerText = getModeDisplayName(data.mode);
    document.getElementById("game-subtitle").innerText = `Độ khó: ${data.difficulty} | ${data.digitsCount} chữ số | ${authState.username || "Khách"}`;

    // Challenge stage header
    const challengeHeader = document.getElementById("challenge-stage-header");
    challengeHeader.classList.toggle("d-none", data.mode !== "Challenge");
    if (data.mode === "Challenge") {
        document.getElementById("challenge-stage-text").innerText = gameState.challengeStage;
    }

    // Timer for TimeAttack
    const timerBadge = document.getElementById("time-countdown-badge");
    const progressContainer = document.getElementById("time-attack-progress-container");

    if (data.mode === "TimeAttack") {
        timerBadge.classList.remove("d-none");
        progressContainer.classList.remove("d-none");
        gameState.timeRemaining = data.timeLimitSeconds;
        updateTimerUI();
        const totalDuration = data.timeLimitSeconds;

        gameState.timerInterval = setInterval(() => {
            gameState.timeRemaining--;
            updateTimerUI();
            const pct = (gameState.timeRemaining / totalDuration) * 100;
            document.getElementById("time-attack-progress-bar").style.width = `${pct}%`;
            if (gameState.timeRemaining <= 0) {
                clearInterval(gameState.timerInterval);
                triggerTimeOut();
            }
        }, 1000);
    } else {
        timerBadge.classList.add("d-none");
        progressContainer.classList.add("d-none");
    }

    renderClues(data.clues);
    generateDigitBoxes(data.digitsCount);
    enableKeypadKeys();

    document.getElementById("menu-screen").classList.add("d-none");
    document.getElementById("play-screen").classList.remove("d-none");
}

function renderClues(clues) {
    const list = document.getElementById("clues-list");
    list.innerHTML = "";
    gameState.currentClues = clues;

    clues.forEach((clue, index) => {
        const div = document.createElement("div");
        div.className = "clue-item clue-primary";
        div.innerHTML = `<span class="badge bg-primary me-2">${index + 1}</span><strong>${clue.description}</strong>`;
        list.appendChild(div);
    });
}

function updateTimerUI() {
    document.getElementById("countdown-text").innerText = `${gameState.timeRemaining}s`;
    const badge = document.getElementById("time-countdown-badge");
    if (gameState.timeRemaining <= 10) {
        badge.className = "badge bg-danger fs-6 px-3 py-2 animate-pulse";
    }
}

function resetPlayUI() {
    if (gameState.timerInterval) clearInterval(gameState.timerInterval);
    const alertBox = document.getElementById("guess-feedback-alert");
    alertBox.className = "alert mt-3 py-2 d-none";
    alertBox.innerHTML = "";
    buildKeypad();
}

function exitToMenu() {
    if (gameState.timerInterval) clearInterval(gameState.timerInterval);
    document.getElementById("play-screen").classList.add("d-none");
    document.getElementById("menu-screen").classList.remove("d-none");
}

// ============================================================
// SUBMIT GUESS
// ============================================================
function submitGuess() {
    const boxes = document.querySelectorAll(".digit-box");
    let guess = "";
    boxes.forEach(box => guess += box.value);

    if (guess.length < gameState.digitsCount) {
        showFeedback("Vui lòng điền đầy đủ các ô số trước khi nộp đáp án!", "warning");
        return;
    }

    fetch("/api/checkanswer", {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify({ sessionId: gameState.sessionId, guess: guess })
    })
    .then(res => res.json())
    .then(data => {
        if (data.isCorrect) {
            handleGameWin(data.secretCode);
        } else {
            handleGameIncorrect(data.message, guess);
        }
    })
    .catch(() => showToast("Lỗi kết nối khi nộp đáp án!", "danger"));
}

function handleGameWin(secretCode) {
    if (gameState.timerInterval) clearInterval(gameState.timerInterval);
    disableKeypadKeys();
    showFeedback(`🎉 CHÚC MỪNG, <strong>${authState.username || "Bạn"}</strong>! Đã bẻ khóa thành công! Mật mã là <strong>${secretCode}</strong>.`, "success");
    showToast(`Giải đúng! Mật mã: <strong>${secretCode}</strong> 🔓`, "success");

    if (gameState.mode === "Challenge" && gameState.challengeStage < 10) {
        setTimeout(() => startChallengeStage(gameState.challengeStage + 1), 3500);
    } else if (gameState.mode === "Infinite") {
        setTimeout(() => initiateNewGame(gameState.difficulty, gameState.digitsCount, "Infinite", 0, true, 0, 9), 3000);
    }
}

function handleGameIncorrect(serverMsg, guess) {
    showFeedback(serverMsg, "danger");
    clearAllDigits();
    addGuessToHistory(guess, serverMsg);
}

function addGuessToHistory(guess, feedbackMsg) {
    const list = document.getElementById("clues-list");
    const div = document.createElement("div");
    div.className = "clue-item clue-danger";
    div.innerHTML = `<span class="badge bg-danger me-2">✗</span><strong>Thử (${guess.split("").join(" ")}):</strong> ${feedbackMsg.replace("Sai rồi! Phản hồi: ", "")}`;
    list.prepend(div);
}

function triggerTimeOut() {
    showFeedback("⏰ HẾT GIỜ! Bạn đã thất bại trong chế độ Time Attack.", "danger");
    disableKeypadKeys();
    forfeitAndShowAnswer();
}

// ============================================================
// HINTS
// ============================================================
function requestHint() {
    const existing = document.getElementById("hintPromptModal");
    if (existing) existing.remove();

    const modalHtml = `
        <div class="modal fade" id="hintPromptModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content card">
                    <div class="modal-header border-bottom border-secondary-subtle">
                        <h5 class="modal-title fw-bold text-warning"><i class="fa-solid fa-lightbulb me-2"></i>Chọn Cấp Độ Gợi Ý</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="d-grid gap-2">
                            <button class="btn btn-outline-info text-start py-3" onclick="triggerFetchHint(1)">
                                <strong>Cấp 1 🔍</strong> — Tiết lộ 1 chữ số tồn tại trong mật mã.
                            </button>
                            <button class="btn btn-outline-warning text-start py-3" onclick="triggerFetchHint(2)">
                                <strong>Cấp 2 📍</strong> — Tiết lộ vị trí chính xác của 1 chữ số.
                            </button>
                            <button class="btn btn-outline-danger text-start py-3" onclick="triggerFetchHint(3)">
                                <strong>Cấp 3 ✂️</strong> — Loại bỏ các số không thể có trên bàn phím.
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;

    document.body.insertAdjacentHTML("beforeend", modalHtml);
    new bootstrap.Modal(document.getElementById("hintPromptModal")).show();
}

window.triggerFetchHint = function(level) {
    bootstrap.Modal.getInstance(document.getElementById("hintPromptModal"))?.hide();

    let currentGuess = "";
    document.querySelectorAll(".digit-box").forEach(box => currentGuess += box.value);

    fetch("/api/gethint", {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify({ sessionId: gameState.sessionId, level, guess: currentGuess })
    })
    .then(res => res.json())
    .then(data => {
        if (level === 1 || level === 2) {
            injectHintClue(data.hint, level);
        } else if (level === 3) {
            gameState.eliminatedDigits = data.eliminatedDigits;
            data.eliminatedDigits.forEach(num => {
                const btn = document.querySelector(`.keypad-btn[data-val='${num}']`);
                if (btn) btn.disabled = true;
            });
            injectHintClue(`Đã loại ${data.eliminatedDigits.join(", ")} ra khỏi bàn phím số!`, 3);
        }
    });
};

function injectHintClue(text, level) {
    const list = document.getElementById("clues-list");
    const div = document.createElement("div");
    div.className = "clue-item clue-success";
    div.innerHTML = `<span class="badge bg-success me-2">💡 Gợi ý ${level}</span> ${text}`;
    list.prepend(div);
}

// ============================================================
// SHOW ANSWER / FORFEIT
// ============================================================
function forfeitAndShowAnswer() {
    if (gameState.timerInterval) clearInterval(gameState.timerInterval);
    fetch(`/api/showanswer?sessionId=${gameState.sessionId}`, { headers: getAuthHeaders() })
        .then(res => res.json())
        .then(data => {
            showFeedback(`Đáp án đúng là: <strong class="text-danger fs-5">${data.secretCode}</strong>`, "danger");
            document.querySelectorAll(".digit-box").forEach((box, i) => {
                box.value = data.secretCode[i] || "";
                box.classList.add("text-danger");
            });
            disableKeypadKeys();
        });
}

function restartCurrentGame() {
    if (gameState.mode === "Daily") {
        startDailyPuzzle();
    } else if (gameState.mode === "Challenge") {
        startChallengeStage(gameState.challengeStage);
    } else {
        handleStartGame();
    }
}

// ============================================================
// STATISTICS
// ============================================================
function fetchAndShowStats() {
    if (!authState.userId) {
        showToast("Đăng nhập để xem thống kê cá nhân!", "warning");
        return;
    }
    fetch("/api/statistics", { headers: getAuthHeaders() })
        .then(res => res.json())
        .then(data => {
            document.getElementById("stats-played").innerText = data.gamesPlayed;
            document.getElementById("stats-winrate").innerText = `${Math.round(data.winRate)}%`;
            document.getElementById("stats-avgtime").innerText = `${Math.round(data.averageTimeSeconds)}s`;
            document.getElementById("stats-streak-current").innerText = data.currentStreak;
            document.getElementById("stats-streak-max").innerText = data.maxStreak;
        });
}

// ============================================================
// HELPERS
// ============================================================
function showFeedback(msg, type) {
    const alertBox = document.getElementById("guess-feedback-alert");
    alertBox.className = `alert alert-${type} mt-3 py-2 d-block`;
    alertBox.innerHTML = msg;
}

function disableKeypadKeys() {
    document.querySelectorAll(".keypad-btn").forEach(k => k.disabled = true);
    document.getElementById("submit-guess-btn").disabled = true;
}

function enableKeypadKeys() {
    document.querySelectorAll(".keypad-btn").forEach(k => k.disabled = false);
    document.getElementById("submit-guess-btn").disabled = false;
    gameState.eliminatedDigits = [];
}

function getModeDisplayName(mode) {
    const map = { Classic:"CHẾ ĐỘ CỔ ĐIỂN", TimeAttack:"⏱ CHẠY ĐUA THỜI GIAN", Infinite:"♾ CHẾ ĐỘ VÔ HẠN", Daily:"📅 THỬ THÁCH HÀNG NGÀY", Challenge:"🏆 THỬ THÁCH CHẶNG" };
    return map[mode] || "GIẢI MÃ SỐ";
}
