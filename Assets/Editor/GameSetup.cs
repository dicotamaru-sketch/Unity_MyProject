using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class GameSetup : Editor
{
    [MenuItem("ゲーム設定/シーンを自動セットアップ（マリオ風）")]
    static void SetupScene()
    {
        // レイヤー・タグ設定
        SetupLayer("Ground", 6);
        SetupLayer("Enemy", 7);
        SetupLayer("Coin", 8);
        SetupTag("Ground");
        SetupTag("Block");
        SetupTag("Player");
        SetupTag("Enemy");

        // 既存オブジェクトをクリア
        ClearOldObjects();

        // 背景色（空色）
        Camera.main.backgroundColor = new Color(0.29f, 0.56f, 0.89f);
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.orthographicSize = 6f; // 少し広めに

        // ---- レベルデザイン基準 ----
        // 地面Y=-4（上面Y=-3.5）
        // gravityScale=1.8, jumpForce=16 → 最大ジャンプ高さ≒7.3ユニット
        // 低段：上面Y=-1.0（地面から2.5上）楽に乗れる
        // 中段：上面Y= 1.0（地面から4.5上）しっかりジャンプで届く
        // 高段：上面Y= 2.5（地面から6.0上）ギリギリ届くやりがいある高さ

        CreateBlock("Ground", new Vector3(60, -4, 0), new Vector3(140, 1, 1), new Color(0.3f, 0.7f, 0.3f), "Ground", 6, false);

        // ---- 段差ブロック ----
        CreateBlock("Block1",  new Vector3(7,   -1.5f, 0), new Vector3(3, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false); // 低段
        CreateBlock("Block2",  new Vector3(12,   0.5f, 0), new Vector3(2, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false); // 中段
        CreateBlock("Block3",  new Vector3(17,   2.0f, 0), new Vector3(3, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false); // 高段
        CreateBlock("Block4",  new Vector3(23,  -1.5f, 0), new Vector3(2, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block5",  new Vector3(28,   0.5f, 0), new Vector3(3, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block6",  new Vector3(34,   2.0f, 0), new Vector3(2, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block7",  new Vector3(40,  -1.5f, 0), new Vector3(4, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block8",  new Vector3(47,   0.5f, 0), new Vector3(2, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block9",  new Vector3(52,   2.0f, 0), new Vector3(3, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block10", new Vector3(58,  -1.5f, 0), new Vector3(2, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block11", new Vector3(64,   0.5f, 0), new Vector3(4, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);
        CreateBlock("Block12", new Vector3(72,   2.0f, 0), new Vector3(2, 1, 1), new Color(0.75f, 0.52f, 0.2f), "Block", 6, false);

        // ---- パイプ ----
        CreateBlock("Pipe1", new Vector3(20, -2.5f, 0), new Vector3(2, 3.0f, 1), new Color(0.1f, 0.75f, 0.2f), "Block", 6, false); // 上面Y=-1.0
        CreateBlock("Pipe2", new Vector3(44, -2.5f, 0), new Vector3(2, 3.0f, 1), new Color(0.1f, 0.75f, 0.2f), "Block", 6, false);
        CreateBlock("Pipe3", new Vector3(68, -2.0f, 0), new Vector3(2, 4.0f, 1), new Color(0.1f, 0.75f, 0.2f), "Block", 6, false); // 上面Y= 0.0

        // ---- プレイヤー ----
        GameObject player = SetupPlayer();

        // ---- 地上の敵（地面上面Y=-3.5の上に配置、X位置にランダム幅）----
        float[] groundEnemyX = { 9f, 15f, 25f, 33f, 42f, 50f, 60f, 70f, 78f, 85f };
        for (int i = 0; i < groundEnemyX.Length; i++)
        {
            float rx = groundEnemyX[i] + Random.Range(-1.5f, 1.5f);
            CreateEnemy("Enemy" + (i + 1), new Vector3(rx, -3f, 0), false);
        }

        // ---- 浮遊敵（高段ブロックより少し上を飛ぶ）----
        float[] flyEnemyX = { 11f, 30f, 48f, 65f, 80f };
        float[] flyEnemyBaseY = { 2f, 3f, 2f, 3f, 2f };
        for (int i = 0; i < flyEnemyX.Length; i++)
        {
            float rx = flyEnemyX[i] + Random.Range(-2f, 2f);
            float ry = flyEnemyBaseY[i] + Random.Range(-0.3f, 0.3f);
            CreateEnemy("FlyEnemy" + (i + 1), new Vector3(rx, ry, 0), true);
        }

        // ---- コイン（各ブロック上面+0.6の位置）----
        CreateCoin("Coin1",  new Vector3(7,   -0.9f, 0)); // Block1上
        CreateCoin("Coin2",  new Vector3(8,   -0.9f, 0));
        CreateCoin("Coin3",  new Vector3(12,   1.1f, 0)); // Block2上
        CreateCoin("Coin4",  new Vector3(17,   2.6f, 0)); // Block3上
        CreateCoin("Coin5",  new Vector3(18,   2.6f, 0));
        CreateCoin("Coin6",  new Vector3(23,  -0.9f, 0)); // Block4上
        CreateCoin("Coin7",  new Vector3(28,   1.1f, 0)); // Block5上
        CreateCoin("Coin8",  new Vector3(34,   2.6f, 0)); // Block6上
        CreateCoin("Coin9",  new Vector3(40,  -0.9f, 0)); // Block7上
        CreateCoin("Coin10", new Vector3(47,   1.1f, 0)); // Block8上
        CreateCoin("Coin11", new Vector3(52,   2.6f, 0)); // Block9上
        CreateCoin("Coin12", new Vector3(64,   1.1f, 0)); // Block11上
        CreateCoin("Coin13", new Vector3(72,   2.6f, 0)); // Block12上

        // ---- ゴール旗 ----
        CreateGoal("Goal", new Vector3(90, -2f, 0));

        // ---- カメラ追従 ----
        SetupCamera(player);

        // ---- GameManager ----
        if (Object.FindAnyObjectByType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("セットアップ完了",
            "マリオ風ステージの自動配置が完了しました！\n\n▶ 再生ボタンで動作確認\n・A/D or 左右キー：移動\n・スペース：ジャンプ（高め）\n・Z：射撃\n・敵を上から踏む：倒す\n・黄色い矢印が向いている方向に進む", "OK");
    }

    static void ClearOldObjects()
    {
        string[] clearNames = {
            "Ground",
            "Block1","Block2","Block3","Block4","Block5","Block6",
            "Block7","Block8","Block9","Block10","Block11","Block12",
            "Pipe1","Pipe2","Pipe3",
            "Enemy1","Enemy2","Enemy3","Enemy4","Enemy5",
            "Enemy6","Enemy7","Enemy8","Enemy9","Enemy10",
            "FlyEnemy1","FlyEnemy2","FlyEnemy3","FlyEnemy4","FlyEnemy5",
            "Coin1","Coin2","Coin3","Coin4","Coin5","Coin6","Coin7",
            "Coin8","Coin9","Coin10","Coin11","Coin12","Coin13",
            "Goal","GameManager"
        };
        foreach (string n in clearNames)
        {
            GameObject obj = GameObject.Find(n);
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    static GameObject SetupPlayer()
    {
        GameObject player = GameObject.Find("Player") ?? new GameObject("Player");
        player.transform.position = new Vector3(0, 0, 0);
        player.tag = "Player";

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null) sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.CreatePlayer();
        sr.drawMode = SpriteDrawMode.Simple;

        if (player.GetComponent<BoxCollider2D>() == null)
            player.AddComponent<BoxCollider2D>();

        if (player.GetComponent<Rigidbody2D>() == null)
        {
            var rb = player.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
        }

        // GroundCheckは PlayerController.Start() で自動生成されるので不要
        PlayerController pc = player.GetComponent<PlayerController>() ?? player.AddComponent<PlayerController>();
        pc.groundLayer = LayerMask.GetMask("Ground");

        return player;
    }

    static void CreateBlock(string name, Vector3 pos, Vector3 scale, Color color, string tag, int layer, bool isTrigger)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        if (tag != "") obj.tag = tag;
        if (layer >= 0) obj.layer = layer;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        // パイプは緑、地面は草、それ以外はレンガ
        if (name.StartsWith("Pipe"))
            sr.sprite = PixelArt.CreatePipe();
        else if (name == "Ground")
            sr.sprite = PixelArt.CreateGround();
        else
            sr.sprite = PixelArt.CreateBlock();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(1, 1);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = isTrigger;
    }

    static void CreateEnemy(string name, Vector3 pos, bool isFlying = false)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.layer = 7;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = isFlying ? PixelArt.CreateFlyEnemy() : PixelArt.CreateGroundEnemy();

        if (isFlying)
        {
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
        else
        {
            obj.AddComponent<BoxCollider2D>();
        }

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        EnemyController ec = obj.AddComponent<EnemyController>();
        ec.isFlying = isFlying;
        if (isFlying)
        {
            ec.flyHeight = pos.y;
            ec.flyAmplitude = Random.Range(0.5f, 1.5f);
            ec.flySpeed = Random.Range(1f, 2.5f);
        }
    }

    static void CreateCoin(string name, Vector3 pos)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        obj.layer = 8;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.CreateCoin();
        sr.sortingOrder = 1;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        obj.AddComponent<Coin>();
    }

    static void CreateGoal(string name, Vector3 pos)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(0.5f, 6f, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.CreateGoal();

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        obj.AddComponent<GoalFlag>();
    }

    static void SetupCamera(GameObject player)
    {
        Camera cam = Camera.main;
        CameraFollow cf = cam.GetComponent<CameraFollow>() ?? cam.gameObject.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.offsetY = 1f;
    }

    static Sprite CreateSprite(Color color)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    static void SetupTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName) return;
        tags.arraySize++;
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    static void SetupLayer(string layerName, int index)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty layer = layers.GetArrayElementAtIndex(index);
        if (layer.stringValue == "") layer.stringValue = layerName;
        tagManager.ApplyModifiedProperties();
    }
}
