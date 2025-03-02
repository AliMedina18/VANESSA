document.addEventListener("DOMContentLoaded", function () {
    let selectedPost = null;

    // Asignar evento a todos los posts para seleccionarlos
    document.querySelectorAll(".post").forEach(post => {
        post.addEventListener("click", function () {
            selectPost(this);
        });
    });

    // Función para seleccionar una publicación
    function selectPost(element) {
        // Remover selección previa
        document.querySelectorAll(".post-sidebar").forEach(p => p.classList.remove("selected"));
        if (element.querySelector(".post-sidebar")) {
            element.querySelector(".post-sidebar").classList.add("selected");
        }

        selectedPost = element;

        // Obtener el ID de la publicación seleccionada
        const postId = selectedPost.getAttribute("data-post-id");

        // Habilitar los botones de edición y eliminación
        document.getElementById("editPost").classList.remove("disabled");
        document.getElementById("deletePost").classList.remove("disabled");

        // **Actualizar el `href` del botón Editar**
        document.getElementById("editPost").setAttribute("href", `/Publicacion/Editar/${postId}`);
    }

    // **Evento para el botón Editar**
    document.getElementById("editPost").addEventListener("click", function (event) {
        if (!selectedPost) {
            event.preventDefault(); // Evita que el enlace funcione sin selección
            alert("⚠️ Debes seleccionar una publicación antes de editar.");
        }
    });

    // **Evento para eliminar publicación**
    document.getElementById("deletePost").addEventListener("click", function (event) {
        event.preventDefault();
        if (!selectedPost) {
            alert("⚠️ Debes seleccionar una publicación antes de eliminar.");
            return;
        }

        const postId = selectedPost.getAttribute("data-post-id");

        if (confirm("❗ ¿Estás seguro de que quieres eliminar esta publicación? Esta acción no se puede deshacer.")) {
            fetch(`/Publicacion/Eliminar/${postId}`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("✅ Publicación eliminada correctamente.");
                        selectedPost.remove();
                        selectedPost = null;
                        document.getElementById("editPost").classList.add("disabled");
                        document.getElementById("deletePost").classList.add("disabled");
                    } else {
                        alert("❌ Error al eliminar la publicación.");
                    }
                })
                .catch(error => {
                    console.error("Error:", error);
                    alert("❌ Hubo un problema al procesar la solicitud.");
                });
        }
    });
});
