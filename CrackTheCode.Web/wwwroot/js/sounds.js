// ============================================================
// CRACK THE CODE - Trình quản lý âm thanh (file local)
// Hiệu ứng + nhạc nền, có thể bật/tắt, lưu vào localStorage.
// ============================================================
const Sound = (() => {
    let muted = localStorage.getItem("muted") === "true";

    // Âm lượng riêng cho từng hiệu ứng
    const SFX = {
        key:   { src: "sounds/key.wav",   vol: 0.35 },
        click: { src: "sounds/click.wav", vol: 0.45 },
        win:   { src: "sounds/win.wav",   vol: 0.55 },
        wrong: { src: "sounds/wrong.wav", vol: 0.50 },
        hint:  { src: "sounds/hint.wav",  vol: 0.50 },
        lose:  { src: "sounds/lose.wav",  vol: 0.50 },
    };

    // Tải sẵn các hiệu ứng
    for (const k in SFX) {
        const a = new Audio(SFX[k].src);
        a.preload = "auto";
        SFX[k].audio = a;
    }

    // Nhạc nền (lặp vô hạn, âm lượng thấp)
    const bgm = new Audio("sounds/bgm.wav");
    bgm.loop = true;
    bgm.volume = 0.22;

    let bgmWanted = false; // người dùng đang ở trạng thái muốn nghe nhạc

    function play(name) {
        if (muted) return;
        const s = SFX[name];
        if (!s) return;
        try {
            // clone để các hiệu ứng có thể chồng lên nhau
            const c = s.audio.cloneNode();
            c.volume = s.vol;
            c.play().catch(() => {});
        } catch (e) { /* bỏ qua */ }
    }

    function startBgm() {
        bgmWanted = true;
        if (!muted) bgm.play().catch(() => {});
    }

    function stopBgm() {
        bgmWanted = false;
        bgm.pause();
    }

    function isMuted() { return muted; }

    function setMuted(m) {
        muted = m;
        localStorage.setItem("muted", m ? "true" : "false");
        if (m) {
            bgm.pause();
        } else if (bgmWanted) {
            bgm.play().catch(() => {});
        }
        updateMuteButton();
    }

    function toggleMute() {
        setMuted(!muted);
        return muted;
    }

    function updateMuteButton() {
        const icon = document.getElementById("sound-icon");
        const btn = document.getElementById("sound-toggle-btn");
        if (!icon) return;
        icon.className = muted ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-high";
        if (btn) btn.title = muted ? "Bật âm thanh" : "Tắt âm thanh";
    }

    // Trình duyệt chặn tự phát: khởi động nhạc nền ở lần tương tác đầu tiên
    function armAutoStart() {
        const kick = () => {
            startBgm();
            document.removeEventListener("click", kick);
            document.removeEventListener("keydown", kick);
        };
        document.addEventListener("click", kick);
        document.addEventListener("keydown", kick);
    }

    document.addEventListener("DOMContentLoaded", () => {
        updateMuteButton();
        const btn = document.getElementById("sound-toggle-btn");
        if (btn) btn.addEventListener("click", toggleMute);
        armAutoStart();

        // Âm thanh nhấn nút chung cho mọi nút (trừ bàn phím số -> dùng âm "key" riêng)
        document.addEventListener("click", (e) => {
            const el = e.target.closest(".btn");
            if (!el) return;
            if (el.classList.contains("keypad-btn")) return;
            play("click");
        });
    });

    return { play, startBgm, stopBgm, toggleMute, isMuted, setMuted };
})();
