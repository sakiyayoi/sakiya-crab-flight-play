# カニ飛ばし（ネオン試作）配置修正報告

2026-11-22 JST。正本 `PLACEMENT-AUDIT.md` 全文を読んで実施。**今回依頼されたソース修正・Unity外での配置再生成・機械検査・ローカルコミットまで完了。新しいゲームの実画面、固定入力リプレイ、ビルドの合格は未確認。**

## 対象版と保存

- 開始版：`ui-simplify` / `d288db27c604ba716c2ef7af921ea0975e28efe1`。開始時の追跡・未追跡変更は0。`git checkout ui-simplify` → `git switch -c placement-fix` を実行。
- ブランチ：`placement-fix`。実装・検証記録コミット：`da938b4858e3bdf977cf68d81f98d02e02fa8e8e`。この報告のGit保存は、その後の文書コミット。最終HEADは `git -C source rev-parse placement-fix` で確認できる。
- 配置データrevision：`neon-placement-fix-1`。既存の `NeonProto.Revision` は変更していないので、旧runtime revision文字列だけで今回版を識別しないこと。
- `Placements.json` SHA256：`510c2dffde875a67b86a32acafaf6f96636d2bbae9332678aba70e277923b2c4`
- `Supports.json` SHA256：`861c3cff98680483db307256d17bfb9fc38e6409dbd1361df1371d3fafa2eb8d`
- 前後集計 `source/docs/placement-fix/audit.json` SHA256：`aa1323b6b81948c7ba9a17e321d2be40a7e62ece276f23d696790a9fc61662bc`
- 報告書は指定のプロトタイプ直下に保存し、同内容を `source/docs/placement-fix/NEON-REPORT-PLACEMENTFIX-by-codex.md` にGit保存。外部送信・push・公開なし。

## 変更前後の集計

母集団は監査と同じ追加 **1,059件**。近傍距離は追加1,059件＋保存された既存348対象の配置原点による3次元ユークリッド距離。回廊は `|z|≤2`、飛行帯は `y<20`、ルートは `x≥0`。連鎖候補の距離は10m。元データから監査値229・254・39・64・385を再現した。

| 指標 | 変更前 | 変更後 |
|---|---:|---:|
| 追加破壊対象数 | 1,059 | 1,059 |
| 全世界登録数（422＋追加、静的推定） | 1,481 | 1,481 |
| 旧監査と同じ既知形状の「底面が地面から0.5m超」 | 229 | 7 |
| プランター底面を取得後の同判定 | 235 | 7 |
| 上記から自然浮遊・実装した支持面を除いた、接地対象の浮遊 | 230 | **0** |
| 厳密な支持面との差（0.00001m超、微小な浮きも含む） | 561 | **0** |
| 中央回廊内の追加（高さを問わない、監査の24%と同条件） | 254 / 1,059（23.98%） | **302 / 1,059（28.52%）** |
| 正方向・飛行帯・回廊（既存含む） | 385 | 454 |
| サイド `|z|≥4` | 749 | 701 |
| サイドの＋z / −z | 431 / 318 | 348 / 353 |
| サイドのうち回廊対象から10m超 | 64 | **0** |
| 全対象から10m超の孤立（追加） | 39 | **0** |
| 高層の孤立 | 23 | **0** |
| 中央回廊の最大空白 | 27.5532m | **12.8953m** |
| モール以降・移動する100m窓の回廊最大数 | 25 | **27**（上限63.55以内） |
| orbit前半2680〜2800mの回廊・飛行帯対象（既存含む） | 13 | 16 |

「地面から0.5m超」の変更後7件は、自然な衛星5件と、地面から立つ塔の上の酸素タンク2件。**この旧集計の7を0と書き換えていない。支持体なしの浮遊が0件**である。監査の既知229件には衛星5件が含まれ、接地対象は224件。そこに底面を新たに取得したプランターの中層6件を足すと230件となる。0.5m以内のドラム底面0.1m、木箱0.155m等の隙間も補正したため、厳密な支持面検査の変更前件数は561件となる。

底面不明だった `rail_glass_planter_display_r10` は `Resources/FarWorldR10/Geometry.json` の実頂点から底面 **0.02m** を取得。28件すべて判定可能になり、接地済み。

| 区画 | 追加数（不変） | 飛行帯・回廊対象（既存含む） | 回廊密度 / 100m | 全zの飛行帯密度 / 100m |
|---|---:|---:|---:|---:|
| tank | 3 | 0 → 0 | 0.00 → 0.00 | 66.67 → 66.67 |
| restaurant | 14 | 3 → 3 | 12.77 → 12.77 | 59.57 → 59.57 |
| mall | 0 | 49 → 49 | 45.79 → 45.79 | 63.55 → 63.55 |
| office | 207 | 67 → 76 | 14.41 → 16.34 | 50.11 → 51.40 |
| port | 125 | 37 → 44 | 12.33 → 14.67 | 45.00 → 46.33 |
| viaduct | 154 | 41 → 51 | 10.25 → 12.75 | 40.00 → 41.25 |
| country | 256 | 71 → 88 | 9.47 → 11.73 | 35.07 → 36.27 |
| launchrange | 180 | 62 → 75 | 9.84 → 11.90 | 30.00 → 30.95 |
| orbit | 120 | 55 → 68 | 10.19 → 12.59 | 24.07 → 24.81 |

モール全74件（54/14/6）は変更していない。表の49はモールの**飛行帯・回廊だけ**の件数、68から求めた63.55は全zの飛行帯密度であり、74という総数と混同しない。モールから先の全z飛行帯密度も逓減を維持。tankは既存配置のまま回廊0件で、全z密度66.67を回廊密度と取り違えない。

## 配置の直し方とスナップ仕様

1. 追加のID、asset、件数、順序、scale、yaw、chainGroup、破壊設定を維持。変更した位置は613件（xが変わった74件、zが変わった112件を含む）。chunkはxから再計算。モールへ入る配置0件、区画をまたぐ移動0件。
2. アセットの実メッシュ全頂点から底面を取り、`配置y = 支持面の高さ − ローカル底面 × scaleY` とする。地面の支持面は0。屋上扱いを根拠なく推測せず、今回追加した明示的な塔上面だけを支持面として扱う。足元の横方向全範囲が塔の上面に収まることも検査する。
3. 生成後、自然浮遊5件を除く全1,054件で底面と支持面の差の絶対値を検査。許容差は丸め対策の **0.00001m**。浮き・沈み、支持体自体の浮き、上面からのはみ出し、形状欠落、非有限位置があれば出力前に失敗する。Python `assert` に頼らず、最適化実行時も検査を無効にしない。
4. 衛星の自然浮遊5件は座標を含め完全維持。高所21mの衛星 `neon-density-orbit-s6-1058` へは、既存の酸素タンク `1024` を `[2947,7,1.8]`、`1026` を `[2951,14,1.8]` へ移し、0→7→14→21mの距離をつなぐ。各タンクの下に地面まで届く塔を作る。残る1,052件は底面を地面へスナップ。
5. サイドから中央へ移すのは**48件**に限定（塔上2件を含む）。空白を埋める対象はドラム・木箱等の小物を優先し、大きいコンテナや太陽光パネルを一列に並べない。通常区間の生成時空白目標14m、orbit前半12m。移動先へID由来の決定的な±1.5mのxずれを加える。同じ入力データから同じ出力を再生成できる。
6. 残すサイド対象は回廊対象から10m以内であることを全件検査。左右は区画ごとにほぼ均等化。接地化直後に追加同士のAABB重なり25組を検出し、0.25m刻みの局所x調整で解消。既存対象との組も照合し、**新しく生じるメッシュAABB重なり0組**。元からある追加対既存の43組はそのままで、既存物の重なり全解消とはしていない。AABBは保守的な形状検査であり、実Collider接触判定の代用ではない。
7. 孤立解消は配置原点間の距離の証明。連鎖には質量条件もあるため、「すべてが実際に連鎖破壊される」「破壊数が増えた」という実行結果にはしていない。

支持体は3本（酸素タンク用2本、看板用1本）の実メッシュで、**静的・非破壊・非衝突の景物**とした。既存背景と同じrender-only構成を使い、Collider/Rigidbody/接触登録を増やさない。支持体自体がカニに衝突したり破壊されたりする仕様は追加していない。見た目として物を支える形状の実装であり、塔の実画面での見え方は未確認。

## orbit看板

`Neon sign orbit 0` を `[2725,3.5,6.5]` から **`[2725,20,6.5]`** へ上げ、下端17.5mを、地面から立つ高さ17.5mの支柱に接地させた。幅7m・高さ5mと、他の63枚の看板位置・色・発光周期は維持。遮蔽物である既存破壊対象は動かしていない。

旧版 `stage6-pixel-sector-orbit.png` を実見し、指摘された前景の遮蔽を確認した。今回版の再レンダリングは行っていない。**遮蔽対策のコードは実装済み、可視化成功は未確認**。新しいNative画像と `orbit sign0` の既存画素寄与閾値 `>0.01` でクロエが判定する。

## 変更ファイル

パスは特記なければ `source/` から。

| ファイル | 変更 |
|---|---|
| `games/flight/crab-flight/Assets/NeonDensity/Resources/NeonDensity/Placements.json` | 接地・回廊・孤立修正を再生成 |
| 同ディレクトリ `Supports.json` と `.meta` | 支持体3本、看板座標の生成データ |
| `games/flight/crab-flight/Assets/NeonDensity/NeonPlacementSupports.cs` と `.meta` | 生成データに一致する支持体を描画。物理部品なし |
| `games/flight/crab-flight/Assets/NeonProto/NeonSectionArt.cs` | 支持体生成とorbit sign0配置の読込み、6行追加 |
| `tools/author-neon-density6.py` | 固定開始版から再生成する生成器、支持面・距離・密度・重なり検査 |
| `tools/placement-fix/baseline-targets.json` | 監査と同じ348対象の保存済み署名・姿勢。元証拠ファイルのSHA付き |
| `tools/placement-fix/author-neon-density6-before.py.txt` | 旧生成器の原文保全 |
| `tools/placement-fix/test_placements.py` | 9件の正常・異常系検査 |
| `tools/placement-fix/check-unity-receipts.py` | クロエの新Native結果を、現配置・署名・2500m/96破壊・看板可視で照合 |
| `qa/placement-fix/placement.spec.cjs`、`playwright.config.cjs` | Unityを起動しない機械検査と既存ファイル保護検査 |
| `tools/.gitignore` | Pythonキャッシュを除外 |
| `tasks.md` | 既存計画表へ今回の範囲と状態を記録 |
| `docs/placement-fix/` | 正本監査の写し、前後集計・全613位置差分、検査結果、この報告 |

リポジトリ外は、指定報告書、既存 `../../HANDOFF.md` の今回記録、`../tools/author-neon-density6.py` の互換入口だけを更新。旧入口は版管理される `source/tools/author-neon-density6.py` を呼ぶ。既存のstage6集計や旧成果物を上書きしない。

## 実行した検査

- `python3 tools/author-neon-density6.py`：Unity外再生成と全機械ゲート成功。
- `python3 tools/author-neon-density6.py --check`：保存済み3出力と再生成結果が全文一致。
- 下記Playwrightコマンド：**2 passed / 0 failed / 0 skipped**。うちPythonの配置検査は **9 passed / 0 failed**。2cmの浮き、浮いた支持体、支持面の幅不足、形状欠落、衛星の例外追加も拒否することを確認。
- 既存Unity playerの参照DLL・defineを用いたRoslyn単独型検査：**exit0 / error0 / warning17**。警告は既存の `CrabModelR3` / `City2Contacts` / `RigidCrabFlight` / `FeelDirectorR27`。Unityエディタ・アセットimport・IL2CPP/WebGLビルドは実行していない。
- 基準版の全追跡ファイルについて、許可した既存3ファイル（配置・看板接続・tasks）以外の **2,144ファイル差分0**。物理・feel設定・飛行コード・UI・モール・既存ジオメトリ・シーン・ProjectSettingsを含む。
- 既存 `WebGL-stage65` の **85ファイルすべて** が保存済み正規manifestと一致。folder SHA256 `93437c043f0a569a8e1fbfabf419ee65c359cd6c7491f0867de8fac933e4aa6c`。旧配布物は不変。
- 新しい照合器に旧stage6の成功receiptを渡すと、613位置不一致と看板可視FAILのため **意図どおり拒否（exit1）**。旧2798m/96破壊を今回版の合格に転用していない。
- `git diff --check` 成功。元の同一版リプレイを勝手に再実行していない。

実行したPlaywrightコマンド（cwdは `source/`、このMacの既存ランタイムを使用）：

```sh
NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
playwright test --config qa/placement-fix/playwright.config.cjs
```

`.git/placement-fix-qa/runtime/node_modules` の `playwright` と `.bin/playwright` は既存Playwright 1.62.1へのローカルsymlink。新規インストールなし。別環境では既存Playwrightの解決場所に合わせるか、独立した `python3 tools/placement-fix/test_placements.py` を実行できる。初回の3起動は順にモジュール欠落・npxの版解決・実行入口欠落でテスト開始前に失敗し、既存ランタイムへの参照を修正して上記の全件成功に至った。履歴は `verification.json` に残した。

このPlaywright検査はブラウザを開かないデータ検査。実ゲームの起動・正式入力・発射・破壊・結果・再投の合格とは区別する。型検査のコマンドとrsp、Playwright生記録はローカル `source/.git/placement-fix-qa/`、共有可能な結果とログは `source/docs/placement-fix/` に保存。

## Unity側で必要な残手順（クロエの別工程、今回は未実施）

配置JSONの再生成にUnityは不要。支持体もResourcesから実行時生成されるため、追加の手動配置・Prefab作成は不要。Unity側では新しい `.cs/.meta/Supports.json` のimport、コンパイル、実行・描画・ビルドが必要。

1. このブランチと配置SHAを固定し、ライセンス利用可能な環境で新しいNative検査を実行する。プロトタイプ直下をcwdにし、既存runnerを使う具体例（freshな2つの出力名を使用）：

```sh
NEON_STAGE6_SHOTS="$PWD/evidence/placement-fix-native01" \
python3 tools/stage65-unity.py placement-fix-unity01 NeonStage6Review.Candidate
python3 source/tools/placement-fix/check-unity-receipts.py evidence/placement-fix-native01
```

この既存Native手順はseed **42043** と `accepted-input.json` を使う。追加の照合器は距離 **2,500m以上**、破壊 **96以上**、result到達、physicsFault空、モール **74（54/14/6）**、348署名の一致、今回1,059件の位置/scale/asset一致、登録1,481件、orbit sign0画素寄与 `>0.01` を必須とする。座標の読戻し許容差0.001mはUnity floatとJSONの差用で、生成器の接地許容差0.00001mとは別。

2. 同版の区画別PNGを実見する。地面との隙間、塔2本とタンクの接地、看板下端と支柱、orbit看板の遮蔽を確認。初回操作、飛行、結果、再投、縦横画面、動きを抑える設定の実操作・Game Studio game-playtestも新ビルドで行う。旧版画像を証拠にしない。距離/破壊が未達なら物理値を触らず配置へ戻る。低密度だけで失速防止を保証しない。

3. WebGLは**別の出力用コピー**でビルドする。既存 `NeonProtoBuild.Stage65Tune` は出力先を `WebGL-stage65` に固定しているので、今のプロトタイプ直下では実行しない。例えば次の準備を新しい作業ディレクトリで行う（これは今回未実行）：

```sh
# cwd: 元のプロトタイプ直下。placement-fix-build01 は未作成の名前を使う。
git clone --local --no-hardlinks --branch placement-fix source placement-fix-build01/source
mkdir -p placement-fix-build01/tools placement-fix-build01/evidence
cp tools/run-unity.py tools/stage65-unity.py placement-fix-build01/tools/
cd placement-fix-build01
NEON_STAGE65_TUNE_BUILD=stage65-tune-placementfix-build01 \
python3 tools/stage65-unity.py placement-fix-web01 NeonProtoBuild.Stage65Tune
```

出力先はそのコピー内の `placement-fix-build01/WebGL-stage65`。元のstage65を上書きしない。既存Stage65Tuneのパネル追加と、ui-simplify済みの現行web-templateコピーを維持する。新しいビルドのC#署名・配布物SHA・対象コミットを記録し、別のローカルURLで実操作を確認する。runtime revisionは継承文字列なのでSHAも必ず照合する。公開はこの手順に含まない。

**未実施：Unity editor/import/build、固定入力リプレイ、今回版のスクリーンショット・可視判定、実操作game-playtest、実機FPS・実聴。** ビルド用コピーの作成も未実施。これらは本人の今回の指示でクロエ工程に分離されており、この版のゲーム完成・採用・公開とはしていない。

## 必須ルール1〜5の自己照合

| 必須ルール | 今回の結果 | 残る確認 |
|---|---|---|
| 1. 全破壊物の接地 | 追加1,059件は自然浮遊5件を除き全1,054件で支持面差0。プランターも判定。支持体3本は地面から連続 | 既存348対象・モールを変更せず保護。全世界1,481件を新実画面で検査したという意味ではない |
| 2. 生成器スナップと浮遊件数 | 実装・再生成済み。支持体なし0、異常時は生成失敗 | Unity import後の実位置・描画 |
| 3. 正面回廊へ、過密失速を避ける | 48件を回廊へ、割合28.52%。モール以降のrolling100m最大27、全区画回廊密度も63.55以下。新規AABB重なり0 | **2,500m以上・96破壊以上の固定入力ゲートはNOT_RUN** |
| 4. 孤立39件の解消 | 同じ原点距離集計で39→0。サイドの回廊10m圏外64→0 | 質量条件を含む実連鎖・実破壊 |
| 5. orbit看板遮蔽 | sign0を接地支柱上へ上げるコード・データを実装 | **新しい画素寄与と実画像での解消判定はNOT_RUN** |

リスク評価（起きやすさ＋影響）：接触増による失速3＋3＝6、支持体/看板の実画面の不整合2＋2＝4。静的検査と保護確認を行い、上記Native・実画面ゲートを残した。既存の床侵入/Collider近似残差、モール反復問題が直ったとは扱わない。

状態は既存HANDOFFとtasksへ記録。案件のKB参照禁止を優先しTEAM本文は未確認。Studio指定の入口は現catalog未接続として扱った。今回の外部送信禁止に従いLinear・Slack等への書込みはしていない。
