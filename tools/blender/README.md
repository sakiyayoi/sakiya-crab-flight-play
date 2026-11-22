# モール Blender パイロット

22家族74配置の描画を差し替える。物理寸法の正本は従来の Geometry / Proposals。配置JSON、物理、手触り、UIは編集しない。

- `build_mall.py`: Blender 4.2.23 LTS向け造形・FBX・PNG生成器。外部素材なし。作者はCodexによる本案件のオリジナル造形、著作権はさきや。第三者アセットのライセンスを変更しない。
- `families.json`: 元の各家族のメートル寸法・破壊片番号・配置。新Native baselineで家族と件数を再照合済み。生成器はこのファイルだけを読み、配置を書き換えない。
- `*.blend`: 各家族の実物。原点は底面中心、Blender Z上、正面は−Y。`Intact` は無傷用の統合メッシュ、`Part_n` は従来の破壊片番号に対応。破壊片はレンダー非表示、FBXには含む。
- `contact_sheet.py`: Blenderレンダー22枚を4列の一覧に組む。ローカルPillowを使う。

プロトタイプのルートから実行する。Unityは起動していない状態で再生成する。

```sh
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python source/tools/blender/build_mall.py
~/.local/bin/codex-creative python source/tools/blender/contact_sheet.py
```

最後に `-- 家族ID` を付けるとその家族のみ更新できる。出力は許可された本パイロットの `.blend`、`Assets/BlenderModels/Resources/BlenderModels/*.fbx`、`evidence/blender-pilot/*.png`。既存のパイロット出力を更新するので、比較版を残す場合は先にGitで保存する。

Unity 6000.3.23f1 / Built-in を使用。FBX importerはメートル単位、軸焼込み、vertex colour、法線を保持し、animation/camera/light/colliderは取り込まない。Unityで確認した対応は Blender `(x,y,z)` → Unity `(x,z,y)`。正面が−Zに来ることを自販機前面の頂点でも検査する。頂点色はlinear、共有材質は2段の明暗だけを持ち、光沢・グラデーション・全面発光は使用しない。

`BlenderFamilyOverride.Apply` は `City2Data.Build` の従来外観処理後に実行。x=28〜135の家族だけ、ResourcesにFBXがあれば無傷と破壊片の描画を置換する。FBXがなければ従来表示を保つ。旧メッシュ由来のCollider・破壊用寸法、支点、mass、threshold、chainGroupは変更しない。原点補正は描画子だけに適用し、元の非ゼロ底面・非対称中心を維持する。生成側で寸法を正規化しておりUnityで配置scaleを再設定しない。

Native検証（毎回新しい出力名を指定）：

```sh
BLENDER_PILOT_OUTPUT="$PWD/evidence/blender-pilot/native-new" \
  python3 tools/stage65-unity.py blender-pilot-new BlenderPilotReview.Run
```

`BLENDER_PILOT_BASELINE=1` で描画overrideを無効にした基準を採れる。既存sceneを開いて実行するが保存しない。seed42043・ルートの `accepted-input.json`・1/60秒Tickで実際の物理を通し、結果と再投まで実行する。カメラ画像はUnityネイティブの描画で、HTMLのHUDや通常スワイプ入力の操作試験ではない。

PlaywrightはNative記録の照合用。ブラウザ操作試験と区別する。

```sh
cd source
NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
  npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
  playwright test --config qa/blender-pilot/playwright.config.cjs
```

`BLENDER_PILOT_CANDIDATE` は `evidence/blender-pilot/` 配下の検証対象名（既定 `native-final`）。外部公開・push・WebGL配布物の上書きは行わない。
