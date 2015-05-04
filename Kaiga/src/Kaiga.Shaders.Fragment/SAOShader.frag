#version 450 core


void main()
{
	// tau[N-1] = optimal number of spiral turns for N samples
	const int tauArray[ ] = {1, 1, 2, 3, 2, 5, 2, 3, 2, 3, 3, 5, 5,
	3, 4, 7, 5, 5, 7, 9, 8, 5, 5, 7, 7, 7, 8, 5, 8, 11, 12, 7,
	10, 13, 8, 11, 8, 7, 14, 11, 11, 13, 12, 13, 19, 17, 13,
	11, 18, 19, 11, 11, 14, 17, 21, 15, 16, 17, 18, 13, 17,
	11, 17, 19, 18, 25, 18, 19, 19, 29, 21, 19, 27, 31, 29, 21,
	18, 17, 29, 31, 31, 23, 18, 25, 26, 25, 23, 19, 34, 19, 27,
	21, 25, 39, 29, 17, 21, 27};

	const int numSamples = 64;
	const float radius = 0.5f;		// The total screen space radius to applied the effect
	const float PI = 3.142f;
	const in tau = tauArray[numSamples];
	const float q = 0.1f;	// The radius at which mip levels start to increase.


	vec3 fragPos = sampleFromPositionBuffer();
	vec3 fragNormal = sampleFromNormalBuffer();

	float angle = sampleFromRandomTexture();
	float aoTotal = 0.0f;
	for ( int i = 0; i < numSamples; i++ )
	{
		float theta = (1.0f / numSamples) * (i + 0.5f);
		float currAngle = 2.0f * PI * theta * tau + angle;

		vec2 offset = vec2( cos( currAngle ), sin( currAngle ) );
		float h = radius * theta;

		vec2 sampleUV = fragCoord + offset * h;
		float mipLevel = abs( log2( h / q ) );

		vec3 samplePosition = textureLod( sampleUV, mipLevel );

		float ao = AO( fragPos, fragNormal, samplePosition );

		aoTotal += ao;
	}

	const float divisor = PI / numSamples;
	aoTotal *= divisor;

	aoTotal = max( 0.0f, 1.0f - sqrt( aoTotal ) );


}

float AO( vec3 fragPos, vec3 fragNormal, vec3 samplePos )
{
	vec3 v = samplePos - fragPos;
	float vLenSquared = dot( v, v );
	float falloff = 1.0f - ( vLenSquared / ( radius * radius ) );

	const float bias = 0.01f;
	const float epsilon = 0.0001f;

	float ao = falloff * max( ( dot( v, fragNormal ) - bias / sqrt(vLenSquared + epsilon), 0.0f );

	return ao;
}

