const btnAct = document.getElementById("btnAct");
const btnTca = document.getElementById("btnTca");
const form1 = document.getElementById("form1");
const form2 = document.getElementById("form2");
const registros = document.getElementById("registros");
const formE = document.getElementById("formE");
const formD = document.getElementById("formD");
const formME = document.getElementById("formME");
const formMD = document.getElementById("formMD");
const irModD = document.getElementById("irModD");
const irModE = document.getElementById("irModE");
const formModD = document.getElementById("formModD");
const formModE = document.getElementById("formModE");

function mostrarRegistros(){
    registros.classList.remove("hide");
}
function esconderRegistros(){
    registros.classList.add("hide");
}

function mover(){
    btnAct.classList.add("des");
    btnTca.classList.remove("des");
    form1.classList.add("esc2");
    form2.classList.remove("esc");
}

function desMover(){
    btnAct.classList.remove("des");
    btnTca.classList.add("des");
    form1.classList.remove("esc2");
    form2.classList.add("esc");
}

function esconderEs(){
    formE.classList.add("hide");
    formD.classList.add("hide");
    formME.classList.remove("hide");
    formMD.classList.add("hide");
    irModE.classList.remove("hide");
    irModD.classList.add("hide");
    formModD.classList.add("hide");
    formModE.classList.add("hide");
}

function esconderDo(){
    formE.classList.add("hide");
    formD.classList.add("hide");
    formMD.classList.remove("hide");
    formME.classList.add("hide");
    irModD.classList.remove("hide");
    irModE.classList.add("hide");
    formModD.classList.add("hide");
    formModE.classList.add("hide");
}

function esconderEyD(){
    formE.classList.remove("hide");
    formD.classList.remove("hide");
    formMD.classList.add("hide");
    formME.classList.add("hide");
    formModD.classList.add("hide");
    formModE.classList.add("hide");
    irModD.classList.add("hide");
    irModE.classList.add("hide");
}

function irModDf(){
    formMD.classList.add("hide");
    formME.classList.add("hide");
    formModD.classList.remove("hide");
    formModE.classList.add("hide");
}
function irModEf(){
    formMD.classList.add("hide");
    formME.classList.add("hide");
    formModD.classList.add("hide");
    formModE.classList.remove("hide");

}

btnAct.addEventListener("click", mover);
btnTca.addEventListener("click", desMover);