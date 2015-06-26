#version 440 core

float G1V(float dotNV, float k)
{
	return 1.0f/(dotNV*(1.0f-k)+k);
}

float angularDot( vec3 A, vec3 B, in float angularSize )
{
	float dotValue = clamp( dot( A, B ), 0.0f, 1.0f );
	float pi = 3.14159f;

	float angle = 1.0f - acos(dotValue) * (2.0f / pi);
	angle = min( angle, angularSize );
	angle *= ( 1 / angularSize );

	float ret = sin( angle * pi * 0.5f );
	
	return ret;
}

float Fresnel( float dotLH, float F0 )
{
	float dotLH5 = pow(1.0f-dotLH,5);
	float F = F0 + (1.0-F0)*(dotLH5);
	return F;
}

float LightingFuncGGXAngular( vec3 N, vec3 V, vec3 L, float roughness, float F0, float angularSize )
{
	float alpha = roughness*roughness;

	vec3 H = normalize(V+L);

	float dotNL = angularDot(N,L,angularSize);
	float dotNV = clamp(dot(N,V), 0.0f, 1.0f);
	float dotNH = angularDot(N,H,angularSize);
	float dotLH = clamp(dot(L,H), 0.0f, 1.0f);

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	float D = alphaSqr/(pi * denom * denom);

	// F
	float F = Fresnel(dotLH, F0);

	// V
	float k = alpha/2.0f;
	float vis = G1V(dotNL,k)*G1V(dotNV,k);

	return min( 100.0f, dotNL * D * F * vis );
}

float LightingFuncGGX( vec3 N, vec3 V, vec3 L, float roughness, float F0 )
{
	float alpha = roughness*roughness;

	vec3 H = normalize(V+L);

	float dotNL = clamp(dot(N,L), 0.0f, 1.0f);
	float dotNV = clamp(dot(N,V), 0.0f, 1.0f);
	float dotNH = clamp(dot(N,H), 0.0f, 1.0f);
	float dotLH = clamp(dot(L,H), 0.0f, 1.0f);

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	float D = alphaSqr/(pi * denom * denom);

	// F
	float F = Fresnel(dotLH, F0);

	// V
	float k = alpha/2.0f;
	float vis = G1V(dotNL,k)*G1V(dotNV,k);

	return min( 100.0f, dotNL * D * F * vis );
}

