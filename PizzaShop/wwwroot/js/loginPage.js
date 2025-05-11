document.addEventListener("DOMContentLoaded", function () {
    const togglePasswords = document.getElementsByClassName("togglePassword");
    const passwordFields = document.getElementsByClassName("password");

    console.log("loginPage.js is loaded successfully!");

    for (let i = 0; i < togglePasswords.length; i++) {
        if (togglePasswords[i] && passwordFields[i]) {
            togglePasswords[i].addEventListener("click", function () {
                if (passwordFields[i].type === "password") {
                    passwordFields[i].type = "text";
                    togglePasswords[i].classList.remove("fa-eye-slash");
                    togglePasswords[i].classList.add("fa-eye");
                } else {
                    passwordFields[i].type = "password";
                    togglePasswords[i].classList.remove("fa-eye");
                    togglePasswords[i].classList.add("fa-eye-slash");
                }
            });
        } else {
            console.error("Toggle password elements not found!");
        }
    }
});
