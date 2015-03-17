using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using Kaiga.Core;
using OpenTK;

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

		public virtual void CreateGraphicsContextResources()
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

		public virtual void DisposeGraphicsContextResources()
		{
			if ( GL.IsProgram( shaderProgram ) )
			{
				GL.DeleteProgram( shaderProgram );
			}
		}

		#endregion

		protected void SetUniformMatrix4( string name, ref Matrix4 matrix, bool transposed = false )
		{
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.UniformMatrix4( location, transposed, ref matrix );
		}

		protected void SetUniformMatrix3( string name, ref Matrix3 matrix, bool transposed = false )
		{
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.UniformMatrix3( location, transposed, ref matrix );
		}

		protected void SetUniform1( string name, float value )
		{
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform1( location, value );
		}

		protected void SetUniform2( string name, Vector2 value )
		{
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform2( location, value );
		}

		protected void SetUniform3( string name, Vector3 value )
		{
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform3( location, value );
		}

		protected void SetUniform4( string name, Vector4 value )
		{
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform4( location, value );
		}
		
		protected abstract string GetShaderSource();
		protected abstract ShaderType GetShaderType();
	}
}

