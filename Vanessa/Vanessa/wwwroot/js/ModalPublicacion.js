document.addEventListener("DOMContentLoaded", function () {
    // Función para abrir el modal con los datos de la publicación
    function openModal(postId) {
        fetch(`/Publicacion/GetPostData?id=${postId}`)
            .then(response => response.json())
            .then(post => {
                if (!post) {
                    console.error("Error: No se encontró la publicación");
                    return;
                }

                // Insertar datos en el modal
                document.getElementById('modalTitle').textContent = post.nombrePublicacion || "Sin título";
                document.getElementById('modalImage').src = post.imagenPublicacion || "ruta_por_defecto.jpg";
                document.getElementById('modalContent').textContent = post.contenidoPublicacion || "Sin contenido";
                document.getElementById('modalType').textContent = post.tipoPublicacion || "No especificado";
                document.getElementById('modalPlace').textContent = post.lugarPublicacion || "No especificado";
                document.getElementById('modalDate').textContent = post.fechaPublicacion || "No disponible";
                document.getElementById('modalTime').textContent = post.horaPublicacion || "No disponible";
                document.getElementById('modalUser').textContent = post.usuarioPublicacion || "Anónimo";

                // Manejo de archivos PDF adjuntos
                const pdfList = document.getElementById('modalPDFList');
                pdfList.innerHTML = ''; // Limpiar lista antes de agregar nuevos elementos

                if (post.actividadesPublicacion && post.actividadesPublicacion.length > 0) {
                    post.actividadesPublicacion.forEach(pdf => {
                        const li = document.createElement('li');
                        const link = document.createElement('a');
                        link.href = pdf.url;
                        link.textContent = pdf.nombre || "Archivo adjunto";
                        link.target = '_blank';
                        li.appendChild(link);
                        pdfList.appendChild(li);
                    });
                } else {
                    const noFilesMsg = document.createElement('li');
                    noFilesMsg.textContent = "No hay archivos adjuntos.";
                    pdfList.appendChild(noFilesMsg);
                }

                // Mostrar el modal
                const modal = document.getElementById('modal-1');
                modal.style.display = 'flex';
                setTimeout(() => {
                    modal.querySelector('.modal-content').classList.add('show');
                }, 100);
            })
            .catch(error => console.error('Error al cargar los datos:', error));
    }

    // Función para cerrar el modal
    function closeModal() {
        const modal = document.getElementById('modal-1');
        modal.querySelector('.modal-content').classList.remove('show');
        setTimeout(() => {
            modal.style.display = 'none';
        }, 300);
    }

    // Cerrar modal al hacer clic fuera de la caja modal
    window.onclick = function (event) {
        const modal = document.getElementById('modal-1');
        if (event.target === modal) {
            closeModal();
        }
    };

    // Cerrar modal al hacer clic en el botón de cerrar
    document.querySelector('.close').addEventListener('click', closeModal);

    // Asignar eventos a cada publicación
    document.querySelectorAll('.post').forEach(post => {
        post.addEventListener('click', function () {
            const postId = this.getAttribute('data-post-id');
            openModal(postId);
        });
    });
});
