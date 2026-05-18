using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Web.DTOs;

/// <summary>DTO de lectura para el listado de ventas.</summary>
public class VentaDTO
{
    public int      Id              { get; set; }
    public DateTime FechaVenta      { get; set; }
    public string   ClienteNombre   { get; set; } = string.Empty;
    public decimal  Total           { get; set; }
    public int      TotalProductos  { get; set; }
}

/// <summary>DTO de detalle completo de una venta (vista Details).</summary>
public class VentaDetalleDTO
{
    public int               Id          { get; set; }
    public DateTime          FechaVenta  { get; set; }
    public ClienteDTO        Cliente     { get; set; } = null!;
    public decimal           Total       { get; set; }
    public List<DetalleVentaDTO> Detalles { get; set; } = new();
}

/// <summary>DTO para cada línea de producto dentro de una venta.</summary>
public class DetalleVentaDTO
{
    public string  ProductoNombre  { get; set; } = string.Empty;
    public int     Cantidad        { get; set; }
    public decimal PrecioUnitario  { get; set; }
    public decimal Subtotal        { get; set; }
}

// ── DTOs para la petición AJAX de registro de venta ──────────────────────────

/// <summary>
/// Payload JSON enviado desde el carrito JS al controlador.
/// Nunca se confía en el Total calculado en cliente; el backend lo recalcula.
/// </summary>
public class VentaCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
    public int ClienteId { get; set; }

    [MinLength(1, ErrorMessage = "El carrito no puede estar vacío.")]
    public List<VentaItemRequest> Items { get; set; } = new();
}

/// <summary>Línea de artículo dentro de la solicitud de venta.</summary>
public class VentaItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Producto inválido.")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad   { get; set; }
}
