export function registrar(raiz, referencia) {
    let activo = true;
    const abierto = () => activo && raiz.isConnected && raiz.dataset.abierto === "true";
    const cerrar = foco => referencia.invokeMethodAsync("Cerrar", foco);
    const fuera = evento => { if (abierto() && !raiz.contains(evento.target)) cerrar(false); };
    const focoFuera = evento => {
        if (abierto() && evento.relatedTarget && !raiz.contains(evento.relatedTarget)) cerrar(false);
    };
    const teclado = evento => {
        if (!abierto()) return;
        if (evento.key === "Escape") {
            evento.preventDefault(); evento.stopPropagation(); cerrar(true); return;
        }
        const entrada = raiz.querySelector(".selector-buscador input");
        if (evento.key === "Enter" && evento.target === entrada) evento.preventDefault();
        if (evento.key === "ArrowDown" || evento.key === "ArrowUp") {
            const opciones = [...raiz.querySelectorAll('.selector-opciones button:not(:disabled), .selector-opciones input:not(:disabled)')];
            if (!opciones.length) return;
            evento.preventDefault();
            const actual = opciones.indexOf(document.activeElement);
            const indice = actual < 0 ? (evento.key === "ArrowDown" ? 0 : opciones.length - 1)
                : (actual + (evento.key === "ArrowDown" ? 1 : -1) + opciones.length) % opciones.length;
            opciones[indice].focus();
        }
    };
    document.addEventListener("pointerdown", fuera);
    raiz.addEventListener("focusout", focoFuera);
    raiz.addEventListener("keydown", teclado);
    return {
        enfocarBusqueda: () => raiz.querySelector(".selector-buscador input")?.focus(),
        enfocarBoton: () => raiz.querySelector(".selector-buscable-boton")?.focus(),
        dispose: () => {
            activo = false;
            document.removeEventListener("pointerdown", fuera);
            raiz.removeEventListener("focusout", focoFuera);
            raiz.removeEventListener("keydown", teclado);
        }
    };
}
