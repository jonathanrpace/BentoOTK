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
		static private AbstractShaderStage s_activeShaderStage;

		readonly string[] shaderResourceIDs;
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

		protected int pipeline;
		public int Pipeline
		{
			set
			{
				pipeline = value;
			}
		}

		protected AbstractShaderStage( string shaderResourceID )
		{
			shaderResourceIDs = new [] { shaderResourceID };
		}
			
		protected AbstractShaderStage( params string[] shaderResourceIDs )
		{
			this.shaderResourceIDs = shaderResourceIDs;
		}

		protected AbstractShaderStage()
		{
			
		}

		public virtual void BindPerPass()
		{
			textureUnit = 0;
		}

		protected override void onValidate()
		{
			string[] shaderSource;

			if ( shaderResourceIDs == null )
			{
				shaderSource = new [] { GetShaderSource() };
			}
			else
			{
				var inlineShaderSource = GetShaderSource();

				shaderSource = new string[shaderResourceIDs.Length + (inlineShaderSource == null ? 0 : 1) ];

				for ( int i = 0; i < shaderResourceIDs.Length; i++ )
				{
					var shaderResourceID = shaderResourceIDs[i];
					var assembly = Assembly.GetExecutingAssembly();
					using (Stream stream = assembly.GetManifestResourceStream( shaderResourceID ))
					using (var reader = new StreamReader( stream ))
					{
						shaderSource[ i ] = reader.ReadToEnd();
					}
				}

				if ( inlineShaderSource != null )
				{
					shaderSource[ shaderSource.Length - 1 ] = inlineShaderSource;
				}
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

		private void SetAsActiveShader()
		{
			if ( s_activeShaderStage == this )
				return;
			s_activeShaderStage = this;
			GL.ActiveShaderProgram( pipeline, ShaderProgram );
		}

		protected virtual string GetShaderSource()
		{
			return null;
		}

		protected abstract ShaderType GetShaderType();

		#region Util

		protected void SetUniformMatrix4( string name, ref Matrix4 matrix, bool transposed = false )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.UniformMatrix4( location, transposed, ref matrix );
		}

		protected void SetUniformMatrix3( string name, ref Matrix3 matrix, bool transposed = false )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.UniformMatrix3( location, transposed, ref matrix );
		}

		protected void SetUniform1( string name, float value )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform1( location, value );
		}

		protected void SetUniform1( string name, int value )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform1( location, value );
		}

		protected void SetUniform1( string name, bool value )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform1( location, value ? 1 : 0 );
		}

		protected void SetUniform2( string name, Vector2 value )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform2( location, value );
		}

		protected void SetUniform3( string name, Vector3 value )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform3( location, value );
		}

		protected void SetUniform4( string name, Vector4 value )
		{
			SetAsActiveShader();
			int location = GL.GetUniformLocation( shaderProgram, name );
			GL.Uniform4( location, value );
		}

		protected void SetTexture( string name, int texture, TextureTarget textureTarget )
		{
			SetAsActiveShader();
			SetUniform1( name, textureUnit );
			GL.ActiveTexture( TextureUnit.Texture0 + textureUnit );
			GL.BindTexture( textureTarget, texture );
			textureUnit++;
		}

		protected void SetTexture2D( string name, int texture )
		{
			SetAsActiveShader();
			SetUniform1( name, textureUnit );
			GL.ActiveTexture( TextureUnit.Texture0 + textureUnit );
			GL.BindTexture( TextureTarget.Texture2D, texture );
			textureUnit++;
		}

		protected void SetRectangleTexture( string name, int texture )
		{
			SetAsActiveShader();
			SetUniform1( name, textureUnit );
			GL.ActiveTexture( TextureUnit.Texture0 + textureUnit );
			GL.BindTexture( TextureTarget.TextureRectangle, texture );
			textureUnit++;
		}

		protected Vector3 Degamma( Vector3 input )
		{
			var output = new Vector3();
			output.X = (float)Math.Pow( (double)input.X, 2.2 );
			output.Y = (float)Math.Pow( (double)input.Y, 2.2 );
			output.Z = (float)Math.Pow( (double)input.Z, 2.2 );
			return output;
		}

		#endregion
	}
}

