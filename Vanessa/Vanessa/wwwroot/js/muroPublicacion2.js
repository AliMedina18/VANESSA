const casita1 = document.getElementById("casita1");
const link1 = document.getElementById("link1");
const inf = document.getElementById("inf");
const desc = document.getElementById("desc");
const cerrar = document.getElementById("cerrar");
const p1 = document.getElementById("publicacionModal1");
const mP = document.getElementById("mostrarPublicaciones");
const mC = document.getElementById("gestionP");
const volver = document.getElementById("volver");
const abr = document.getElementById("abr");


function casitas(){
    casita1.classList.add("casita2");
    casita1.classList.remove("casita1");
    link1.classList.remove("hidden");
}

function casitas2(){
    casita1.classList.remove("casita2");
    casita1.classList.add("casita1");
    link1.classList.add("hidden");
}


function publicacion1(){
    inf.classList.add("hide");
    desc.classList.remove("hide");
}

function publicacion2(){
    inf.classList.remove("hide");
    desc.classList.add("hide");
}

function abrirP(){
    p1.classList.remove("hide");
}

function cerrarP(){
    p1.classList.add("hide");
}

function abrirGestion(){
    mP.classList.add("hide");
    mC.classList.remove("hide");
    volver.classList.remove("hide");
    abr.classList.add("hide");
}

function cerrarGestion(){
    mP.classList.remove("hide");
    mC.classList.add("hide");
    volver.classList.add("hide");
    abr.classList.remove("hide");
}

var images = $(".imagenPuB");

$(images).on("error", function(event) {
    $(event.target).css("display", "none");
});