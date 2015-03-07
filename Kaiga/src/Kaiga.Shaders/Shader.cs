using System;
using Ramen;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using Kaiga.Core;

namespace Kaiga.Shaders
{
	abstract public class Shader
	{
		protected int shaderProgram;

		public int ShaderProgram
		{
			get
			{
				return shaderProgram;
			}
		}

		protected Shader()
		{
			Init();
		}

		~Shader()
		{
			GL.DeleteProgram( shaderProgram );
		}

		private void Init()
		{
			var shaderSource = new string[1];
			shaderSource[ 0 ] = GetShaderSource();

			shaderProgram = GL.CreateShaderProgram
			( 
				GetShaderType(),
				shaderSource.Length,
				shaderSource
			);
			var log = GL.GetProgramInfoLog( shaderProgram );
			Debug.Write( log );
		}

		public virtual void BindShader( RenderParams renderParams )
		{

		}

		protected abstract string GetShaderSource();
		protected abstract ShaderType GetShaderType();
	}
}

