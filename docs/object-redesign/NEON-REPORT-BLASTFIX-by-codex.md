# オブジェクト再設計 blast経路 MissingReferenceException 修正報告

2026-11-27 JST。版名 `object-redesign-blastfix`。指定されたソース修正・オフライン検証・ローカルコミットを完了。Native固定入力リプレイの再実行、Unity/WebGLビルド、実ゲームの合格判定は今回の範囲外。

## 対象版と保存

- worktree: `../object-redesign`、branch: `object-redesign`。
- 基点: `75cb1a15ea43420ce23c8e2d7afb22a63506fa00`。開始時clean。
- **修正・試験根拠コミット: `8631b74ff60ac5dfc257edd228d4b812abadec31`**。
- 本報告、最終tasks、commit読戻し記録は後続docsコミットに分離。上記コミットの変更50ファイルをGitから読み戻し、作業実体と全件byte一致。根拠: [commit-readback.json](blastfix/commit-readback.json)。
- 製品コードの変更は `Assets/ObjectRedesign/ObjectRuntime.cs` と `Assets/Scripts/City2Contacts.cs` の2ファイル。配置・作用量・形・キャプション・基礎物理・散布計算・粉砕集計コードは変更していない。
- 保護26ファイル（25件のpanel FBX meta＋FlightField.unity）は、元sourceの実体と対象worktreeの実体／sparse対象Git blobを、それぞれ作業前後で全26件SHA256照合。**main 26一致・branch 26一致、変更0**。両者は元から別の値なので、互いを同一とはしていない。[作業前](blastfix/protected-before.json) / [作業後](blastfix/protected-after.json)。コミット対象にも含まれない。
- ソース／試験入口のSHA256は [source-sha256.json](blastfix/source-sha256.json)、型チェックした全133ソースのSHA256は [compile.json](blastfix/compile.json) に保存。

## 根本原因

推定されていた「同一対象の二重破壊」は例外の原因ではなかった。`pendingBreaks` は `Dictionary<BreakPanel,Vector3>` で対象参照ごとに一件となり、既存 `ApplyBreaks` は `p.broken` を確認してから、集計・破片生成より前に `p.broken=true` にする。今回、同じ対象が二重に集計される反例は確認していない。

例外を起こすのは **生成中のCity2破片を、同じ生成バッチ内の回収処理が先に破棄すること**。ソースを追跡し、旧メソッドと実 `ScatterShardsR6` を使う単体反例で、報告された `worldCenterOfMass` の破壊済み参照を再現した。

1. redesignの帯などの一般破片で `fragments.Count=48`、`cityDebris.Count=0` になった状態で、City2対象を一度だけ破壊する。
2. `SpawnCity2Shards` が1個目を作る際、旧 `RecycleCityFragment()` は一般破片を1個回収する。その後City2破片Aを追加し、散布待ち `spawned` にAのRigidbodyを保存する。総数は再び48。
3. 2個目を作る際、回収処理は `cityDebris.FirstOrDefault()` を優先するため、**いま生成したA** を回収する。Aは `spawned` からは除かれない。
4. Editorで `Application.isPlaying=false` の既存 `Release` は `DestroyImmediate` を使う。6個の生成を試みる反例では、散布待ち6参照のうち5個が既に破壊され、`ScatterShardsR6` の `rb.worldCenterOfMass` で例外となる。対象の破壊は1回だけで再現する。
5. Playerの遅延Destroyでは同時点で参照がまだ有効でも、非アクティブ化・破棄予約された破片へ散布してしまう。単なるnullチェックでは、この寿命の不整合を解決しない。

旧処理を使うこの負の対照は [unit-contracts.log](blastfix/unit-contracts.log) の先頭に保存した。Nativeリプレイ自体の当該フレームのプール内訳は今回未取得。上記は同じ例外箇所に到達するソース上の原因とオフライン再現であり、固定入力リプレイ完走の報告ではない。

## 修正内容

**回収側の原因修正。** `RecycleCityFragment` に散布待ちリストを渡せるようにし、そのリスト中のRigidbodyとGameObjectを回収候補から除く。古いCity2破片が無ければ、生成中バッチ以外の一般破片を回収する。上限48個／City2上限20個、生成上限6個、散布のエネルギー予算と計算式は維持。前バッチは散布済みなので次バッチで通常どおり回収できる。

`SpawnCity2Shards` は **`UseObjectRedesign ? spawned : null`** を渡す。legacyおよび既存Life経路はnullを渡し、従来のFIFO選択のまま。全819通りの総数0〜48／City2数0〜20の有効な組合せで6個生成し、旧回収メソッドとlegacy側の残存順序・City2数・Collider登録数が一致することを確認した。これはオフラインの回収契約であり、旧Native軌道の再実測ではない。

**キュー投入側の重複・消費済みガード。** `QueueObjectBreak` でlegacy・null・破壊済み・抑制済み対象を拒否。`objectBreakPending.Add` の成否を使い、同じ作用対象のキュー情報を書き直さない。先に帯接触、後からblastとなる場合は、blastの分類だけ追加して処理回数は増やさない。

作用で確定した対象は `primaryPending` に登録し、先行した `secondarySources` を除去する。既存の二次接触コールバックが持つ `primaryPending.Contains(p)` ガードにより、後から二次破片が破壊元を上書きすることも防ぐ。通常の接触→blast／blast→通常の接触の両順序では、共有Dictionaryの一件を既存 `ApplyBreaks` が一度だけ消費する。

`ScatterShardsR6` にはnullチェック・例外握り潰しを追加していない。散布待ちの生存を回収側で保証し、既存の散布・Editorエネルギー検査をそのまま実行させる。

## 二重加算の確認

新規契約は実ソースから `QueueObjectBreak`、`QueueBreak`、`ApplyBreaks`、`RecycleCityFragment` をそのまま抽出してコンパイルし、実 `ObjectRules` と実 `ShardScatterR6.cs` を使う。Unityのオブジェクト寿命・描画・副作用は記録用ダブルで、PhysXではない。`ApplyBreaks` のCity2生成呼出しは回数を記録し、回収と散布の寿命試験は別に実メソッドで行う。

- 同一ステップの接触→blast／blast→接触に、blast再投入と帯再投入を重ねてもpendingは1件。
- 実 `ApplyBreaks` の破片生成呼出し、`GlassBroken`、`DestructionEvent`、`BreakEvents`、`BrokenObjects`、`ObjectRun.MaxChain` がすべて1。2回目の消費でも増えない。
- 破壊済み対象は両投入経路で拒否。帯→blastは分類のみ昇格し、キュー・衝撃値を再作成しない。投げ直しのbroken／Runリセット後は再び1回だけ破壊できる。
- 破棄済み二次破片の参照が先に存在しても、blastが所有を引き継ぎ、二次破片の生存に依存せず1回集計する。
- 素材分岐を含む集計本体と `p.broken` の判定・設定順はbyte不変。一般粉砕数の加算を新作用コードへ移したり追加したりしていない。

## 検証結果

| 検査 | 結果 | 記録 |
|---|---|---|
| Editor型チェック（実Unity参照＋Effekseer 1.80.8） | errors 0、ゲームwarnings 18／Effekseer warnings 0 | [typecheck.log](blastfix/typecheck.log) / [compile.json](blastfix/compile.json) |
| WebGL条件分岐型チェック | errors 0、ゲームwarnings 17／Effekseer warnings 0 | 同上 |
| Stage 1 ObjectRules | 85 passed / 0 failed | [rules-contracts.log](blastfix/rules-contracts.log) |
| Stage 1配置・基礎物理・同版型検査等 | 232 passed / 0 failed | [contracts.json](blastfix/contracts.json) |
| 既存Effekseer／Juice | 45 + 33 passed / 0 failed | [bridge](blastfix/bridge-contracts.log) / [juice](blastfix/legacy-juice-contracts.log) |
| ストーリー形状／不変境界 | 46 + 13 passed / 0 failed | [形状](blastfix/shapes-contracts.log) / [境界](blastfix/story-contracts.json) |
| 新規blast・回収・消費契約 | 258 passed / 0 failed、静的9 passed / 0 failed | [実行](blastfix/unit-contracts.log) / [結果](blastfix/blast-contracts.json) |
| 既存Stage 1＋ストーリー＋音声/HUD Playwright | 34 passed / 0 failed / 0 skipped / 0 flaky | [playwright.log](blastfix/playwright.log) / [JSON](blastfix/playwright.json) |
| 保護26ファイル | main・branch各26一致、変更0 | [前](blastfix/protected-before.json) / [後](blastfix/protected-after.json) |

旧契約の「City2全文不変」「ストーリー層でRuntime全文不変」は、今回許可された修正と両立しないため、[boundaries.py](blastfix/boundaries.py) に記載した**完全一致する今回の差分だけ**を逆適用して旧基点と比較する形へ更新した。基点・検査数・その他の不変領域・閾値は維持し、新規契約でもこの差分を限定して検査。変更全体を許可する比較や、基点のすり替えはしていない。ストーリーの試験入口には証拠出力先の指定だけを加え、旧記録とPNGを上書きしていない。

新規258件には、旧コードの例外を意図どおり検出する負の対照1件を含む。即時・遅延破棄、空き容量／混在プール、次バッチ、エネルギー上限を検査した。実ゲームの見え方や実音の検査ではない。PlaywrightのPNGも合成状態のHTML部品で、Unity実画面の証拠として扱わない。

### 再実行コマンド（worktreeルート）

```sh
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/blastfix python3 docs/object-redesign/typecheck.py
python3 docs/object-redesign/blastfix/contracts.py
KANI_STORY_EVIDENCE_DIR=docs/object-redesign/blastfix python3 docs/object-redesign/story/contracts.py
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/blastfix KANI_EFFEKSEER_EVIDENCE_DIR=docs/object-redesign/blastfix KANI_JUICE_EVIDENCE_DIR=docs/object-redesign/blastfix KANI_STORY_EVIDENCE_DIR=docs/object-redesign/blastfix NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/blastfix/playwright.config.cjs
```

最後のPlaywright内でStage 1／Effekseer／Juiceの契約も実行する。Unity 6000.3.23f1付属Roslyn、元sourceのLibrary参照、既存 `/tmp/kani-effekseer-1808-reference`、既存PlaywrightとChromeを使用。RoslynのWebGL分岐型検査は、IL2CPP・Shader・Unity import・WebGLビルドではない。

## 残件と引継ぎ

発生可能性3＋影響3＝6の停止不具合として優先修正した。指定されたオフライン修正の完成条件は満たした。**同版PortNativeReview再実行・WebGL再ビルド・実画面・40投・game-playtestはNOT_RUN**。次はクロエ側で上記修正コミットを既存の完全な隔離checkoutへ適用し、`objectMode=redesign` の固定入力リプレイを再実行し、例外0、結果到達、再投、粉砕IDの重複0を確認する。legacy固定入力との差分確認もNative側に残る。編集worktreeはsparseのため、単独で完全Unityプロジェクトとは扱わない。

今回の回収保護はredesign専用。legacyの回収選択・入力・力・集計は従来のまま。既知の床侵入5.26cm、Collider残差、モール反復FAILの状態は変更していない。

既存tasksと案件HANDOFFへ記録。TEAM.mdは案件固有のKB読取禁止に従い未確認。Studio入口は現行native catalogで未検出。Reasonのobject検索に別稼働担当はなく、既存Unity Editorプロセスなしを確認。他セッションの再起動、外部送信、Linear/KB更新、Git push、公開、課金、新認証は実施していない。
