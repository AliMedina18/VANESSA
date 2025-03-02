const sonidoTabla = document.getElementById("table-broke-sound");
const form = document.getElementById("formulario");
const formA = document.getElementById("formularioA");
// const abejita = document.getElementById("abeja-btn-retroceder");
let sound = "off";

const btnDatos = document.getElementById("btn-Datos");
const btnIngreso = document.getElementById("btnI");
const btnRegistro = document.getElementById("btnR");

const cabezaMarcela = document.getElementById("cabeza");
const lM = document.getElementById ("lM");
const Ml = document.getElementById ("Ml");

const acc = document.getElementById("acc");
acc.onclick = function(){
    formA.classList.remove("hidden");
    btnDatos.classList.add("caida");
    btnIngreso.classList.remove("hidden");
    // btnDatos.appendChild(btnIngreso);
    btnRegistro.classList.add("hidden");
    btnRegistro.classList.add("btnS");
    btnIngreso.classList.remove("btnS");
    cabezaMarcela.classList.add("caida2");
    lM.classList.add("caidaLm");
    Ml.classList.add("caidaLengua");
    abeja();
};

const atras = document.getElementById("retroceder");

function retroceder(){
    if(h123 == 1){
        formUp.classList.remove("hidden");
        formInv.classList.add("hidden");
        btnRegistro.classList.add("hidden");
        h123 = 2;
    }
    else{
        formA.classList.add("hidden");
        form.classList.add("hidden");   
        btnRegistro.classList.add("hidden");
        unbroke(); 
    }    
}


const reg = document.getElementById("reg");
reg.onclick = function(){
    form.classList.remove("hidden");
    nameIn.classList.remove("hidden");
    btnDatos.classList.add("caida");
    btnRegistro.classList.remove("hidden");
    btnIngreso.classList.add("btnS");
    btnRegistro.classList.remove("btnS");
    // btnDatos.appendChild(btnRegistro);
    btnIngreso.classList.add("hidden");
    cabezaMarcela.classList.add("caida2");
    lM.classList.add("caidaLm");
    Ml.classList.add("caidaLengua");
    if(h123 == 1 || h123 == 2){
        btnRegistro.classList.add("hidden");
    }
    abeja();
};





const r1 = document.getElementById ("tablaR1");
const r2 = document.getElementById ("tablaR2");

const libelula = document.getElementById("libelula");
const libelulaa = document.getElementById("libelulaa");

const hierbas = document.getElementById("marco").children;
const hierrba1 = hierbas[19];
const hierrba2 = hierbas[20];
const hierrba3 = hierbas[31];
const hierrba4 = hierbas[16];
const hierrba5 = hierbas[17];
const hierrba6 = hierbas[12];
const hierrba7 = hierbas[11];
const hierrba8 = hierbas[30];


function sacarLenguaMarcela(){
    Ml.classList.add("lengua");
    Ml.classList.add("lenguaM");

    setTimeout(function(){

        Ml.classList.remove("lengua");
        Ml.classList.remove("lenguaM");
    }, 4000);
}


const marta = document.getElementById("marta");

function broke(){
    marta.style.transform = 'translateY(33%)';
    marta.style.transition = '.3s ease';
    cabezaMarcela.classList.add("caida2");
    lM.classList.add("caidaLm");
    Ml.classList.add("caidaLengua");
    r1.classList.add("tablaR1");
    r2.classList.add("tablaR2");
    btnDatos.classList.add("caida");

        if(sound == "off"){
            acc.classList.add("hidden");
            reg.classList.add("hidden");
            // background-image: ;
    
            hierrba1.classList.add("hidden");
            hierrba2.classList.add("hidden");
            hierrba3.classList.add("hidden");
            hierrba4.classList.add("hidden");
            hierrba5.classList.add("hidden");
            hierrba6.classList.add("hidden");
            hierrba7.classList.add("hidden");
            hierrba8.classList.add("hidden");       
            
        }else{
            sound = "on";
    }
}

function unbroke(){
    r1.classList.remove("tablaR1");
    r2.classList.remove("tablaR2");
            btnDatos.classList.remove("caida");   
            btnDatos.classList.remove("caida2");
            acc.classList.remove("hidden");
            reg.classList.remove("hidden");
            // background-image: ;
    
            hierrba1.classList.remove("hidden");
            hierrba2.classList.remove("hidden");
            hierrba3.classList.remove("hidden");
            hierrba4.classList.remove("hidden");
            hierrba5.classList.remove("hidden");
            hierrba6.classList.remove("hidden");
            hierrba7.classList.remove("hidden");
            hierrba8.classList.remove("hidden");
            nameIn.classList.add("hidden");
            
            cabezaMarcela.classList.remove("caida2");
            lM.classList.remove("caida");
            Ml.classList.remove("caidaLengua");
            marta.style.transform = 'translateY(0%) rotate(-20deg)';
            marta.style.transition = '.3s ease';
            
}

const nameIn = document.getElementById("name");
nameIn.onclick = function(){
    nameIn.classList.add("back-color");
};



// const identificacion = document.getElementById("id");
// identificacion.onclick = function(){
//     identificacion.classList.add("back-color");
// };

// const tel = document.getElementById("tel");
// tel.onclick = function(){
//     tel.classList.add("back-color");
// };

// const  correo = document.getElementById("correo");
// correo.onclick = function(){
//     correo.classList.add("back-color");
// };

// const curso = document.getElementById("curso");
// curso.onclick = function(){
//     curso.classList.add("back-color");
// };

// const jorn = document.getElementById("jornada");
// jorn.onclick = function(){
//     jorn.classList.add("back-color");
// };

// const corr = document.getElementById("corr");
// corr.onclick = function(){
//     corr.classList.add("back-color");
// };

// const pass = document.getElementById("pass");
// pass.onclick = function(){
//     pass.classList.add("back-color");
// };

// const iden = document.getElementById("iden");
// iden.onclick = function(){
//     iden.classList.add("back-color");
// };

libelula.classList.add("moveLibe");
libelulaa.classList.add("moveLibe2");



// acc.addEventListener("click", broke);
// reg.addEventListener("click", broke);(broke);

lM.addEventListener("mousemove", sacarLenguaMarcela);

atras.addEventListener("click", retroceder);
broke();