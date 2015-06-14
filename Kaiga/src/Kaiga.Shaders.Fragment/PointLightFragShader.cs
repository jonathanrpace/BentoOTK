using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;
using OpenTK;

namespace Kaiga.Shaders.Fragment
{
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

