#version 450 core

//-----------------------------------------------------------
// Outputs
//-----------------------------------------------------------
layout( location = 0 ) out vec4 out_fragColor;


//-----------------------------------------------------------
// Inputs
//-----------------------------------------------------------
uniform sampler2DRect s_material;
uniform sampler2D s_positionBuffer;
uniform sampler2D s_normalBuffer;
uniform sampler2DRect s_resolveBuffer;
uniform sampler2D s_randomDirectionTexture;

uniform mat4 u_projectionMatrix;
uniform float u_roughnessJitter;
uniform float u_zDistanceMin;

const float rayStep = 0.02f;
const float minRayStep = 0.01f;
const int maxSteps = 64;
const int numBinarySearchSteps = 6;
const float distanceFalloff = rayStep * maxSteps + minRayStep;

in Varying
{
	in vec2 in_uv;
};


//-----------------------------------------------------------
// Functions
//-----------------------------------------------------------


/*
uniform sampler2D gColor;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gEffect;
uniform vec2 gTexSizeInv;
 
const float reflectionSpecularFalloffExponent = 3.0;


*/