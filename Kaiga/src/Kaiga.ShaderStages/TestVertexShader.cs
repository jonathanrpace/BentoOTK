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

			int modelViewMatrixLocation = GL.GetUniformLocation( shaderProgram, "ModelViewMatrix" );
			GL.UniformMatrix4( modelViewMatrixLocation, false, ref renderParams.ModelViewMatrix );

			int normalModelViewMatrixLocation = GL.GetUniformLocation( shaderProgram, "NormalModelViewMatrix" );
			GL.UniformMatrix3( normalModelViewMatrixLocation, false, ref renderParams.NormalModelViewMatrix );
		}

		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			// Uniforms
			uniform mat4 MVPMatrix;
			uniform mat4 ModelViewMatrix;
			uniform mat3 NormalModelViewMatrix;

			// Inputs
			layout(location = 0) in vec3 in_Position;
			layout(location = 1) in vec3 in_Normal;
			layout(location = 2) in vec4 in_Color;

			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			out Varying
			{
				vec3 out_ViewNormal;
				vec4 out_ViewPosition;
			};

			void main(void)
			{
				out_ViewNormal = normalize( in_Normal * NormalModelViewMatrix );
				
				out_ViewPosition = ModelViewMatrix * vec4(in_Position,1);

				gl_Position = MVPMatrix * vec4(in_Position, 1);
			} 
			";
		}

	}
}

