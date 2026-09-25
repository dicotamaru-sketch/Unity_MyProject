# Unity My Project

Unity 6000.4.0f1 の 2D WebGL ゲームです。

## 公開されているゲーム: 「流れを結ぶ」

線をなぞるのではなく、動き続ける粒子の流れに指で触れるアート寄りのミニゲーム。
指の位置・向き・速さが粒子の速度に影響し、指を離した後も粒子は個別に動き続ける。

- 流れの輪に暗い節が二箇所ある。粒子が進む向きに沿って指でなぞると充填する。
- 有効なドラッグは合計4回まで（軽いタップは消費しない）。
- 二節とも充填し、指を離すと安定化に入る。安定した状態を1.75秒保てば完成。
- 4回使い切って二節が未接続なら失敗。右上の「やり直す」で再挑戦。

実装: `Assets/Scripts/Flow/`（`FlowSimulation.cs` が状態とモデルの正本、
`FlowInputController.cs` が入力、`FlowRenderer.cs` が描画、`FlowUIController.cs` がUI、
`FlowBootstrap.cs` が起動時の配線）。シーンは `Assets/Scenes/FlowScene.unity`。

### 描画（Phase 1・2）

- `Assets/Resources/Shaders/FlowTrail.shader`: 軌跡の加算合成。uv.x=共鳴値、uv.y=減衰。HDR強度で Bloom に乗る。
- `Assets/Resources/Shaders/FlowFeedback.shader` + `FlowFeedback.cs`: 残像バッファ。前フレームを
  シミュレーションと同じリング流れ場で移流させ減衰し、今フレームの軌跡を加える。
  クリア時は減衰停止（構図が固定）、失敗時は減衰加速（流れがほどける）。
- `FlowBootstrap.cs` が実行時に URP Volume（Bloom・ACES）を生成し、カメラの Post Processing を有効化する。
- レイヤー 9 `FlowTrails` を軌跡専用に使う（残像用カメラの描画対象）。
- シェーダーは `Resources/` 配下に置くことでビルド時のストリップを避けている。

**Editor で最初に確認する点**: 残像が流れと逆方向（上下逆）に流れる場合、`FlowFeedback.shader` の
uv→world 変換の y 符号がプラットフォームで反転している。`(uv - 0.5)` の y を反転して直す。

### Standalone ビルド

Editor メニュー `Flow/Build macOS` または `Flow/Build Windows`。出力は `Builds/`（git 管理外）。
コマンドライン: `-executeMethod StandaloneBuilder.BuildMac`。

### 検証状況（重要）

このコンテナには Unity Editor バイナリが無く、コンパイル・実行・実機確認は一切できていない。
ロジックの数値（0.72 / 4筆 / 0.70 / 1.75秒 など）は以前の JS 試作の仕様を踏襲した初期値であり、
Unity上での再検証はしていない。**Unity Editor で開いて実際に再生し、コンパイルエラーが無いか、
操作感が意図通りかを確認すること。** 未確認のまま「動作する」とは主張しない。

以前の「マリオ風」プラットフォーマー（`PlayerController.cs` 等）は削除せず `Assets/Scripts/`
直下に残してある。公開シーンからは外れているが、`Assets/Scenes/SampleScene.unity` で今も遊べる。

## Local WebGL build

```sh
/Applications/Unity/Hub/Editor/6000.4.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath "$(pwd)" \
  -buildTarget WebGL \
  -executeMethod WebGLBuilder.BuildGitHubPages
```

Build output is written to `docs/`. This now builds `FlowScene.unity`.

GitHub Pages is configured to publish from the `main` branch's `/docs` folder.

## Play

After pushing to GitHub and letting the Pages workflow finish, open:

```text
https://dicotamaru-sketch.github.io/Unity_MyProject/
```
