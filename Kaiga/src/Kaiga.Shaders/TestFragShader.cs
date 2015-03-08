using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.Shaders
{
	public class TestFragShader : Shader
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
		
		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			in Varying
			{
				 vec4 Color;
			};

			
			out vec4 out_Color;

			void main(void)
			{ 
				out_Color = Color;
			}
			";
		}

	}
}

