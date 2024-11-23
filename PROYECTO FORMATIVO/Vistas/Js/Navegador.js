const navegador = document.getElementById('barraNavegador');
const toggleBtn = document.getElementById('toggleBtn');
const encabezado = document.querySelector('h1');
let isVisible = true;

toggleBtn.addEventListener('click', () => {
    // Alternar visibilidad de la barra de navegación
    isVisible = !isVisible;
    if (isVisible) {
        navegador.classList.remove('ocultar');
        encabezado.classList.remove('ocultar-h1');
        toggleBtn.style.left = '385px';
    } else {
        navegador.classList.add('ocultar');
        encabezado.classList.add('ocultar-h1');
        toggleBtn.style.left = '-8px';
    }
});