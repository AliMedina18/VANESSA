 // Función para mostrar u ocultar la contraseña
 function togglePasswordVisibility(inputId) {
    const passwordField = document.getElementById(inputId);
    const passwordIcon = document.getElementById(inputId === 'NuevaContraseña' ? 'password-icon' : 'confirm-password-icon');
    const icon = passwordIcon.querySelector("i");

    if (passwordField.type === "password") {
        passwordField.type = "text";
        icon.classList.remove("fa-eye");
        icon.classList.add("fa-eye-slash"); // Cambio de ícono a "ojo cerrado"
    } else {
        passwordField.type = "password";
        icon.classList.remove("fa-eye-slash");
        icon.classList.add("fa-eye"); // Cambio de ícono a "ojo abierto"
    }
}

// Función para verificar la fortaleza de la contraseña
function verificarFortaleza() {
    const password = document.getElementById("NuevaContraseña").value;
    const strengthIndicator = document.getElementById("password-strength");
    if (!password) {
        strengthIndicator.textContent = "";
        return;
    }

    const weak = password.length < 6;
    const strong = /[a-z]/.test(password) && /[A-Z]/.test(password) && /\d/.test(password) && password.length >= 8;
    const medium = !weak && !strong;

    strengthIndicator.textContent = strong
        ? "Fortaleza: Fuerte"
        : medium
        ? "Fortaleza: Media"
        : "Fortaleza: Débil";

    strengthIndicator.className = strong
        ? "password-strength strong"
        : medium
        ? "password-strength medium"
        : "password-strength weak";
}

// Función para validar si las contraseñas coinciden
function validarCoincidencia() {
    const nuevaPassword = document.getElementById("NuevaContraseña").value;
    const confirmarPassword = document.getElementById("ConfirmarContraseña").value;
    const feedback = document.getElementById("password-match-feedback");

    if (nuevaPassword === confirmarPassword) {
        feedback.textContent = "Las contraseñas coinciden";
        feedback.className = "feedback success";
    } else {
        feedback.textContent = "Las contraseñas no coinciden";
        feedback.className = "feedback error";
    }
}