function toggleMenu() {
    var menu = document.getElementById("toolbarMenu");
    menu.classList.toggle("show");
}

// Cerrar menú si se hace clic fuera
document.addEventListener("click", function(event) {
    var menu = document.getElementById("toolbarMenu");
    var button = document.querySelector(".toolbar-btn");
    if (!button.contains(event.target) && !menu.contains(event.target)) {
        menu.classList.remove("show");
    }
});