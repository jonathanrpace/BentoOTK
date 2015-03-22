using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	public class GVertexShader : AbstractVertexShaderStage
	{
		public void BindPerModel( RenderParams renderParams )
		{
			SetUniformMatrix4( "MVPMatrix", ref renderParams.ModelViewProjectionMatrix );
			SetUniformMatrix4( "ModelViewMatrix", ref renderParams.ModelViewMatrix );
			SetUniformMatrix3( "NormalModelViewMatrix", ref renderParams.NormalModelViewMatrix );
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
			layout(location = 2) in vec4 in_Uv;
			layout(location = 3) in vec4 in_Color;

			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			out Varying
			{
				vec3 out_ViewNormal;
				vec4 out_ViewPosition;
				vec4 out_Color;
			};

			void main(void)
			{
				out_ViewNormal = normalize( in_Normal * NormalModelViewMatrix );
				out_ViewPosition = ModelViewMatrix * vec4(in_Position,1);
				out_Color = in_Color;

				gl_Position = MVPMatrix * vec4(in_Position, 1);
			} 
			";
		}

	}
}