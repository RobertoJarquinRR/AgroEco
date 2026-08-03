// esta funcion la cree para compartirla me di cuenta que en todos tenia que repetir codigo mejor crea uno solo que comparta la logica 
//nota: antes no me dejba tendria que usar un nombre diferente a btnClicked porque ya estaba declarado en el otro archivo
//ojo ponganla antes de la logica del modulo para que se ejecute primero
function updateSidebar(selector = '.sidebar ul li a') {
    //selecionar el boton algo asi como el radiobutton de WinForms
    const btnClicked =document.querySelectorAll(selector);

    //validacion por si no existe el selector por si acaso se les olvida pero si no responde el sidebar 
    //es porque no linkearon este JS a su html o no pusieron el selector correcto
    if(btnClicked.length === 0) return;

    //comprobar la pagina actual  es para agregar el active al boton que corresponde a la pagina actual
    const currentPage = window.location.pathname;

    
    btnClicked.forEach(btn => {
        //comprobar si el href del boton es igual a la pagina actual
        const href = btn.getAttribute('href');
        //los hice sin {} para que sea mas legible lo es para mi xd tunai que se pueda hacerlo lo mismo que en c#
        if(href=== currentPage) btn.classList.add('active');
        else btn.classList.remove('active');

        btn.addEventListener('click', (e) =>
        {
            e.preventDefault();
            
            //remover la clase active de todos los botones
            btnClicked.forEach(b => b.classList.remove('active'));
            //agregar la clase active al boton clickeado
            btn.classList.add('active');

            //validacion extra por si el boton no tiene href o es un link vacio
            if (!href || href === '#') return;
            //validar no recarga si es el mismo boton que ya esta activo
            if(href && href !== currentPage) {
                //redirigir a la pagina del boton clickeado
                window.location.href = href;
            }
        })
    })
    


}