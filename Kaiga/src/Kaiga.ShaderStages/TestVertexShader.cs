using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	public class TestVertexShader : ShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.VertexShader;
		}

		override public void BindShader( RenderParams renderParams )
		{
			int mvpMatrixLocation = GL.GetUniformLocation( shaderProgram, "MVPMatrix" );
			GL.UniformMatrix4( mvpMatrixLocation, false, ref renderParams.ViewProjectionMatrix );
		}

		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			uniform mat4 MVPMatrix;

			layout(location = 0) in vec3 in_position;
			layout(location = 1) in vec3 in_normal;
			layout(location = 2) in vec3 in_uv;
			layout(location = 3) in vec4 in_color;

			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			out Varying
			{
				 vec4 Color;
			};

			void main(void)
			{
				gl_Position = MVPMatrix * vec4(in_position, 1);
				Color = in_color;
			} 
			";
		}

	}
}

