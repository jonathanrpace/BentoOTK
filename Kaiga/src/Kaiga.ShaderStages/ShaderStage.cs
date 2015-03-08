using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	abstract public class ShaderStage : IGraphicsContextDependant
	{
		protected int shaderProgram;

		public int ShaderProgram
		{
			get
			{
				return shaderProgram;
			}
		}

		#region IGraphicsContextDependant implementation

		public void CreateGraphicsContextResources()
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

		public void DisposeGraphicsContextResources()
		{
			if ( GL.IsProgram( shaderProgram ) )
			{
				GL.DeleteProgram( shaderProgram );
			}
		}

		#endregion
		
		public virtual void BindShader( RenderParams renderParams )
		{

		}

		protected abstract string GetShaderSource();
		protected abstract ShaderType GetShaderType();
	}
}

