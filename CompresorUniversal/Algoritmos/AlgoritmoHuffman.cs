using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompresorUniversal.Algoritmos
{
    // *Implementación del algoritmo de Huffman*
    // Se implementa utilizando el algoritmo que se explica en el libro:
    // contar frecuencias, armar un árbol donde los símbolos más
    // comunes queden con códigos más cortos
    // y luego usar ese árbol para convertir los datos a bits.
    public class AlgoritmoHuffman
    {
        // Nodo del árbol de Huffman.
        // Si "Simbolo" es null quiere decir que es un nodo interno del árbol.
        //inicialización de la clase NodoHuffman.
        private class NodoHuffman
        {
            public byte? Simbolo { get; set; }// Símbolo si es hoja
            public int Frecuencia { get; set; } // cuántas veces aparece
            public NodoHuffman Izquierdo { get; set; }
            public NodoHuffman Derecho { get; set; }

            public bool EsHoja => Izquierdo == null && Derecho == null; 
        }

        // Compresión
        public byte[] Comprimir(byte[] datos)
        {
            if (datos == null || datos.Length == 0)
                return new byte[0]; // Caso vacío

            // 1) Contar cuántas veces aparece cada byte.
            var frecuencias = new Dictionary<byte, int>();
            foreach (byte b in datos)
            {
                if (!frecuencias.ContainsKey(b))
                    frecuencias[b] = 0;  // inicializar
                frecuencias[b]++;   // contar
            }

            // 2) Crear un nodo por símbolo y ordenarlos por frecuencia.
            // el libro lo hace con cola de prioridad, aquí se usa una lista.
            var nodos = new List<NodoHuffman>();
            foreach (var par in frecuencias)
                nodos.Add(new NodoHuffman { Simbolo = par.Key, Frecuencia = par.Value });
            nodos.Sort((a, b) => a.Frecuencia.CompareTo(b.Frecuencia)); // ordenar por frecuencia

            // 3) Armar el árbol juntando siempre los dos nodos menos frecuentes
            while (nodos.Count > 1)
            {
                var n1 = nodos[0];
                var n2 = nodos[1];
                nodos.RemoveAt(0);
                nodos.RemoveAt(0);

                var padre = new NodoHuffman
                {
                    Simbolo = null,      // nodo interno
                    Frecuencia = n1.Frecuencia+ n2.Frecuencia,
                    Izquierdo = n1,
                    Derecho = n2
                };

                //Insertar el nodo padre devolviéndolo a la lista ordenada
                int pos = nodos.FindIndex(n => n.Frecuencia >= padre.Frecuencia);
                if (pos >= 0)
                    nodos.Insert(pos, padre); // se inserta
                else
                    nodos.Add(padre);  // va al final
            }

            var raiz = nodos[0]; // el árbol final

            // 4) Recorrer el árbol y asignar códigos booleanos:izquierda=0, derecha=1
            var codigos = new Dictionary<byte,List<bool>>();

            GenerarCodigos(raiz, new List<bool>(), codigos);


            // 5) Reemplazar cada byte del archivo con su código binario.  
            var bitsComprimidos = CodificarDatos(datos, codigos);


            // 6) Guardar árbol y datos en un solo arreglo de bytes
            return SerializarResultado(raiz, bitsComprimidos, datos.Length);


        }

        // Descompresión: Para la descompresión simplemente consiste en hacer lo contrario de la compresión.
        public byte[] Descomprimir(byte[] datosComprimidos, int tamañoOriginal)
        {
            if (datosComprimidos == null || datosComprimidos.Length == 0)
                return new byte[0];

            using (var ms = new MemoryStream(datosComprimidos))

            using (var reader = new BinaryReader(ms))
            {
                // leer info del árbol (cuántos bits tiene y cuántos bytes ocupa)
                int bitsArbol = reader.ReadInt32();
                int bytesArbol = reader.ReadInt32();



                var arbolBytes = reader.ReadBytes(bytesArbol);
                var arbolBits = ConvertirBytesABits(arbolBytes, bitsArbol);

                int pos = 0;
                var raiz = DeserializarArbol(arbolBits, ref pos); // se reconstruye el árbol

                // Revisar que el tamaño coincida
                int tamañoGuardado = reader.ReadInt32();
                if (tamañoGuardado != tamañoOriginal)
                    throw new InvalidDataException("El tamaño original no coincide.");

                // caso donde solo haya un símbolo en todo el archivo (caso de error)
                if (raiz.EsHoja)

                    return Enumerable.Repeat(raiz.Simbolo.Value, tamañoOriginal).ToArray();

                // Leer el resto de datos (los bits del mensaje comprimido)
                var datosBytes = reader.ReadBytes((int)(ms.Length - ms.Position)); 
                var datosBits = ConvertirBytesABits(datosBytes);

                var resultado = new List<byte>();
                var actual = raiz;

                // Recorrer el árbol según los bits.
                foreach (bool bit in datosBits)
                {
                    actual = bit ? actual.Derecho : actual.Izquierdo; // derecha = 1, izquierda = 0

                    if (actual.EsHoja)
                    {
                        resultado.Add(actual.Simbolo.Value);
                        actual = raiz;

                        if (resultado.Count == tamañoOriginal)
                            break;
                    }
                }

                return resultado.ToArray();
            }
        }

        // asignar códigos a cada simbolo recorriendo el árbol
        private void GenerarCodigos(NodoHuffman nodo, List<bool> codigo, Dictionary<byte, List<bool>> codigos)
        {
            if (nodo == null) return;

            if (nodo.EsHoja)
            {
                // se copia el código actual y se asocia al símbolo
                codigos[nodo.Simbolo.Value] = new List<bool>(codigo);
            }
            else
            {
                // izquierda = 0
                codigo.Add(false);
                GenerarCodigos(nodo.Izquierdo, codigo, codigos);
                codigo.RemoveAt(codigo.Count - 1);

                // derecha = 1
                codigo.Add(true);
                GenerarCodigos(nodo.Derecho, codigo, codigos);
                codigo.RemoveAt(codigo.Count - 1);
            }
        }

        // reemplazar cada byte original por su secuencia de bits
        private List<bool> CodificarDatos(byte[] datos, Dictionary<byte, List<bool>> codigos)
        {
            var bits = new List<bool>(); // lista acumulada.
            foreach (var b in datos)
                bits.AddRange(codigos[b]); // agregar código correspondiente
            return bits;
        }

        // Empaquetar árbol junto a los bits comprimidos en formato binario
        private byte[] SerializarResultado(NodoHuffman raiz, List<bool> bitsComprimidos, int tamañoOriginal)
        {
            var arbolBits = new List<bool>();
            SerializarArbol(raiz, arbolBits);

            var arbolBytes = ConvertirBitsABytes(arbolBits);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(arbolBits.Count);
                writer.Write(arbolBytes.Length);
                writer.Write(arbolBytes);
                writer.Write(tamañoOriginal);

                writer.Write(ConvertirBitsABytes(bitsComprimidos));

                return ms.ToArray();
            }
        }

        // Guardar el árbol como bits
        private void SerializarArbol(NodoHuffman nodo, List<bool> bits)
        {
            if (nodo.EsHoja)
            {
                
                bits.Add(true); // 1 = hoja
                byte simbolo = nodo.Simbolo.Value;

                for (int i = 7; i >= 0; i--)
                    bits.Add(((simbolo >> i) & 1) == 1);
            }
            else
            {
                // 0 = nodo interno
                bits.Add(false);
                SerializarArbol(nodo.Izquierdo, bits);
                SerializarArbol(nodo.Derecho, bits);
            }
        }


        // Reconstruir el árbol leyendo los bits
        private NodoHuffman DeserializarArbol(List<bool> bits, ref int pos)
        {
            if (pos >= bits.Count)
                return null;

            bool esHoja = bits[pos++];

            if (esHoja)
            {
                byte simbolo = 0;

                for (int i = 0; i < 8; i++)


                    simbolo = (byte)((simbolo << 1) | (bits[pos++] ? 1 : 0)); // leer los 8 bits

                return new NodoHuffman { Simbolo = simbolo };
            }
            else
            {
                var nodo = new NodoHuffman();
                nodo.Izquierdo = DeserializarArbol(bits, ref pos);
                nodo.Derecho = DeserializarArbol(bits, ref pos);
                return nodo;
            }
        }

        // Método para pasar bits a bytes
        private byte[] ConvertirBitsABytes(List<bool> bits)
        {
            int n = (bits.Count + 7) / 8; // redondeo
            var bytes = new byte[n];
            int index = 0;

            for (int i = 0; i < n; i++)
            {
                byte b = 0;
                for (int j = 0; j < 8 && index < bits.Count; j++)
                {
                    if (bits[index])
                        b |= (byte)(1 << (7 - j)); // escribir bit correspondiente
                    index++;
                }
                bytes[i] = b;
            }
            return bytes;
        }



        // Método para pasar bytes a bits
        private List<bool> ConvertirBytesABits(byte[] bytes, int limiteBits = -1)
        {
            int total = (limiteBits >= 0) ? limiteBits : bytes.Length * 8;
            var lista = new List<bool>(total);

            foreach (byte b in bytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (lista.Count == total)
                        return lista;
                    lista.Add(((b >> i) & 1) == 1); // se extrae bit por bit
                }
            }

            return lista;
        }
    }
}
