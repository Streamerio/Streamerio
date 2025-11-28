Shader "Custom/IrisOutShader"
{
  Properties
  {
      _AspectRatio ("Width per Heigth", Float) = 1.0
      _CenterUV ("Center UV", Vector) = (0.5, 0.5, 0, 0)
      _Radius ("Radius", Float) = 0
  }
  CGINCLUDE
  float _AspectRatio;
  float4 _CenterUV;
  float _Radius;
  
  float4 paint(float2 uv)
  {
    // [0, 1]の範囲を[-1, 1]に変換（ピクセル座標を中心に調整）
    float2 normalizedCoord = (uv-_CenterUV.xy) * 2.0;
    
    // アスペクト比を適用して縦方向のスケーリング
    normalizedCoord.y /= _AspectRatio;
    
    // 中心からの距離を計算
    float distanceFromCenter = length(normalizedCoord);

    // 半径でステップ関数を使い、円内なら白、それ以外は透明
    float transparency = step(distanceFromCenter, _Radius);
    
    // アルファ値に基づいて黒色を返す（透明部分のアルファを0に設定）
    return float4(0, 0, 0, 1-transparency);
  }
  ENDCG
  
  SubShader
  {
    Tags { "RenderType"="Transparent" }
    LOD 100
    
    // アルファブレンド（通常の透明度設定）
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
        float2 texcoord : TEXCOORD0;
      };
        
      struct fin
      {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
      };

      fin vert(appdata v)
      {
        fin o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = v.texcoord;
        return o;
      }

      float4 frag(fin IN) : SV_TARGET
      {
        return paint(IN.texcoord.xy);
      }
      ENDCG
    }
  }
  Fallback "Diffuse"
}