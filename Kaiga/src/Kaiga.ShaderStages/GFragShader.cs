using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Materials;

namespace Kaiga.ShaderStages
{
	public class GFragShader : AbstractFragmentShaderStage
	{
		public void BindPerMaterial( StandardMaterial material )
		{
			SetUniform1( "reflectivity", material.reflectivity );
			SetUniform1( "glossiness", material.glossiness );
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

			uniform float reflectivity;
			uniform float glossiness;
			
			// Outputs
			layout( location = 0 ) out vec4 out_ViewNormal;
			layout( location = 1 ) out vec4 out_ViewPosition;
			layout( location = 2 ) out vec4 out_Albedo;
			layout( location = 3 ) out vec4 out_Material;

			void main(void)
			{ 
				vec4 ViewPosition = in_ViewPosition;
				out_ViewNormal = vec4( in_ViewNormal.xyz, 1.0 );
				out_ViewPosition = ViewPosition;
				out_Albedo = in_Color;

				vec4 material = vec4( reflectivity, glossiness, 1, 1 );
				out_Material = material;
			}
			";
		}

	}
}

