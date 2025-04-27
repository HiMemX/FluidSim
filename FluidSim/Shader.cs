using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Windows.Forms;

namespace FluidSim
{
    public class Shader
    {
        public int Handle { get; private set; }
        public Shader(string vertLocation, string fragLocation)
        {
            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, File.ReadAllText(vertLocation));
            GL.CompileShader(vertShader);

            GL.GetShader(vertShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) // 0 means failure
            {
                string infoLog = GL.GetShaderInfoLog(vertShader);
                MessageBox.Show($"Vertex Shader compilation failed:\n{infoLog}");
            }

            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, File.ReadAllText(fragLocation));
            GL.CompileShader(fragShader);

            GL.GetShader(fragShader, ShaderParameter.CompileStatus, out int success2);
            if (success2 == 0) // 0 means failure
            {
                string infoLog = GL.GetShaderInfoLog(fragShader);
                MessageBox.Show($"Frag Shader compilation failed:\n{infoLog}");
            }

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertShader);
            GL.AttachShader(Handle, fragShader);
            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, vertShader);
            GL.DetachShader(Handle, fragShader);
            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);
        }

        public Shader(string computeLocation)
        {
            int computeShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(computeShader, File.ReadAllText(computeLocation));
            GL.CompileShader(computeShader);

            GL.GetShader(computeShader, ShaderParameter.CompileStatus, out int success2);
            if (success2 == 0) // 0 means failure
            {
                string infoLog = GL.GetShaderInfoLog(computeShader);
                MessageBox.Show($"Compute Shader compilation failed:\n{infoLog}");
            }

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, computeShader);
            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, computeShader);
            GL.DeleteShader(computeShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }
    }
}