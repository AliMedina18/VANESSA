const cerrar = document.getElementById("cerrar");
const abrir = document.getElementById("abrir");
const modal = document. getElementById("modalS");
const modalC = document. getElementById("modalC");
const gestion = document.getElementById("gestionS");
const modalesA = document.getElementsByClassName("modalSemillero");
const semiC = document.getElementById("semillerosCreados");
const eliS = document.getElementById("eliminarSemillero");
let t = "abrir";

function cerrarModal(){
    modal.classList.add("hide");
}

function abrirModal(){
    modal.classList.remove("hide");    
}

function abrirGestion(){
    gestion.classList.remove("translate");
}

function cerrarGestion(){
    gestion.classList.add("translate");

}

function abrirCrear(){
    modalC.classList.remove("hide");
}

function cerrarCrear(){
    modalC.classList.add("hide");

}

function moverCrud(){
    eliS.classList.remove("translate2");
    semiC.classList.add("translate2");
}

function moverCrudEd(){
    eliS.classList.add("translate2");
    semiC.classList.remove("translate2");
}

// cerrar.addEventListener("click", cerrarModal());

