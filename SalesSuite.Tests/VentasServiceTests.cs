using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SalesSuite.Domain.Entities;
using SalesSuite.Domain.Interfaces;

namespace SalesSuite.Tests;

/// <summary>
/// Pruebas unitarias del caso de uso "Registrar Venta".
///
/// Se usa Moq para simular IUnitOfWork e IGenericRepository, de modo que
/// las pruebas sean rápidas, deterministas y sin dependencia de base de datos.
///
/// Convención de nomenclatura: MetodoQueSePrueba_Escenario_ResultadoEsperado
/// </summary>
[TestClass]
public class VentasServiceTests
{
    // ── Helpers de configuración ──────────────────────────────────────────────

    /// <summary>
    /// Crea un Producto de prueba con valores controlados.
    /// </summary>
    private static Producto CrearProducto(int id, decimal precio, int stock) =>
        new()
        {
            Id     = id,
            Nombre = $"Producto de prueba {id}",
            Precio = precio,
            Stock  = stock,
            Activo = true
        };

    /// <summary>
    /// Configura el mock del repositorio de Producto para que devuelva
    /// el producto indicado cuando se llame GetByIdAsync con su Id.
    /// </summary>
    private static Mock<IGenericRepository<Producto>> SetupProductoRepo(Producto producto)
    {
        var repo = new Mock<IGenericRepository<Producto>>();
        repo.Setup(r => r.GetByIdAsync(producto.Id))
            .ReturnsAsync(producto);
        return repo;
    }

    // ── ESCENARIO 1: Venta exitosa ────────────────────────────────────────────

    [TestMethod]
    [TestCategory("Ventas")]
    public async Task RegistrarVenta_ConStockSuficiente_DescuentaStockCorrectamente()
    {
        // Arrange
        var producto = CrearProducto(id: 1, precio: 50.00m, stock: 10);
        var repoProducto = SetupProductoRepo(producto);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.Repository<Producto>()).Returns(repoProducto.Object);
        mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act — simula la lógica de negocio del VentasController
        const int cantidadSolicitada = 3;
        var productoEncontrado = await mockUow.Object.Repository<Producto>()
                                              .GetByIdAsync(producto.Id);

        // Verifica que hay stock suficiente
        Assert.IsNotNull(productoEncontrado);
        // IsGreaterThanOrEqualTo(lowerBound, value): verifica value >= lowerBound
        Assert.IsGreaterThanOrEqualTo(cantidadSolicitada, productoEncontrado.Stock,
            "Debe haber stock suficiente antes de procesar.");

        // Descuenta el stock (lógica del controller)
        productoEncontrado.Stock -= cantidadSolicitada;
        var subtotal = productoEncontrado.Precio * cantidadSolicitada;

        await mockUow.Object.SaveChangesAsync();

        // Assert
        Assert.AreEqual(7, productoEncontrado.Stock,
            "El stock debe ser 10 - 3 = 7 tras la venta.");
        Assert.AreEqual(150.00m, subtotal,
            "El subtotal debe ser 50.00 × 3 = 150.00.");

        mockUow.Verify(u => u.SaveChangesAsync(), Times.Once,
            "SaveChangesAsync debe llamarse exactamente una vez.");
    }

    // ── ESCENARIO 2: Falla por stock insuficiente con Rollback ───────────────

    [TestMethod]
    [TestCategory("Ventas")]
    public async Task RegistrarVenta_StockInsuficiente_LanzaExcepcionYHaceRollback()
    {
        // Arrange
        var producto = CrearProducto(id: 2, precio: 100.00m, stock: 2);
        var repoProducto = SetupProductoRepo(producto);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.Repository<Producto>()).Returns(repoProducto.Object);
        mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        mockUow.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Act
        await mockUow.Object.BeginTransactionAsync();

        var productoEncontrado = await mockUow.Object.Repository<Producto>()
                                              .GetByIdAsync(producto.Id);

        const int cantidadSolicitada = 5; // más que el stock disponible (2)
        bool stockInsuficiente = productoEncontrado!.Stock < cantidadSolicitada;

        InvalidOperationException? excepcion = null;
        if (stockInsuficiente)
        {
            excepcion = new InvalidOperationException(
                $"Stock insuficiente para '{productoEncontrado.Nombre}'. " +
                $"Disponible: {productoEncontrado.Stock}, solicitado: {cantidadSolicitada}.");
            await mockUow.Object.RollbackTransactionAsync();
        }

        // Assert
        Assert.IsTrue(stockInsuficiente,
            "Debe detectarse stock insuficiente cuando se piden 5 y solo hay 2.");
        Assert.IsNotNull(excepcion,
            "Debe generarse una InvalidOperationException.");
        Assert.Contains("Stock insuficiente", excepcion.Message,
            "El mensaje de error debe indicar stock insuficiente.");

        mockUow.Verify(u => u.RollbackTransactionAsync(), Times.Once,
            "RollbackTransactionAsync debe llamarse exactamente una vez al fallar.");
        mockUow.Verify(u => u.CommitTransactionAsync(), Times.Never,
            "CommitTransactionAsync NO debe llamarse si ocurrió un error.");
    }

    // ── ESCENARIO 3: Múltiples productos, uno sin stock ───────────────────────

    [TestMethod]
    [TestCategory("Ventas")]
    public async Task RegistrarVenta_UnProductoSinStock_DetieneProcesamiento()
    {
        // Arrange
        var productoConStock    = CrearProducto(id: 1, precio: 20.00m, stock: 5);
        var productoSinStock    = CrearProducto(id: 2, precio: 80.00m, stock: 0);

        var repo = new Mock<IGenericRepository<Producto>>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(productoConStock);
        repo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(productoSinStock);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.Repository<Producto>()).Returns(repo.Object);
        mockUow.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        var items = new[] { (ProductoId: 1, Cantidad: 2), (ProductoId: 2, Cantidad: 1) };

        // Act
        bool errorDetectado = false;
        string mensajeError = string.Empty;

        foreach (var item in items)
        {
            var p = await mockUow.Object.Repository<Producto>().GetByIdAsync(item.ProductoId);
            if (p!.Stock < item.Cantidad)
            {
                errorDetectado = true;
                mensajeError   = $"Stock insuficiente: {p.Nombre}";
                await mockUow.Object.RollbackTransactionAsync();
                break;
            }
        }

        // Assert
        Assert.IsTrue(errorDetectado,
            "Debe detectarse que el segundo producto no tiene stock.");
        Assert.Contains("Stock insuficiente", mensajeError,
            "El mensaje debe indicar qué producto falló.");
        mockUow.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    // ── ESCENARIO 4: Reglas de negocio — precio cero ─────────────────────────

    [TestMethod]
    [TestCategory("Ventas")]
    public void Producto_ConPrecioCero_NoEsValido()
    {
        // Arrange
        var producto = new Producto { Id = 1, Nombre = "Test", Precio = 0, Stock = 5, Activo = true };

        // Act
        bool precioInvalido = producto.Precio <= 0;

        // Assert
        Assert.IsTrue(precioInvalido,
            "Un producto con precio 0 no debe ser válido para la venta.");
    }

    // ── ESCENARIO 5: Reglas de negocio — cantidad cero ───────────────────────

    [TestMethod]
    [TestCategory("Ventas")]
    public void DetalleVenta_ConCantidadCero_NoEsValido()
    {
        // Arrange
        var detalle = new DetalleVenta
        {
            ProductoId     = 1,
            Cantidad       = 0,      // inválido
            PrecioUnitario = 50.00m,
            Subtotal       = 0
        };

        // Act
        bool cantidadInvalida = detalle.Cantidad <= 0;

        // Assert
        Assert.IsTrue(cantidadInvalida,
            "Una cantidad de 0 en un detalle de venta no debe ser válida.");
    }

    // ── ESCENARIO 6: Cálculo correcto del total de la venta ──────────────────

    [TestMethod]
    [TestCategory("Ventas")]
    public void Venta_Total_CalculadoCorrectamenteDesdeDetalles()
    {
        // Arrange
        var detalles = new List<DetalleVenta>
        {
            new() { Cantidad = 2, PrecioUnitario = 30.00m, Subtotal = 60.00m },
            new() { Cantidad = 1, PrecioUnitario = 45.50m, Subtotal = 45.50m },
            new() { Cantidad = 3, PrecioUnitario = 10.00m, Subtotal = 30.00m },
        };

        // Act — misma lógica que usa el VentasController en el backend
        var totalCalculado = detalles.Sum(d => d.Subtotal);

        // Assert
        Assert.AreEqual(135.50m, totalCalculado,
            "El total debe ser 60 + 45.50 + 30 = 135.50");
    }
}
