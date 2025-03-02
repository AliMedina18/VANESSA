const proyectos = document.getElementById("proyectos");
const proyectosGestion = document.getElementById("gestionProyectos");
const formCrear = document.getElementById("formCrear");
const formEditar = document.getElementById("formEditar");
const mosPro = document.getElementById("mostrarPro");
const doc = document.getElementById("documentos");

function mostrarGestion(){
    proyectos.classList.add("hide");
    proyectosGestion.classList.remove("hide");
}

function ocultarGestion(){
    proyectosGestion.classList.add("hide");
    proyectos.classList.remove("hide");
}

function FormCrear(){
    formCrear.classList.remove("hide");

    formEditar.classList.add("hide");
}

function FormEditar(){
    formCrear.classList.add("hide");
    mosPro.classList.add("hide");
    formEditar.classList.remove("hide");
}

function FormMostrar(){
    formCrear.classList.add("hide");
    mosPro.classList.remove("hide");
    formEditar.classList.add("hide");
}

function abrirDocumento(){
    doc.classList.remove("hide");
    proyectos.classList.add("hide");
}