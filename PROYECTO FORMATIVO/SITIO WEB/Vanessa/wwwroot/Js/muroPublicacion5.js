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
const pubCP = document.getElementById("pubCP");
const pubCP2 = document.getElementById("pubCP2");
const trans1 = document.getElementById("trans1");
const trans2 = document.getElementById("trans2");
const trans3 = document.getElementById("trans3");
const trans4 = document.getElementById("trans4");
const formEditar = document.getElementById("formEditar");



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


function  translateCPp(){
    pubCP.classList.add("translateCP");
}

function  translateCPP(){
    pubCP.classList.remove("translateCP");
}

function  translateCPp2(){
    pubCP2.classList.add("translateCP");
}

function  translateCPP2(){
    pubCP2.classList.remove("translateCP");
}

function esconderActualizar(){
    pubCP2.classList.add("hide");
    pubCP2.classList.remove("grid");
    pubCP.classList.remove("hide");
    pubCP.classList.add("grid");
    trans1.classList.remove("hide");
    trans2.classList.remove("hide");
    trans3.classList.add("hide");
    trans4.classList.add("hide");
}

function mostrarActualizar(){
    pubCP2.classList.remove("hide");
    pubCP2.classList.add("grid");
    pubCP.classList.add("hide");
    pubCP.classList.remove("grid");
    trans1.classList.add("hide");
    trans2.classList.add("hide");
    trans3.classList.remove("hide");
    trans4.classList.remove("hide");
}

function mostrarFormEditar(){
    formEditar.classList.remove("hide");
}