using System;
using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	public class PointLightShader : AbstractShader<PointLightVertexShader, PointLightFragShader>
	{
		public void BindPerLight(PointLight pointLight )
		{
			vertexShader.BindPerLight();
			fragmentShader.BindPerLight( pointLight );
		}
	}

	public class PointLightStencilShader : AbstractShader<PointLightVertexShader, NullFragShader>
	{
		public void BindPerLight()
		{
			vertexShader.BindPerLight();
		}
	}

	public class PointLightVertexShader : AbstractVertexShaderStage
	{
		public void BindPerLight()
		{
			SetUniformMatrix4( "u_mvpMat", ref RenderParams.ModelViewProjectionMatrix );
		}

		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			// Uniforms
			uniform mat4 u_mvpMat;

			// Inputs
			layout(location = 0) in vec3 in_position;

			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			void main(void)
			{
				gl_Position = u_mvpMat * vec4(in_position, 1);
			} 
			";
		}
	}

	public class PointLightFragShader : AbstractFragmentShaderStage
	{
		public PointLightFragShader() : base( "LightingLib.frag", "PointLight.frag" )
		{
		}

		override public void BindPerPass()
		{
			base.BindPerPass();

			SetTexture( "s_positionBuffer", RenderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_materialBuffer", RenderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_albedoBuffer", RenderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_normalBuffer", RenderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight( PointLight light )
		{
			SetUniform1( "u_attenuationRadius", light.AttenuationRadius );
			SetUniform1( "u_attenuationPower", light.AttenuationPower );
			SetUniform1( "u_radius", light.Radius );
			SetUniform3( "u_color", light.Color );

			// Scale intensity by surface area
			var surfaceArea = 4.0f * (float)Math.PI * light.Radius * light.Radius;
			var intensity = surfaceArea * light.Intensity;
			SetUniform1( "u_intensity", intensity );
			SetUniform3( "u_lightPosition", RenderParams.ModelViewMatrix.ExtractTranslation() );
		}

	}
}

