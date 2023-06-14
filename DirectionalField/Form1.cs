using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectionalField
{
    public partial class Form1 : Form
    {
        List<Operator> operators;
        Expresion[] Solucion;
        SpringMassSystem system;

        bool Start ;
        float hx;
        float hy;

        float escala;
        float eulerStep;

        bool mouseDown;
        float xmouseDown;
        float ymouseDown;


        public Form1()
        {           
            operators = new List<Operator>();

            Operator suma = new Operator("+", 1, 2,(val1, val2) => { return val1 + val2; });
            operators.Add(suma);

            Operator resta = new Operator("-", 1,2,(val1, val2) => { return val1 - val2; });
            operators.Add(resta);

            Operator producto = new Operator("*", 2,2, (val1, val2) => { return val1 * val2; });
            operators.Add(producto);

            Operator division = new Operator("/", 2, 2,(val1, val2) => { return val1 / val2; });
            operators.Add(division);

            Operator exponencial = new Operator("^", 3,2, (val1, val2) => { return Math.Pow(val1, val2); });
            operators.Add(exponencial);

            Operator openpharentesis = new Operator("(", 0,0, null);
            operators.Add(openpharentesis);

            Operator closedpharentesis = new Operator(")", 0,0, null);
            operators.Add(closedpharentesis);

            Operator seno = new Operator("sen", 4, 1, (val1, val2) => { return Math.Sin(val2); });
            operators.Add(seno);

            Operator coseno = new Operator("cos", 4, 1, (val1, val2) => { return Math.Cos(val2); });
            operators.Add(coseno);

            Operator ln = new Operator("ln", 4, 1, (val1, val2) => { return Math.Log(val2); });
            operators.Add(ln);

            InitializeComponent();
            Reset();
            
        }

        //Actualizar
        private void button1_Click(object sender, EventArgs e)
        {
            UpdateNow();
        }

        private void UpdateNow()
        {
            double[] Mass = new double[2];
            double[] spring = new double[3];
            double[] IP = new double[2];
            double[] IS = new double[2];

            double temp;
            if (!double.TryParse(Mass1.Value.ToString(), out temp))
            {
                MessageBox.Show("Mass 1 invalid."); return;
            }
            Mass[0] = temp;

            if (!double.TryParse(Mass2.Value.ToString(), out temp))
            {
                MessageBox.Show("Mass 2 invalid."); return;
            }
            Mass[1] = temp;

            if (!double.TryParse(Spring1.Value.ToString(), out temp))
            {
                MessageBox.Show("Spring 1 invalid."); return;
            }
            spring[0] = temp;

            if (!double.TryParse(Spring2.Value.ToString(), out temp))
            {
                MessageBox.Show("Spring 2 invalid."); return;
            }
            spring[1] = temp;


                if (!double.TryParse(Spring3.Value.ToString(), out temp))
                {
                    MessageBox.Show("Spring 3 invalid."); return;
                }


            if (!double.TryParse(IP1.Value.ToString(), out temp))
            {
                MessageBox.Show("Initial Position 1 invalid."); return;
            }
            IP[0] = temp;

            if (!double.TryParse(IP2.Value.ToString(), out temp))
            {
                MessageBox.Show("Initial Position 2 invalid."); return;
            }
            IP[1] = temp;

            if (!double.TryParse(IS1.Value.ToString(), out temp))
            {
                MessageBox.Show("Initial Speed 1 invalid."); return;
            }
            IS[0] = temp;

            if (!double.TryParse(IS2.Value.ToString(), out temp))
            {
                MessageBox.Show("Initial Speed 2 invalid."); return;
            }
            IS[1] = temp;

            escala = (float)numericEscala.Value;
            eulerStep= (float)euler.Value;
            try
            {
                Solucion = new Expresion[2];
                Solucion[0] = new ExpresionCompuesta(SS1.Text, operators);
                Solucion[1] = new ExpresionCompuesta(SS2.Text, operators);
                system = new SpringMassSystem(Mass, spring, IP, IS);

                Start = true;
                Pizarra.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de Sintaxis");
            }

        }

        private void Pizarra_Paint(object sender, PaintEventArgs e)
        {
            if (Start)
            {
                Dictionary<char, double> asignaciones = new Dictionary<char, double>();
                asignaciones.Add('e', Math.E);
     
                Graphics g = e.Graphics;

                System.Drawing.Drawing2D.Matrix matriz = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, hx, hy);
                g.MultiplyTransform(matriz);

                float ancho = Pizarra.Width;

                //Fondo
                Brush bNegra = Brushes.Black;
                g.FillRectangle(bNegra, -hx, hy - ancho, ancho, ancho);

                //Ejes
                Pen pBlanco = new Pen(Color.White, 1);
                g.DrawLine(pBlanco, -hx, 0, ancho - hx, 0);
                g.DrawLine(pBlanco, 0, hy - ancho, 0, hy);

                //Euler
                Pen pRed = new Pen(Color.Red, 1);
                Pen pOrange = new Pen(Color.Orange, 1);

                IEnumerable<double[]> euler = system.EulerMethod(eulerStep);
                IEnumerator<double[]> eulerEnum=euler.GetEnumerator();


                eulerEnum.MoveNext();
                double[] First = eulerEnum.Current;

                double[] Actual = First;
                double[] Last = First;
                
                bool st;
                bool nd;
                bool begin = false;
                while ((ValidateActualPoints(Actual[0],Actual[1],ancho,out st) | ValidateActualPoints(Actual[0], Actual[2], ancho, out nd) | !begin) && eulerEnum.MoveNext())
                {
                    Actual = eulerEnum.Current;
                    if (Actual[0] > -hx * escala) begin = true;
                    if (st && checkBox3.Checked)
                    {
                        Point lastone = new Point((int)(Last[0] / escala), (int)(Last[1] / escala));
                        Point thisone = new Point((int)(Actual[0] / escala), (int)(Actual[1] / escala));
                        Point[] points = {lastone,thisone};
                        g.DrawCurve(pRed, points);
                    }
                    if (nd && checkBox4.Checked)
                    {
                        Point lastone = new Point((int)(Last[0] / escala), (int)(Last[2] / escala));
                        Point thisone = new Point((int)(Actual[0] / escala), (int)(Actual[2] / escala));
                        Point[] points = { lastone, thisone };
                        g.DrawCurve(pOrange, points);
                    }
                    Array.Copy(Actual,Last,Actual.Length);
                }

                //Analitical Solution

                Pen pGreen = new Pen(Color.Green, 1);
                Pen pLime = new Pen(Color.Lime, 1);

                asignaciones['t'] = 0;

                First =new double[3];
                First[0] = 0;
                First[1] = Solucion[0].Evaluar(asignaciones);
                First[2] = Solucion[1].Evaluar(asignaciones);

                Array.Copy(First,Actual,First.Length);
                Array.Copy(First, Last, First.Length);
                begin = false;
                while ((ValidateActualPoints(Actual[0], Actual[1], ancho, out st) | ValidateActualPoints(Actual[0], Actual[2], ancho, out nd)) | !begin)
                {
                    Actual[0]+=escala;
                    if (Actual[0] > -hx * escala) begin = true;
                    asignaciones['t'] = Actual[0];
                    Actual[1] = Solucion[0].Evaluar(asignaciones);
                    Actual[2] = Solucion[1].Evaluar(asignaciones);

                    if (st && checkBox1.Checked)
                    {
                        Point lastone = new Point((int)(Last[0] / escala), (int)(Last[1] / escala));
                        Point thisone = new Point((int)(Actual[0] / escala), (int)(Actual[1] / escala));
                        Point[] points = { lastone, thisone };
                        g.DrawCurve(pGreen, points);
                    }
                    if (nd && checkBox2.Checked)
                    {
                        Point lastone = new Point((int)(Last[0] / escala), (int)(Last[2] / escala));
                        Point thisone = new Point((int)(Actual[0] / escala), (int)(Actual[2] / escala));
                        Point[] points = { lastone, thisone };
                        g.DrawCurve(pLime, points);
                    }
                    Array.Copy(Actual, Last, Actual.Length);
                }


            }
            else
            {
                e.Graphics.FillRectangle(Brushes.Black,0,0,Pizarra.Width,Pizarra.Width);
            }
        }

        private bool ValidateActualPoints(double actualx,double actualy,float ancho,out bool b)
        {

            if (!(actualx > -hx * escala && actualx < (ancho - hx) * escala))
            { b = false; return false; }
            if (!(actualy > (hy - ancho) * escala && actualy < hy * escala))
            { b = false; return false; }

            b = true;
            return true;

        }

        private void Reset()
        {
            
            numericEscala.Value = (decimal)0.01;
            euler.Value = (decimal)0.001;

            IP1.Value = (decimal)4.00;
            IP2.Value = (decimal)2.00;
            IS1.Value = (decimal)0.00;
            IS2.Value = (decimal)0.00;

            Mass1.Value = (decimal)1.00;
            Mass2.Value = (decimal)1.00;

            Spring1.Value = (decimal)0.00;
            Spring2.Value = (decimal)2.00;
            Spring3.Value = (decimal)0.00;

            SS1.Text = "3+cos(2*t)";
            SS2.Text = "3-cos(2*t)";

            Start = false;
            hx = Pizarra.Width / 2;
            hy = Pizarra.Width / 2;
            mouseDown = false;
            Pizarra.Refresh();

        }

        //Reset
        private void button2_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void Pizarra_MouseMove(object sender, MouseEventArgs e)
        {
            
            float cordX = (e.X-hx)*escala;
            float cordY = (hy-e.Y)*escala;
            
            labCordenadas.Text = "(X,Y)=(" + cordX.ToString() + "," + cordY.ToString() + ")";

            if (mouseDown)
            {
                hx +=(e.X - xmouseDown);
                xmouseDown = e.X;
                hy += (e.Y-ymouseDown);
                ymouseDown = e.Y;
                Pizarra.Refresh();
            }
        }

        private void Pizarra_MouseEnter(object sender, EventArgs e)
        {
            Coordenadas.Visible = true;
        }

        private void Pizarra_MouseLeave(object sender, EventArgs e)
        {
            Coordenadas.Visible = false;
            mouseDown = false;
        }

        private void numeric_ValueChanged(object sender, EventArgs e)
        {
           if(Start) UpdateNow();
        }

        private void Pizarra_MouseDown(object sender, MouseEventArgs e)
        {
            xmouseDown = e.X;
            ymouseDown = e.Y;
            mouseDown = true;
        }

        private void Pizarra_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           if(Start) UpdateNow();
        }
    }
}
