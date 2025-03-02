document.addEventListener("DOMContentLoaded", function () {
    // Establece la fecha y hora actual por defecto
    let now = new Date();
    let fechaActual = now.toISOString().slice(0, 16); // Formato YYYY-MM-DDTHH:MM
    document.getElementById("postDate").value = fechaActual;

    // Validaciones en tiempo real
    document.getElementById("postTitle").addEventListener("input", validarTitulo);
    document.getElementById("postImage").addEventListener("change", validarImagen);
    document.getElementById("postContent").addEventListener("input", validarContenido);
    document.getElementById("postDate").addEventListener("input", validarFecha);
});

function validarTitulo() {
    let title = document.getElementById("postTitle").value.trim();
    let error = document.getElementById("titleError");
    if (title.length < 5 || title.length > 100) {
        error.innerText = "El título debe tener entre 5 y 100 caracteres.";
        return false;
    }
    error.innerText = "";
    return true;
}

function validarImagen() {
    let image = document.getElementById("postImage").files[0];
    let error = document.getElementById("imageError");
    if (image) {
        if (image.size > 2 * 1024 * 1024) { // Máx. 2MB
            error.innerText = "La imagen no debe superar los 2MB.";
            return false;
        }
    }
    error.innerText = "";
    return true;
}

function validarContenido() {
    let content = document.getElementById("postContent").value.trim();
    let error = document.getElementById("contentError");
    if (content.length < 10) {
        error.innerText = "El contenido debe tener al menos 10 caracteres.";
        return false;
    }
    error.innerText = "";
    return true;
}

function validarFecha() {
    let inputFecha = document.getElementById("postDate").value;
    let error = document.getElementById("dateError");
    let fechaSeleccionada = new Date(inputFecha);
    let fechaActual = new Date();

    if (fechaSeleccionada < fechaActual) {
        error.innerText = "La fecha no puede ser en el pasado.";
        return false;
    }
    error.innerText = "";
    return true;
}

function validarFormulario() {
    let valido = true;
    if (!validarTitulo()) valido = false;
    if (!validarImagen()) valido = false;
    if (!validarContenido()) valido = false;
    if (!validarFecha()) valido = false;

    return valido;
}