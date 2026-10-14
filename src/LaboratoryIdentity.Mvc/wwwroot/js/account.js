// Mostrar / ocultar contraseña en los formularios de Account.
document.querySelectorAll("[data-toggle-password]").forEach(button => {
    button.addEventListener("click", () => {
        const input = document.getElementById(button.dataset.togglePassword);
        if (!input) return;

        const isHidden = input.type === "password";
        input.type = isHidden ? "text" : "password";

        const icon = button.querySelector("i");
        icon?.classList.toggle("bi-eye", !isHidden);
        icon?.classList.toggle("bi-eye-slash", isHidden);
        button.setAttribute("aria-label", isHidden ? "Ocultar contraseña" : "Mostrar contraseña");
    });
});
