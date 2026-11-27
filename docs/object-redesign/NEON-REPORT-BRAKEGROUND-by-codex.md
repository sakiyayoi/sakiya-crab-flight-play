# NEON オブジェクト再設計 第3便：もち地面ライン・レビュー作用ログ

指定のソース修正とオフライン検証を完了。brakeの中心3件だけを地面ラインへ移し、レビュー時に旧helperを再注入して作用ログが消える経路を修正した。Editor/WebGL型エラー0、既存契約1054件維持、追加97件、Playwright37件が全PASS。実ゲームの新軌道・接触結果は本報告の検証範囲に含めない。

- ブランチ：`object-redesign`
- 基点：`8b3e4211c2299dafcd65e14418fc1a2439da02bd`
- 実装・契約・検証根拠コミット：`0623741eb505ecb3cdcaa649049f2777edc08f4d`
- この報告は上記コミットの検証結果。報告の保存コミットは後続。push・公開なし。

## 変更した中心座標

| 対象 | 旧中心 (x,y) | 新中心 (x,y) |
|---|---|---|
| 店内 `restaurant-brake` | (19,20) | **(21,2.5)** |
| 商業施設A `mall-brake-a` | (64,20) | **(58,3)** |
| 商業施設B `mall-brake-b` | (108,20) | **(108,3)** |

Z=0、既存の負方向への鏡像方式は不変。[ObjectRules.cs](../../games/flight/crab-flight/Assets/ObjectRedesign/ObjectRules.cs) の変更はこの3行だけ。brake以外の13中心、作用物16個/片側（鏡像を含め32個）、効果量、形、色、大きさ、浮動・回転の規則、カメラ、ヒットストップは不変。その他の追跡済みゲームAssetsは基点と変更なしを契約で確認した。

既存の全16配置点検と契約期待値を更新。1.6倍とfloat振幅を保持したbrakeの全位相AABBは、店内Y=0.31〜4.69m、商業施設A/BはY=0.81〜5.19m。[配置点検](brakeground/hit-map.json)。これは静的な範囲計算であり、カニの標準軌道・跳ね軌道との非接触を実証したものではない。

## action-events.jsonl が出なかった原因

`/tmp/build-object.sh` は `git archive object-redesign` で最新版を展開した後、Nativeレビュー直前に次を実行していた。

```sh
cp /tmp/PortNativeReviewOpening.cs $HOME/kani-object-build01/games/flight/crab-flight/Assets/PortPrototype/Editor/PortNativeReview.cs
```

旧 `/tmp/PortNativeReviewOpening.cs` には `WriteActionEvents` がない。実際のbuild copyのhelperがこの旧ファイルとbyte一致し、旧レビュー出力 `evidence/object-review/camon` には `route.csv` や `break-events.jsonl` はあるが `action-events.jsonl` がないことを確認した。[原因の読戻し](brakeground/cause-evidence.json)／[修正前driver](brakeground/before-build-object.sh.txt)／[再注入された旧helper](brakeground/before-PortNativeReviewOpening.cs.txt)。

Git側の現行helperには、消費済みの `sim.ObjectHits` を `game.Prepare("")` の前に書き出す処理が既にある。欠落の原因はwriterの発火条件ではなく、実行するhelperの差し替えだった。前便の51件はwriter単体とソース接続の確認で、ビルドdriver→レビュー入口→出力先の実行契約がなかった。

## 修正したレビュー経路

1. ローカル `/tmp/build-object.sh` の旧helperコピーを除去した。元本文を保存し、今回の2箇所の差分を [再適用用patch](brakeground/build-object-review-fix.patch) と [前後SHA256](brakeground/build-driver-readback.json) に残した。driver自体は実行していない。
2. 同じarchiveに含まれる [run_native_review.py](../../tools/object-redesign/run_native_review.py) から `PortNativeReview.Run` を実行するようdriverの呼出しを変更した。runnerは旧helperをUnity起動前に拒否し、既存出力への上書きを拒否する。実行後は `camon/action-events.jsonl` の存在と全行のkind/position/time、時刻順、有限数値、既存のcamon再投receiptを読み戻す。ログ欠落時は非0終了にし、レビュー成功扱いを防ぐ。
3. 本番 `PortNativeReview.cs` は基点とbyte不変。入力、seed42043、撮影、カメラ、ループ、結果、再投の動作を変更せず、既存writerを正しく届ける修正とした。

確定した出力契約は **`PORT_REVIEW_OUTPUT/camon/action-events.jsonl`**。1行1作用、`kind` は小文字6種、`position` は消費時点の世界座標x/y/z、`time` はシミュレーション秒。32件のUIリングではなく全消費履歴から書き、0件でも空ファイルを作る。

新しい契約は本番helper全文をコンパイルし、runner→オフライン実行アダプタ→本番 `Run` →本番 `RunOne` →本番writerまで実行する。48件のログと0件の空ログを実際に保存・読戻しし、再投が消費履歴を消す前に保存済みであること、全件の種別・位置・時刻・順番を確認した。旧helperの再注入とログ欠落の反例は拒否を確認した。

- [48件の実出力ファイル（合成入力のオフライン試験）](brakeground/offline-review-hits/camon/action-events.jsonl)
- [0件の実出力ファイル（オフライン試験）](brakeground/offline-review-empty/camon/action-events.jsonl)
- [入口の61件](brakeground/review-entry-hits.log)／[空投の13件](brakeground/review-entry-empty.log)／[経路・失敗検知23件](brakeground/review-contracts.json)

これらの48件は試験アダプタの消費履歴であり、クロエの自然投げやUnity PhysXの新しい実測ではない。シリアライズは試験用System.Text.Jsonアダプタを使い、実Unity APIのhelperは別途Editor型チェックした。試験アダプタのダミー撮影は画像証拠として保存していない。

## 検証結果

| 検査 | 結果 | 根拠 |
|---|---|---|
| Editor / WebGL条件分岐の型チェック | 136ソース、errors **0 / 0**。既存warnings 18 / 17。Effekseer両方errors0/warnings0 | [ログ](brakeground/typecheck.log)／[コンパイル対象hash](brakeground/compile.json) |
| Native helper＋実importerのEditor型チェック | errors **0** / warnings **0** | [ログ](brakeground/native-typecheck.log) |
| 既存契約14群 | **1054 passed / 0 failed**、前便と件数同一 | [件数照合](brakeground/verification-summary.json) |
| 追加レビュー契約 | **97 passed / 0 failed**（61＋13＋23） | [入口・出力契約](brakeground/review-contracts.json) |
| Playwright最終一括実行 | **37 passed / 0 failed / 0 skipped / 0 flaky** | [ログ](brakeground/playwright.log)／[JSON](brakeground/playwright.json) |
| 保護26ファイル | source実体26 / branch実体またはsparse Git26、作業前後SHA256全一致、commit対象0 | [前](brakeground/protected-before.json)／[後](brakeground/protected-after.json)／[読取方式](brakeground/protected-read-method.json) |
| その他ゲームAssets | 基点からの変更はObjectRulesの3中心のみ | [境界契約](brakeground/review-contracts.json)／[対象ソースSHA256](brakeground/source-sha256.json) |

既存1054件の内訳：Rules85＋Stage1境界235＋Bridge45＋Juice33＋形状46＋story境界13＋blast258＋blast境界9＋Port30＋camera/hitstop151＋境界10＋顔66＋作用ログ51＋配置境界22。件数・閾値を減らしていない。

実行コマンドは [commands.json](brakeground/commands.json)。各 `KANI_*_EVIDENCE_DIR` を `docs/object-redesign/brakeground` に設定し、既存証跡を上書きしない。

```sh
python3 docs/object-redesign/typecheck.py
python3 docs/object-redesign/story/contracts.py
python3 docs/object-redesign/blastfix/contracts.py
python3 docs/object-redesign/camhit/port-contracts.py
python3 docs/object-redesign/brakeground/contracts.py
NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/brakeground/playwright.config.cjs
```

初回Playwrightは既存36PASS・新規1FAIL。新規契約で `/var` と正規化後の `/private/var` を文字列比較した誤検出を、同一実体パスの比較へ修正した。出力ファイル自体と61入口検査は初回から成功していた。[初回ログ](brakeground/playwright-initial.log) を保持し、修正後の一括実行37PASSを最終結果とした。

既存契約が読むiCloud未取得92ファイルはOS標準のダウンロードAPIで実体を復元し、全件HEADのbyteと一致を読戻した。[復元読戻し](brakeground/hydration-readback.json)。タイムアウトや合格閾値を変更していない。保存したPlaywright PNGは既存HTML部品の合成状態で、今回の3D配置・実ゲームの視認性確認には数えない。

## 残る確認と再開方法

今回の指定はオフライン。Unity起動、再ビルド、Native固定入力、実ゲームの画面・正式入力一巡、実聴、iPhone、FPS、Game Studio game-playtestは **NOT_RUN（今回範囲外）**。標準投げが3brakeを回避し、急降下・地面這いだけで遭遇するかはクロエの同版Native再検証に残る。既存の床侵入・Collider残差・背景反復FAILの解消は主張しない。

次のNativeレビューでは、同コミットを含む完全なbuild copy（既存Effekseer導入・redesign設定済み）を使い、そのcopyに含まれるrunnerを呼ぶ。旧 `/tmp/PortNativeReviewOpening.cs` をコピーしない。例のパスは新規出力先へ置き換える。

```sh
PORT_REVIEW_INPUT=/absolute/path/to/accepted-input.json \
PORT_REVIEW_OUTPUT=/absolute/path/to/new-review-output \
python3 tools/object-redesign/run_native_review.py --log-file /absolute/path/to/new-review.log
```

期待出力は `new-review-output/camon/action-events.jsonl` と既存route/receipt。runnerが欠落や不正な行を検出した場合は非0終了する。現在残っている旧build copy・旧レビュー成果物は今回変更していない。更新済みローカルdriverの再ビルド・配信・Tailscale設定処理も今回未実行。

既存tasks/HANDOFFに記録。TEAM.mdは案件固有KB読取禁止のため未確認、Studio入口は現行native catalog未接続。外部送信、Linear/KB更新、push、公開、課金、新認証、他セッションの再起動なし。ソース修正・オフライン検証の完了であり、ゲーム全体の品質合格・本人採用・公開とは区別する。
