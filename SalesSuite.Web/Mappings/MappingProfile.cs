using AutoMapper;
using SalesSuite.Domain.Entities;
using SalesSuite.Web.DTOs;

namespace SalesSuite.Web.Mappings;

/// <summary>
/// Perfil de AutoMapper que define todas las conversiones entre
/// entidades del dominio y DTOs/ViewModels de la capa Web.
///
/// Al heredar de Profile y registrar con AddAutoMapper(Assembly),
/// AutoMapper detecta esta clase automáticamente sin configuración adicional.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Producto ──────────────────────────────────────────────────────────
        CreateMap<Producto, ProductoDTO>();
        CreateMap<ProductoInputModel, Producto>()
            .ForMember(dest => dest.Detalles, opt => opt.Ignore()); // navegación no viene del form
        CreateMap<Producto, ProductoInputModel>();

        // ── Cliente ───────────────────────────────────────────────────────────
        CreateMap<Cliente, ClienteDTO>();
        CreateMap<ClienteInputModel, Cliente>()
            .ForMember(dest => dest.Ventas, opt => opt.Ignore());
        CreateMap<Cliente, ClienteInputModel>();

        // ── Venta (listado) ───────────────────────────────────────────────────
        CreateMap<Venta, VentaDTO>()
            .ForMember(dest => dest.ClienteNombre,
                       opt => opt.MapFrom(src => $"{src.Cliente.Nombre} {src.Cliente.Apellido}"))
            .ForMember(dest => dest.TotalProductos,
                       opt => opt.MapFrom(src => src.Detalles.Sum(d => d.Cantidad)));

        // ── Venta (detalle completo) ──────────────────────────────────────────
        CreateMap<Venta, VentaDetalleDTO>();

        // ── DetalleVenta ──────────────────────────────────────────────────────
        CreateMap<DetalleVenta, DetalleVentaDTO>()
            .ForMember(dest => dest.ProductoNombre,
                       opt => opt.MapFrom(src => src.Producto.Nombre));
    }
}
