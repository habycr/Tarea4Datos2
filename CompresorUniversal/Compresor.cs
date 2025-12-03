using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CompresorUniversal.Algoritmos;




// Clase que se encarga de generar y leer archivos .myzip.
namespace CompresorUniversal
{
    
    public class Compresor
    {
        private static readonly byte[] MAGIC = Encoding.ASCII.GetBytes("MYZIP");   // firma de formato

        private const byte CODIGO_HUFFMAN = 1;   // Huffman.
        private const byte CODIGO_LZ77 = 2;         //LZ77
        private const byte CODIGO_LZ78 = 3;   // LZ78






        // Método que recibe varias rutas y genera un único archivo comprimido.
        // El archivo .myzip contiene metadatos básicos y los bytes comprimidos..
        public void ComprimirArchivos(List<string> rutasArchivos, string archivoSalida,
            string algoritmo, out long totalOriginal, out long totalComprimido)

        {
            totalOriginal = 0;

            totalComprimido = 0;

            byte codigoAlgoritmo = ObtenerCodigoAlgoritmo(algoritmo);  // convertir nombre a número.

            using (FileStream fs = new FileStream(archivoSalida, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))



            {

                // Cabecera del archivo.
                writer.Write(MAGIC);
                writer.Write(codigoAlgoritmo);

                writer.Write(rutasArchivos.Count);

                foreach (string ruta in rutasArchivos)
                {
                    byte[] datosOriginales = File.ReadAllBytes(ruta);

                    totalOriginal += datosOriginales.Length;     // acumula tamaño original

                    // Comprimir usando el algoritmo elegido por el usuario (desde la UI).
                    byte[] datosComprimidos = ComprimirDatos(datosOriginales, algoritmo);

                    totalComprimido += datosComprimidos.Length;


                    string nombreArchivo = Path.GetFileName(ruta);    // sólo el nombre, sin ruta

                    writer.Write(nombreArchivo);              // nombre del archivo dentro del paquete
                    writer.Write(datosOriginales.Length);     // tamaño original
                    writer.Write(datosComprimidos.Length);    // tamaño comprimido


                    writer.Write(datosComprimidos);           // bytes comprimidos reales
                }
            }
        }


        // Descomprime un archivo .myzip completo y lo deja en una carpeta destino.
        // retorna estadísticas para la UI (tamaños y algoritmo).
        public void DescomprimirArchivos(string archivoEntrada, string carpetaSalida,
            out long totalOriginal, out long totalComprimido, out string algoritmoUsado)
        {
            totalOriginal = 0;
            totalComprimido = 0;

            using (FileStream fs = new FileStream(archivoEntrada, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // Leer la firma del archivo. Si no coincide, el archivo no es válido..
                byte[] magic = reader.ReadBytes(MAGIC.Length);
                if (!magic.SequenceEqual(MAGIC))
                {
                    throw new Exception("El archivo no es un .myzip válido");    // error
                }

                // El archivo guarda el número del algoritmo, no el nombre.
                byte codigoAlgoritmo = reader.ReadByte();
                algoritmoUsado = ObtenerNombreAlgoritmo(codigoAlgoritmo);

                int numArchivos = reader.ReadInt32();  // cuántos archivos contiene el .myzip

                for (int i = 0; i < numArchivos; i++)
                {
                    string nombreArchivo = reader.ReadString();           // nombre original
                    int tamañoOriginal = reader.ReadInt32();              // guardado en cabecera
                    int tamañoComprimido = reader.ReadInt32();
                    byte[] datosComprimidos = reader.ReadBytes(tamañoComprimido);

                    totalOriginal += tamañoOriginal;
                    totalComprimido += tamañoComprimido;

                    // Descomprimir según el algoritmo registrado.
                    byte[] datosOriginales = DescomprimirDatos(datosComprimidos,
                        tamañoOriginal, algoritmoUsado);

                    // Guardar el archivo restaurado en la carpeta de salida.
                    string rutaSalida = Path.Combine(carpetaSalida, nombreArchivo);
                    File.WriteAllBytes(rutaSalida, datosOriginales);   // se escribe en disco
                }
            }
        }








        // Recibe los datos y delega la compresión al algoritmo correcto.
        private byte[] ComprimirDatos(byte[] datos, string algoritmo)
        {
            switch (algoritmo)
            {
                case "Huffman":

                    AlgoritmoHuffman h = new AlgoritmoHuffman();

                    return h.Comprimir(datos);  

                case "LZ77":
                    AlgoritmoLZ77 lz77 = new AlgoritmoLZ77();

                    return lz77.Comprimir(datos);

                case "LZ78":

                    AlgoritmoLZ78 lz78 = new AlgoritmoLZ78();

                    return lz78.Comprimir(datos);

                default:


                    throw new Exception("Algoritmo no reconocido: " + algoritmo);
            }


        }








        // Hace lo opuesto: recibe bytes comprimidos y usa el algoritmo indicado para restaurarlos.
        // tamañoOriginal sirve para validación y para saber cuándo parar.
        private byte[] DescomprimirDatos(byte[] datosComprimidos, int tamañoOriginal, string algoritmo)
        {
            switch (algoritmo)
            {
                case "Huffman":
                    AlgoritmoHuffman h = new AlgoritmoHuffman();
                    return h.Descomprimir(datosComprimidos, tamañoOriginal);

                case "LZ77":
                    AlgoritmoLZ77 lz77 = new AlgoritmoLZ77();
                    return lz77.Descomprimir(datosComprimidos, tamañoOriginal);

                case "LZ78":
                    AlgoritmoLZ78 lz78 = new AlgoritmoLZ78();
                    return lz78.Descomprimir(datosComprimidos, tamañoOriginal);

                default:
                    throw new Exception("Algoritmo no reconocido: " + algoritmo);   // caso imposible desde UI
            }
        }








        // Recibe un nombre de algoritmo y lo convierte a un código numérico
        // que se guarda en la cabecera del archivo.
        private byte ObtenerCodigoAlgoritmo(string algoritmo)
        {
            switch (algoritmo)
            {
                case "Huffman": return CODIGO_HUFFMAN;
                case "LZ77": return CODIGO_LZ77;
                case "LZ78": return CODIGO_LZ78;
                default:
                    throw new Exception("Algoritmo desconocido: " + algoritmo);
            }
        }








        // Conversión inversa:  convierte código → nombre.
        // Útil cuando se lee la cabecera de un .myzip.
        private string ObtenerNombreAlgoritmo(byte codigo)
        {
            switch (codigo)
            {
                case CODIGO_HUFFMAN: return "Huffman";    // normal.
                case CODIGO_LZ77: return "LZ77";
                case CODIGO_LZ78: return "LZ78";
                default:
                    throw new Exception("Código de algoritmo desconocido: " + codigo);
            }
        }
    }
}
