const somb1 = document.getElementById("somb1");
const somb2 = document.getElementById("somb2");
const somb3 = document.getElementById("somb3");
const somb4 = document.getElementById("somb4");
const somb5 = document.getElementById("somb5");

function sombreado1(){
    
    setTimeout(function(){
        somb1.classList.add("sombreado");
    }, 115);
    somb1.classList.remove("sombreado");
}
function sombreado2(){
    somb2.classList.add("sombreado");
}
function sombreado3(){
    somb3.classList.add("sombreado");
}
function sombreado4(){
    somb4.classList.add("sombreado");
}
function sombreado5(){
    somb5.classList.add("sombreado");
}