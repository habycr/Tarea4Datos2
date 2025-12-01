using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompresorUniversal.Algoritmos
{
    // Implementación del algoritmo de compresión LZ77
    // usar una ventana que mira hacia atrás
    // para encontrar cadenas repetidas y codificarlas como desplazamiento, longitud, siguienteByte.



    public class AlgoritmoLZ77
    {
        // Tamaño de la ventana de búsqueda: 4KB 
        private const int TAM_VENTANA = 4096;

        // Clase interna para representar una tripleta (desplazamiento, longitud, siguiente byte)
        private class Tripleta
        {
            public int Desplazamiento;
            public int Longitud;
            public byte SiguienteByte;

            public Tripleta(int desplazamiento, int longitud, byte siguienteByte)
            {
                Desplazamiento = desplazamiento;
                Longitud = longitud;
                SiguienteByte = siguienteByte; // byte literal que va después de la coincidencia.
            }

            public Tripleta() { }
        }





        // Método de compresión
        public byte[] Comprimir(byte[] datos)
        {
            if (datos == null || datos.Length == 0)
                return new byte[0];

            List<Tripleta> tripletas = new List<Tripleta>();
            int i = 0;
            // Recorrer todos los datos de entrada
            while (i < datos.Length)
            {
                int mejorDesplazamiento = 0;
                int mejorLongitud = 0;
                // La ventana mira hacia atrás desde i
                int inicioVentana = (i > TAM_VENTANA) ? i - TAM_VENTANA : 0;

                // Buscar hacia atrás la secuencia más larga que coincida con la posición actual
                for (int j = inicioVentana; j < i; j++)
                {
                    int longitud = 0;
                    // Comparar mientras haya coincidencia y no se salga del arreglo
                    while (i + longitud < datos.Length && datos[j + longitud] == datos[i + longitud])
                    {
                        longitud++;
                        if (i + longitud >= datos.Length) break; //para evitar salirse del array
                    }

                    //si se encuentra algo mejor, se guarda
                    if (longitud > mejorLongitud)
                    {
                        mejorLongitud = longitud;
                        mejorDesplazamiento = i - j;
                    }
                    // Si se encuentra una coincidencia que llega hasta el final de los datos, no se necesita buscar más
                    if (mejorLongitud == datos.Length - i)
                        break;
                }

                // Determinar el siguiente byte después de la secuencia coincidente
                if (mejorLongitud > 0 && i + mejorLongitud < datos.Length)
                {
                    byte siguienteByte = datos[i + mejorLongitud]; // byte literal
                    tripletas.Add(new Tripleta(mejorDesplazamiento, mejorLongitud, siguienteByte)); 
                    i += mejorLongitud + 1;
                }
                else
                {
                    // Si no hubo coincidencia (longitud 0) o la coincidencia llega hasta el final de los datos
                    tripletas.Add(new Tripleta(0, 0, datos[i]));
                    i += 1;
                }
            }

            // Serializar la lista de tripletas en bytes
            return SerializarTripletas(tripletas);
        }






        // Método de descompresión
        public byte[] Descomprimir(byte[] datosComprimidos, int tamañoOriginal)
        {
            if (datosComprimidos == null || datosComprimidos.Length == 0)
                return new byte[0];

            List<Tripleta> tripletas = DeserializarTripletas(datosComprimidos);
            List<byte> resultado = new List<byte>();

            foreach (Tripleta t in tripletas)


            {
                // Si hay una referencia atrás (desplazamiento y longitud mayor a 0), copiar los bytes indicados
                if (t.Desplazamiento > 0 && t.Longitud > 0)
                {
                    int inicioCopia = resultado.Count - t.Desplazamiento;
                    for (int k = 0; k < t.Longitud; k++)
                    {
                        resultado.Add(resultado[inicioCopia + k]);
                    }

                }

                // Agregar el siguiente byte literal
                resultado.Add(t.SiguienteByte);
            }

            // Verificar el tamaño original reconstruido
            if (resultado.Count != tamañoOriginal)
            {
                throw new InvalidDataException("El tamaño de los datos descomprimidos no coincide con el tamaño original.");
            }


            return resultado.ToArray();
        }





        // Serializa la lista de tripletas a un arreglo de bytes
        private byte[] SerializarTripletas(List<Tripleta> tripletas)
        {


            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // Escribir el número de tripletas
                writer.Write(tripletas.Count);
                // Escribir cada tripleta: desplazamiento, longitud, carácter siguiente
                foreach (Tripleta t in tripletas)
                {
                    writer.Write(t.Desplazamiento);
                    writer.Write(t.Longitud);
                    writer.Write(t.SiguienteByte);
                }
                return ms.ToArray();
            }
        }





        // Deserializa un arreglo de bytes en una lista de tripletas
        private List<Tripleta> DeserializarTripletas(byte[] datos)
        {
            List<Tripleta> tripletas = new List<Tripleta>();
            using (MemoryStream ms = new MemoryStream(datos))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int numTripletas = reader.ReadInt32();
                for (int i = 0; i < numTripletas; i++)
                {
                    Tripleta t = new Tripleta();
                    t.Desplazamiento = reader.ReadInt32();
                    t.Longitud = reader.ReadInt32();
                    t.SiguienteByte = reader.ReadByte();
                    tripletas.Add(t); // se agrega la tripleta reconstruida
                }
            }
            return tripletas;
        }
    }
}
