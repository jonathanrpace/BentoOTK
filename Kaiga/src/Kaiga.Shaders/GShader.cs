using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Materials;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class GShader : AbstractShader<GVertexShader, GFragShader>
	{
		public void BindPerMaterial( StandardMaterial material )
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			//vertexShader.BindPerMaterial( material );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerMaterial( material );
		}

		public void BindPerModel()
		{
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerModel();

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			//fragmentShader.BindPerModel();
		}
	}

	public class GVertexShader : AbstractVertexShaderStage
	{
		public void BindPerModel()
		{
			SetUniformMatrix4( "MVPMatrix", ref RenderParams.ModelViewProjectionMatrix );
			SetUniformMatrix4( "ModelViewMatrix", ref RenderParams.ModelViewMatrix );
			SetUniformMatrix3( "NormalModelViewMatrix", ref RenderParams.NormalModelViewMatrix );
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

	public class GFragShader : AbstractFragmentShaderStage
	{
		public void BindPerMaterial( StandardMaterial material )
		{
			SetUniform1( "u_reflectivity", material.Reflectivity );
			SetUniform1( "u_roughness", material.Roughness );
			SetUniform1( "u_emissive", material.Emissive );
			SetUniform3( "u_albedo", Degamma(material.Diffuse) );
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
			layout( location = 4 ) out vec4 out_DirectLight;

			void main(void)
			{ 
				vec4 ViewPosition = in_ViewPosition;
				out_ViewNormal = vec4( in_ViewNormal.xyz, 1.0f );
				out_ViewPosition = ViewPosition;
				out_Albedo = vec4( u_albedo, 1.0f );
				out_DirectLight = vec4( u_albedo * u_emissive, 1.0f );

				vec4 material = vec4( u_roughness, u_reflectivity, u_emissive, 1 );
				out_Material = material;
			}
			";
		}

	}
}

