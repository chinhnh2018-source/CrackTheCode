// ============================================================
// CRACK THE CODE - Điều hướng màn hình (Sảnh game <-> các game)
// ============================================================
const ALL_SCREENS = [
    "auth-screen", "game-select-screen", "menu-screen", "play-screen",
    "minesweeper-screen", "kakuro-screen", "kenken-screen"
];

function gotoScreen(id) {
    ALL_SCREENS.forEach(s => {
        const el = document.getElementById(s);
        if (el) el.classList.toggle("d-none", s !== id);
    });
    window.scrollTo(0, 0);
}

function gotoHub() {
    gotoScreen("game-select-screen");
}

function openGame(game) {
    if (game === "crack") {
        gotoScreen("menu-screen");
    } else if (game === "minesweeper") {
        gotoScreen("minesweeper-screen");
        if (window.initMinesweeper) initMinesweeper();
    } else if (game === "kakuro") {
        gotoScreen("kakuro-screen");
        if (window.initKakuro) initKakuro();
    } else if (game === "kenken") {
        gotoScreen("kenken-screen");
        if (window.initKenKen) initKenKen();
    }
}

document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-game]").forEach(card => {
        card.addEventListener("click", () => openGame(card.dataset.game));
    });
    document.querySelectorAll("[data-hub-back]").forEach(btn => {
        btn.addEventListener("click", gotoHub);
    });
});
