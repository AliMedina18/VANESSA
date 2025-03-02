const RadioOption = document.getElementsByName('inv');
const btnC = document.getElementById('btnC');
const formUp = document.getElementById('formUp');
const formInv = document.getElementById('formInv');

 
h = 0;
h3 = 0;
h123 = 0;

function btnContinuar(){
 if(h == 0){
   btnC.classList.add("hidden");
   btnRegistro.classList.remove("hidden");
   h = 1;
 }
 if(h == 1){
   h = 0
   btnC.classList.remove("hidden");
   btnRegistro.classList.add("hidden");
 }
 h123 = 1;
}

function RbtnContinuar(){
    if(h3 == 0){
        btnC.classList.add("hidden");
        btnRegistro.classList.remove("hidden");
        h3 = 1;
     }
     if(h3 == 1){
        h3 = 0;
     }
     h123 = 0;

}

function formContinuar(){
   formUp.classList.add("hidden");
   formInv.classList.remove("hidden");
   btnC.classList.remove("hidden");
   btnRegistro.classList.add("hidden");
   h123 = 1;
}