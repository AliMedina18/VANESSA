const infoS = document.getElementById("infoS");
const infoMv = document.getElementById("infoMv");
const infoPro = document.getElementById("infoPro");


function mostrarSomos(){
    infoS.classList.remove('hide');
}

function esconderSomos(){
    infoS.classList.add('hide');
}

function mostrarMv(){
    infoMv.classList.remove('hide');
}

function esconderMv(){
    infoMv.classList.add('hide');
}

function mostrarPro(){
    infoPro.classList.remove('hide');
}

function esconderPro(){
    infoPro.classList.add('hide');
}