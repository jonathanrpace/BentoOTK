using OpenTK.Graphics.OpenGL4;

namespace Kaiga.ShaderStages
{
	public class BoxBlurFragShader : AbstractFragmentShaderStage
	{
		public void Bind( int texture )
		{
			SetUniformTexture( 0, "s_texture", texture, TextureTarget.TextureRectangle );
		}

		protected override string GetShaderSource()
		{
			return @"

#version 450 core

// Samplers
uniform sampler2DRect s_texture;

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

void main(void)
{ 
	vec4 sampleA = texture2DRect( s_texture, gl_FragCoord.xy - vec2( 0.5f, 0.0f ) );
	vec4 sampleB = texture2DRect( s_texture, gl_FragCoord.xy + vec2( 0.5f, 0.0f ) );
	
	out_fragColor = (sampleA + sampleB) * 0.5f;
}

";
		}
	}
}

