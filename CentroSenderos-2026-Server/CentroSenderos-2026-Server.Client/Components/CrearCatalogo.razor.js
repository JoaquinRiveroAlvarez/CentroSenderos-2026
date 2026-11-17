export function abrir(dialogo, referencia) {
    dialogo.addEventListener("cancel", evento => {
        evento.preventDefault();
        referencia.invokeMethodAsync("SolicitarCierre");
    });
    dialogo.addEventListener("click", evento => {
        const rect = dialogo.getBoundingClientRect();
        if (evento.target === dialogo && (evento.clientX < rect.left || evento.clientX > rect.right || evento.clientY < rect.top || evento.clientY > rect.bottom)) {
            referencia.invokeMethodAsync("SolicitarCierre");
        }
    });
    dialogo.showModal();
}

export function cerrar(dialogo) {
    if (dialogo?.open) dialogo.close();
}
