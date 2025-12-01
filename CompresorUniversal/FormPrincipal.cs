using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;




namespace CompresorUniversal

{
    public partial class FormPrincipal : Form
    {
        private List<string> archivosSeleccionados;

        private ListBox listBoxArchivos;
        private ComboBox comboAlgoritmo;
        private Button btnComprimir;
        private Button btnDescomprimir;
        private Label lblTiempo;
        private Label lblMemoria;
        private Label lblTasa;


        public FormPrincipal()
        {
            archivosSeleccionados = new List<string>();
            InitializeComponent();
        }



        private void InitializeComponent()
        {
            // título de la ventana
            this.Text = "Compresor Universal";
            this.Size = new Size(510, 460);   
            StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;        
            this.BackColor = SystemColors.Control; 

            int y = 18;  


            
            

            Label lblArchivos = new Label();
            lblArchivos.Text = "Archivos a comprimir:";
            lblArchivos.Location = new Point(20, y);
            lblArchivos.AutoSize = true;
            this.Controls.Add(lblArchivos);

            y += 23;

            //listbox con los archivos 
            listBoxArchivos = new ListBox();
            listBoxArchivos.Location = new Point(20, y);
            listBoxArchivos.Size = new Size(445, 95);
            Controls.Add(listBoxArchivos);

            y += 105;

            Button btnAgregar = new Button();
            btnAgregar.Text = "Agregar archivos...";
            btnAgregar.Location = new Point(20, y);
            btnAgregar.Size = new Size(135, 29);   
            btnAgregar.Click += BtnAgregar_Click;
            this.Controls.Add(btnAgregar);

            Button btnLimpiar = new Button();
            btnLimpiar.Text = "Limpiar lista";
            btnLimpiar.Location = new Point(165, y + 1);  
            btnLimpiar.Size = new Size(95, 28);
            btnLimpiar.Click += BtnLimpiar_Click;
            Controls.Add(btnLimpiar);

            y += 42;

            Label lblAlgoritmo = new Label();
            lblAlgoritmo.Text = "Algoritmo:";
            lblAlgoritmo.Location = new Point(20, y);
            lblAlgoritmo.AutoSize = true;
            Controls.Add(lblAlgoritmo);

            // label 
            Label lblNotaAlg = new Label();
            lblNotaAlg.Text = "(Huffman / LZ77 / LZ78)";
            lblNotaAlg.Location = new Point(90, y + 1);
            lblNotaAlg.AutoSize = true;
            lblNotaAlg.ForeColor = Color.DimGray;
            this.Controls.Add(lblNotaAlg);

            y += 24;

            comboAlgoritmo = new ComboBox();
            comboAlgoritmo.Location = new Point(20, y);
            comboAlgoritmo.Size = new Size(155, 23);
            comboAlgoritmo.DropDownStyle = ComboBoxStyle.DropDownList;
            comboAlgoritmo.Items.Add("Huffman");
            comboAlgoritmo.Items.Add("LZ77");
            comboAlgoritmo.Items.Add("LZ78");
            comboAlgoritmo.SelectedIndex = 0;  //huffman por defecto
            this.Controls.Add(comboAlgoritmo);

            y += 44;

            btnComprimir = new Button();
            btnComprimir.Text = "Comprimir";
            btnComprimir.Location = new Point(20, y);
            btnComprimir.Size = new Size(155, 38);
            btnComprimir.Click += BtnComprimir_Click;
            Controls.Add(btnComprimir);

            btnDescomprimir = new Button();
            btnDescomprimir.Text = "Descomprimir";
            btnDescomprimir.Location = new Point(190, y - 2);  
            btnDescomprimir.Size = new Size(155, 41);
            btnDescomprimir.Click += BtnDescomprimir_Click;
            this.Controls.Add(btnDescomprimir);

            y += 55;

            
            Label lblEstadisticas = new Label();
            lblEstadisticas.Text = "Estadísticas:";
            lblEstadisticas.Location = new Point(20, y);
            lblEstadisticas.AutoSize = true;
            Controls.Add(lblEstadisticas);

            y += 22;

            lblTiempo = new Label();
            lblTiempo.Text = "Tiempo: --";
            lblTiempo.Location = new Point(25, y);
            lblTiempo.AutoSize = true;
            this.Controls.Add(lblTiempo);

            y += 19;

            lblMemoria = new Label();
            lblMemoria.Text = "Memoria: --";
            lblMemoria.Location = new Point(25, y);
            lblMemoria.AutoSize = true;
            Controls.Add(lblMemoria);

            y += 19;

            lblTasa = new Label();
            lblTasa.Text = "Tasa de compresión: --";
            lblTasa.Location = new Point(25, y);
            lblTasa.AutoSize = true;
            this.Controls.Add(lblTasa);
        }



        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            ofd.Multiselect = true;
            ofd.Title = "Seleccionar archivos";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string archivo in ofd.FileNames)
                {
                    if (!archivosSeleccionados.Contains(archivo))
                    {
                        archivosSeleccionados.Add(archivo);
                        listBoxArchivos.Items.Add(Path.GetFileName(archivo));  //solo nombre
                    }
                }
            }
        }


        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            archivosSeleccionados.Clear();
            listBoxArchivos.Items.Clear();

            lblTiempo.Text = "Tiempo: --";
            lblMemoria.Text = "Memoria: --";
            lblTasa.Text = "Tasa de compresión: --";
        }



        private void BtnComprimir_Click(object sender, EventArgs e)
        {
            if (archivosSeleccionados.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un archivo.","Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string algoritmo = comboAlgoritmo.SelectedItem.ToString();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivo comprimido (*.myzip)|*.myzip";
            sfd.Title = "Guardar archivo comprimido";
            sfd.FileName = "archivo_comprimido.myzip";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    //se limpia memoria
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    long memBefore = GC.GetTotalMemory(true);

                    Compresor compresor = new Compresor();
                    long totalOriginal, totalComprimido;

                    compresor.ComprimirArchivos(archivosSeleccionados, sfd.FileName, algoritmo,
                                                out totalOriginal, out totalComprimido);

                    sw.Stop();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    long memAfter = GC.GetTotalMemory(true);
                    long memUsada = Math.Max(0, memAfter - memBefore);

                    double ratio = (double)totalComprimido / totalOriginal * 100.0;

                    lblTiempo.Text = "Tiempo: " + sw.ElapsedMilliseconds + " ms";
                    lblMemoria.Text = "Memoria: " + (memUsada / 1024.0 / 1024.0).ToString("F2") + " MB";
                    lblTasa.Text = "Tasa de compresión: " + ratio.ToString("F2") + "%";

                    MessageBox.Show("Compresión completada.\n\nArchivo: " +Path.GetFileName(sfd.FileName) +
                                    "\nAlgoritmo: " + algoritmo +
                                    "\nTiempo: " + sw.ElapsedMilliseconds +" ms" +
                                    "\nTamaño original: " + totalOriginal.ToString("N0") + " bytes" +
                                    "\nTamaño comprimido: " + totalComprimido.ToString("N0") + " bytes" +
                                    "\nTasa: " + ratio.ToString("F2") + "%",
                                    "Compresión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error durante la compresión:\n\n" + ex.Message, "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void BtnDescomprimir_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivo comprimido (*.myzip)|*.myzip";
            ofd.Title = "Seleccionar archivo .myzip";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Seleccionar carpeta de destino";

                if (fbd.ShowDialog() == DialogResult.OK)


                {


                    try



                    {

                        Stopwatch sw = Stopwatch.StartNew();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        long memBefore = GC.GetTotalMemory(true);

                        Compresor compresor = new Compresor();
                        long totalOriginal, totalComprimido;
                        string algoritmoUsado;

                        compresor.DescomprimirArchivos(ofd.FileName, fbd.SelectedPath,
                                                       out totalOriginal, out totalComprimido,
                                                       out algoritmoUsado);

                        sw.Stop();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        long memAfter = GC.GetTotalMemory(true);
                        long memUsada = Math.Max(0, memAfter - memBefore);

                        double ratio = (double)totalComprimido / totalOriginal * 100.0;

                        lblTiempo.Text = "Tiempo: " + sw.ElapsedMilliseconds + " ms";
                        lblMemoria.Text = "Memoria: " + (memUsada / 1024.0 / 1024.0).ToString("F2") + " MB";
                        lblTasa.Text = "Tasa de compresión: " + ratio.ToString("F2") + "%";

                        MessageBox.Show("Descompresión completada.\n\nAlgoritmo usado: " + algoritmoUsado +
                                        "\nDestino: " + fbd.SelectedPath +
                                        "\nTiempo: " + sw.ElapsedMilliseconds + " ms",
                                        "Descompresión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error durante la descompresión:\n\n" + ex.Message, "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
