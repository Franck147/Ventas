using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Web.DTOs;

/// <summary>DTO de lectura para listas y selects de clientes.</summary>
public class ClienteDTO
{
    public int    Id                 { get; set; }
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string Nombre             { get; set; } = string.Empty;
    public string Apellido           { get; set; } = string.Empty;
    public string? Email             { get; set; }
    public string? Telefono          { get; set; }

    /// <summary>Nombre completo para mostrar en selects y tablas.</summary>
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

/// <summary>DTO de escritura para formularios de creación y edición de clientes.</summary>
public class ClienteInputModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
    [MaxLength(20, ErrorMessage = "Máximo 20 caracteres.")]
    [Display(Name = "Documento de Identidad")]
    public string DocumentoIdentidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [MaxLength(20, ErrorMessage = "Máximo 20 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }
}
