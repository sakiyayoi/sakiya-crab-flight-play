# Blender 全区画展開

`BlenderFamilyOverride` が扱う有効なCity対象を、9区画・正負両方向で差し替える。
基準は `blender-pilot` の `f3911b0473d84b7aece4f08c493c3dcd170abb11`。
既存22家族に19家族を追加し、実効41家族・1,407件を扱う。
背景・静的装飾、別系統のRestaurantの40対象とLifeContactsの34対象は、このCity家族機構に含まれない。

制作物の著作権はさきや。新規19家族はCodexが本案件用にBlender Pythonで造形したオリジナル。
外部モデル・有料サービスは使用していない。第三者素材の権利を変更しない。
原寸の正本は既存Geometry/Proposals。配置、scale、yaw、Collider、質量、破壊支点、UI、手触りを変更しない。

## ファイル

- `inventory_rollout.py`: Nativeが構築した実体ID・変換後のasset IDを区画と方向別に集計。
- `rollout-families.json`: 最終実体集計、各家族の原寸、破壊片番号、元GeometryのSHA256、全配置ID。
- `build_rollout.py`: 新規19家族の造形。`build_mall.py`の原点正規化・旧破壊片領域分割・FBX/PNG書出しをそのまま再利用。
- `<家族ID>.blend`: 底面中心、実寸メートル。`Intact`と旧番号に対応する`Part_n`を持つ。
- `contact_rollout.py`: 既存22家族の未変更レンダーと新規19家族のレンダーから、区画別一覧を作る。
- `../../qa/blender-rollout/`: Native生記録のPlaywright照合。通常ブラウザ入力のE2Eではない。

## 再生成

Unityを終了した状態で、`source`から実行する。

```sh
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python tools/blender/build_rollout.py
```

家族を1個だけ再生成する例：

```sh
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python tools/blender/build_rollout.py -- hay_roll_neon6
~/.local/bin/codex-creative python tools/blender/contact_rollout.py
```

既存パイロットの22原稿は再生成しない。出力は本展開の19 `.blend` / FBX / `evidence/blender-rollout/models/`。
Unityが開いている間に原稿を作る場合は、`BLENDER_ROLLOUT_STAGING`を独立したFBX出力先に指定し、Unity終了後に同一byteを取り込む。
比較対象を残すときは、再生成前にGit保存する。

## Native検証

プロトタイプのルートから。既存Sceneを開いて実行し、保存しない。新しい出力先を指定する。

```sh
BLENDER_ROLLOUT_OUTPUT="$PWD/evidence/blender-rollout/native-new" \
  python3 tools/stage65-unity.py blender-rollout-new BlenderRolloutReview.Run
```

seed42043・ルートの`accepted-input.json`を使う。基準は実際のpilot時点を新プロセスで再実行した`native-baseline`。
現行のoverride無効化をpilot基準の代用にしない。基準の再生成には上記pilot commitの隔離コピーと、保存済みの開始時Sceneが必要。
`source`へ戻り、最終実体を再集計してから照合する。

```sh
python3 tools/blender/inventory_rollout.py ../evidence/blender-rollout/native-new/targets.json
BLENDER_ROLLOUT_CANDIDATE=native-new \
  NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
  npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
  playwright test --config qa/blender-rollout/playwright.config.cjs
```

現提出の最終出力は`native-final02`。Nativeは実物のUnity描画だがHTMLのHUD・スワイプ入力や実iPhoneの試験ではない。
通常操作・Game Studio game-playtest・実聴・実FPS・本人の手触り採用は別の検証範囲。
