using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuoJia_Genealogy
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }
        public string SelectedLayout { get; private set; }
        public string SelectedFont { get; private set; }
        public string SelectedColor { get; private set; }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) => UpdatePicture();
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) => UpdatePicture();
        private void radioButtonBlack_CheckedChanged(object sender, EventArgs e) => UpdatePicture();
        private void radioButtonRed_CheckedChanged(object sender, EventArgs e) => UpdatePicture();


        private void UpdatePicture()
        {
            if (comboBox1.SelectedItem == null|| comboBox2.SelectedItem == null|| (!radioButton1.Checked&& !radioButton2.Checked))
            {
                return;
            }

            string color = radioButton1.Checked ? "black" : "red";
            string font = comboBox2.SelectedItem.ToString() == "楷体" ? "kai" : "song";
            string style = comboBox1.SelectedItem.ToString() == "现代苏式" ? "xd" : "ct";
            string fileName = $"{color}_{font}_{style}.jpg";
            string folder = Path.Combine(Application.StartupPath, "Pictures");
            string filePath = Path.Combine(folder, fileName);

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            if (File.Exists(filePath))
            {
                pictureBox1.Image = Image.FromFile(filePath);
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectedLayout = comboBox1.SelectedItem.ToString();
            SelectedFont = comboBox2.SelectedItem.ToString();
            SelectedColor = radioButton1.Checked ? "黑色" : "红色";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
