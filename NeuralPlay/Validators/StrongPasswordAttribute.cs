using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NeuralPlay.Validators
{
    /// <summary>
    /// Validador personalizado para contraseñas fuertes.
    /// Requiere:
    /// - Mínimo 8 caracteres
    /// - Al menos una letra mayúscula
    /// - Al menos una letra minúscula
    /// - Al menos un número
    /// - Al menos un carácter especial
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return $"La {name} debe cumplir los siguientes requisitos: " +
                   "mínimo 8 caracteres, contener mayúscula, minúscula, número y carácter especial (!@#$%^&*).";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Permitir null/vacío para [Required] que manejarlo
            }

            string password = value.ToString() ?? string.Empty;

            // Validar longitud mínima
            if (password.Length < 8)
            {
                return new ValidationResult("La contraseña debe tener al menos 8 caracteres.");
            }

            // Validar mayúscula
            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                return new ValidationResult("La contraseña debe contener al menos una mayúscula (A-Z).");
            }

            // Validar minúscula
            if (!Regex.IsMatch(password, "[a-z]"))
            {
                return new ValidationResult("La contraseña debe contener al menos una minúscula (a-z).");
            }

            // Validar número
            if (!Regex.IsMatch(password, "[0-9]"))
            {
                return new ValidationResult("La contraseña debe contener al menos un número (0-9).");
            }

            // Validar carácter especial
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            {
                return new ValidationResult("La contraseña debe contener al menos un carácter especial (!@#$%^&*).");
            }

            return ValidationResult.Success;
        }
    }
}
