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
		public void BindPerLight( RenderParams renderParams, PointLight pointLight )
		{
			ActivateVertexShader();
			vertexShader.BindPerLight( renderParams );

			ActivateFragmentShader();
			fragmentShader.BindPerLight( renderParams, pointLight );
		}
	}

	public class PointLightStencilShader : AbstractShader<PointLightVertexShader, NullFragShader>
	{
		public void BindPerLight( RenderParams renderParams )
		{
			ActivateVertexShader();
			vertexShader.BindPerLight( renderParams );
		}
	}

	public class PointLightVertexShader : AbstractVertexShaderStage
	{
		public void BindPerLight( RenderParams renderParams )
		{
			SetUniformMatrix4( "u_mvpMat", ref renderParams.ModelViewProjectionMatrix );
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
		public PointLightFragShader() : base( "LightingLib.frag", "PointLightShader.frag" )
		{
		}

		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );

			SetTexture( "s_positionBuffer", renderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_albedoBuffer", renderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight( RenderParams renderParams, PointLight light )
		{
			SetUniform1( "u_attenuationRadius", light.AttenuationRadius );
			SetUniform1( "u_attenuationPower", light.AttenuationPower );
			SetUniform1( "u_radius", light.Radius );
			SetUniform3( "u_color", light.Color );

			// Scale intensity by surface area
			var surfaceArea = 4.0f * (float)Math.PI * light.Radius * light.Radius;
			var intensity = surfaceArea * light.Intensity;
			SetUniform1( "u_intensity", intensity );
			SetUniform3( "u_lightPosition", renderParams.ModelViewMatrix.ExtractTranslation() );
		}

	}
}

