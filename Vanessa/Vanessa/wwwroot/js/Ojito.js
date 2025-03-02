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