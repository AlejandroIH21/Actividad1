using System;
using Xunit;

namespace Expendedora.Pruebas;

public class UnitTest1
{
    [Theory]
    [InlineData(Producto.Cafe, 100)]
    [InlineData(Producto.Infusion, 70)]
    [InlineData(Producto.Magdalena, 120)]
    public void Precio_ProductosValidos_DevuelvePrecioCorrecto(Producto producto, int esperado)
    {
        Assert.Equal(esperado, Program.Precio(producto));
    }

    [Fact]
    public void Precio_ProductoNoDefinido_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Program.Precio((Producto)99));
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(200, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    [InlineData(25, false)]
    [InlineData(-10, false)]
    public void EsMonedaValida_DistintosValores_DevuelveSiEsValida(int centimos, bool esperado)
    {
        Assert.Equal(esperado, Program.EsMonedaValida(centimos));
    }

    [Theory]
    [InlineData(100, 100, 0)]
    [InlineData(200, 120, 80)]
    [InlineData(100, 70, 30)]
    public void Cambio_PagoSuficiente_DevuelveDiferencia(int pagado, int precio, int esperado)
    {
        Assert.Equal(esperado, Program.Cambio(pagado, precio));
    }

    [Fact]
    public void Cambio_PagoInsuficiente_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Program.Cambio(50, 100));
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(80, "1 x 0,50 € · 1 x 0,20 € · 1 x 0,10 €")]
    [InlineData(30, "1 x 0,20 € · 1 x 0,10 €")]
    [InlineData(380, "1 x 2,00 € · 1 x 1,00 € · 1 x 0,50 € · 1 x 0,20 € · 1 x 0,10 €")]
    public void Desglose_CantidadesValidas_DevuelveTextoCorrecto(int centimos, string esperado)
    {
        Assert.Equal(esperado, Program.Desglose(centimos));
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(15)]
    public void Desglose_CantidadesNoValidas_LanzaExcepcion(int centimos)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Program.Desglose(centimos));
    }

    [Theory]
    [InlineData(5, 3, 10, 8)]
    [InlineData(0, 10, 10, 10)]
    [InlineData(7, 0, 10, 7)]
    public void NuevoStock_CantidadesValidas_DevuelveSuma(int actual, int cantidad, int maximo, int esperado)
    {
        Assert.Equal(esperado, Program.NuevoStock(actual, cantidad, maximo));
    }

    [Theory]
    [InlineData(5, -1, 10)]
    [InlineData(8, 3, 10)]
    public void NuevoStock_FueraDeRango_LanzaExcepcion(int actual, int cantidad, int maximo)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Program.NuevoStock(actual, cantidad, maximo));
    }
}