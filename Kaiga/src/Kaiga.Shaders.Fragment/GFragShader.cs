using Kaiga.Materials;

namespace Kaiga.Shaders.Fragment
{
	public class GFragShader : AbstractFragmentShaderStage
	{
		public void BindPerMaterial( StandardMaterial material )
		{
			SetUniform1( "u_reflectivity", material.Reflectivity );
			SetUniform1( "u_roughness", material.Roughness );
			SetUniform1( "u_emissive", material.Emissive );
			SetUniform3( "u_albedo", material.Diffuse );
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

			uniform float u_roughness;
			uniform float u_reflectivity;
			uniform float u_emissive;
			uniform vec3 u_albedo;

			// Outputs
			layout( location = 0 ) out vec4 out_ViewPosition;
			layout( location = 1 ) out vec4 out_ViewNormal;
			layout( location = 2 ) out vec4 out_Albedo;
			layout( location = 3 ) out vec4 out_Material;

			void main(void)
			{ 
				vec4 ViewPosition = in_ViewPosition;
				out_ViewNormal = vec4( in_ViewNormal.xyz, 1.0f );
				out_ViewPosition = ViewPosition;
				out_Albedo = vec4( u_albedo, 1.0f );

				vec4 material = vec4( u_roughness, u_reflectivity, u_emissive, 1 );
				out_Material = material;
			}
			";
		}

	}
}
