// This code was written for the OpenTK library and has been released
// to the Public Domain.
// It is provided "as is" without express or implied warranty of any kind.

using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Examples
{
	public class Test : GameWindow
	{
		int vertexShaderHandle,
		fragmentShaderHandle,
		shaderProgramHandle,
		modelviewMatrixLocation,
		projectionMatrixLocation,
		vaoHandle,
		positionVboHandle,
		normalVboHandle,
		eboHandle;

		Vector3[] positionVboData = new Vector3[]{
			new Vector3(-1.0f, -1.0f,  1.0f),
			new Vector3( 1.0f, -1.0f,  1.0f),
			new Vector3( 1.0f,  1.0f,  1.0f),
			new Vector3(-1.0f,  1.0f,  1.0f),
			new Vector3(-1.0f, -1.0f, -1.0f),
			new Vector3( 1.0f, -1.0f, -1.0f), 
			new Vector3( 1.0f,  1.0f, -1.0f),
			new Vector3(-1.0f,  1.0f, -1.0f) };

		uint[] indicesVboData = new uint[]
		{
			// front face
			0, 1, 2, 2, 3, 0,
			// top face
			3, 2, 6, 6, 7, 3,
			// back face
			7, 6, 5, 5, 4, 7,
			// left face
			4, 0, 3, 3, 7, 4,
			// bottom face
			0, 1, 5, 5, 4, 0,
			// right face
			1, 5, 6, 6, 2, 1, 
		};

		Matrix4 projectionMatrix, modelviewMatrix;

		public Test()
			: base(640, 480,
				new GraphicsMode(), "OpenGL 3 Example", 0,
				DisplayDevice.Default, 3, 2,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
		{ }

		protected override void OnLoad (System.EventArgs e)
		{
			VSync = VSyncMode.On;

			CreateShaders();
			CreateVBOs();
			CreateVAOs();

			// Other state
			GL.Enable(EnableCap.DepthTest);
			GL.ClearColor(System.Drawing.Color.MidnightBlue);
		}

		static string LoadTextFile( string filePath )
		{
			var streamReader = new StreamReader (filePath);
			string text = streamReader.ReadToEnd ();
			streamReader.Close ();
			return text;
		}

		void CreateShaders()
		{
			vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
			fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

			GL.ShaderSource(vertexShaderHandle, LoadTextFile ("../Shaders/Test.vert"));
			GL.ShaderSource(fragmentShaderHandle, LoadTextFile ("../Shaders/Test.frag"));

			GL.CompileShader(vertexShaderHandle);
			GL.CompileShader(fragmentShaderHandle);

			Debug.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
			Debug.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

			// Create program
			shaderProgramHandle = GL.CreateProgram();

			GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
			GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

			GL.BindAttribLocation(shaderProgramHandle, 0, "in_position");
			GL.BindAttribLocation(shaderProgramHandle, 1, "in_normal");

			GL.LinkProgram(shaderProgramHandle);
			Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
			GL.UseProgram(shaderProgramHandle);

			// Set uniforms
			projectionMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "projection_matrix");
			modelviewMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "modelview_matrix");

			float aspectRatio = ClientSize.Width / (float)(ClientSize.Height);
			Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspectRatio, 1, 100, out projectionMatrix);
			modelviewMatrix = Matrix4.LookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

			GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
			GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

			GL.DeleteShader (vertexShaderHandle);
			GL.DeleteShader (fragmentShaderHandle);
		}

		void CreateVBOs()
		{
			positionVboHandle = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
				new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
				positionVboData, BufferUsageHint.StaticDraw);

			normalVboHandle = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
				new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
				positionVboData, BufferUsageHint.StaticDraw);

			eboHandle = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
			GL.BufferData(BufferTarget.ElementArrayBuffer,
				new IntPtr(sizeof(uint) * indicesVboData.Length),
				indicesVboData, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		void CreateVAOs()
		{
			// GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
			// This means we do not have to re-issue VertexAttribPointer calls
			// every time we try to use a different vertex layout - these calls are
			// stored in the VAO so we simply need to bind the correct VAO.
			vaoHandle = GL.GenVertexArray();
			GL.BindVertexArray(vaoHandle);

			GL.EnableVertexAttribArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

			GL.EnableVertexAttribArray(1);
			GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);

			GL.BindVertexArray(0);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
			Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
			GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape])
				Exit();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.BindVertexArray(vaoHandle);
			GL.DrawElements(PrimitiveType.Triangles, indicesVboData.Length,
				DrawElementsType.UnsignedInt, IntPtr.Zero);

			SwapBuffers();
		}

		[STAThread]
		public static void Main()
		{
			using (Program example = new Program())
			{
				//Utilities.SetWindowTitle(example);
				example.Run(30);
			}
		}
	}
}