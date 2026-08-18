window.togglePassword = (inputId, button) => {
    const input = document.getElementById(inputId);
    const icon = button.querySelector("i");

    if (!input || !icon) return;

    const estabaVisible = input.type === "text";

    input.type = estabaVisible ? "password" : "text";

    icon.classList.toggle("bi-eye", estabaVisible);
    icon.classList.toggle("bi-eye-slash", !estabaVisible);

    button.setAttribute(
        "aria-label",
        estabaVisible ? "Mostrar contraseña" : "Ocultar contraseña"
    );
};