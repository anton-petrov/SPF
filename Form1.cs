/*
 * 
 Author: Anton Petrov, 2005-2014.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;



namespace SPF
{
	/// <summary>
	/// GUI for SPF algorithm
	/// </summary>
	/// 

	class Sound
	{
		[DllImport("winmm")]
		public static extern int PlaySound(string pszSound, int hmod, int fdwSound);
		public static bool mute = false;
		public static void Play(string sndName)
		{
			//if(!mute)
			//	PlaySound("snd\\"+sndName, 0, 1 /* SND_ASYNC */);
		}
	}

	#region Implementating visual presentation of the vertices

	class VisualVertex : PictureBox
	{
		public const int VisualSize = 48;
		public int VertexNumber = 1;
		private bool isDragging = false;
		public Color C = Color.Black;
		private int oldX;
		private int oldY;
		private int Shape = 0;
		private int W = 2;

		public VisualVertex(int x, int y)
		{
			this.SizeMode = PictureBoxSizeMode.Normal;
			this.BorderStyle = BorderStyle.None;
			this.Location = new System.Drawing.Point(x, y);
			this.Size = new System.Drawing.Size(VisualSize, VisualSize);
			this.Cursor = Cursors.Hand;
			//this.Image = new Bitmap("b1.bmp");

			this.MouseDown += new System.Windows.Forms.MouseEventHandler(VisualVertex_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(VisualVertex_MouseUp);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(VisualVertex_MouseMove);

			this.MouseEnter += new EventHandler(VisualVertex_MouseEnter);
			this.MouseLeave += new EventHandler(VisualVertex_MouseLeave);
	
			this.Paint += new System.Windows.Forms.PaintEventHandler(VisualVertex_Paint);

			this.SetStyle(ControlStyles.DoubleBuffer |
				ControlStyles.UserPaint | 
				ControlStyles.AllPaintingInWmPaint,
				true);
			this.UpdateStyles();
		}

		private void ChangeColor(Color newColor)
		{
			foreach(VisualVertex v in GlobalVisual.Vertexes)
				if(v.C == newColor)
				{
					v.C = Color.Black;
					v.BackColor = Color.White;
				}
			this.C = newColor;
			this.BackColor = Color.WhiteSmoke;
			Parent.Invalidate();
		}

		private void VisualVertex_MouseDown(object sender, MouseEventArgs e)
		{
			if( e.Button == MouseButtons.Left && !GlobalVisual.isCreateLink && !GlobalVisual.isSelect1)
			{
				if(e.Clicks == 1)
				{
					isDragging = true;
			
				}
				else if(e.Clicks == 2)
				{
					if(Shape == 2)
						Shape = 0;
					else
						Shape++;
					Invalidate();
				}
			}
			if(GlobalVisual.isCreateLink || e.Button == MouseButtons.Right)
			{
				GlobalVisual.isConnecting = true;
				GlobalVisual.Link.v1 = this;
			}
			if(GlobalVisual.isSelect1)
			{
				GlobalVisual.VertexA = this.VertexNumber-1;
				ChangeColor(Color.ForestGreen);
			}
			if(GlobalVisual.isSelect2)
			{
				GlobalVisual.VertexB = this.VertexNumber-1;
				ChangeColor(Color.Firebrick);
			}
			if (e.Clicks == 2)
			{
				isDragging = false;
			}
			oldX = e.X;
			oldY = e.Y;
		}

		private void VisualVertex_MouseUp(object sender, MouseEventArgs e)
		{
			Point p = new Point(this.Left + e.X, this.Top + e.Y);
			bool duplicate = false;
			int m;
			isDragging = false;
			try 
			{
				m = Convert.ToInt32(GlobalVisual.text.Text);
			}
			catch
			{
				m = 1;
			}
			if (GlobalVisual.isConnecting)
			{
				foreach (VisualVertex v in GlobalVisual.Vertexes)
				{
					Rectangle r = new Rectangle(v.Left, v.Top, v.Width, v.Height);
					if (r.Contains(p) && !this.Equals(v))
					{
						foreach (VisualLink vl in GlobalVisual.Links)
						{
							if ((this.Equals(vl.v1) || this.Equals(vl.v2)) &&
								(v.Equals(vl.v1) || v.Equals(vl.v2)))
							{
								duplicate = true;
								if (m <= 0)
									GlobalVisual.Links.Remove(vl);
								else
									vl.Weight = m;
								break;
							}
						}
						if (!duplicate)
						{
							VisualLink l = new VisualLink(this, v);
							l.Weight = m;
							if (l.Weight > 0)
							{
								GlobalVisual.Links.Add(l);
							}
						}
						break;
					}
				}
			}
			//else
			//	;
			if(this.C != Color.Firebrick && this.C != Color.ForestGreen)
				this.C = Color.White;
			GlobalVisual.isConnecting = false;
			GlobalVisual.isCreateLink = false;
			GlobalVisual.isSelect1 = false;
			GlobalVisual.isSelect2 = false;
			GlobalVisual.button3.BackColor = Color.SteelBlue;
			GlobalVisual.button4.BackColor = Color.SteelBlue;
			GlobalVisual.BuildMatrix();

			Invalidate();
			Parent.Invalidate();
		}

		private void VisualVertex_MouseMove(object sender, MouseEventArgs e)
		{
			if(isDragging)
			{
				if(this.C != Color.Firebrick && this.C != Color.ForestGreen)
					this.C = Color.Black;
				this.Top = this.Top + (e.Y - oldY);
				this.Left = this.Left + (e.X - oldX);
			}
			if(GlobalVisual.isConnecting)
			{
				GlobalVisual.Link.ptY.X = e.X + this.Left;
				GlobalVisual.Link.ptY.Y = e.Y + this.Top;
			}
			Parent.Invalidate();
		}

		private void InitializeComponent()
		{

		}
	
		private void VisualVertex_Paint(object sender, PaintEventArgs e)
		{
			Point[] points = {
								 new Point(0, VisualSize-2), 
								 new Point(VisualSize, VisualSize-2),
								 new Point(VisualSize/2, 0)
							 };
			int t = 7;
			Graphics g = e.Graphics;
			//if(this.C != Color.Black)
				this.BackColor = Color.White;
			//else
			//	this.BackColor = Color.White;
			g.SmoothingMode =  System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			if (VertexNumber > 9) t = 12;
			g.DrawString(VertexNumber.ToString(), new Font("Arial", 12, FontStyle.Bold), new SolidBrush(Color.SeaGreen), VisualSize/2-t, VisualSize/2-9);

			// Рисуем форму узла
			switch(Shape)
			{
				case 0: 
					g.DrawEllipse(new Pen(new SolidBrush(C), W), 1, 1, VisualVertex.VisualSize-3, VisualVertex.VisualSize-3);
					break;
				case 1: 
					g.DrawRectangle(new Pen(new SolidBrush(C), W), 1, 1, VisualVertex.VisualSize-3, VisualVertex.VisualSize-3);
					break;
				case 2: 
					g.DrawPolygon(new Pen(new SolidBrush(C), W), points);
					break;
			}
		}

		private void VisualVertex_MouseEnter(object sender, EventArgs e)
		{
			if(this.C != Color.Firebrick && this.C != Color.ForestGreen)
			{
				//this.C = Color.CadetBlue;
				this.W = 3;
				Invalidate();
			}
		}

		private void VisualVertex_MouseLeave(object sender, EventArgs e)
		{
			if(this.C != Color.Firebrick && this.C != Color.ForestGreen)
			{
				this.C = Color.Black;
				this.W = 2;
				Invalidate();
			}
		}
	}

	#endregion

	#region User Interface
	class GlobalVisual
	{
		public static bool ControlsVisible = true;
		public static bool DoubleBuffered = true;
		public static string Path;

		public static int VertexA = 0;
		public static int VertexB = 1;

		public static bool DrawMetrics = false;

		public static Button button1 = new Button();
		public static Button button2 = new Button();
		public static Button button3 = new Button();
		public static Button button4 = new Button();

		public static VisualLink Link = new VisualLink();
		public static bool isConnecting = false;
		public static bool isCreateLink = false;
		public static bool isSelect1 = false;
		public static bool isSelect2 = false;

		public static ArrayList Links = new ArrayList();
		public static ArrayList Vertexes = new ArrayList();

		public static TextBox text = new TextBox();

		public static int vertCount = 0;

		public static DijkstraAlgorithm da = new DijkstraAlgorithm();
		public static int[,] adjacencyMatrix;
		public static bool BuildMatrix()
		{
			adjacencyMatrix = new int[Vertexes.Count, Vertexes.Count];
			for(int i = 0; i < Vertexes.Count; i++)
				for(int j = 0; j < Vertexes.Count; j++)
					adjacencyMatrix[i, j] = MathConsts.INFINITE;
			foreach(VisualLink l in Links)
			{
				l.UpdateLink();
				adjacencyMatrix[l.v1.VertexNumber-1, l.v2.VertexNumber-1] = l.Weight;
				adjacencyMatrix[l.v2.VertexNumber-1, l.v1.VertexNumber-1] = l.Weight;
			}
			da.SetMatrix(adjacencyMatrix);

			if(Vertexes.Count > 1)
				return true;
			else
				return false;
		}
	}

	#endregion

	#region Мisual representation of links and vertexes

	class VisualLink : IDisposable
	{
		public Point ptX = new Point(0, 0);
		public Point ptY = new Point(0, 0);
		public VisualVertex v1;
		public VisualVertex v2;
		public Color C = Color.Silver;
		
		public int Weight = MathConsts.INFINITE;
		public VisualLink()	{}
		public VisualLink(VisualLink l)	
		{
			this.v1 = l.v1;
			this.v2 = l.v2;
			UpdateLink();
		}
		public VisualLink(int x1, int y1, int x2, int y2)
		{
			ptX = new Point(x1, y1);
			ptY = new Point(x2, y2);
		}
		public VisualLink(VisualVertex Vertex1, VisualVertex Vertex2)
		{
			v1 = Vertex1;
			v2 = Vertex2;

			UpdateLink();
		}
		public void UpdateLink()
		{
			if(v1 != null)
			{
				ptX.X = v1.Left+ VisualVertex.VisualSize/2;
				ptX.Y = v1.Top+ VisualVertex.VisualSize/2;

			}
			if(v2 != null)
			{
				ptY.X = v2.Left + VisualVertex.VisualSize/2;
				ptY.Y = v2.Top + VisualVertex.VisualSize/2;
			}
		}
		#region IDisposable Members

		public void Dispose()
		{
			// TODO:  Add VisualLink.Dispose implementation
//			this.Finalize();
		}

		#endregion
	}

	#endregion

	public class MainForm : Form
	{
		private VisualVertex box;
		private bool isDragging;
		public bool isConnecting = false;

		private System.Windows.Forms.Timer timer1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Creating controls
		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			CenterToScreen();
			
			GlobalVisual.text.Dock = DockStyle.Right; //| DockStyle.Top;
			GlobalVisual.text.Width = 50;
			GlobalVisual.text.Text = "1";
			GlobalVisual.text.Visible = true;
			GlobalVisual.text.BackColor = Color.SteelBlue;
			GlobalVisual.text.ForeColor = Color.White;
			GlobalVisual.text.Font = new Font("Arial", 11, FontStyle.Bold);
			Controls.Add(GlobalVisual.text);

			//GlobalVisual.button1.Dock = DockStyle.Bottom | DockStyle.Right;
			GlobalVisual.button1.Top  = this.Height - 125;
			GlobalVisual.button1.Left = this.Width - 158;
			GlobalVisual.button1.Width = 150;
			GlobalVisual.button1.Text = "Find";
			GlobalVisual.button1.Font = new Font("Verdana", 10, FontStyle.Bold);
			GlobalVisual.button1.BackColor = Color.SteelBlue;
			GlobalVisual.button1.ForeColor = Color.White;
			GlobalVisual.button1.Click += new EventHandler(button1_Click);
			GlobalVisual.button1.Visible = true;
			Controls.Add(GlobalVisual.button1);
			
			GlobalVisual.button2 = new Button();
			GlobalVisual.button2.Top  = this.Height - 102;
			GlobalVisual.button2.Left = this.Width - 158;
			GlobalVisual.button2.Width = 150;
			GlobalVisual.button2.Text = "Clear";
			GlobalVisual.button2.Font = new Font("Verdana", 10);
			GlobalVisual.button2.BackColor = Color.SteelBlue;
			GlobalVisual.button2.ForeColor = Color.White;
			GlobalVisual.button2.Click += new EventHandler(button2_Click);
			GlobalVisual.button2.Visible = true;
			Controls.Add(GlobalVisual.button2);

			GlobalVisual.button3 = new Button();
			GlobalVisual.button3.Top  = this.Height - 79;
			GlobalVisual.button3.Left = this.Width - 158;
			GlobalVisual.button3.Width = 150;
			GlobalVisual.button3.Font = new Font("Verdana", 10, FontStyle.Bold);
			GlobalVisual.button3.Text = "Initial node";
			GlobalVisual.button3.BackColor = Color.DarkGreen;
			GlobalVisual.button3.ForeColor = Color.White;
			GlobalVisual.button3.Click += new EventHandler(button3_Click);
			GlobalVisual.button3.Visible = true;
			Controls.Add(GlobalVisual.button3);		
			
			GlobalVisual.button4 = new Button();
			GlobalVisual.button4.Top  = this.Height - 56;
			GlobalVisual.button4.Left = this.Width - 158;
			GlobalVisual.button4.Width = 150;
			GlobalVisual.button4.Font = new Font("Verdana", 10, FontStyle.Bold);
			GlobalVisual.button4.Text = "Target node";
			GlobalVisual.button4.BackColor = Color.Red;
			GlobalVisual.button4.ForeColor = Color.White;
			GlobalVisual.button4.Click += new EventHandler(button4_Click);
			GlobalVisual.button4.Visible = true;
			Controls.Add(GlobalVisual.button4);	

			timer1 = new Timer();
			timer1.Enabled = false;
			timer1.Interval = 50;
			timer1.Tick += new EventHandler(timer1_Tick);
		}

		#endregion

		private void timer1_Tick(object Sender, EventArgs e)   
		{
			button1_Click(null, null);			
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Metric m = new Metric();
			if(!GlobalVisual.BuildMatrix())
				return;

			m = GlobalVisual.da.GetMetric(GlobalVisual.VertexA, GlobalVisual.VertexB);
			foreach(VisualLink l in GlobalVisual.Links)
			{
				l.C = Color.Silver;
				bool mustPaint = false;
				for(int i = 0; i < m.intPath.Count-1; i++)
				{
					if( ( m.intPath.GetVertex(i)+1 == l.v1.VertexNumber &&
						m.intPath.GetVertex(i+1)+1 == l.v2.VertexNumber ) ||
						( m.intPath.GetVertex(i+1)+1 == l.v1.VertexNumber &&
						m.intPath.GetVertex(i)+1 == l.v2.VertexNumber ) )
					{
						mustPaint = true;
					}
				}
				if(mustPaint)
					l.C = Color.Orange;
				mustPaint = false;
			}
				
			GlobalVisual.Path = "";
			for(int i = 0; i < m.intPath.Count; i++)
			{
				GlobalVisual.Path += (m.intPath.GetVertex(i)+1).ToString();
				if(i !=  m.intPath.Count-1)
					GlobalVisual.Path += "->";
			}
			if(m.metric == MathConsts.INFINITE)
				GlobalVisual.Path = "Path not found! m=INF";
			else
				GlobalVisual.Path += " m=" + m.metric.ToString();
			GlobalVisual.DrawMetrics = true;
			
			Invalidate();

			if(!timer1.Enabled)
			{
				//Sound.Play();
				timer1.Enabled = true;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			GlobalVisual.Links.RemoveRange(0, GlobalVisual.Links.Count);
			foreach(VisualVertex v in GlobalVisual.Vertexes)
			{
				v.Dispose();
			}
			GlobalVisual.Vertexes.RemoveRange(0, GlobalVisual.Vertexes.Count);
			GlobalVisual.vertCount = 0;
			GlobalVisual.VertexA = 0;
			GlobalVisual.VertexB = 1;
			//Invalidate();
			ShowHelp();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			GlobalVisual.isSelect1 = true;
			GlobalVisual.button3.BackColor = Color.Gainsboro;
		}
		
		private void button4_Click(object sender, EventArgs e)
		{
			GlobalVisual.isSelect2 = true;
			GlobalVisual.button4.BackColor = Color.Gainsboro;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Form init
		public void EnableDoubleBuffering(bool Enable)
		{
			if(Enable)
			{
				this.SetStyle(ControlStyles.DoubleBuffer |
					ControlStyles.UserPaint | 
					ControlStyles.AllPaintingInWmPaint,
					true);
			}
			else
			{
				this.SetStyle(//ControlStyles.DoubleBuffer |
					ControlStyles.UserPaint | 
					ControlStyles.AllPaintingInWmPaint,
					true);
			}

			this.UpdateStyles();
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.Size = new System.Drawing.Size(650, 550);
			this.Text = "Dijkstra's algorithm v. 1.0";

			this.BackColor = Color.White;
			this.Resize += new EventHandler(MainForm_Resize);
			this.Paint += new System.Windows.Forms.PaintEventHandler(MainForm_Paint);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(MainForm_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(MainForm_MouseUp);
			this.KeyPress += new KeyPressEventHandler(MainForm_KeyPress);
			this.KeyDown += new KeyEventHandler(MainForm_KeyDown);
			this.KeyUp += new KeyEventHandler(MainForm_KeyUp);
			this.KeyPreview = true;

			EnableDoubleBuffering(GlobalVisual.DoubleBuffered);

			ShowHelp();
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			foreach(string s in args)
			{
				if (s == "-FastDraw")
				{
					GlobalVisual.DoubleBuffered = false;
					
				}
				else if (s == "-NoSound")
					Sound.mute = true;

			}
			Application.Run(new MainForm());
		}

		public void MainForm_Resize(object sender, System.EventArgs e)
		{
			GlobalVisual.button1.Top  = this.Height - 125;
			GlobalVisual.button1.Left = this.Width - 158;
			GlobalVisual.button2.Top  = this.Height - 102;
			GlobalVisual.button2.Left = this.Width - 158;
			GlobalVisual.button3.Top  = this.Height - 79;
			GlobalVisual.button3.Left = this.Width - 158;
			GlobalVisual.button4.Top  = this.Height - 56;
			GlobalVisual.button4.Left = this.Width - 158;
			Invalidate();
		}
		private void ShowHelp()
		{
			GlobalVisual.DrawMetrics = false;

			const int o = 80;
			Graphics g = Graphics.FromHwnd(this.Handle);
			g.Clear(Color.White);
			this.Refresh();

			g.TranslateTransform(0, 50);

			Font f = new Font("Verdana", 14, FontStyle.Bold);
			SolidBrush b = new SolidBrush(Color.IndianRed); 
			g.DrawString("Help", f, b, 120, 10);
			f = new Font("Verdana", 10);
			g.DrawString("* Double-click to create new node", f, b, o, 35);
			g.DrawString("", f, b, o+19, 50);

			g.DrawString("* To create a new connection between the nodes, do left-click on the node", f, b, o, 75);
			g.DrawString("and drag the cursor to another node", f, b, o + 19, 90);
		}

		#region Drawing on form

		public void MainForm_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.SmoothingMode =  System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			if (GlobalVisual.isConnecting)
			{
				Pen p = new Pen(Color.Silver, 2);
				p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
				GlobalVisual.Link.UpdateLink();
				g.DrawLine(p, GlobalVisual.Link.ptX, GlobalVisual.Link.ptY);

			}

			foreach(VisualLink l in GlobalVisual.Links)
			{
				int t_x, t_y, xn, yn, t=0; double x1, x2, y1, y2, y3, x3, k;
				l.UpdateLink();
				LineCap temp = LineCap.RoundAnchor;
				Pen thePen = new Pen(new SolidBrush(l.C), 2);

				thePen.DashStyle = DashStyle.Solid;
				if (l.Weight >= 10) thePen.DashStyle = DashStyle.Dot;

				thePen.StartCap = temp;
				thePen.EndCap = temp;
				g.DrawLine(thePen, l.ptX, l.ptY);

				x2 = t_x = Math.Abs(l.ptX.X+l.ptY.X)/2;
				y2 = t_y = Math.Abs(l.ptX.Y+l.ptY.Y)/2;
				x3 = l.ptX.X; x1 = l.ptY.X;
				y3 = l.ptX.Y; y1 = l.ptY.Y;
				k =  ( (x3 - x1)/(y3 - y1) );

				if (k > 0.1 && k <= 0.7) t = (int)(k * 25); //15;
				else if (k > 0.7 && k <=2) t = 15;
				else if (k > 2 && k <= 3) t = 17;
				else if (k > 3) t = 20;
				//g.DrawString(k.ToString(), new Font("Arial", 14), new SolidBrush(l.C), 100, 100);

				// ----------------------------------------------------------

				g.DrawString(l.Weight.ToString(), new Font("Arial", 14), new SolidBrush(Color.DimGray), 
					(int)x2+t/3, (int)y2-t);
			}

			if(GlobalVisual.ControlsVisible)
			{
				g.DrawString("Metric", new Font("Arial", 10), new SolidBrush(Color.DarkBlue), 
					this.Width-158, 4);
			}

			
			// Метрики - отображение базы данных состояния связей
			if(GlobalVisual.DrawMetrics)
			{

				// Найденный путь вершин
				g.DrawString(GlobalVisual.Path, new Font("Courier New", 16, FontStyle.Bold),
					new SolidBrush(Color.DarkGreen), 10, 5);
				//-----------------------------------------------------------------------------

				g.DrawString("Metrics", 
					new Font("Arial", 11, FontStyle.Bold | FontStyle.Underline),
					new SolidBrush(Color.Black),
					14, 35);
				int n = (int)Math.Sqrt(GlobalVisual.da.matrix.Length)-1;

				int y_offset = 5;
				//g.DrawString("От->до   Метрика", 
				//    new Font("Courier New", 10, FontStyle.Bold | FontStyle.Italic),
				//    new SolidBrush(Color.MidnightBlue), 14, 55 + y_offset);
				//y_offset += 20;
				for(int i = 0;  i <= Math.Sqrt(GlobalVisual.da.matrix.Length)-1; i++)
				{
					for(int j = 0; j <= Math.Sqrt(GlobalVisual.da.matrix.Length)-n-1; j++)
					{
						if(/*i != j &&*/ GlobalVisual.da.matrix[i, j] != MathConsts.INFINITE)
						{
							g.DrawString((i+1).ToString()+"->"+(j+1).ToString()+" : "+GlobalVisual.da.matrix[i, j], 
								new Font("Courier New", 10, FontStyle.Bold), 
								new SolidBrush(Color.Black),
								14, 55+y_offset);
							y_offset += 15;
							g.DrawString((j+1).ToString()+"->"+(i+1).ToString()+" : "+GlobalVisual.da.matrix[i, j], 
								new Font("Courier New", 10, FontStyle.Bold), 
								new SolidBrush(Color.DarkViolet),
								14, 55+y_offset);
							y_offset += 15;
						}
					}
					n--;
				}
			}
		}

		#endregion

		private void MainForm_MouseUp(object sender, MouseEventArgs e)
		{
			isDragging = false;
		}

		private void MainForm_MouseDown(object sender, MouseEventArgs e)
		{
			isDragging = true;
			if(e.Clicks == 2)
			{
				isDragging = false;
				box = new VisualVertex(e.X-VisualVertex.VisualSize/2, e.Y-VisualVertex.VisualSize/2);
				box.VertexNumber = (GlobalVisual.vertCount++)+1;
				GlobalVisual.Vertexes.Add(box);
				Controls.Add(box);
				GlobalVisual.BuildMatrix();
		
			}
		}

		private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
		{
			
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Space)
			{
				Focus();
				GlobalVisual.ControlsVisible = !GlobalVisual.ControlsVisible;
				foreach(Control c in Controls)
					if(!(c is PictureBox))
						c.Visible = !c.Visible;
			}
			Invalidate();
		}

		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			Focus();
		}
	}
}