//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Windows.Forms;
//using OpenTK;
//using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;



namespace tomogram
{
    public partial class Form1 : Form
    {
        int curretLayer = 0;
        int min = 0; int max= 2000;
        Bin Bin = new Bin();
        view view = new view();
        bool loaded = false;
        int FrameCount;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        public Form1()
        {
            InitializeComponent();
        }

        //private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog dialog = new OpenFileDialog();
        //    if (dialog.ShowDialog() == DialogResult.OK)
        //    {
        //        string str = dialog.FileName;
        //        Bin.readBIN(str);
        //        view.SetupView(Width, Height);
        //        view.SetupView(glControl1.Width, glControl1.Height);
        //        loaded = true;
        //        glControl1.Invalidate();
        //    }
        //}

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                Bin.readBIN(str);
                trackBar1.Maximum = Bin.Z - 1;
                trackBar2.Minimum = 0;
                trackBar2.Maximum = 500;
                trackBar3.Minimum = 0;
                trackBar3.Maximum = 5000;
                //view.SetupView(Width, Height);
                view.SetupView(glControl2.Width, glControl2.Height);
                loaded = true;
                glControl2.Invalidate();
            }
        }

        private void glControl2_Paint(object sender, PaintEventArgs e)
        {

            if(true)
            {
                view.DrawQuads(curretLayer, min, max);
                glControl2.SwapBuffers();
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            curretLayer = trackBar1.Value;
            glControl2.Invalidate();
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl2.IsIdle)
            {
                DisplayFPS();
                glControl2.Invalidate();
            }
        }

        void DisplayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = string.Format("CT Visualiser (fps={0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

         
        private void button1_Click(object sender, EventArgs e)
        {
            if ( curretLayer >0 ) curretLayer -= 1;
            glControl2.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ( curretLayer < Bin.Z - 1) curretLayer += 1;
            glControl2.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            min = trackBar2.Value;
            glControl2.Invalidate();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            max = trackBar3.Value;
            glControl2.Invalidate();
        }

        //private void Application_Idle1(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //private void Form1_Load(object sender, EventArgs e)
        //{

        //}

        //private void fffToolStripMenuItem_Click(object sender, EventArgs e)
        //{

        //}
    }

    class Bin
    {
        public static int X, Y, Z;
        public static short[] array;
        public Bin() { }

        public static void readBIN(string path)
        {
            if (File.Exists(path))
            {
                BinaryReader reader =
                    new BinaryReader(File.Open(path, FileMode.Open));

                X = reader.ReadInt32();
                Y = reader.ReadInt32();
                Z = reader.ReadInt32();
                int arraySize = X * Y * Z;
                array = new short[arraySize];

                for (int i = 0; i < arraySize; ++i)
                {
                    array[i] = reader.ReadInt16();
                }
            }
        }
    }

    class view
    {
        public view() { }
        public static void SetupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
            GL.Viewport(0, 0, width, height);
        }

        //public static int clamp(int val, int min, int max)
        //{
        //    if (val <= min) return min;
        //    else if (val >= max) return max;
        //    else return val;
        //}

        public int clamp(int val, int min, int max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }
        Color TransferFunction(short value, int _min, int top)
        {
            int min = _min; int max = _min + top;
            int newVal = clamp((value - min) * 255 / (max - min), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
            //return Color.FromArgb(255, 200, 100, 150);
        }
        public void DrawQuads(int LayerNumber, int _min, int top)
        {
           
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(PrimitiveType.Quads);
            for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
            {
                for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {
                    short value;
                    value = Bin.array[x_coord + y_coord * Bin.X + LayerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, _min, top));
                    GL.Vertex2(x_coord, y_coord);

                    value = Bin.array[x_coord + (y_coord + 1) * Bin.X + LayerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, _min, top));
                    GL.Vertex2(x_coord, y_coord + 1);

                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.X + LayerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, _min, top));
                    GL.Vertex2(x_coord + 1, y_coord + 1);

                    value = Bin.array[x_coord + 1 + y_coord * Bin.X + LayerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, _min, top));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            }
            GL.End();
        }
    }
}
