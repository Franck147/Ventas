using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Web.DTOs;

/// <summary>DTO de lectura para mostrar productos en listas y detalles.</summary>
public class ProductoDTO
{
    public int    Id           { get; set; }
    public string Nombre       { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal Precio      { get; set; }
    public int    Stock        { get; set; }
    public int    StockMinimo  { get; set; }
    public bool   Activo       { get; set; }

    /// <summary>Propiedad calculada para alertas visuales de reabastecimiento.</summary>
    public bool BajoStock => Stock <= StockMinimo;
}

/// <summary>DTO de escritura para formularios de creación y edición de productos.</summary>
public class ProductoInputModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "Código de Barras")]
    public string? CodigoBarras { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Display(Name = "Precio (S/.)")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    [Display(Name = "Stock Actual")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    [Display(Name = "Stock Mínimo")]
    public int StockMinimo { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;
}
