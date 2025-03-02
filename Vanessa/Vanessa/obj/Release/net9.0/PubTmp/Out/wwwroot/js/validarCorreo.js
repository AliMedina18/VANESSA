function validarCorreo() {
    const emailInput = document.getElementById("correo");
    const feedback = document.getElementById("email-feedback");
    const icon = document.getElementById("email-icon");
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailInput.value) {
        feedback.textContent = "Por favor, ingresa tu correo.";
        feedback.className = "feedback error";
        icon.textContent = "❌";
        return false;
    } else if (!emailPattern.test(emailInput.value)) {
        feedback.textContent = "El formato del correo no es válido.";
        feedback.className = "feedback error";
        icon.textContent = "❌";
        return false;
    } else {
        feedback.textContent = "Correo válido.";
        feedback.className = "feedback success";
        icon.textContent = "✅";
        return true;
    }
}