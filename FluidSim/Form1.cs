using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace FluidSim
{
    public partial class mainForm : Form
    {
        GLControl glControl;
        Shader shader;
        Shader lineShader;
        Shader computeShader;
        int vao, ssbo;
        int linevao, linevbo;


        private Stopwatch stopwatch = new Stopwatch();
        private double deltaTime = 0.0;
        private const double targetFrameTime = 1.0 / 120.0;
        private Vector2 mousePos = Vector2.Zero;
        private int mouseAction = 0;

        float dt = 1 / 120.0f;

        float scale = 40;

        float sizex = 35;
        float sizey = 10;

        Vector2 bounds;

        float[] points;// = {0, 0, 0.5f, 0.5f};
        float[] lines; // Used for rendering border;

        public mainForm()
        {
            bounds = new Vector2(sizex / 2, sizey / 2);
            lines = new float[] { -sizex / 2, sizey / 2, sizex / 2, sizey / 2, sizex / 2, -sizey /2 , -sizex / 2, -sizey /2};


            Random rng = new Random();

            int pointnum =1000;
            points = new float[4 * pointnum];
            /*for (int i = 0; i < 4*pointnum; i+=4)
            {
                float angle = (float)i / (float)(4 * pointnum) * 2 * (float)Math.PI;
                float magnitude = 10.0f * (float)Math.Sqrt((float)rng.NextDouble());

                points[i] = magnitude * (float)Math.Cos(angle);
                points[i+1] = magnitude * (float)Math.Sin(angle);

                points[i + 2] = - magnitude * (float)Math.Sin(angle);
                points[i + 3] = magnitude * (float)Math.Cos(angle);

                //points[i] = 20.0f * (float)rng.NextDouble() -10.0f;
                //points[i+1] = 20.0f * (float)rng.NextDouble() - 10.0f;
                //points[i + 2] = 60.0f * (float)rng.NextDouble() - 30.0f;
                //points[i + 3] = 60.0f * (float)rng.NextDouble() - 30.0f;
            }*/

            float radius = 5.0f;

            float pos_x = 0;
            float pos_y = 0;

            float ds = (float)Math.Sqrt(Math.PI / pointnum) * radius;

            int i = 0;
            for(float x = pos_x - radius; x<(pos_x + radius); x+= ds)
            {
                for (float y = pos_y - radius; y < (pos_y + radius); y += ds)
                {
                    if(Math.Pow(x - pos_x, 2) + Math.Pow(y - pos_y, 2) > radius * radius) { continue; }

                    points[i] = x;
                    points[i + 1] = y;

                    i += 4;

                    if (i >= pointnum * 4) { break; }
                }
                if (i >= pointnum * 4) { break; }
            }
            pointnum = i / 4 - 1;
            points = points.Take(i-4).ToArray();

            InitializeComponent();

            OnLoad(null, null);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            glControl = new GLControl();
            renderPanel.Controls.Add(glControl);
            glControl.Parent = renderPanel;
            glControl.Dock = DockStyle.Fill;
            glControl.Paint += OnRender;
            glControl.Load += LoadGL;
            glControl.KeyDown += OnKeyDown;
            glControl.MouseMove += OnMouseMove;
            glControl.MouseUp += OnMouseUp;
            glControl.MouseDown += OnMouseDown;


            stopwatch.Start();
        }

        public void LoadGL(object sender, EventArgs e)
        {

            GL.Viewport(0, 0, renderPanel.Width, renderPanel.Height);

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.ProgramPointSize);


            shader = new Shader("vertex.glsl", "frag.glsl");
            lineShader = new Shader("line_shader_vert.glsl", "line_shader_frag.glsl");
            computeShader = new Shader("compute.glsl");

            linevbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, linevbo);
            GL.BufferData(BufferTarget.ArrayBuffer, lines.Length * 4, lines, BufferUsageHint.StaticDraw);

            linevao = GL.GenVertexArray();
            GL.BindVertexArray(linevao);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8, 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);



            ssbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer,
            points.Length * 4,
                          points, BufferUsageHint.DynamicDraw);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, ssbo);

            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 16, 0);
            GL.EnableVertexAttribArray(0);





            // Later during rendering
        }

        private void mainForm_Resize(object sender, EventArgs e)
        {

            GL.Viewport(0, 0, renderPanel.Width, renderPanel.Height);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {

                scale /= 1.1f;
            }
            if (e.KeyCode == Keys.S)
            {

                scale *= 1.1f;
            }

            if (e.KeyCode == Keys.A)
            {

                dt /= 1.1f;
            }
            if (e.KeyCode == Keys.D)
            {

                dt *= 1.1f;
            }

        }

        public void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mousePos = new Vector2(e.X, e.Y);
        }

        public void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                mouseAction = 1;
            else if(e.Button == MouseButtons.Right)
            {
                mouseAction = -1;
            }
        }

        public void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || (e.Button == MouseButtons.Right))
                mouseAction = 0;
        }

        public void OnRender(object sender, PaintEventArgs e)
        {
            deltaTime = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            Matrix4 mat = Matrix4.CreateScale(1 / scale, 1 / scale * glControl.Width / glControl.Height, 1);


            computeShader.Use();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ssbo);
            

            Vector4 mouseNorm =  new Vector4(2 * mousePos.X / renderPanel.Width - 1, 1.0f - 2 *  mousePos.Y / renderPanel.Height,0 ,1); // Invert Y axis if needed

            GL.UniformMatrix4(GL.GetUniformLocation(computeShader.Handle, "mat"), false, ref mat);
            GL.Uniform1(GL.GetUniformLocation(computeShader.Handle, "pointcount"), points.Length / 4);
            GL.Uniform1(GL.GetUniformLocation(computeShader.Handle, "dt"), dt);
            GL.Uniform2(GL.GetUniformLocation(computeShader.Handle, "bounds"), bounds);
            GL.Uniform2(GL.GetUniformLocation(computeShader.Handle, "mousePos"), mouseNorm.Xy);
            GL.Uniform1(GL.GetUniformLocation(computeShader.Handle, "isPulling"), mouseAction);

            GL.DispatchCompute((points.Length / 4 + 127) / 128, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            

            GL.Clear(ClearBufferMask.ColorBufferBit);

            
            lineShader.Use();
            GL.UniformMatrix4(GL.GetUniformLocation(lineShader.Handle, "mat"), false, ref mat);
            GL.BindVertexArray(linevao);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, lines.Length / 2);

            shader.Use();
            GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "mat"), false, ref mat);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Points, 0, points.Length / 4);

            

            glControl.SwapBuffers();
            glControl.Invalidate();


            // --- Your normal simulation code here ---
            // ComputeShaderStep(deltaTime);
            // Draw();


            // --- Framerate limiter (optional) ---
            double frameTime = stopwatch.Elapsed.TotalSeconds;
            fpsLabel.Text = (1 / (float)deltaTime).ToString();
            if (frameTime < targetFrameTime)
            {
                Thread.Sleep((int)((targetFrameTime - frameTime) * 1000.0));
            }
        }
    }
}
