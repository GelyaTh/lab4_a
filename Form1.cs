using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Lab4_AOCI
{
    public partial class Form1 : Form
    {
        private PrimitivesSearch primitivesSearch;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                primitivesSearch = new PrimitivesSearch(new Image<Bgr, byte>(fileName));
            }
            if (primitivesSearch is null)
                return;
            imageBox1.Image = primitivesSearch.SrcImg.Resize(640, 480, Inter.Linear);
        }

        //треугольники
        private void button2_Click(object sender, EventArgs e)
        {
            if (primitivesSearch is null)
                return;
            imageBox2.Image = primitivesSearch.findTriangles(trackBar1.Value, trackBar2.Value, checkBox1.Checked).Resize(640, 480, Inter.Linear);
            label7.Text = primitivesSearch.LastContoursCount.ToString();
        }

        //прямоугольники
        private void button3_Click(object sender, EventArgs e)
        {
            if (primitivesSearch is null)
                return;
            imageBox2.Image = primitivesSearch.findRectangles(trackBar1.Value, trackBar2.Value, checkBox1.Checked).Resize(640, 480, Inter.Linear);
            label7.Text = primitivesSearch.LastContoursCount.ToString();
        }

        //окружности
        private void button4_Click(object sender, EventArgs e)
        {
            if (primitivesSearch is null)
                return;

            imageBox2.Image = primitivesSearch.findCircles(trackBar1.Value, checkBox1.Checked).Resize(640, 480, Inter.Linear);
            label7.Text = primitivesSearch.LastContoursCount.ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label4.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label5.Text = trackBar2.Value.ToString();
        }


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (primitivesSearch is null)
                return;
            imageBox2.Image = primitivesSearch.drawContours(trackBar1.Value, checkBox1.Checked).Resize(640, 480, Inter.Linear);
            label7.Text = primitivesSearch.LastContoursCount.ToString();
        }
    }
}
