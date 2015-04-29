using System;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using Kaiga.Core;
using OpenTK;
using System.Reflection;
using System.IO;

namespace Kaiga.Shaders
{
	abstract public class AbstractShaderStage : AbstractValidatable
	{
		readonly string shaderResourceID;
		int textureUnit;

		protected int shaderProgram;
		public int ShaderProgram
		{
			get
			{
				validate();
				return shaderProgram;
			}
		}

		protected AbstractShaderStage( string shaderResourceID )
		{
			this.shaderResourceID = shaderResourceID;
		}

		protected AbstractShaderStage()
		{
			
		}

		protected override void onValidate()
		{
			var shaderSource = new string[1];

			if ( shaderResourceID != null )
			{
				var assembly = Assembly.GetExecutingAssembly();
				using (Stream stream = assembly.GetManifestResourceStream( shaderResourceID ))
				using (var reader = new StreamReader( stream ))
				{
					shaderSource[ 0 ] = reader.ReadToEnd();
				}
			}
			else
			{
				shaderSource[ 0 ] = GetShaderSource();
			}

			shaderProgram = GL.CreateShaderProgram
				( 
					GetShaderType(),
					shaderSource.Length,
					shaderSource
				);
			var log = GL.GetProgramInfoLog( shaderProgram );
			Debug.Write( log );
		}

		protected override void onInvalidate()
		{
			if ( GL.IsProgram( shaderProgram ) )
			{
				GL.DeleteProgram( shaderProgram );
			}
		}
		
		public virtual void BindPerPass( RenderParams renderParams )
		{
			textureUnit = 0;
		}

		public virtual void UnbindPerPass()
		{

		}

		protected virtual string GetShaderSource()
		{
			return null;
		}

		protected abstract ShaderType GetShaderType();

		#region Util

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

		protected void SetUniform1( string name, int value )
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

		protected void SetUniformTexture( string name, int texture, TextureTarget textureTarget )
		{
			SetUniform1( name, textureUnit );
			GL.ActiveTexture( TextureUnit.Texture0 + textureUnit );
			GL.BindTexture( textureTarget, texture );
			textureUnit++;
		}

		#endregion
	}
}

