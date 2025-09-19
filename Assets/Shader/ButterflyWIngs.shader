Shader "Custom/OpacityMaskWithNormals" {
    Properties {
        _MainTex ("Base Texture (RGB)", 2D) = "white" {}
        _OpacityMask ("Opacity Mask (R)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _OpacityStrength ("Opacity Strength", Range(0,1)) = 1.0
        _BumpScale ("Normal Strength", Range(0,2)) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }
    
    SubShader {
        Tags {
            "Queue"="Overlay+1000" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        
        LOD 200
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_Cull]
        
        Pass {
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"
            
            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };
            
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBinormal : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            
            sampler2D _MainTex;
            sampler2D _OpacityMask;
            sampler2D _BumpMap;
            float _OpacityStrength;
            float _BumpScale;
            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            
            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Calculate tangent space vectors
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                o.worldBinormal = normalize(cross(o.worldNormal, o.worldTangent) * v.tangent.w * unity_WorldTransformParams.w);
                
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // Sample textures
                fixed4 col = tex2D(_MainTex, i.texcoord);
                fixed4 mask = tex2D(_OpacityMask, i.texcoord);
                fixed3 normalTex = UnpackNormal(tex2D(_BumpMap, TRANSFORM_TEX(i.texcoord, _BumpMap)));
                
                // Scale normal map intensity
                normalTex.xy *= _BumpScale;
                
                // Transform normal from tangent space to world space
                float3x3 tangentToWorld = float3x3(
                    i.worldTangent,
                    i.worldBinormal,
                    i.worldNormal
                );
                
                float3 worldNormal = normalize(mul(normalTex, tangentToWorld));
                
                // Handle double-sided normals
                float facing = dot(normalize(i.worldPos - _WorldSpaceCameraPos), worldNormal);
                if (facing > 0) {
                    worldNormal = -worldNormal;
                }
                
                // Basic lighting calculation
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = max(0, dot(worldNormal, lightDir));
                
                // Get shadow attenuation
                float attenuation = LIGHT_ATTENUATION(i);
                
                // Ambient + diffuse lighting
                fixed3 ambient = ShadeSH9(float4(worldNormal, 1));
                fixed3 diffuse = _LightColor0.rgb * NdotL * attenuation;
                
                // Final color with lighting
                fixed3 lighting = ambient + diffuse;
                col.rgb *= lighting;
                
                // Apply opacity mask
                float opacity = mask.r * _OpacityStrength;
                col.a *= opacity;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
