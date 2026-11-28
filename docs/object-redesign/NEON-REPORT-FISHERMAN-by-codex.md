# NEON オブジェクト再設計 第4便：かに漁師の移設

指定のソース修正とオフライン検証を完了。death 2件の中心座標だけを移設し、関連契約の期待値を更新した。Editor／WebGL型エラー0、既存契約1151件とPlaywright37件がすべてPASS。実ゲームで「良い当たり方の直後に即死する」現象が解消したかは、今回のオフライン検証では未判定。

- ブランチ：`object-redesign`
- 基点：`9148354d969404a3070face96d10143dbcdd4d69`
- 実装・契約・検証根拠コミット：`62d89f29b4d7ba5ee7ca431493f0b876d4aff505`
- この報告とコミット読戻し記録は後続の文書コミット。push・公開なし。

## 変更した中心座標

| 対象 | 旧中心 (x,y) | 新中心 (x,y) |
|---|---|---|
| 店内 `restaurant-death` | (22,26) | **(13,26)** |
| 商業施設 `mall-death` | (114,26) | **(95,3)** |

ゲーム本体の変更は [ObjectRules.cs](../../games/flight/crab-flight/Assets/ObjectRedesign/ObjectRules.cs) のこの2行だけ。Z=0、負方向への鏡像生成、その他14個の中心、効果量、形、色、大きさ、横滑り振幅±0.9m・2Hzを含む全動作は不変。作用物16個／片側・鏡像を含め32個も不変。`games/` 全体の差分が同ファイルだけであり、全文が基点の2座標置換とbyte一致することを確認した。[変更境界](fisherman/source-boundary.json)／[ソースSHA256](fisherman/source-sha256.json)。

## 関連契約の更新

旧座標を復元して過去の基点と照合するstory・camera・placecute・brakegroundの期待値に、今回の2座標だけを追加した。Rulesのdeath検査は店内 `(13,26,0)`・商業施設 `(95,3,0)` の厳密一致へ更新。店内の高所条件は保持し、商業施設は今回指定された地面ラインとして扱う。

旧契約の「全deathが標準帯の外」と「全配置の前方破壊帯との空き0.5m以上」は、新しい商業施設座標と両立しない。商業施設だけ期待値を次の実配置に更新し、その他15個の0.5m条件は保持した。契約を削除・skipしていない。

| death | 横滑り全位相を含むX範囲 | Y範囲 | 名目Y=0〜15m帯 |
|---|---|---|---|
| 店内 | 10.26〜15.74m | 24.16〜27.84m | 外 |
| 商業施設 | 92.26〜97.74m | 1.16〜4.84m | 内（指定の低空危険ライン） |

商業施設deathと既存x=92の破壊帯との最小X空きは **0.01m**（95 − 0.9 − 1.84 − (92 + 0.25)）。この値を契約で明示し、0.5m以上あるとは扱わない。中心・大きさ・横滑りの追加調整はしていない。[全16配置の静的点検](fisherman/hit-map.json)。このAABB計算は実軌道・接触回避の保証ではない。

前便の証跡を保持するため、brakeground契約の入力fixtureと出力先を分け、`KANI_BRAKEGROUND_EVIDENCE_DIR` で今回の保存先を指定できるようにした。既存の検査内容・件数・Playwrightの選択条件とタイムアウトは保持した。

## 検証結果

| 検査 | 結果 | 根拠 |
|---|---|---|
| Editor／WebGL条件分岐の型チェック | 136ソース、errors **0／0**、既存warnings 18／17。Effekseerは両方errors0／warnings0 | [ログ](fisherman/typecheck.log)／[コンパイル対象とhash](fisherman/compile.json) |
| Native helper＋実importerのEditor型チェック | errors **0**／warnings **0** | [ログ](fisherman/native-typecheck.log) |
| 既存契約 | **1151 passed／0 failed**。前便1151件と各群同数 | [件数照合](fisherman/verification-summary.json) |
| Playwright | **37 passed／0 failed／0 skipped／0 flaky** | [ログ](fisherman/playwright.log)／[JSON](fisherman/playwright.json) |
| 保護26ファイル | 元source／worktree各26件、作業前後SHA256全一致、commit対象0 | [前](fisherman/protected-before.json)／[後](fisherman/protected-after.json)／[読取方式](fisherman/protected-read-method.json) |
| 保存内容 | 実装コミット85ファイルが作業実体とbyte一致。ソース・証跡manifestも一致 | [読戻し](fisherman/implementation-readback.json)／[証跡SHA256](fisherman/evidence-sha256.json) |

契約内訳は Rules85＋Stage1境界235＋Bridge45＋Juice33＋形状46＋story境界13＋blast258＋blast境界9＋Port30＋camera/hitstop151＋境界10＋顔66＋作用ログ51＋配置境界22＋レビュー入口61＋空投13＋経路境界23＝1151。前便の追加97件も今回の既存契約に含めて維持した。

実行したコマンド・環境変数・終了コードは [commands.json](fisherman/commands.json)。出力先は今回の `fisherman/` に統一し、旧証跡を上書きしていない。`npm_config_offline=true`、既存のローカルPlaywrightとChromeを使用した。

```sh
python3 docs/object-redesign/typecheck.py
python3 docs/object-redesign/story/contracts.py
python3 docs/object-redesign/blastfix/contracts.py
python3 docs/object-redesign/camhit/port-contracts.py
npx --no-install playwright test -c docs/object-redesign/fisherman/playwright.config.cjs
```

最後のPlaywright実行には、Rules／Effekseer／camera／placecute／brakegroundの既存契約実行も含まれる。再実行する場合は `commands.json` の各 `KANI_*_EVIDENCE_DIR` と `NODE_PATH` を設定する。検査中に取得されたPNGは既存HTML部品の合成状態で、移設後のUnity実画面の証拠には数えない。レビュー作用ログもオフラインアダプタによる合成入力であり、自然投げの実測ではない。

## 残る確認

今回指定はオフライン。Unity起動・ビルド・Native実投・正式入力一巡・実ゲーム画面・実聴・iPhone・FPS・Game Studio game-playtestは **NOT_RUN（今回範囲外）**。クロエの同版実プレイで、店内spin／商業施設spin後のご褒美ラインと、商業施設の急降下・低空ラインでの接触を確認する。ゲーム全体の完成・本人採用・公開は判定していない。既知の床侵入・Collider残差・背景反復FAILについても解消を主張しない。

既存tasksと案件HANDOFFへ記録。TEAM.mdは案件固有のKB読取禁止により未確認、Studio指定入口は現行native catalog未接続。iCloud未取得ファイルはOS標準の取得処理で読み取りを回復した。外部送信・Linear／KB更新・push・公開・課金・新認証・他セッション再起動なし。
