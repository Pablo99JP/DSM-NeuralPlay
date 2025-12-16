// Validador de requisitos de contraseña
document.addEventListener('DOMContentLoaded', function() {
    const passwordInput = document.getElementById('passwordInput');
    const passwordConfirm = document.getElementById('passwordConfirm');
    const passwordRequirements = document.getElementById('passwordRequirements');
    const confirmError = document.getElementById('confirmError');

    if (!passwordInput) return; // Si no está en la página, salir

    const requirements = {
        length: { id: 'req-length', check: (pwd) => pwd.length >= 8 },
        uppercase: { id: 'req-uppercase', check: (pwd) => /[A-Z]/.test(pwd) },
        lowercase: { id: 'req-lowercase', check: (pwd) => /[a-z]/.test(pwd) },
        number: { id: 'req-number', check: (pwd) => /[0-9]/.test(pwd) },
        special: { id: 'req-special', check: (pwd) => /[!$%^&*()_+\-=\[\]{};':"\\|,.<>\/]/.test(pwd) || pwd.indexOf('@') !== -1 }
    };

    function updatePasswordRequirements() {
        const password = passwordInput.value;
        const rightPanel = document.querySelector('.right-panel');
        
        // Mostrar panel de requisitos si hay texto
        if (password.length > 0) {
            passwordRequirements.classList.add('show');
            rightPanel.classList.add('requirements-shown');
        } else {
            passwordRequirements.classList.remove('show');
            rightPanel.classList.remove('requirements-shown');
        }

        let metCount = 0;
        let totalRequirements = Object.keys(requirements).length;
        const requirementsTitle = passwordRequirements.querySelector('h4');

        // Validar cada requisito
        for (const [key, requirement] of Object.entries(requirements)) {
            const element = document.getElementById(requirement.id);
            const isMet = requirement.check(password);
            
            if (isMet) {
                element.classList.add('met');
                element.querySelector('.requirement-icon').textContent = '✓';
                element.style.display = 'none'; // Ocultar requisito cumplido
                metCount++;
            } else {
                element.classList.remove('met');
                element.querySelector('.requirement-icon').textContent = '✕';
                element.style.display = 'flex'; // Mostrar requisito no cumplido
            }
        }

        // Actualizar título según requisitos pendientes
        const pendingCount = totalRequirements - metCount;
        if (pendingCount === 0) {
            requirementsTitle.textContent = '✓ Todos los requisitos cumplidos';
            requirementsTitle.style.color = '#28a745';
        } else if (pendingCount === 1) {
            requirementsTitle.textContent = 'Requisito pendiente';
            requirementsTitle.style.color = '#aaa';
        } else {
            requirementsTitle.textContent = `${pendingCount} Requisitos pendientes`;
            requirementsTitle.style.color = '#aaa';
        }

        // Validar confirmación de contraseña
        validatePasswordMatch();
    }

    function validatePasswordMatch() {
        if (passwordConfirm.value.length === 0) {
            confirmError.textContent = '';
            return;
        }

        if (passwordInput.value !== passwordConfirm.value) {
            confirmError.textContent = 'Las contraseñas no coinciden';
            confirmError.style.color = 'var(--np-red)';
        } else {
            confirmError.textContent = '✓ Las contraseñas coinciden';
            confirmError.style.color = '#28a745';
        }
    }

    // Event listeners
    passwordInput.addEventListener('input', updatePasswordRequirements);
    passwordConfirm.addEventListener('input', validatePasswordMatch);

    // Validación al enviar formulario
    const form = document.querySelector('.login-form');
    if (form) {
        form.addEventListener('submit', function(e) {
            const password = passwordInput.value;
            
            // Verificar que todos los requisitos están cumplidos
            let allMet = true;
            for (const requirement of Object.values(requirements)) {
                if (!document.getElementById(requirement.id).classList.contains('met')) {
                    allMet = false;
                    break;
                }
            }

            // Verificar que las contraseñas coinciden
            if (password !== passwordConfirm.value) {
                e.preventDefault();
                confirmError.textContent = 'Las contraseñas no coinciden';
                confirmError.style.color = 'var(--np-red)';
                return;
            }

            if (!allMet) {
                e.preventDefault();
                alert('Por favor, cumple con todos los requisitos de contraseña.');
                return;
            }
        });
    }
});
