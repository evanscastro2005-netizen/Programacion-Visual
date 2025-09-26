using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visor_de_Imagenes
{
    public partial class Form1 : Form
    {
        // para las rutas:
        private List<string> imagePaths = new List<string>();

        // Lista de imágenes originales cargadas desde disco
        private List<Image> originalImages = new List<Image>();

        // Lista de imágenes de trabajo 
        private List<Image> workingImages = new List<Image>();

        // Nombres de las imágenes (para combobox y status)
        private List<string> imageNames = new List<string>();

        // índice actual en la colección
        private int currentIndex = 0;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;

            // Context menu para la picture box :D (solo añadir una vez)
            var cms = new ContextMenuStrip();
            var copy = new ToolStripMenuItem("Copiar");
            var rotateLeft = new ToolStripMenuItem("Girar imagen 90° a la izquierda");
            var rotateRight = new ToolStripMenuItem("Girar imagen 90° a la derecha");

            copy.Click += Copy_Click;
            rotateLeft.Click += RotateLeft_Click;
            rotateRight.Click += RotateRight_Click;

            cms.Items.AddRange(new ToolStripItem[] { copy, rotateLeft, rotateRight });
            pbVisorImage.ContextMenuStrip = cms;
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            if (pbVisorImage.Image == null)
            {
                MessageBox.Show("No hay imagen para copiar.", "Copiar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Clipboard.SetImage(pbVisorImage.Image);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al copiar la imagen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            originalImages.Clear();
            workingImages.Clear();
            imageNames.Clear();
            imagePaths.Clear();

            // Carpeta donde están tus imágenes
            string folder = @"C:\Users\Evans Castro\Desktop\Fotos Visual";  // 👈 cámbialo a tu carpeta
            if (!Directory.Exists(folder))
            {
                MessageBox.Show($"La carpeta no existe: {folder}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] files = Directory.GetFiles(folder, "*.*")
                                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                            f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                            f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                            f.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                                            f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                                .ToArray();

            foreach (string file in files)
            {
                try
                {
                    // Abrir la imagen y crear una copia en memoria para no bloquear el fichero
                    using (var img = Image.FromFile(file))
                    {
                        var orig = new Bitmap(img); // copia independiente
                        var work = new Bitmap(orig); // copia para trabajar
                        originalImages.Add(orig);
                        workingImages.Add(work);
                    }

                    imagePaths.Add(file);
                    imageNames.Add(Path.GetFileName(file)); // solo el nombre
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error cargando " + file + ": " + ex.Message);
                }
            }

            // llenar ComboBox
            cbImagenAc.Items.Clear();
            cbImagenAc.Items.AddRange(imageNames.ToArray());

            if (workingImages.Count > 0)
            {
                currentIndex = 0;
                cbImagenAc.SelectedIndex = currentIndex;
                ShowCurrentImage();
            }
        }

        private void ShowCurrentImage()
        {
            if (workingImages == null || workingImages.Count == 0)
            {
                // liberar imagen temporal mostrada si existe y no es manejada por nuestras listas
                DisposeIfTemporary(pbVisorImage.Image);
                pbVisorImage.Image = null;
                toolStripStatusLabel1.Text = "";
                return;
            }

            // indice normalizado
            if (currentIndex < 0) currentIndex = 0;
            if (currentIndex >= workingImages.Count) currentIndex = workingImages.Count - 1;

            // Liberar la imagen mostrada anteriormente si es temporal (p.ej. una escala de grises creada)
            DisposeIfTemporary(pbVisorImage.Image);

            // Si la opción "Escala de grises" está activa, se va a mostrar la imagen con su versión
            if (cbEscalaGris.Checked || (escalaDeGrisesToolStripMenuItem != null && escalaDeGrisesToolStripMenuItem.Checked) || (tsGris != null && tsGris.Checked))
            {
                // ConvertToGrayscale crea un nuevo Bitmap (temporal)
                pbVisorImage.Image = ConvertToGrayscale(workingImages[currentIndex]);
            }
            else
            {
                // mostrar la imagen de trabajo (no clonar para evitar uso de memoria innecesario)
                pbVisorImage.Image = workingImages[currentIndex];
            }

            // Aplicar SizeMode 
            if ((rbCentral != null && rbCentral.Checked) || (centradaToolStripMenuItem != null && centradaToolStripMenuItem.Checked) || (tsCentral != null && tsCentral.Checked))
            {
                pbVisorImage.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else if ((rbAjust != null && rbAjust.Checked) || (ajustarToolStripMenuItem != null && ajustarToolStripMenuItem.Checked) || (tsAjustar != null && tsAjustar.Checked))
            {
                pbVisorImage.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else // zoom
            {
                pbVisorImage.SizeMode = PictureBoxSizeMode.Zoom;
            }

            // Actualizar status con la ruta completa (si existe)
            toolStripStatusLabel1.Text = imagePaths.ElementAtOrDefault(currentIndex)
                ?? imageNames.ElementAtOrDefault(currentIndex)
                ?? "";
        }

        // Dispose de una imagen sólo si NO es una referencia a nuestros arrays (es temp)
        private void DisposeIfTemporary(Image img)
        {
            if (img == null) return;
            bool isManaged = originalImages.Contains(img) || workingImages.Contains(img);
            if (!isManaged)
            {
                try { img.Dispose(); }
                catch { /* ignorar */ }
            }
        }

        // ----------------- HELPER ---------------------
        private void SetGrayscale(bool on)
        {
            if (cbEscalaGris != null) cbEscalaGris.Checked = on;
            if (escalaDeGrisesToolStripMenuItem != null) escalaDeGrisesToolStripMenuItem.Checked = on;
            if (tsGris != null) tsGris.Checked = on;

            if (cbNormal != null) cbNormal.Checked = !on;
            if (normalToolStripMenuItem != null) normalToolStripMenuItem.Checked = !on;

            ShowCurrentImage();
        }

        private void SetSizeMode(PictureBoxSizeMode mode)
        {
            pbVisorImage.SizeMode = mode;

            if (rbCentral != null) rbCentral.Checked = (mode == PictureBoxSizeMode.CenterImage);
            if (rbAjust != null) rbAjust.Checked = (mode == PictureBoxSizeMode.StretchImage);
            if (rbZoom != null) rbZoom.Checked = (mode == PictureBoxSizeMode.Zoom);

            if (centradaToolStripMenuItem != null) centradaToolStripMenuItem.Checked = (mode == PictureBoxSizeMode.CenterImage);
            if (ajustarToolStripMenuItem != null) ajustarToolStripMenuItem.Checked = (mode == PictureBoxSizeMode.StretchImage);
            if (zoomToolStripMenuItem != null) zoomToolStripMenuItem.Checked = (mode == PictureBoxSizeMode.Zoom);

            if (tsCentral != null) tsCentral.Checked = (mode == PictureBoxSizeMode.CenterImage);
            if (tsAjustar != null) tsAjustar.Checked = (mode == PictureBoxSizeMode.StretchImage);
            if (tsZoom != null) tsZoom.Checked = (mode == PictureBoxSizeMode.Zoom);
        }

        // ----------------- Navegación -----------------
        private void btFirst_Click(object sender, EventArgs e)
        {
            currentIndex = 0;
            ShowCurrentImage();
        }

        private void btBack_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0) currentIndex--;
            ShowCurrentImage();
        }

        private void btForward_Click(object sender, EventArgs e)
        {
            if (currentIndex < workingImages.Count - 1) currentIndex++;
            ShowCurrentImage();
        }

        private void btLast_Click(object sender, EventArgs e)
        {
            if (workingImages.Count == 0) return;
            currentIndex = Math.Max(0, workingImages.Count - 1);
            ShowCurrentImage();
        }

        private void cbImagenAc_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cbImagenAc.SelectedIndex;
            if (idx >= 0 && idx < workingImages.Count)
            {
                currentIndex = idx;
                ShowCurrentImage();
            }
        }

        // ----------------- Guardar -----------------
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pbVisorImage.Image == null)
            {
                MessageBox.Show("No hay imagen para guardar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|GIF Image|*.gif|TIFF Image|*.tif;*.tiff";
                sfd.Title = "Guardar imagen editada";
                sfd.FileName = imageNames.ElementAtOrDefault(currentIndex) ?? "imagen";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat fmt = ImageFormat.Png;
                    string ext = Path.GetExtension(sfd.FileName).ToLower();
                    switch (ext)
                    {
                        case ".jpg":
                        case ".jpeg":
                            fmt = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            fmt = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            fmt = ImageFormat.Gif;
                            break;
                        case ".tif":
                        case ".tiff":
                            fmt = ImageFormat.Tiff;
                            break;
                        default:
                            fmt = ImageFormat.Png;
                            break;
                    }

                    // Si se está mostrando la versión en escala de grises, pbVisorImage.Image será un Bitmap temporal.
                    // Guardamos pbVisorImage.Image si existe, si no guardamos la imagen de trabajo.
                    Image toSave = pbVisorImage.Image ?? workingImages[currentIndex];

                    try
                    {
                        // Si toSave es una referencia a workingImages (que usamos en UI), guardar una copia para evitar conflictos.
                        if (workingImages.Contains(toSave) || originalImages.Contains(toSave))
                        {
                            using (var copy = new Bitmap(toSave))
                            {
                                copy.Save(sfd.FileName, fmt);
                            }
                        }
                        else
                        {
                            toSave.Save(sfd.FileName, fmt);
                        }

                        MessageBox.Show("Imagen guardada correctamente.", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ----------------- Visión: Normal / Escala de grises -----------------
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (workingImages.Count == 0) return;

            // Reestablecer la imagen actual a su original (liberando la anterior)
            var old = workingImages[currentIndex];
            workingImages[currentIndex] = (Image)originalImages[currentIndex].Clone();
            try { old.Dispose(); } catch { }

            // desactivar escala de grises
            if (cbEscalaGris != null) cbEscalaGris.Checked = false;
            if (escalaDeGrisesToolStripMenuItem != null) escalaDeGrisesToolStripMenuItem.Checked = false;
            if (tsGris != null) tsGris.Checked = false;
            if (cbNormal != null) cbNormal.Checked = true;
            if (normalToolStripMenuItem != null) normalToolStripMenuItem.Checked = true;

            ShowCurrentImage();
        }

        private void escalaDeGrisesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGrayscale(true);
            if (cbEscalaGris != null) cbEscalaGris.Checked = true;
            if (escalaDeGrisesToolStripMenuItem != null) escalaDeGrisesToolStripMenuItem.Checked = true;
            if (tsGris != null) tsGris.Checked = true;
            if (cbNormal != null) cbNormal.Checked = false;
            if (normalToolStripMenuItem != null) normalToolStripMenuItem.Checked = false;
            ShowCurrentImage();
        }

        private void cbNormal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbNormal.Checked)
            {
                if (normalToolStripMenuItem != null) normalToolStripMenuItem.Checked = true;
                if (cbEscalaGris != null) cbEscalaGris.Checked = false;
                if (escalaDeGrisesToolStripMenuItem != null) escalaDeGrisesToolStripMenuItem.Checked = false;
                ShowCurrentImage();
            }
        }

        private void cbEscalaGris_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEscalaGris.Checked)
            {
                if (escalaDeGrisesToolStripMenuItem != null) escalaDeGrisesToolStripMenuItem.Checked = true;
                if (cbNormal != null) cbNormal.Checked = false;
                if (normalToolStripMenuItem != null) normalToolStripMenuItem.Checked = false;
                ShowCurrentImage();
            }
            else
            {
                ShowCurrentImage();
            }
        }

        // ----------------- Tamaño: Centrada / Ajustar / Zoom -----------------
        private void centradaToolStripMenuItem_Click(object sender, EventArgs e) => SetSizeMode(PictureBoxSizeMode.CenterImage);
        private void ajustarToolStripMenuItem_Click(object sender, EventArgs e) => SetSizeMode(PictureBoxSizeMode.StretchImage);
        private void zoomToolStripMenuItem_Click(object sender, EventArgs e) => SetSizeMode(PictureBoxSizeMode.Zoom);

        private void rbCentral_CheckedChanged(object sender, EventArgs e) { if (rbCentral.Checked) SetSizeMode(PictureBoxSizeMode.CenterImage); }
        private void rbAjust_CheckedChanged(object sender, EventArgs e) { if (rbAjust.Checked) SetSizeMode(PictureBoxSizeMode.StretchImage); }
        private void rbZoom_CheckedChanged(object sender, EventArgs e) { if (rbZoom.Checked) SetSizeMode(PictureBoxSizeMode.Zoom); }

        // ----------------- Rotaciones por menú contextual -----------------
        private void RotateLeft_Click(object sender, EventArgs e)
        {
            RotateCurrentImage(RotateFlipType.Rotate270FlipNone); // 90° a la izquierda == 270° a la derecha
        }

        private void RotateRight_Click(object sender, EventArgs e)
        {
            RotateCurrentImage(RotateFlipType.Rotate90FlipNone);
        }

        private void RotateCurrentImage(RotateFlipType type)
        {
            if (workingImages == null || workingImages.Count == 0) return;

            // Crear una copia rotada para no alterar referencias inesperadas y liberar la anterior
            Image old = workingImages[currentIndex];
            try
            {
                Bitmap bmp = new Bitmap(old); // copia
                bmp.RotateFlip(type);
                workingImages[currentIndex] = bmp;
            }
            catch
            {
                // en caso de fallo, no sustituir
            }
            finally
            {
                try { if (old != null) old.Dispose(); } catch { }
            }

            ShowCurrentImage();
        }

        // ----------------- Helper: convertir a escala de grises -----------------
        private Bitmap ConvertToGrayscale(Image sourceImage)
        {
            // crear bitmap a partir de cualquier Image seguro
            using (var tmp = new Bitmap(sourceImage))
            {
                Bitmap bmp = new Bitmap(tmp.Width, tmp.Height);

                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[]{0.3f, 0.3f, 0.3f, 0, 0},
                        new float[]{0.59f,0.59f,0.59f,0,0},
                        new float[]{0.11f,0.11f,0.11f,0,0},
                        new float[]{0,0,0,1,0},
                        new float[]{0,0,0,0,1}
                    });

                using (Graphics g = Graphics.FromImage(bmp))
                using (ImageAttributes ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(colorMatrix);
                    g.DrawImage(tmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
                        0, 0, tmp.Width, tmp.Height, GraphicsUnit.Pixel, ia);
                }

                return bmp; // bmp será responsabilidad del llamador (ShowCurrentImage) para liberarlo cuando ya no se use
            }
        }

        // ----------------- Liberar recursos al cerrar el form -----------------
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Dispose imágenes manejadas
            foreach (var img in workingImages) try { img.Dispose(); } catch { }
            foreach (var img in originalImages) try { img.Dispose(); } catch { }

            // imagen mostrada si fuera temporal
            DisposeIfTemporary(pbVisorImage.Image);
        }

        // ------ Handlers del diseñador (reusar los ya definidos) ------
        private void cbEscalaGris_CheckedChanged_1(object sender, EventArgs e) => cbEscalaGris_CheckedChanged(sender, e);
        private void btLast_Click_1(object sender, EventArgs e) => btLast_Click(sender, e);
        private void btForward_Click_1(object sender, EventArgs e) => btForward_Click(sender, e);
        private void btBack_Click_1(object sender, EventArgs e) => btBack_Click(sender, e);
        private void btFirst_Click_1(object sender, EventArgs e) => btFirst_Click(sender, e);

        // ToolStripButtons (iconos) 
        private void tsNormal_Click(object sender, EventArgs e) => SetGrayscale(false);
        private void tsGris_Click(object sender, EventArgs e) => SetGrayscale(true);
        private void tsCentral_Click(object sender, EventArgs e) => SetSizeMode(PictureBoxSizeMode.CenterImage);
        private void tsAjustar_Click(object sender, EventArgs e) => SetSizeMode(PictureBoxSizeMode.StretchImage);
        private void tsZoom_Click(object sender, EventArgs e) => SetSizeMode(PictureBoxSizeMode.Zoom);

        // RadioButtons (reusar)
        private void rbCentral_CheckedChanged_1(object sender, EventArgs e) => rbCentral_CheckedChanged(sender, e);
        private void rbAjust_CheckedChanged_1(object sender, EventArgs e) => rbAjust_CheckedChanged(sender, e);
        private void rbZoom_CheckedChanged_1(object sender, EventArgs e) => rbZoom_CheckedChanged(sender, e);

        // Menú (reusar)
        private void normalToolStripMenuItem_Click_1(object sender, EventArgs e) => normalToolStripMenuItem_Click(sender, e);
        private void escalaDeGrisesToolStripMenuItem_Click_1(object sender, EventArgs e) => escalaDeGrisesToolStripMenuItem_Click(sender, e);
        private void centradaToolStripMenuItem_Click_1(object sender, EventArgs e) => centradaToolStripMenuItem_Click(sender, e);
        private void ajustarToolStripMenuItem_Click_1(object sender, EventArgs e) => ajustarToolStripMenuItem_Click(sender, e);
        private void zoomToolStripMenuItem_Click_1(object sender, EventArgs e) => zoomToolStripMenuItem_Click(sender, e);

        // ComboBox seleccionado (sufijo _1) 
        private void cbImagenAc_SelectedIndexChanged_1(object sender, EventArgs e) => cbImagenAc_SelectedIndexChanged(sender, e);
        private void cbNormal_CheckedChanged_1(object sender, EventArgs e) => cbNormal_CheckedChanged(sender, e);

        // Status label (puede quedar vacío o mostrar info)
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            // puedes dejarlo vacío o mostrar información; lo dejé sin mensaje molestoso
        }

        private void lbImagen_Click(object sender, EventArgs e) { /* vacío */ }

        private void tsZoom_Click_1(object sender, EventArgs e)
        {

        }
    }
}
