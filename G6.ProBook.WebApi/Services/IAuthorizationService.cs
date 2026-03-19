using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Services
{
    /// <summary>
    /// Servicio de utilidad para validar roles en controladores
    /// Proporciona métodos reusables para validar acceso basado en roles
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Valida que el usuario tenga el rol requerido
        /// </summary>
        /// <param name="userId">ID del usuario a validar</param>
        /// <param name="requiredRole">Rol requerido (ej: "manager", "huesped")</param>
        /// <returns>True si el usuario tiene el rol, false en caso contrario</returns>
        Task<bool> UserHasRoleAsync(string userId, string requiredRole);

        /// <summary>
        /// Obtiene el usuario actual basado en el ID
        /// </summary>
        Task<Models.User?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Valida que el usuario sea manager/administrador
        /// </summary>
        Task<bool> IsManagerAsync(string userId);

        /// <summary>
        /// Valida que el usuario sea huésped
        /// </summary>
        Task<bool> IsGuestAsync(string userId);
    }
}
