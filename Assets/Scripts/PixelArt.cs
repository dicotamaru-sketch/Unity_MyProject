using UnityEngine;

// ピクセルアートスプライトをコードで生成するユーティリティ
public static class PixelArt
{
    // 色定数
    static Color _ = Color.clear;
    static Color W = Color.white;
    static Color K = Color.black;

    static Color C(float r, float g, float b) => new Color(r / 255f, g / 255f, b / 255f);

    // ===== プレイヤー（16x16） =====
    public static Sprite CreatePlayer()
    {
        Color S = C(255, 80,  20);  // 赤オレンジ（体）
        Color H = C(180, 50,  10);  // 濃い赤（影）
        Color F = C(255, 200, 150); // 肌色（顔）
        Color B = C(60,  40,  20);  // 茶（ひげ・帽子）
        Color R = C(220, 30,  30);  // 赤（帽子）
        Color O = C(100, 60,  20);  // 茶（ズボン）

        Color[] p = {
            _,_,_,R,R,R,R,R,R,R,_,_,_,_,_,_,
            _,_,R,R,R,R,R,R,R,R,R,R,R,_,_,_,
            _,_,B,B,B,F,F,F,F,F,_,_,_,_,_,_,
            _,B,F,F,B,F,F,F,B,F,F,F,_,_,_,_,
            _,B,F,F,F,F,F,F,F,F,F,B,B,_,_,_,
            _,_,B,B,F,F,F,F,F,F,B,B,_,_,_,_,
            _,_,_,_,F,F,B,B,F,F,_,_,_,_,_,_,
            _,_,_,S,S,S,S,S,S,_,_,_,_,_,_,_,
            _,_,S,S,S,S,S,S,S,S,_,_,_,_,_,_,
            _,O,O,S,S,W,W,S,S,O,O,_,_,_,_,_,
            _,O,O,O,S,W,W,S,O,O,O,_,_,_,_,_,
            _,O,O,O,O,_,_,O,O,O,O,_,_,_,_,_,
            _,O,O,_,_,_,_,_,_,O,O,_,_,_,_,_,
            _,H,H,_,_,_,_,_,_,H,H,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== 地上敵（クリボー風、16x16） =====
    public static Sprite CreateGroundEnemy()
    {
        Color E = C(120, 60,  10);  // 茶（体）
        Color D = C(60,  30,   5);  // 濃い茶（影）
        Color F = C(200, 140, 80);  // 肌（足）

        Color[] p = {
            _,_,_,E,E,E,E,E,E,E,_,_,_,_,_,_,
            _,_,E,E,E,E,E,E,E,E,E,_,_,_,_,_,
            _,E,E,E,E,E,E,E,E,E,E,E,_,_,_,_,
            _,E,K,K,E,E,E,E,K,K,E,E,_,_,_,_,
            _,E,K,W,E,E,E,E,K,W,E,E,_,_,_,_,
            _,E,E,E,E,E,E,E,E,E,E,E,_,_,_,_,
            _,E,E,D,D,D,D,D,D,D,E,E,_,_,_,_,
            _,_,E,D,D,D,D,D,D,D,E,_,_,_,_,_,
            _,_,E,E,E,E,E,E,E,E,E,_,_,_,_,_,
            _,_,E,E,E,E,E,E,E,E,E,_,_,_,_,_,
            _,F,F,E,E,_,_,E,E,F,F,_,_,_,_,_,
            F,F,F,F,E,_,_,E,F,F,F,F,_,_,_,_,
            F,F,F,F,_,_,_,_,F,F,F,F,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== 浮遊敵（コウモリ風、16x16） =====
    public static Sprite CreateFlyEnemy()
    {
        Color V = C(120, 20, 180);  // 紫（体）
        Color D = C(60,  10,  90);  // 濃い紫
        Color R = C(220, 30,  30);  // 赤（目）
        Color G = C(180, 180, 200); // 灰（羽）

        Color[] p = {
            G,_,_,_,_,_,_,_,_,_,_,_,G,_,_,_,
            G,G,_,_,_,_,_,_,_,_,_,G,G,_,_,_,
            G,G,G,_,V,V,V,V,V,V,_,G,G,_,_,_,
            G,G,G,V,V,V,V,V,V,V,V,G,G,_,_,_,
            _,G,G,V,R,K,V,V,R,K,V,G,G,_,_,_,
            _,G,V,V,V,V,V,V,V,V,V,V,G,_,_,_,
            _,_,V,V,D,D,D,D,D,D,V,V,_,_,_,_,
            _,_,V,V,V,K,K,K,K,V,V,V,_,_,_,_,
            _,_,_,V,V,V,V,V,V,V,V,_,_,_,_,_,
            _,_,V,V,_,V,V,V,V,_,V,V,_,_,_,_,
            _,V,V,_,_,_,_,_,_,_,_,V,V,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== 地面タイル（16x16） =====
    public static Sprite CreateGround()
    {
        Color G = C(80,  160, 60);   // 草（上部）
        Color D = C(140, 100, 50);   // 土
        Color L = C(100, 70,  30);   // 暗い土
        Color H = C(180, 140, 80);   // 明るい土

        Color[] p = {
            G,G,G,G,G,G,G,G,G,G,G,G,G,G,G,G,
            G,G,G,G,G,G,G,G,G,G,G,G,G,G,G,G,
            G,G,H,G,G,G,G,G,G,G,G,H,G,G,G,G,
            D,D,D,D,D,D,D,D,D,D,D,D,D,D,D,D,
            D,H,D,D,L,D,D,D,H,D,D,D,L,D,D,D,
            D,D,D,L,D,D,D,D,D,D,L,D,D,D,D,D,
            D,D,D,D,D,H,D,D,D,D,D,D,D,H,D,D,
            L,D,D,D,D,D,D,L,D,D,D,D,D,D,D,L,
            D,D,H,D,D,D,D,D,D,H,D,D,D,D,D,D,
            D,D,D,D,L,D,D,D,D,D,D,L,D,D,D,D,
            D,D,D,D,D,D,H,D,D,D,D,D,D,D,H,D,
            L,D,D,D,D,D,D,D,L,D,D,D,D,D,D,L,
            D,D,D,H,D,D,D,D,D,D,H,D,D,D,D,D,
            D,D,D,D,D,L,D,D,D,D,D,D,L,D,D,D,
            D,H,D,D,D,D,D,H,D,D,D,D,D,D,H,D,
            D,D,D,D,D,D,D,D,D,D,D,D,D,D,D,D,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== ブロックタイル（16x16） =====
    public static Sprite CreateBlock()
    {
        Color B = C(180, 120, 50);  // レンガ色
        Color D = C(120,  70, 20);  // 暗いレンガ
        Color H = C(220, 170, 90);  // 明るいレンガ
        Color M = C(80,   50, 15);  // 目地

        Color[] p = {
            H,H,B,B,B,B,B,B,M,H,H,B,B,B,B,M,
            H,B,B,B,B,B,B,B,M,H,B,B,B,B,B,M,
            B,B,B,B,B,B,B,B,M,B,B,B,B,B,B,M,
            D,D,D,D,D,D,D,D,M,D,D,D,D,D,D,M,
            M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,
            B,B,B,M,H,H,B,B,B,B,M,H,H,B,B,M,
            B,B,B,M,H,B,B,B,B,B,M,H,B,B,B,M,
            B,B,B,M,B,B,B,B,B,B,M,B,B,B,B,M,
            D,D,D,M,D,D,D,D,D,D,M,D,D,D,D,M,
            M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,
            H,H,B,B,B,B,B,B,M,H,H,B,B,B,B,M,
            H,B,B,B,B,B,B,B,M,H,B,B,B,B,B,M,
            B,B,B,B,B,B,B,B,M,B,B,B,B,B,B,M,
            D,D,D,D,D,D,D,D,M,D,D,D,D,D,D,M,
            M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,
            M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,M,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== パイプ（16x16） =====
    public static Sprite CreatePipe()
    {
        Color G = C(60,  180, 60);   // 緑
        Color L = C(30,  120, 30);   // 暗い緑
        Color H = C(140, 230, 140);  // 明るい緑
        Color K2 = C(20, 80,  20);   // 最暗緑

        Color[] p = {
            L,G,G,H,H,G,G,G,G,G,G,G,L,_,_,_,
            L,G,H,H,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,H,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            L,G,G,G,G,G,G,G,G,G,G,G,L,_,_,_,
            K2,L,L,L,L,L,L,L,L,L,L,L,K2,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== コイン（16x16） =====
    public static Sprite CreateCoin()
    {
        Color Y = C(255, 210, 0);   // 金色
        Color H = C(255, 240, 100); // 明るい金
        Color D = C(180, 130, 0);   // 暗い金

        Color[] p = {
            _,_,_,_,Y,Y,Y,Y,Y,Y,_,_,_,_,_,_,
            _,_,_,Y,Y,H,H,H,Y,Y,Y,_,_,_,_,_,
            _,_,Y,Y,H,H,Y,Y,H,Y,Y,Y,_,_,_,_,
            _,_,Y,H,H,Y,Y,Y,Y,H,Y,Y,_,_,_,_,
            _,_,Y,H,Y,Y,Y,Y,Y,Y,D,Y,_,_,_,_,
            _,_,Y,Y,Y,Y,Y,Y,Y,Y,D,Y,_,_,_,_,
            _,_,Y,Y,Y,Y,Y,Y,Y,D,D,Y,_,_,_,_,
            _,_,Y,Y,Y,Y,Y,Y,Y,D,Y,Y,_,_,_,_,
            _,_,Y,Y,Y,Y,Y,Y,D,D,Y,Y,_,_,_,_,
            _,_,Y,Y,Y,Y,Y,Y,D,Y,Y,Y,_,_,_,_,
            _,_,Y,Y,Y,Y,Y,D,Y,Y,Y,Y,_,_,_,_,
            _,_,Y,Y,H,Y,D,D,Y,H,Y,Y,_,_,_,_,
            _,_,Y,Y,Y,D,D,Y,Y,Y,Y,Y,_,_,_,_,
            _,_,_,Y,Y,Y,Y,Y,Y,Y,Y,_,_,_,_,_,
            _,_,_,_,Y,Y,Y,Y,Y,Y,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== ゴール旗（16x16） =====
    public static Sprite CreateGoal()
    {
        Color P = C(200, 200, 200); // 旗竿（銀）
        Color F = C(255, 80,  0);   // 旗（オレンジ）
        Color H = C(255, 160, 80);  // 旗の明部
        Color S = C(255, 220, 0);   // 星（黄）

        Color[] p = {
            _,P,F,F,F,F,F,F,F,F,_,_,_,_,_,_,
            _,P,F,H,H,H,F,F,F,F,_,_,_,_,_,_,
            _,P,F,H,S,H,F,F,F,F,_,_,_,_,_,_,
            _,P,F,H,H,H,F,F,F,F,_,_,_,_,_,_,
            _,P,F,F,F,F,F,F,F,F,_,_,_,_,_,_,
            _,P,F,F,F,F,F,F,F,_,_,_,_,_,_,_,
            _,P,F,F,F,F,F,F,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,P,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
            P,P,P,_,_,_,_,_,_,_,_,_,_,_,_,_,
            _,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,
        };
        return MakeSprite(p, 16, 16);
    }

    // ===== 弾（8x8） =====
    public static Sprite CreateBullet()
    {
        Color O = C(255, 140, 0);
        Color Y = C(255, 240, 0);

        Color[] p = {
            _,_,O,O,O,O,_,_,
            _,O,O,Y,Y,O,O,_,
            O,O,Y,Y,Y,Y,O,O,
            O,O,Y,Y,Y,Y,O,O,
            O,O,Y,Y,Y,Y,O,O,
            O,O,Y,Y,Y,O,O,_,
            _,O,O,O,O,O,_,_,
            _,_,O,O,O,_,_,_,
        };
        return MakeSprite(p, 8, 8);
    }

    // ===== ピクセルデータからSprite生成 =====
    public static Sprite MakeSprite(Color[] pixels, int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point; // ピクセルアートらしくシャープに
        tex.wrapMode = TextureWrapMode.Clamp;

        // Unityのテクスチャは下から上なので反転
        Color[] flipped = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                flipped[y * w + x] = pixels[(h - 1 - y) * w + x];

        tex.SetPixels(flipped);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), w);
    }
}
