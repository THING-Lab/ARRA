// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ColorRoomShader_preview"
{
	Properties
	{

	_MyArr("TextureArray", 2DArray) = "" {}

	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog
#pragma target 3.5
#include "UnityCG.cginc"
#define MAX_SIZE 100

		struct appdata
	{
		float4 vertex : POSITION;
	
	};

	struct v2f
	{
	
		float4 vertex : SV_POSITION; // clip space position
		float4 worlspace : TEXCOORD0;


		float4 vertexInProjectionSpace : TEXCOORD1;

	};

	




	float4x4 _MyObjectToWorld;
	float4x4 _WorldToCameraMatrixArray[MAX_SIZE];
	float4x4 _CameraProjectionMatrixArray[MAX_SIZE];
	float4 _vertexInProjectionSpaceArray[MAX_SIZE];
	float4 vertexPositionInCameraSpaceArray[MAX_SIZE];
	v2f vert(appdata v)
	{
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);
				
		o.worlspace = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));




		return o;
	}
	UNITY_DECLARE_TEX2DARRAY(_MyArr);
	fixed4 frag(v2f i) : SV_Target
	{


		for (int k = 0; k < MAX_SIZE; ++k)
		{
			vertexPositionInCameraSpaceArray[k] = mul(_WorldToCameraMatrixArray[k], float4(i.worlspace.xyz, 1));
			_vertexInProjectionSpaceArray[k] = mul(_CameraProjectionMatrixArray[k], float4(vertexPositionInCameraSpaceArray[k].xyz, 1.0));
		}
	for (int j = 0; j < MAX_SIZE; ++j)
	{
		if (vertexPositionInCameraSpaceArray[j].z > 0)
			continue;

		float2 projectedTex = (_vertexInProjectionSpaceArray[j].xy / _vertexInProjectionSpaceArray[j].w);
		if (abs(projectedTex.x) < 1.0 && abs(projectedTex.y) < 1.0)
		{
			float2 unitTexcoord = ((projectedTex * 0.5) + float4(0.5, 0.5, 0, 0));
			if ((unitTexcoord.x < (1024.0 / 1280.0)) && (unitTexcoord.y >(208.0 / 720.0)))
			{
				unitTexcoord.x = unitTexcoord.x *(1280.0 / 1024.0);
				unitTexcoord.y = unitTexcoord.y *(720.0 / 512.0) - (208.0 / 512.0);


				float3 zAxis = { unitTexcoord.x,unitTexcoord.y, j };
			

				return UNITY_SAMPLE_TEX2DARRAY(_MyArr, zAxis);
			}
			else
				continue;
			

		}
	}
	return fixed4(1,1,1,1);



	}
		ENDCG
	}
	}
}
