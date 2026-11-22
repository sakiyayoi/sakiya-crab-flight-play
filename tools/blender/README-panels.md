# Restaurant / Core / Overdrive のBlender外観

2026-11-23。現状は **実装・原稿保存済み／Unity Native検証BLOCKED**。初回NativeはEditorライセンス不認識でゲーム読込前にexit198。正規のUnity Hubでの復旧後に下記Nativeを再開する。認証を変更するスクリプトはない。

## 内容

- `inventory_panels.py` はSceneの有効フラグと実際のAddPanel呼出から40 IDを列挙する。動的登録の確認の代わりにはしない。
- `panels-families.json` はそのソース根拠・寸法・モデル対応。左右19モデル、破片6モデル。
- `build_panels.py` は元モデルを変更せず、`panel_*.blend` と同名FBXだけ生成する。FBXは既存BlenderPilotImportが頂点色・軸変換を取り込む。
- `review_panels.py` は25原稿を開き直し、実寸・原点・頂点色を検査してBlenderプレビューを保存する。
- `BlenderPanelOverride` は無傷時と破片のMeshRendererだけを差し替える。旧MeshFilter/Collider/Rigidbodyは保持。料理破片の旧CC0名・計数も保持するが表示は新Blenderモデルになる。
- OverdriveGroundPileの既存描画offsetはそのまま実行される。rootの位置・scale、配置データは変えない。

## 再生成（sourceで実行）

```sh
python3 tools/blender/inventory_panels.py
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python tools/blender/build_panels.py
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python tools/blender/review_panels.py
~/.local/bin/codex-creative python tools/blender/contact_panels.py
```

特定モデルだけ生成するならbuildコマンド末尾へ `-- panel_wood_chair` を付ける。City41家族はこの工程で再生成しない。

## Native再開（ライセンス復旧後）

```sh
python3 tools/blender/run_native_panels.py baseline --name licensed-baseline01
python3 tools/blender/run_native_panels.py candidate --name licensed-candidate01
NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
  npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
  playwright test --config qa/blender-panels/playwright.config.cjs
```

baselineはstart.jsonのblender-rollout commitを新しい隔離コピーへ展開し、開始時Sceneと読取用Editor検査だけ追加する。候補のoverrideをOFFにしたものを基準とはしない。原本Sceneと既存Libraryは書き換えない。新しい実行名が必要で、既存証拠を上書きしない。

比較するもの：accepted-input SHA/seed42043/飛距離/粉砕イベント/Tick/route.csv/break-events.jsonl、全Cityの物理・外観署名、全登録対象の物理署名と系統別Blender残数、再投復帰、実生成されたパネル破片、Unity import寸法・色・Collider不在、対象区画正負のNative画像。画像を実際に開いて確認し、結果とcommitを報告へ追記する。

現時点の制限を再現して独立検査のみ実施する場合は、Playwrightコマンドに `BLENDER_PANELS_NATIVE_BLOCKED=license` を付ける。この場合6件が明示SKIPとなる。2 PASSを全件合格と扱わない。通常コマンドではNative証拠が欠ければ失敗する。
