let selectedSemilleroId = null;

function selectSemillero(id, element) {
    // Desmarcar cualquier semillero seleccionado previamente
    document.querySelectorAll('.semillero.selected').forEach(el => {
        el.classList.remove('selected');
    });

    // Marcar el semillero actual como seleccionado
    element.classList.add('selected');
    selectedSemilleroId = id;

    // Actualizar el enlace del botón "Editar"
    const editLink = document.getElementById("editLink");
    editLink.href = `/Semillero/Edit/${selectedSemilleroId}`;
    editLink.classList.remove("disabled");
    editLink.querySelector(".crudN").classList.remove("disabled");

    // Actualizar el enlace del botón "Eliminar"
    const deleteLink = document.getElementById("deleteLink");
    deleteLink.href = `/Semillero/Delete/${selectedSemilleroId}`;
    deleteLink.classList.remove("disabled");
    deleteLink.querySelector(".crudN").classList.remove("disabled");
}