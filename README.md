
# README – Compresor Universal (Huffman, LZ77, LZ78)


Este proyecto es una aplicación Windows Forms que permite comprimir y descomprimir archivos de texto utilizando los algoritmos **Huffman**, **LZ77** y **LZ78**.  
El programa genera archivos con formato `.myzip` y muestra estadísticas de tiempo, memoria y tasa de compresión.



# USO DEL PROGRAMA

## 1. _Abrir el proyecto_
1. Abrir la solución en Visual Studio 2022.
2. Ejecutar el WinForms llamado Compresor Universal.
3. Se abrirá la ventana principal del programa.



## 2. _Agregar archivos_
1. Presionar el botón **Agregar archivos...**
2. Navegar a:



Tarea4Datos2 / Archivos_txt



3. Seleccionar los archivos de prueba:

### prueba_repeticiones.txt
Archivo con patrones repetitivos.  
Sirve para ver cómo reaccionan los algoritmos cuando aparece la misma secuencia varias veces.  
Huffman asigna códigos más cortos y LZ77/LZ78 aprovechan las repeticiones directamente.

### prueba_texto_natural.txt
Archivo con texto normal, sin repeticiones marcadas.  
Se usa para un caso más realista donde la compresión suele ser menor.



## 3. _Comprimir archivos_
1. Seleccionar un algoritmo: Huffman, LZ77 o LZ78.
2. Presionar el botón **Comprimir**.
3. En la ventana emergente, ir a:



Archivos_txt / Comprimidos



4. Dentro de esa carpeta hay una subcarpeta por algoritmo.  
   Guardar el archivo `.myzip` dentro de la subcarpeta correspondiente.
5. Si todo funcionó, aparecerá el mensaje “Compresión completada”.
6. Para probar otro algoritmo basta repetir el proceso cambiando la selección.



## 4. _Descomprimir archivos_
1. Seleccionar el algoritmo que se desea usar para la descompresión.
2. Presionar el botón **Descomprimir**.
3. Elegir un `.myzip` dentro de:



Archivos_txt / Comprimidos / (carpeta del algoritmo)



4. Seleccionar la carpeta de destino:



Archivos_txt / Descomprimidos / (carpeta del algoritmo)



5. Si todo es correcto, aparecerá el mensaje “Descompresión completada”.



## 5. _Formato .myzip_
El archivo `.myzip` generado incluye:

- firma “MYZIP”
- código del algoritmo usado
- cantidad de archivos
- por cada archivo:
  - nombre original
  - tamaño original
  - tamaño comprimido
  - datos comprimidos con el algoritmo correspondiente

Esto permite restaurar los archivos sin pérdida durante la descompresión.



## 6. _Limpiar lista_
Para usar otros archivos, se puede presionar **Limpiar lista**.  
Esto borra los archivos cargados y reinicia las estadísticas mostradas.

