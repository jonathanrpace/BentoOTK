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
			};

			
			// Outputs
			layout( location = 0 ) out vec4 out_ViewNormal;
			layout( location = 1 ) out vec4 out_ViewPosition;

			void main(void)
			{ 
				out_ViewNormal = in_ViewNormal;
				out_ViewPosition = in_ViewPosition;
			}
			";
		}

	}
}

