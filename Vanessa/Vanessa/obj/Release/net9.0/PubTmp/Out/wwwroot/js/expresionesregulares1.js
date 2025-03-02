btnR = document.getElementById("btnR");

    const correo = document.getElementById('correo');
    const name = document.getElementById('name');
    const documentoR = document.getElementById('documentoR');
    const passRw = document.getElementById("passRw");
    const passRe = document.getElementById("passRe");


    let validarNombre =  /^[a-z0-9_-]{3,36}$/;
    let validarCorreo =  /\w+@\w+\.+[a-z]/;
    let validarDocumento = /^\d{8,16}$/;
    let validarContraseña = /^(?=\w*\d)(?=\w*[A-Z])(?=\w*[a-z])\S{8,16}$/;
    
    
    let verificar = 0;


    if(passRw.value == passRe.value){
        if(validarContraseña.test(passRw.value) ){
            alert('La contraseña ingresada es valida');
            verificar = verificar + 1;
        }else{
            alert('La contraseña no es valida, ingrese otra');
            passRw.style.color = "red";
            passRe.style.color = "red";
        }      
    }else{
        alert('La contraseña no coincide');
    }
    if( validarDocumento.test(documentoR.value) ){
        alert('El documento ingresado es valido');
        verificar = verificar + 1;
    }else{
        alert('El documento ingresado no es valido, ingrese otro');
        documentoR.style.color = "red";
    }
    if( validarCorreo.test(correo.value) ){
        alert('El correo ingresado es valido');
        verificar = verificar + 1;
    }else{
        alert('El correo no es valido, ingrese otro');
        correo.style.color = "red";
    }
    if( validarNombre.test(name.value) ){        
        alert('El nombre ingresado es valido');
        verificar = verificar + 1;
    }else{
        alert('El nombre ingresado no es valido, ingrese otro');
        name.style.color =  "red";
    }
    if(verificar == 4){
        document.getElementById("formUp").submit();
    }


btnR.addEventListener('click', enviarDatos = (event) =>{
    
event.preventDefault()

    
});

function mostrarAlerta(mensaje) {
    var customAlert = document.getElementById("custom-alert");
    var alertMessage = document.getElementById("alert-message");
    var closeBtn = document.getElementById("close-alert");

    alertMessage.textContent = mensaje;
    customAlert.classList.remove("esconderAlert");

    closeBtn.addEventListener("click", function() {
        customAlert.style.display = "none";
    });
}