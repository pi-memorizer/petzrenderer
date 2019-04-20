Shader "Unlit/Eyeball"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "blue" {}
		_Eyelid("Eyelid",Vector) = (0,0,0,0)
		_Direction("Pupil",Vector) = (0,0,0,0)
		_Up("Up",Vector) = (0,1,0,0)
		_Right("Right",Vector) = (1,0,0,0)
		_Closedness("Closedness",Float) = 0
		_Pupil("Pupil",Float) = 0
		_Iris("Iris",Float) = 0
		_EyelidColor("EyelidColor",Color) = (0.5,0.5,0.5,1)
	}
	SubShader
	{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Eyelid;
			float4 _Direction;
			float4 _Up;
			float4 _Right;
			fixed4 _EyelidColor;
			float _Closedness;
			float _Iris;
			float _Pupil;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 a = float4(i.uv.x * 2 - 1, i.uv.y * 2 - 1,0,0);
				float magA = length(a);
				if (magA > 1) discard;
				a.z = sqrt(1 - magA*magA);
				float ba = dot(_Eyelid,a);
				if (ba >= _Closedness)
				{
					return _EyelidColor;
				}
				else {
					ba = dot(_Direction, a);
					if (ba >= _Pupil)
						return fixed4(0, 0, 0, 1);
					else if (ba >= _Iris)
					{
						fixed2 uv = fixed2(dot(_Right, a) / 2 + .5, dot(_Up, a) / 2 + .5);
						return tex2D(_MainTex,uv);
					}
					else
						return fixed4(1, 1, 1, 1);
				}
			}
			ENDCG
		}
	}
}
