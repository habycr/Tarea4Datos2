using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompresorUniversal.Algoritmos
{
    // Implementación del algoritmo de compresión LZ78
    // La idea general es mantener un diccionario de "frases"
    // y cada salida es (índicePrefijo, símboloNuevo). Esto va armando un diccionario creciente


    public class AlgoritmoLZ78
    {
        // Clase interna para representar una pareja (índice de prefijo, símbolo)
        private class Pareja
        {
            public int Prefijo; // índice en el diccionario
            public byte Simbolo; // byte que se agrega

            public Pareja(int prefijo, byte simbolo)
            {
                Prefijo = prefijo; // prefijo a usar en la nueva entrada
                Simbolo = simbolo; // símbolo que se añade
            }
        }





        // Nodo del trie usado internamente para el diccionario de LZ78
        private class NodoTrie
        {
            public int Indice; // índice asignado a esta frase
            public Dictionary<byte, NodoTrie> Hijos = new Dictionary<byte, NodoTrie>();

            public NodoTrie(int indice)
            {
                Indice = indice;
            }
        }






        // Método de compresión
        public byte[] Comprimir(byte[] datos)
        {
            if (datos == null || datos.Length == 0)
                return new byte[0];

            // Se usa un TRIE real donde cada nodo representa una frase
            // y tiene hijos etiquetados por byte
            List<Pareja> salida = new List<Pareja>();

            // Raíz del trie: índice 0 = secuencia vacía
            NodoTrie raiz = new NodoTrie(0);
            int siguienteIndice = 1;

            NodoTrie nodoActual = raiz;
            int indicePrefijo = 0;
            List<byte> bytesAcumulados = new List<byte>(); // Bytes acumulados en la secuencia actual

            // Recorrer cada byte en los datos de entrada
            foreach (byte b in datos)
            {
                // Existe ya el hijo con este símbolo desde la frase actual?
                if (nodoActual.Hijos.TryGetValue(b, out NodoTrie hijo))
                {
                    // Si la combinación ya existe en el diccionario, entonces se sigue
                    // extendiendo la frase actual
                    nodoActual = hijo;
                    indicePrefijo = hijo.Indice;
                    bytesAcumulados.Add(b);
                }
                else
                {
                    // Si no existe, se emite la pareja actual 
                    salida.Add(new Pareja(indicePrefijo, b));

                    // Crear nueva frase en el trie con un índice nuevo
                    NodoTrie nuevo = new NodoTrie(siguienteIndice++);
                    nodoActual.Hijos[b] = nuevo;

                    // Reiniciar el prefijo para la siguiente secuencia
                    nodoActual = raiz;
                    indicePrefijo = 0;
                    bytesAcumulados.Clear();
                }
            }

            // Si quedaron bytes acumulados (terminamos en medio de una secuencia),
            // los emitimos como parejas individuales desde la raíz
            if (bytesAcumulados.Count > 0)
            {
                foreach (byte b in bytesAcumulados)
                {
                    salida.Add(new Pareja(0, b));
                }
            }

            // Serializar la lista de parejas en bytes
            return SerializarParejas(salida);
        }






        // Método de descompresión
        public byte[] Descomprimir(byte[] datosComprimidos, int tamañoOriginal)
        {
            if (datosComprimidos == null || datosComprimidos.Length == 0)
                return new byte[0];

            using (MemoryStream ms = new MemoryStream(datosComprimidos))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int numParejas = reader.ReadInt32(); // cuántas parejas vienen serializadas

                // Lista para el diccionario de secuencias donde índice 0 = secuencia vacía
                List<List<byte>> diccionario = new List<List<byte>>();
                diccionario.Add(new List<byte>());

                List<byte> resultado = new List<byte>();

                // Recorrer cada pareja para reconstruir las frases originales
                for (int i = 0; i < numParejas; i++)
                {
                    int prefijo = reader.ReadUInt16(); // índice del diccionario (ahora es ushort)
                    byte simbolo = reader.ReadByte(); // byte a agregar

                    List<byte> nuevaSecuencia;
                    if (prefijo == 0)
                    {
                        // Si el prefijo es 0, la frase comienza desde cero
                        nuevaSecuencia = new List<byte>();
                    }
                    else
                    {
                        // Si no, copiamos la frase indicada por el prefijo
                        nuevaSecuencia = new List<byte>(diccionario[prefijo]);
                    }

                    // Agregar el símbolo a la secuencia
                    nuevaSecuencia.Add(simbolo);

                    // Añadir la secuencia al resultado de salida
                    resultado.AddRange(nuevaSecuencia);

                    // Agregar la nueva secuencia al diccionario
                    diccionario.Add(nuevaSecuencia);
                }

                // Verificar que el tamaño obtenido coincida con el tamaño original
                if (resultado.Count != tamañoOriginal)
                {

                    throw new InvalidDataException("El tamaño de los datos descomprimidos no coincide con el tamaño original.");

                }


                return resultado.ToArray();
            }
        }








        // Serializa la lista de parejas a un arreglo de bytes
        private byte[] SerializarParejas(List<Pareja> parejas)

        {

          
            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                // Se escribe el número de parejas
                writer.Write(parejas.Count);
                // para escribir cada pareja se uso el índice de prefijo (ushort) y símbolo (byte)
                foreach (Pareja p in parejas)
                {

                    writer.Write((ushort)p.Prefijo); // índice del diccionario (2 bytes en lugar de 4)
                    writer.Write(p.Simbolo); // símbolo que se añade (1 byte)

                }
                return ms.ToArray();
            }
        }
    }
}