using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class RenderBufferDownsampleFragShader : AbstractFragmentShaderStage
	{
		public void SetTexture( int texture )
		{
			SetUniformTexture( "s_tex", texture, TextureTarget.Texture2D );
			SetUniform1( "u_radius", 0.1f );
		}

		protected override string GetShaderSource()
		{
			return @"
#version 450 core

// Inputs
in Varying
{
	vec2 in_uv;
};

uniform sampler2D s_tex;
uniform float u_radius;

layout( location = 0 ) out vec4 out_fragColor;

void main()
{
	ivec2 texSize = textureSize( s_tex, 0 );
	float aspectRatio = texSize.x / texSize.y;

	//vec2 uv = (gl_FragCoord.xy / u_outputSize);
	//uv.y = 1.0-uv.y;
 	//uv *= vec2( texSize );

	vec2 uv = in_uv;

	vec4 sampleLeft 	= texture( s_tex, uv + vec2( -u_radius, 0.0f ) );
	vec4 sampleRight 	= texture( s_tex, uv + vec2( u_radius, 0.0f ) );
	vec4 sampleTop 		= texture( s_tex, uv + vec2( 0.0f, -u_radius ) );
	vec4 sampleBottom 	= texture( s_tex, uv + vec2( 0.0f, u_radius ) );

	out_fragColor = vec4(1.0,0.0,0.0,1.0);//( sampleLeft + sampleRight + sampleTop + sampleBottom ) / 4;
}

";
		}
	}
}

