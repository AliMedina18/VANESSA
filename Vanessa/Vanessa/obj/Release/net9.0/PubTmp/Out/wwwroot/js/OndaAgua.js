 // Función para generar ondas concéntricas dentro de una onda principal
 function generateConcentricWaves(x, y) {
    const numberOfWaves = 10; // Número de ondas a generar al hacer clic
    const body = document.body;

    // Crear la onda principal al hacer clic
    const wave = document.createElement("div");
    wave.classList.add("wave");

    // Tamaño aleatorio para la onda principal (80px a 250px)
    const size = Math.floor(Math.random() * 170) + 80;  // Tamaño ajustado entre 80px y 250px
    // Posición basada en las coordenadas del clic
    const topPosition = y + "px";
    const leftPosition = x + "px";
    // Tiempo de animación aleatorio para la onda principal (15 a 25 segundos)
    const animationDuration = Math.floor(Math.random() * 10) + 15; // entre 15 y 25 segundos
    // Retraso aleatorio en la animación de la onda principal
    const delay = Math.random() * 6;

    // Establecer tamaño, posición, y animación de la onda principal
    wave.style.width = `${size}px`;
    wave.style.height = `${size}px`;
    wave.style.top = topPosition;
    wave.style.left = leftPosition;
    wave.style.animationDuration = `${animationDuration}s`; // Tiempo de animación aleatorio
    wave.style.animationDelay = `-${delay}s`;

    // Crear ondas concéntricas dentro de la onda principal
    const numInnerWaves = Math.floor(Math.random() * 2) + 3; // Entre 3 y 5 ondas internas

    for (let j = 0; j < numInnerWaves; j++) {
        const innerWave = document.createElement("div");
        innerWave.classList.add("inner-wave");

        // Tamaño aleatorio para las ondas internas (50% a 90% del tamaño de la onda principal)
        const innerSize = Math.floor(size * (Math.random() * 0.4 + 0.5)); // Entre 50% y 90% del tamaño de la onda principal
        innerWave.style.width = `${innerSize}px`;
        innerWave.style.height = `${innerSize}px`;
        innerWave.style.top = `${(size - innerSize) / 2}px`; // Centrado dentro de la onda principal
        innerWave.style.left = `${(size - innerSize) / 2}px`;

        // Tiempo de animación aleatorio para la onda interna (18 a 30 segundos)
        const innerAnimationDuration = Math.floor(Math.random() * 12) + 18; // entre 18 y 30 segundos
        // Retraso aleatorio para las ondas internas
        const innerDelay = Math.random() * 5;
        innerWave.style.animationDuration = `${innerAnimationDuration}s`; // Tiempo de animación aleatorio
        innerWave.style.animationDelay = `-${innerDelay}s`;

        // Añadir la onda interna dentro de la onda principal
        wave.appendChild(innerWave);
    }

    // Añadir la onda principal al cuerpo
    body.appendChild(wave);
}

// Escuchar el evento de clic en el fondo
document.body.addEventListener('click', function(event) {
    const x = event.clientX;  // Obtener la posición X del clic
    const y = event.clientY;  // Obtener la posición Y del clic
    generateConcentricWaves(x, y);  // Llamar a la función para generar ondas en la ubicación del clic
});

// Generar ondas aleatorias al cargar la página (ondas predeterminadas)
function generateInitialWaves() {
    const numberOfWaves = 30; // Número de ondas a generar al cargar la página
    const body = document.body;

    for (let i = 0; i < numberOfWaves; i++) {
        const x = Math.random() * window.innerWidth;
        const y = Math.random() * window.innerHeight;
        generateConcentricWaves(x, y);
    }
}

// Llamar a la función para generar ondas aleatorias al cargar la página
generateInitialWaves();