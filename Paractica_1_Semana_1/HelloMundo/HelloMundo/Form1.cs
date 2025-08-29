using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelloMundo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            BtIniciar.Click += BtIniciar_Click;  // <-- Esto conecta el botón con el evento
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }
        int intentos = 0;
        private void BtIniciar_Click(object sender, EventArgs e)
        {
            string usuario = ctUsuario.Text;
            string clave = ctClave.Text;

            if (usuario == "Evans" && clave == "230423")
            {
                MessageBox.Show("Bienvenido al Sistema " + usuario);
                Close();
            }
            else
            {
               intentos++;
                if (intentos >= 3)
                {
                    MessageBox.Show("Has excedido el número de intentos. El programa se cerrará.");
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("Usuario o Clave incorrecta. Intento " + intentos + " de 3.");
                    ctUsuario.Focus();
                    ctClave.Clear();
                }
            }
            
        }

        private void ctUsuario_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            double peso, altura;

            if (!double.TryParse(txtPeso.Text, out peso) || !double.TryParse(txtAltura.Text, out altura))
            {
                MessageBox.Show("Ingrese valores numericos validos.");
                return;
            }

            if (altura <= 0)
            {
                MessageBox.Show("La Altura debe ser mayor a 0.");
                return;
            }

            double imc = peso / (altura * altura);
            string categoria = "";

            if (imc < 18.5)
                categoria = "Bajo peso";
            else if (imc < 24.9)
                categoria = "Normal";
            else if (imc < 29.9)
                categoria = "Sobrepeso";
            else if (imc < 34.9)
                categoria = "Obesidad Clase 1";
            else if (imc < 39.9)
                categoria = "Obesidad Clase 2";
            else
                categoria = "Obesidad Clase 3";

            MessageBox.Show($"IMC: {imc:F2} - {categoria}");
        }

        private void BtIniciar_Click_1(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnConvertir_Click(object sender, EventArgs e)
        {
            double valor;

            if (!double.TryParse(txtTemperatura.Text, out valor))
            {
                MessageBox.Show("Ingrese un número válido.");
                return;
            }

            if (rbCelsius.Checked) // Convertir de Celsius a Fahrenheit
            {
                double fahrenheit = (valor * 9 / 5) + 32;
                lblResultado.Text = valor + " °C = " + fahrenheit.ToString("F2") + " °F";
            }
            else if (rbFahrenheit.Checked) // Convertir de Fahrenheit a Celsius
            {
                double celsius = (valor - 32) * 5 / 9;
                lblResultado.Text = valor + " °F = " + celsius.ToString("F2") + " °C";
            }

        }

        private void txtTemperatura_TextChanged(object sender, EventArgs e)
        {

        }
        int contador = 0;
        private void btnContar_Click(object sender, EventArgs e)
        {
            contador++;
            lblContador.Text = "Clics: " + contador;

        }
    }
}
