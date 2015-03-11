using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;

namespace Kaiga.ShaderStages
{
	public class GShader : ShaderStage
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
				vec4 in_Normal;
				vec4 in_Position;
			};
			
			layout( location = 0 ) out out_Normal;
			layout( location = 1 ) out out_Position;
			
			void main(void)
			{ 
				out_Normal = in_Normal;
				out_Position = in_Position
			}
			";
		}

	}
}

