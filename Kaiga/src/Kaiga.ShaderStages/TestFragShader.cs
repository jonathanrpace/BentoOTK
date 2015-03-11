using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	public class TestFragShader : ShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
		
		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			// Inputs
			in Varying
			{
				vec4 in_ViewNormal;
				vec4 in_ViewPosition;
				vec4 in_Color;
			};

			
			// Outputs
			layout( location = 0 ) out vec4 out_ViewNormal;
			layout( location = 1 ) out vec4 out_ViewPosition;
			layout( location = 2 ) out vec4 out_Albedo;

			void main(void)
			{ 
				vec4 ViewPosition = in_ViewPosition;
				ViewPosition.z = -ViewPosition.z;

				out_ViewNormal = in_Color;
				out_ViewPosition = ViewPosition;
				out_Albedo = in_Color;
			}
			";
		}

	}
}

