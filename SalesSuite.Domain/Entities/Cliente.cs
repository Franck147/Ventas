using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Domain.Entities;

/// <summary>
/// Representa a un cliente registrado en el sistema.
/// Se asocia a una o más ventas a lo largo del tiempo.
/// </summary>
public class Cliente
{
    // ── Clave primaria ────────────────────────────────────────────────────────
    public int Id { get; set; }

    // ── Identificación ────────────────────────────────────────────────────────

    /// <summary>
    /// Documento nacional de identidad, RUC, pasaporte u otro identificador oficial.
    /// Se usa para búsquedas rápidas en el POS.
    /// </summary>
    [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El documento no puede superar 20 caracteres.")]
    public string DocumentoIdentidad { get; set; } = string.Empty;

    // ── Datos personales ──────────────────────────────────────────────────────

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    // ── Contacto (opcionales) ─────────────────────────────────────────────────

    /// <summary>Correo electrónico para envío de comprobantes digitales.</summary>
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [MaxLength(150, ErrorMessage = "El email no puede superar 150 caracteres.")]
    public string? Email { get; set; }

    [MaxLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string? Telefono { get; set; }

    // ── Propiedad de navegación ───────────────────────────────────────────────

    /// <summary>Historial de compras del cliente. Relación uno-a-muchos con Venta.</summary>
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
