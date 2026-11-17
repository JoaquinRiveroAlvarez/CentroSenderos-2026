export function prepararEntrada(entrada) {
    // Enter consulta la hora escrita; nunca envía el formulario del turno por accidente.
    entrada.addEventListener("keydown", evento => {
        if (evento.key === "Enter") {
            evento.preventDefault();
            entrada.blur();
        }
    });
}
