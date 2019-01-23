#if !defined(NORMALS_RENDERING_PASS)
#define NORMALS_RENDERING_PASS

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
uniform float4 _NormalMap_ST;  // Is this the right way to do this?

Varyings NormalsRenderingVertex(Attributes attributes)
{
	Varyings o;
    o.positionCS = TransformObjectToHClip(attributes.positionOS);
    o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap); 
	o.uv = attributes.uv;
	o.color = attributes.color;
	o.normal = TransformObjectToWorldDir(float3(0, 0, 1));
	o.tangent = TransformObjectToWorldDir(float3(1, 0, 0));
	o.bitangent = TransformObjectToWorldDir(float3(0, 1, 0));
    return o;
}

float4 NormalsRenderingFragment(Varyings i) : SV_Target
{
	float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

	float4 normalColor;
	float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
	float3 normalWS = TransformTangentToWorld(normalTS, half3x3(i.tangent.xyz, i.bitangent.xyz, -i.normal.xyz));
	float3 normalVS = TransformWorldToViewDir(normalWS);

	normalColor.rgb = 0.5 * ((normalVS) + 1);
	normalColor.a = mainTex.a;

	return normalColor;
}

#endif
