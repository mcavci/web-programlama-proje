

document.addEventListener("DOMContentLoaded", function () {

  
    const togglePassword = document.querySelector('#togglePassword');
    const password = document.querySelector('#passwordInput');

    if (togglePassword && password) {
        togglePassword.addEventListener('click', function (e) {
           
            const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
            password.setAttribute('type', type);

            
            this.classList.toggle('fa-eye-slash');
        });
    }

   
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('focus', () => {
            if (input.parentElement) {
                input.parentElement.classList.add('shadow-sm');
            }
        });
        input.addEventListener('blur', () => {
            if (input.parentElement) {
                input.parentElement.classList.remove('shadow-sm');
            }
        });
    });
});


document.addEventListener("DOMContentLoaded", function () {

   
    const togglePassword = document.querySelector('#togglePassword');
    const password = document.querySelector('#passwordInput');

    if (togglePassword && password) {
        togglePassword.addEventListener('click', function (e) {
            const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
            password.setAttribute('type', type);
            this.classList.toggle('fa-eye-slash');
        });
    }
});

document.addEventListener("DOMContentLoaded", function () {
   
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[title]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })
});