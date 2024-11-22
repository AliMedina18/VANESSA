 // Mostrar la ventana emergente al cargar la página
 window.onload = function() {
    document.getElementById('popup').style.display = 'flex';
};

// Función para cerrar la ventana emergente
function closePopup() {
    document.getElementById('popup').style.display = 'none';
}