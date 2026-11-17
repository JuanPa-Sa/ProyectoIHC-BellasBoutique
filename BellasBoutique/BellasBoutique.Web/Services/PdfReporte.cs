using System.Globalization;
using System.Text;

namespace BellasBoutique.Web.Services;

/* Responsable: Joel
   Genera los reportes de ventas en PDF con la imagen de la marca: encabezado con logo,
   tarjetas de indicadores y tablas con los colores de BellasBoutique.
   No usa librerías externas: escribe el PDF directamente (A4, fuentes estándar Helvetica y Times). */

/// <summary>Datos que se muestran en un reporte.</summary>
public sealed class DatosReporte
{
    public string Etiqueta { get; set; } = "Reporte de ventas";   // texto pequeño sobre el título
    public string Titulo { get; set; } = "";
    public string Periodo { get; set; } = "";
    public DateTime Generado { get; set; } = DateTime.Now;
    public List<(string Nombre, string Valor)> Indicadores { get; } = new();
    public List<TablaReporte> Tablas { get; } = new();
}

/// <summary>Una tabla del reporte.</summary>
public sealed class TablaReporte
{
    public string Titulo { get; set; } = "";
    public string[] Encabezados { get; set; } = [];
    public float[] Anchos { get; set; } = [];          // en puntos; deben sumar 515 (ancho útil de la hoja)
    public bool[] Derecha { get; set; } = [];          // columnas alineadas a la derecha (números)
    public List<string[]> Filas { get; } = new();
    public string[]? Total { get; set; }               // fila final resaltada (opcional)
    public int ColumnaBarra { get; set; } = -1;        // columna con porcentaje ("45%") que se dibuja como barra
    public int ColumnaEstado { get; set; } = -1;       // columna con etiqueta de color ("Agotado" / "Stock bajo")
    public string Vacio { get; set; } = "No hay datos para este periodo.";
}

public static class PdfReporte
{
    // ---------- Medidas de la hoja (A4 en puntos) ----------
    const float Ancho = 595, Alto = 842, Margen = 40, AnchoUtil = Ancho - 2 * Margen;
    const float LimiteInferior = 70;   // espacio reservado para el pie de página

    // ---------- Colores de la marca (los mismos de site.css) ----------
    const string Tinta = "0.169 0.125 0.157";       // #2B2028
    const string TintaSuave = "0.431 0.380 0.412";  // #6E6169
    const string Linea = "0.925 0.890 0.902";       // #ECE3E6
    const string Crema = "0.984 0.965 0.953";       // #FBF6F3
    const string Rosa = "0.957 0.655 0.725";        // #F4A7B9
    const string RosaClaro = "0.988 0.910 0.933";   // #FCE8EE
    const string RosaOscuro = "0.663 0.314 0.431";  // #A9506E
    const string Blanco = "1 1 1";
    const string Error = "0.702 0.212 0.310", ErrorFondo = "0.988 0.894 0.910";   // #B3364F / #FCE4E8
    const string Aviso = "0.541 0.416 0", AvisoFondo = "1 0.953 0.769";           // #8A6A00 / #FFF3C4

    // Fuentes: F1 Helvetica, F2 Helvetica-Bold, F3 Times-Bold, F4 Helvetica-Oblique
    const string Normal = "F1", Negrita = "F2", Titular = "F3", Cursiva = "F4";

    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Crea el PDF. logoJpg es opcional: si viene null el encabezado se dibuja sin logo.</summary>
    public static byte[] Crear(DatosReporte datos, byte[]? logoJpg)
    {
        var paginas = new List<StringBuilder>();
        StringBuilder pagina = null!;
        float y = 0;

        void NuevaPagina(bool primera)
        {
            pagina = new StringBuilder();
            paginas.Add(pagina);
            y = primera ? Encabezado(pagina, datos, logoJpg != null) : EncabezadoContinuacion(pagina, datos);
        }

        NuevaPagina(true);

        // ---------- Tarjetas de indicadores ----------
        if (datos.Indicadores.Count > 0)
        {
            const float alto = 66, separacion = 12;
            int n = datos.Indicadores.Count;
            float ancho = (AnchoUtil - separacion * (n - 1)) / n;
            float arriba = y;
            for (int i = 0; i < n; i++)
            {
                float x = Margen + i * (ancho + separacion);
                Rect(pagina, x, arriba - alto, ancho, alto, Blanco, Linea);
                Rect(pagina, x, arriba - 3, ancho, 3, Rosa);
                // Si el texto no cabe en la tarjeta se reduce el tamaño de letra
                var nombre = datos.Indicadores[i].Nombre.ToUpperInvariant();
                float tamNombre = 7;
                while (tamNombre > 5.5f && Medir(nombre, Negrita, tamNombre, 1) > ancho - 28) tamNombre -= 0.25f;
                TextoEspaciado(pagina, nombre, x + 14, arriba - 22, Negrita, tamNombre, TintaSuave, 1);
                var valor = datos.Indicadores[i].Valor;
                float tamValor = 20;
                while (tamValor > 12 && Medir(valor, Titular, tamValor) > ancho - 28) tamValor -= 0.5f;
                Texto(pagina, Recortar(valor, Titular, tamValor, ancho - 28), x + 14, arriba - 50, Titular, tamValor, Tinta);
            }
            y = arriba - alto - 30;
        }

        // ---------- Tablas ----------
        const float altoEncabezado = 24, altoFila = 22, altoTitulo = 30;
        foreach (var tabla in datos.Tablas)
        {
            // Título de la sección (se pasa de página si no cabe con al menos dos filas)
            if (y - altoTitulo - altoEncabezado - 2 * altoFila < LimiteInferior) NuevaPagina(false);
            Rect(pagina, Margen, y - 14, 3, 16, Rosa);
            Texto(pagina, tabla.Titulo, Margen + 10, y - 11, Titular, 14, Tinta);
            y -= altoTitulo;

            EncabezadoTabla(pagina, tabla, y, altoEncabezado);
            y -= altoEncabezado;

            if (tabla.Filas.Count == 0)
            {
                Rect(pagina, Margen, y - 34, AnchoUtil, 34, Crema);
                Texto(pagina, tabla.Vacio, Margen + 12, y - 21, Cursiva, 9.5f, TintaSuave);
                y -= 34;
            }

            for (int f = 0; f < tabla.Filas.Count; f++)
            {
                if (y - altoFila < LimiteInferior)
                {
                    NuevaPagina(false);
                    Texto(pagina, tabla.Titulo + " (continuación)", Margen, y - 11, Titular, 12, TintaSuave);
                    y -= 24;
                    EncabezadoTabla(pagina, tabla, y, altoEncabezado);
                    y -= altoEncabezado;
                }
                if (f % 2 == 1) Rect(pagina, Margen, y - altoFila, AnchoUtil, altoFila, Crema);
                FilaTabla(pagina, tabla, tabla.Filas[f], y, altoFila, Normal, Tinta);
                LineaH(pagina, Margen, y - altoFila, AnchoUtil, Linea, 0.6f);
                y -= altoFila;
            }

            if (tabla.Total != null && tabla.Filas.Count > 0)
            {
                if (y - altoFila < LimiteInferior) NuevaPagina(false);
                Rect(pagina, Margen, y - altoFila - 2, AnchoUtil, altoFila + 2, RosaClaro);
                LineaH(pagina, Margen, y, AnchoUtil, RosaOscuro, 1);
                FilaTabla(pagina, tabla, tabla.Total, y - 1, altoFila, Negrita, Tinta, esTotal: true);
                y -= altoFila + 2;
            }
            y -= 28;
        }

        // ---------- Pie de página (se agrega al final para saber el total de páginas) ----------
        for (int i = 0; i < paginas.Count; i++)
        {
            var p = paginas[i];
            LineaH(p, Margen, 48, AnchoUtil, Linea, 0.8f);
            Texto(p, "BellasBoutique | Tienda Online  -  Reporte generado automáticamente por el sistema", Margen, 34, Normal, 7.5f, TintaSuave);
            var num = $"Página {i + 1} de {paginas.Count}";
            Texto(p, num, Ancho - Margen - Medir(num, Negrita, 7.5f), 34, Negrita, 7.5f, TintaSuave);
        }

        return Escribir(paginas, logoJpg);
    }

    // =====================================================================
    //  Partes del diseño
    // =====================================================================

    /// <summary>Banda superior de la primera página: logo, etiqueta, título, periodo y fecha de generación.</summary>
    static float Encabezado(StringBuilder p, DatosReporte d, bool conLogo)
    {
        const float alto = 132;
        Rect(p, 0, Alto - alto, Ancho, alto, RosaClaro);
        Rect(p, 0, Alto - alto - 4, Ancho, 4, Rosa);

        float xTexto = Margen;
        if (conLogo)
        {
            // Tarjeta blanca con el logo (480 x 339 px, proporción 1.416)
            const float altoLogo = 84, anchoLogo = altoLogo * 1.416f;
            Rect(p, Margen, Alto - 24 - altoLogo - 8, anchoLogo + 12, altoLogo + 8, Blanco, Linea);
            p.Append(Inv, $"q {anchoLogo:0.##} 0 0 {altoLogo:0.##} {Margen + 6:0.##} {Alto - 28 - altoLogo:0.##} cm /Im1 Do Q\n");
            xTexto = Margen + anchoLogo + 34;
        }

        TextoEspaciado(p, d.Etiqueta.ToUpperInvariant(), xTexto, Alto - 42, Negrita, 8, RosaOscuro, 1.6f);
        Texto(p, d.Titulo, xTexto, Alto - 72, Titular, 26, Tinta);
        Texto(p, "Periodo:", xTexto, Alto - 94, Negrita, 9.5f, Tinta);
        Texto(p, d.Periodo, xTexto + Medir("Periodo: ", Negrita, 9.5f), Alto - 94, Normal, 9.5f, Tinta);
        Texto(p, "Generado:", xTexto, Alto - 109, Negrita, 9.5f, TintaSuave);
        Texto(p, d.Generado.ToString("dd/MM/yyyy HH:mm", Inv), xTexto + Medir("Generado: ", Negrita, 9.5f), Alto - 109, Normal, 9.5f, TintaSuave);

        return Alto - alto - 4 - 26;   // donde empieza el contenido
    }

    /// <summary>Banda delgada para las páginas siguientes.</summary>
    static float EncabezadoContinuacion(StringBuilder p, DatosReporte d)
    {
        Rect(p, 0, Alto - 44, Ancho, 44, RosaClaro);
        Rect(p, 0, Alto - 47, Ancho, 3, Rosa);
        Texto(p, "BellasBoutique", Margen, Alto - 28, Titular, 15, Tinta);
        var derecha = d.Titulo + "  |  " + d.Periodo;
        Texto(p, derecha, Ancho - Margen - Medir(derecha, Normal, 9), Alto - 27, Normal, 9, TintaSuave);
        return Alto - 47 - 26;
    }

    static void EncabezadoTabla(StringBuilder p, TablaReporte t, float y, float alto)
    {
        Rect(p, Margen, y - alto, AnchoUtil, alto, Tinta);
        float x = Margen;
        for (int c = 0; c < t.Encabezados.Length; c++)
        {
            var txt = t.Encabezados[c].ToUpperInvariant();
            bool der = c < t.Derecha.Length && t.Derecha[c];
            float w = Medir(txt, Negrita, 7.5f, 0.8f);
            float tx = der ? x + t.Anchos[c] - 10 - w : x + 10;
            TextoEspaciado(p, txt, tx, y - alto + 9, Negrita, 7.5f, Blanco, 0.8f);
            x += t.Anchos[c];
        }
    }

    static void FilaTabla(StringBuilder p, TablaReporte t, string[] celdas, float y, float alto, string fuente, string color, bool esTotal = false)
    {
        const float tam = 9.5f;
        float x = Margen, base_ = y - alto + 7.5f;
        for (int c = 0; c < t.Anchos.Length && c < celdas.Length; c++)
        {
            float w = t.Anchos[c];
            var txt = celdas[c] ?? "";
            bool der = c < t.Derecha.Length && t.Derecha[c];

            if (c == t.ColumnaBarra && !esTotal && txt.Length > 0)
            {
                // Barra de participación: fondo gris claro y relleno rosa según el porcentaje
                float pct = Math.Clamp(Porcentaje(txt), 0, 100);
                float largo = w - 58;
                Rect(p, x + 10, base_ + 1, largo, 5, Linea);
                if (pct > 0) Rect(p, x + 10, base_ + 1, Math.Max(2, largo * pct / 100), 5, RosaOscuro);
                Texto(p, txt, x + w - 10 - Medir(txt, Negrita, 8.5f), base_, Negrita, 8.5f, Tinta);
            }
            else if (c == t.ColumnaEstado && !esTotal && txt.Length > 0)
            {
                // Etiqueta de color, igual que los estados del panel de administración
                bool agotado = txt.StartsWith("Agotado", StringComparison.OrdinalIgnoreCase);
                float wTxt = Medir(txt.ToUpperInvariant(), Negrita, 7, 0.6f);
                Rect(p, x + 10, base_ - 4, wTxt + 14, 15, agotado ? ErrorFondo : AvisoFondo);
                TextoEspaciado(p, txt.ToUpperInvariant(), x + 17, base_, Negrita, 7, agotado ? Error : Aviso, 0.6f);
            }
            else
            {
                txt = Recortar(txt, fuente, tam, w - 20);
                float tx = der ? x + w - 10 - Medir(txt, fuente, tam) : x + 10;
                Texto(p, txt, tx, base_, fuente, tam, color);
            }
            x += w;
        }
    }

    // =====================================================================
    //  Dibujo básico
    // =====================================================================

    static void Rect(StringBuilder p, float x, float y, float w, float h, string relleno, string? borde = null)
    {
        if (borde == null) p.Append(Inv, $"{relleno} rg {x:0.##} {y:0.##} {w:0.##} {h:0.##} re f\n");
        else p.Append(Inv, $"{relleno} rg {borde} RG 0.8 w {x:0.##} {y:0.##} {w:0.##} {h:0.##} re B\n");
    }

    static void LineaH(StringBuilder p, float x, float y, float w, string color, float grosor) =>
        p.Append(Inv, $"{color} RG {grosor:0.##} w {x:0.##} {y:0.##} m {x + w:0.##} {y:0.##} l S\n");

    static void Texto(StringBuilder p, string s, float x, float y, string fuente, float tam, string color) =>
        TextoEspaciado(p, s, x, y, fuente, tam, color, 0);

    static void TextoEspaciado(StringBuilder p, string s, float x, float y, string fuente, float tam, string color, float espaciado) =>
        p.Append(Inv, $"BT {color} rg /{fuente} {tam:0.##} Tf {espaciado:0.##} Tc {x:0.##} {y:0.##} Td ({Escapar(s)}) Tj ET\n");

    static string Escapar(string s) => Limpiar(s).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    /// <summary>Cambia caracteres que no existen en las fuentes estándar del PDF.</summary>
    static string Limpiar(string s) => s.Replace("₡", "CRC ").Replace("–", "-").Replace("—", "-")
        .Replace("“", "\"").Replace("”", "\"").Replace("‘", "'").Replace("’", "'").Replace("\u00A0", " ").Replace("\u202F", " ").Replace("\r", " ").Replace("\n", " ");

    static float Porcentaje(string s)
    {
        var digitos = new string(s.Where(ch => char.IsDigit(ch) || ch == '.').ToArray());
        return float.TryParse(digitos, NumberStyles.Float, Inv, out var v) ? v : 0;
    }

    /// <summary>Corta el texto con "..." si no cabe en el ancho disponible.</summary>
    static string Recortar(string s, string fuente, float tam, float maximo)
    {
        s = Limpiar(s);
        if (Medir(s, fuente, tam) <= maximo) return s;
        while (s.Length > 1 && Medir(s + "...", fuente, tam) > maximo) s = s[..^1];
        return s.TrimEnd() + "...";
    }

    // Anchos de las fuentes estándar (en milésimas del tamaño) para los caracteres 32 a 126.
    static readonly short[] AnchoHelvetica =
    [
        278,278,355,556,556,889,667,191,333,333,389,584,278,333,278,278,556,556,556,556,556,556,556,556,556,556,278,278,584,584,584,556,
        1015,667,667,722,722,667,611,778,722,278,500,667,556,833,722,778,667,778,722,667,611,722,667,944,667,667,611,278,278,278,469,556,
        333,556,556,500,556,556,278,556,556,222,222,500,222,833,556,556,556,556,333,500,278,556,500,722,500,500,500,334,260,334,584
    ];
    static readonly short[] AnchoHelveticaNegrita =
    [
        278,333,474,556,556,889,722,238,333,333,389,584,278,333,278,278,556,556,556,556,556,556,556,556,556,556,333,333,584,584,584,611,
        975,722,722,722,722,667,611,778,722,278,556,722,611,833,722,778,667,778,722,667,611,722,667,944,667,667,611,333,278,333,584,556,
        333,556,611,556,611,556,333,611,611,278,278,556,278,889,611,611,611,611,389,556,333,611,556,778,556,556,500,389,280,389,584
    ];
    static readonly short[] AnchoTimesNegrita =
    [
        250,333,555,500,500,1000,833,278,333,333,500,570,250,333,250,278,500,500,500,500,500,500,500,500,500,500,333,333,570,570,570,500,
        930,722,667,722,722,667,611,778,778,389,500,778,667,944,722,778,611,778,722,556,667,722,722,1000,722,722,667,333,278,333,581,500,
        333,500,556,444,556,444,333,500,556,278,333,556,278,833,556,500,556,556,444,389,333,556,500,722,500,500,444,394,220,394,520
    ];

    /// <summary>Ancho aproximado de un texto en puntos (las letras con tilde usan el ancho de la letra base).</summary>
    static float Medir(string s, string fuente, float tam, float espaciado = 0)
    {
        var tabla = fuente switch { Negrita => AnchoHelveticaNegrita, Titular => AnchoTimesNegrita, _ => AnchoHelvetica };
        float total = 0;
        foreach (var original in Limpiar(s))
        {
            char ch = original;
            if (ch > 126) ch = ch.ToString().Normalize(NormalizationForm.FormD)[0];   // á -> a, Ñ -> N
            total += ch is >= ' ' and <= '~' ? tabla[ch - 32] : 556;
        }
        return total * tam / 1000 + espaciado * s.Length;
    }

    // =====================================================================
    //  Escritura del archivo PDF
    // =====================================================================

    static byte[] Escribir(List<StringBuilder> paginas, byte[]? logoJpg)
    {
        var enc = Encoding.Latin1;
        var objetos = new List<byte[]>();
        int Agregar(byte[] contenido) { objetos.Add(contenido); return objetos.Count; }
        byte[] B(string s) => enc.GetBytes(s);

        int catalogo = Agregar([]);   // se completan al final
        int arbol = Agregar([]);
        int f1 = Agregar(B("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));
        int f2 = Agregar(B("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"));
        int f3 = Agregar(B("<< /Type /Font /Subtype /Type1 /BaseFont /Times-Bold /Encoding /WinAnsiEncoding >>"));
        int f4 = Agregar(B("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Oblique /Encoding /WinAnsiEncoding >>"));

        string recursoImagen = "";
        if (logoJpg != null && TamanoJpg(logoJpg) is var (w, h, comp) && w > 0)
        {
            var dic = B($"<< /Type /XObject /Subtype /Image /Width {w} /Height {h} /ColorSpace /{(comp == 1 ? "DeviceGray" : comp == 4 ? "DeviceCMYK" : "DeviceRGB")} /BitsPerComponent 8 /Filter /DCTDecode /Length {logoJpg.Length} >>\nstream\n");
            int img = Agregar([.. dic, .. logoJpg, .. B("\nendstream")]);
            recursoImagen = $" /XObject << /Im1 {img} 0 R >>";
        }

        var hijos = new List<string>();
        foreach (var pagina in paginas)
        {
            var contenido = pagina.ToString();
            int flujo = Agregar(B($"<< /Length {enc.GetByteCount(contenido)} >>\nstream\n{contenido}endstream"));
            int pag = Agregar(B($"<< /Type /Page /Parent {arbol} 0 R /MediaBox [0 0 {Ancho} {Alto}] " +
                $"/Resources << /Font << /F1 {f1} 0 R /F2 {f2} 0 R /F3 {f3} 0 R /F4 {f4} 0 R >>{recursoImagen} >> /Contents {flujo} 0 R >>"));
            hijos.Add($"{pag} 0 R");
        }
        objetos[catalogo - 1] = B($"<< /Type /Catalog /Pages {arbol} 0 R >>");
        objetos[arbol - 1] = B($"<< /Type /Pages /Kids [{string.Join(" ", hijos)}] /Count {hijos.Count} >>");

        using var ms = new MemoryStream();
        void W(byte[] b) => ms.Write(b);
        W(B("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n"));
        var posiciones = new List<long>();
        for (int i = 0; i < objetos.Count; i++)
        {
            posiciones.Add(ms.Position);
            W(B($"{i + 1} 0 obj\n")); W(objetos[i]); W(B("\nendobj\n"));
        }
        long xref = ms.Position;
        W(B($"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n"));
        foreach (var pos in posiciones) W(B(pos.ToString("D10", Inv) + " 00000 n \n"));
        W(B($"trailer\n<< /Size {objetos.Count + 1} /Root {catalogo} 0 R >>\nstartxref\n{xref}\n%%EOF\n"));
        return ms.ToArray();
    }

    /// <summary>Lee ancho, alto y número de componentes de color de un JPG (marcadores SOF).</summary>
    static (int Ancho, int Alto, int Componentes) TamanoJpg(byte[] d)
    {
        int i = 2;
        while (i + 9 < d.Length)
        {
            if (d[i] != 0xFF) { i++; continue; }
            byte marca = d[i + 1];
            if (marca is >= 0xC0 and <= 0xCF && marca is not (0xC4 or 0xC8 or 0xCC))
                return ((d[i + 7] << 8) | d[i + 8], (d[i + 5] << 8) | d[i + 6], d[i + 9]);
            i += 2 + ((d[i + 2] << 8) | d[i + 3]);
        }
        return (0, 0, 0);
    }
}
