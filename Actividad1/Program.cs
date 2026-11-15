using System;
using System.Globalization;
using System.Text;
using System.Diagnostics;

public enum Producto { Cafe, Infusion, Magdalena }

public class Program
{
    const int CAPACIDAD = 10; // R6: capacidad máxima en variante G
    const int CENTIMOS_POR_EURO = 100;
    static readonly CultureInfo ESPANOL = new CultureInfo("es-ES");

    /// <summary>
    /// Punto de entrada principal que coordina el menú y las opciones de la máquina.
    /// </summary>
    /// <param name="args">Argumentos de la línea de comandos.</param>
    public static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = ESPANOL;
        Console.OutputEncoding = Encoding.UTF8;

        int stockCafe = CAPACIDAD, stockInfusion = CAPACIDAD, stockMagdalena = CAPACIDAD;
        int ventasCafe = 0, ventasInfusion = 0, ventasMagdalena = 0;
        int recaudacion = 0;
        int intentosAgotado = 0; // R9: contador extra de intentos con producto agotado

        bool apagar = false;

        // R2: repetir el menú hasta que se apague la máquina
        do
        {
            Console.WriteLine();
            MostrarMenu();
            int? opcion = LeerEntero("Opción: ");

            // R12: salir con aviso si no hay entrada de teclado en Docker
            if (opcion == null)
            {
                Console.WriteLine("\nSin entrada de teclado. Máquina apagada.");
                return;
            }

            switch (opcion)
            {
                case 1: // R1, R3, R4, R5: elegir producto, pagar con monedas y dar cambio
                    Console.WriteLine($"1. Café {Precio(Producto.Cafe) / (decimal)CENTIMOS_POR_EURO:C}  " +
                                      $"2. Infusión {Precio(Producto.Infusion) / (decimal)CENTIMOS_POR_EURO:C}  " +
                                      $"3. Magdalena {Precio(Producto.Magdalena) / (decimal)CENTIMOS_POR_EURO:C}  0. Volver");

                    int? prodOpcion = LeerEntero("Producto: ");
                    if (prodOpcion == null) { Console.WriteLine("\nSin entrada de teclado. Máquina apagada."); return; }
                    if (prodOpcion == 0) break;

                    if (prodOpcion is < 1 or > 3)
                    {
                        Console.Error.WriteLine("Producto no válido.");
                        break;
                    }

                    Producto elegido = (Producto)(prodOpcion - 1);
                    Debug.Assert(Enum.IsDefined(typeof(Producto), elegido), "Producto fuera del enum.");

                    int stockActual = elegido switch
                    {
                        Producto.Cafe => stockCafe,
                        Producto.Infusion => stockInfusion,
                        _ => stockMagdalena
                    };

                    if (stockActual == 0)
                    {
                        intentosAgotado++;
                        Console.Error.WriteLine($"Producto agotado: {elegido}.");
                        break;
                    }

                    int precio = Precio(elegido);
                    int pagado = 0;
                    bool cancelada = false;

                    while (pagado < precio)
                    {
                        Debug.Assert(pagado < precio, "El bucle debe parar al cubrir el precio.");

                        int falta = precio - pagado;
                        int? moneda = LeerEntero($"Faltan {falta / (decimal)CENTIMOS_POR_EURO:C}. Moneda en céntimos (0 cancela): ");

                        if (moneda == null) { Console.WriteLine("\nSin entrada de teclado. Máquina apagada."); return; }

                        if (moneda == 0)
                        {
                            cancelada = true;
                            break;
                        }

                        if (!EsMonedaValida(moneda.Value))
                        {
                            Console.Error.WriteLine("Moneda no admitida: 10, 20, 50, 100 o 200.");
                            continue;
                        }

                        pagado += moneda.Value;
                    }

                    if (cancelada)
                    {
                        Console.WriteLine($"Compra cancelada. Se devuelven {pagado / (decimal)CENTIMOS_POR_EURO:C}.");
                        break;
                    }

                    try
                    {
                        int cambio = Cambio(pagado, precio);
                        Debug.Assert(cambio >= 0, "El cambio nunca puede ser negativo.");

                        string desglose = Desglose(cambio);
                        Console.Write($"Retire su {elegido}. Cambio: {cambio / (decimal)CENTIMOS_POR_EURO:C}");
                        if (cambio > 0)
                        {
                            Console.Write($" ({desglose})");
                        }
                        Console.WriteLine();

                        recaudacion += precio;
                        switch (elegido)
                        {
                            case Producto.Cafe: stockCafe--; ventasCafe++; break;
                            case Producto.Infusion: stockInfusion--; ventasInfusion++; break;
                            case Producto.Magdalena: stockMagdalena--; ventasMagdalena++; break;
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.Error.WriteLine("Error interno al calcular el cambio.");
                    }
                    break;

                case 2: // R7, R11: reponer unidades sin superar el máximo
                    Console.WriteLine("1. Café  2. Infusión  3. Magdalena  0. Volver");
                    int? repOpcion = LeerEntero("Producto: ");
                    if (repOpcion == null) { Console.WriteLine("\nSin entrada de teclado. Máquina apagada."); return; }
                    if (repOpcion == 0) break;

                    if (repOpcion is < 1 or > 3)
                    {
                        Console.Error.WriteLine("Producto no válido.");
                        break;
                    }

                    int? unidades = LeerEntero("Unidades: ");
                    if (unidades == null) { Console.WriteLine("\nSin entrada de teclado. Máquina apagada."); return; }

                    try
                    {
                        switch (repOpcion)
                        {
                            case 1: stockCafe = NuevoStock(stockCafe, unidades.Value, CAPACIDAD); break;
                            case 2: stockInfusion = NuevoStock(stockInfusion, unidades.Value, CAPACIDAD); break;
                            case 3: stockMagdalena = NuevoStock(stockMagdalena, unidades.Value, CAPACIDAD); break;
                        }
                        Console.WriteLine("Reposición completada.");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Console.Error.WriteLine($"No se puede reponer: cantidad negativa o supera el máximo de {CAPACIDAD} unidades.");
                    }
                    break;

                case 3: // R8: informe en columnas con ventas, stock, recaudación y más vendido
                    Console.WriteLine($"{"Producto",-12}{"Precio",10}{"Ventas",8}{"Stock",8}");
                    Console.WriteLine(new string('-', 38));
                    Console.WriteLine($"{"Café",-12}{Precio(Producto.Cafe) / (decimal)CENTIMOS_POR_EURO,10:C}{ventasCafe,8}{stockCafe,8}");
                    Console.WriteLine($"{"Infusión",-12}{Precio(Producto.Infusion) / (decimal)CENTIMOS_POR_EURO,10:C}{ventasInfusion,8}{stockInfusion,8}");
                    Console.WriteLine($"{"Magdalena",-12}{Precio(Producto.Magdalena) / (decimal)CENTIMOS_POR_EURO,10:C}{ventasMagdalena,8}{stockMagdalena,8}");
                    Console.WriteLine(new string('-', 38));
                    Console.WriteLine($"Recaudación: {recaudacion / (decimal)CENTIMOS_POR_EURO:C}");
                    Console.WriteLine($"Intentos de compra agotado: {intentosAgotado}");

                    string masVendido = "Café";
                    int maxVentas = ventasCafe;
                    if (ventasInfusion > maxVentas) { masVendido = "Infusión"; maxVentas = ventasInfusion; }
                    if (ventasMagdalena > maxVentas) { masVendido = "Magdalena"; }
                    Console.WriteLine($"Más vendido: {masVendido}");
                    break;

                case 0: // R10: pedir confirmación antes de apagar
                    Console.Write("¿Seguro que quiere apagar la máquina? (S/N): ");
                    string? resp = Console.ReadLine();
                    if (resp == null) { Console.WriteLine("\nSin entrada de teclado. Máquina apagada."); return; }
                    if (resp.Trim().Equals("S", StringComparison.OrdinalIgnoreCase))
                    {
                        apagar = true;
                    }
                    break;

                default:
                    Console.Error.WriteLine("Opción no válida.");
                    break;
            }
        }
        while (!apagar);

        // TODO: guardar la recaudación y las ventas en un archivo al apagar
        Console.WriteLine("Máquina apagada.");
    }

    /// <summary>
    /// Devuelve lo que cuesta un producto en céntimos.
    /// </summary>
    /// <param name="producto">El producto del que queremos saber el precio.</param>
    /// <returns>El precio en céntimos.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Si el producto no existe.</exception>
    public static int Precio(Producto producto)
    {
        return producto switch
        {
            Producto.Cafe => 100,
            Producto.Infusion => 70,
            Producto.Magdalena => 120,
            _ => throw new ArgumentOutOfRangeException(nameof(producto), "Producto no valido")
        };
    }

    /// <summary>
    /// Comprueba si la moneda introducida es de las que acepta la máquina.
    /// </summary>
    /// <param name="centimos">Valor de la moneda en céntimos.</param>
    /// <returns>True si es de 10, 20, 50, 100 o 200; false si es otra.</returns>
    public static bool EsMonedaValida(int centimos)
    {
        return centimos is 10 or 20 or 50 or 100 or 200;
    }

    /// <summary>
    /// Calcula el cambio restando el precio a lo que ha pagado el usuario.
    /// </summary>
    /// <param name="pagado">Céntimos totales introducidos.</param>
    /// <param name="precio">Precio del producto en céntimos.</param>
    /// <returns>El cambio a devolver en céntimos.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Si lo pagado no llega al precio.</exception>
    public static int Cambio(int pagado, int precio)
    {
        if (pagado < precio)
        {
            throw new ArgumentOutOfRangeException(nameof(pagado), "El pago es insuficiente");
        }
        return pagado - precio;
    }

    const int MONEDA_200 = 200;
    const int MONEDA_100 = 100;
    const int MONEDA_50 = 50;
    const int MONEDA_20 = 20;
    const int MONEDA_10 = 10;

    /// <summary>
    /// Saca las monedas necesarias para devolver el cambio usando las más grandes primero.
    /// </summary>
    /// <param name="centimos">Cambio a devolver en céntimos.</param>
    /// <returns>Texto con las monedas del cambio o vacío si es 0.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Si es negativo o no es múltiplo de 10.</exception>
    public static string Desglose(int centimos)
    {
        if (centimos < 0 || centimos % MONEDA_10 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(centimos), "Debe ser un múltiplo de 10 no negativo.");
        }
        string resultado = "";
        int cantidad;

        cantidad = centimos / MONEDA_200;
        centimos %= MONEDA_200;
        if (cantidad > 0)
        {
            resultado += $"{cantidad} x {MONEDA_200 / 100m:C}";
        }

        cantidad = centimos / MONEDA_100;
        centimos %= MONEDA_100;
        if (cantidad > 0)
        {
            if (resultado != "") resultado += " · ";
            resultado += $"{cantidad} x {MONEDA_100 / 100m:C}";
        }

        cantidad = centimos / MONEDA_50;
        centimos %= MONEDA_50;
        if (cantidad > 0)
        {
            if (resultado != "") resultado += " · ";
            resultado += $"{cantidad} x {MONEDA_50 / 100m:C}";
        }

        cantidad = centimos / MONEDA_20;
        centimos %= MONEDA_20;
        if (cantidad > 0)
        {
            if (resultado != "") resultado += " · ";
            resultado += $"{cantidad} x {MONEDA_20 / 100m:C}";
        }

        cantidad = centimos / MONEDA_10;
        if (cantidad > 0)
        {
            if (resultado != "") resultado += " · ";
            resultado += $"{cantidad} x {MONEDA_10 / 100m:C}";
        }

        return resultado;
    }

    /// <summary>
    /// Suma las unidades nuevas al stock actual comprobando que no pase del máximo.
    /// </summary>
    /// <param name="actual">Stock que hay ahora mismo.</param>
    /// <param name="cantidad">Unidades que se quieren añadir.</param>
    /// <param name="maximo">Capacidad máxima permitida.</param>
    /// <returns>El nuevo stock sumado.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Si la cantidad es negativa o supera el máximo.</exception>
    public static int NuevoStock(int actual, int cantidad, int maximo)
    {
        if (cantidad < 0 || actual + cantidad > maximo)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), "Cantidad no valida");
        }
        return actual + cantidad;
    }

    /// <summary>
    /// Muestra por pantalla el título y las opciones del menú.
    /// </summary>
    static void MostrarMenu()
    {
        Console.WriteLine("---------------------------------------------------------------------------------");
        Console.WriteLine("                                  MAQUINA EXPENDEDORA");
        Console.WriteLine("---------------------------------------------------------------------------------");
        Console.WriteLine("1. Comprar  2. Reponer  3. Informe  0. Apagar");
    }

    /// <summary>
    /// Pide un número entero al usuario y repite si escribe texto no válido.
    /// </summary>
    /// <param name="pregunta">Mensaje que se muestra para pedir el dato.</param>
    /// <returns>El número entero leído, o null si se acaba la entrada.</returns>
    static int? LeerEntero(string pregunta)
    {
        while (true)
        {
            Console.Write(pregunta);
            string? entrada = Console.ReadLine();

            if (entrada == null)
            {
                return null;
            }

            if (int.TryParse(entrada, out int numero))
            {
                return numero;
            }

            Console.Error.WriteLine("Dato no válido. Escribe un número entero.");
        }
    }
}