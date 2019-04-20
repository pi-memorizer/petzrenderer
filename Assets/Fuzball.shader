Shader "Unlit/Fuzball"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Noise("Noise",2D) = "black" {}
		_Fuzziness("Fuzziness",Float) = 100
		_FuzzAmplitude("FuzzAmplitude",Float) = 8
		_Up("Up",Vector) = (0,1,0,0)
		_Right("Right",Vector) = (1,0,0,0)
		_Outline("Outline",Float) = 0
		_OutlineDirection("OutlineDirection",Vector) = (0,-1,0,0)
	}
	SubShader
	{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

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
				float4 addball : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 addball : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _Noise;
			float _Fuzziness;
			float4 _MainTex_ST;
			float4 _Up;
			float4 _Right;
			float _FuzzAmplitude;
			float4 _OutlineDirection;
			float _Outline;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float2(v.uv.x, v.uv.y);
				o.addball = v.addball;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 a = float4(i.uv.x * 2 - 1, i.uv.y * 2 - 1,0,0);
				float magA = length(a);
				float v = dot(a/magA, float2(0, 1));
				a.z = sqrt(1 - magA*magA);
				if (dot(i.addball.xyz*2-1, a) < i.addball.a*2-1) discard;
				v = acos(v);
				if (a.x < 0) v -= 2 * 3.1415926;
				v = v * 127 / (2 * 3.1415926 * (127-_Fuzziness));
				v = tex2D(_Noise, float2(v, _Fuzziness / 127));
				float m = 1 - v / (127 - _Fuzziness) * _FuzzAmplitude;
				if (magA > m) { discard; };
				float d1 = dot(float4(1, 0, 0, 0), _Right);
				float d2 = dot(float4(0, 1, 0, 0), _Up);
				if (_Right.z > 0) d1 = -d1 -2;
				if (_Up.z < 0) d2 = -d2 -2;
				if (magA > m*.95&&dot(a, _OutlineDirection) >= _Outline)
					return fixed4(0, 0, 0, 1);
				else return tex2D(_MainTex, i.uv + float2(d1*.5+.5,d2*.5+.5));
			}
			ENDCG
		}
	}
}
