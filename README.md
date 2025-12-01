```md
# **README – Compresor Universal (Huffman, LZ77, LZ78)**

## **Introducción**
Este proyecto es una aplicación Windows Forms que permite comprimir y descomprimir archivos de texto utilizando los algoritmos **Huffman**, **LZ77** y **LZ78**.  
El programa genera archivos con formato `.myzip` y muestra estadísticas de tiempo, memoria y tasa de compresión.

---

# **USO DEL PROGRAMA**

## **__1. Abrir el proyecto__**
1. Abrir la solución en Visual Studio 2022.
2. Ejecutar el WinForms llamado **Compresor Universal**.
3. Se abrirá la ventana principal del programa.

---

# **__2. Agregar archivos__**
1. Presionar el botón **Agregar archivos...**
2. Navegar a:

```

Tarea4Datos2 / Archivos_txt

```

3. Seleccionar los dos archivos:

### **prueba_repeticiones.txt**
Archivo con patrones repetitivos.  
Sirve para probar cómo reaccionan los algoritmos cuando hay secuencias que se repiten mucho.  
Huffman asigna códigos cortos a símbolos frecuentes y LZ77/LZ78 aprovechan los patrones.

### **prueba_texto_natural.txt**
Archivo con texto normal, sin repeticiones marcadas.  
Se usa para medir un caso más realista, donde la compresión suele ser menor.

---

# **__3. Comprimir archivos__**
1. Seleccionar un algoritmo en la lista (Huffman / LZ77 / LZ78).
2. Presionar el botón **Comprimir**.
3. En la ventana emergente, navegar a:

```

Archivos_txt / Comprimidos

```

4. Dentro de esta carpeta existen 3 subcarpetas, una para cada algoritmo.  
   Guardar el `.myzip` en la subcarpeta correspondiente.
5. Si todo funciona, aparecerá el mensaje **"Compresión completada"**.
6. Para probar los demás algoritmos, repetir el proceso cambiando el algoritmo seleccionado.

---

# **__4. Descomprimir archivos__**
1. Seleccionar el algoritmo que se desea usar para descomprimir.
2. Presionar **Descomprimir**.
3. Ir a:

```

Archivos_txt / Comprimidos / (carpeta del algoritmo usado)

```

4. Seleccionar el archivo `.myzip`.
5. Elegir la carpeta de destino:

```

Archivos_txt / Descomprimidos / (carpeta del algoritmo)

```

6. Si el proceso es correcto, aparecerá **"Descompresión completada"**.

---

# **__5. Cómo funciona el formato .myzip__**
El archivo `.myzip` en esta implementación contiene:

- firma fija `"MYZIP"`
- código del algoritmo usado (1=Huffman, 2=LZ77, 3=LZ78)
- número de archivos incluidos
- por cada archivo:
  - nombre original
  - tamaño original
  - tamaño comprimido
  - datos comprimidos según el algoritmo

El formato permite que la descompresión restaure cada archivo con su tamaño correcto.

---

# **__6. Limpiar lista__**
Si se desean cargar otros archivos, se puede usar el botón **Limpiar lista**, que borra los archivos seleccionados y reinicia las estadísticas del programa.
```

