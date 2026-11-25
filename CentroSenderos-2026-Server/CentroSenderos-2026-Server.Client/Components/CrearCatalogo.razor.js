export function abrir(dialogo, referencia) {
    dialogo.addEventListener("cancel", evento => {
        evento.preventDefault();
        if (!dialogo.querySelector('[role="alertdialog"]')) {
            referencia.invokeMethodAsync("SolicitarCierre");
        }
    });
    dialogo.showModal();
}

export function cerrar(dialogo) {
    if (dialogo?.open) dialogo.close();
}
