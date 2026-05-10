# Unity My Project

Unity 6000.4.0f1 の 2D WebGL ゲームです。

## Local WebGL build

```sh
/Applications/Unity/Hub/Editor/6000.4.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath "$(pwd)" \
  -buildTarget WebGL \
  -executeMethod WebGLBuilder.BuildGitHubPages
```

Build output is written to `docs/`, which is deployed to GitHub Pages by `.github/workflows/pages.yml`.

## Play

After pushing to GitHub and letting the Pages workflow finish, open:

```text
https://dicotamaru-sketch.github.io/Unity_MyProject/
```
