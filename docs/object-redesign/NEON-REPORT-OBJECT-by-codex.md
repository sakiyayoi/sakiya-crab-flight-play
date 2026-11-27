# カニ飛ばし オブジェクト再設計 Stage 1 実装報告

2026-11-27 JST。版名 `object-redesign-stage1`。本人の「オブジェクト再設計 Stage 1 実装指示」に基づくソース提出。今回の完了範囲は隔離worktreeでの実装、指定オフライン検証、ローカルコミット、報告書保存。Unity起動・import・ビルド・実ゲームの品質判定・公開は含まない。

## 版と保全

- 基点: `effekseer-proto` / `68c007f59ad1cfaa76dd39a193783a3b216b15da`。
- 作業場所: `../object-redesign`（新規sparse worktree）、ブランチ `object-redesign`。
- 作用・配置・物理入力・状態: `c76f10e6627e327f3af1768e007a4c61a7d9da26`。
- HTML操作と結果: `4e9d35a4df059712320cd5f85c98be1144d4b5d5`。
- 最終配置修正: `4d21982e59179bc3323eef85cb31fc2e0ceed2e6`。店内brakeをy=12へ上げ、手前の帯との空きを全16配置で検査。最終型検査と10件試験はこのコードへ再実行済み。
- 初回契約・試験根拠: `8636cf2c43285ae1f52a8d8483dd69bbc8d52e2e`。本報告・最終検証記録・tasks更新はその後のdocsコミットへ分離する。
- メイン木の `panel_*.fbx.meta` 25件と `FlightField.unity` は開始/終了SHA256が全26件一致。`protected-before.json` / `protected-after.json` に全値を保存。object-redesign側の同26パスも基点のGit blobを維持し、ステージ対象外。
- 実装22ファイルをコミットから読戻し、作業実体と一致。`source-sha256.json` が対象一覧。メイン木のコード、既存effekseer-protoの未コミットtasksは編集していない。指定された `source/docs/object-redesign/NEON-REPORT-OBJECT-by-codex.md` には、この報告書だけを写す。
- 重力、質量算出、solver設定、床処理、基礎材質、速度/角速度安全上限、シーンファイル、既存配置JSON、他区間の配置は変更なし。基礎物理ファイルの不変は契約で照合。

## 実装

`Assets/ObjectRedesign/ObjectRules.cs` が6定義・16配置・破壊帯・入力定数の正本。`ObjectRuntime.cs` は既存の独立PhysicsSceneの固定1/240秒ステップに接続した。作用物の形、実Colliderを同じTransformで動かし、実形状の `ComputePenetration` と剛体の `SweepTestAll` で接触を検出する。接触済みは消費フラグ、Collider無効化、表示無効化で再発動を止め、色付きmarksで飛散させる。移動は物側だけ。カニの位置・姿勢・速度ベクトルを新作用コードから直接代入していない。

作用は `AddForce(..., Impulse)`、急降下は `AddForce(..., Acceleration)`、回転は `AddTorque(..., VelocityChange)` で渡す。`m` はその投の実Rigidbody質量。効果は固定Δv、固定比率、固定目標値で定義し、乱数は使わない。基礎エンジンの安全上限は従来どおり作用後にも適用される。

| 種 | 色 / 形 / 動き | 固定物理パラメータ | ToonHit倍率 |
|---|---|---|---|
| boost | #0000FF / 矢印 / 前後±0.65m・0.8Hz | 現在の速度方向へΔv=24m/s、I=m×24。停止近傍は発射方向 | 2.4 |
| bounce | #00FF00 / バネと天板 / 上下±0.55m・1.2Hz | 上向き目標26m/s、I=m×max(0,26−vy)。既にそれ以上の上昇なら減速しない | 2.8 |
| spin | #FFFF00 / 3枚プロペラ / 1.6回転/秒 | 物理時計で3秒。角速度目標240rad/s（約38.2回転/秒）。軸(0,.28,−.96)正規化×進行符号。bounce目標52m/s、地面等の反発も増幅、接触粉砕判定の入力衝撃量×2 | 3.0 |
| brake | #8000FF / 雲 / 上下±0.35m・0.3Hz | 速度の80%を逆向きインパルスで減衰、残率0.20。I=−m×0.8×v | 2.2 |
| blast | #FF8000 / 樽 / 上下±0.18m・2Hzの発光変調 | 半径9mの有効な破壊対象を既存破壊経路へ渡す。カニへ外向きΔv=30m/s、I=m×30。中心一致時のみ(進行符号,.6,0)を正規化 | 3.6 |
| death | #FF00FF＋黒 / 放射トゲ / 前後±0.9m・2Hz | 力を与える前の優先パスでRESULTへ。solver直後にも破壊処理より先に確認。保留中の作用/破壊を破棄し、physicsFaultではなく通常の投げ終了として扱う | 4.0 |

発光は色×4。blastの変調は0.7〜1.0倍・最大2Hz。reduced-motion/演出offでは定常発光にする。物の動きは当たり判定を伴うゲーム要素として維持する。deathの黒い部分はMaterialPropertyBlockで設定し、余分な所有Materialを増やしていない。

既存 `burstMode` は保持。`marks` は従来どおり、作用物には色と大きさを変えたmarksを使う。`effekseer` では同じ24ハンドルのプールに作用ToonHitを優先再生し、marksを併用する。色は `SetAllColor`、倍率は `SetScale` で渡す。従来の通常破壊のscale=1、上限、優先入替、off/reduced/pause解除は保持。即死だけは結果へ入った瞬間にToonHitを消さず寿命まで残す。ポーズ・off・reduced・次の準備では必ず消す。この例外も契約で検査した。

## 配置

+x側に16個（restaurant 6 / mall 10）。全てz=0。−x側はx符号と形の左右、スライド/回転方向を鏡像化し、yは維持する。ゲーム内の両側合計は32個であり、「14〜20」は指定された+xの試作区間の個数として扱う。

| ID | 種 | x (m) | y (m) | z (m) |
|---|---|---:|---:|---:|
| restaurant-entry | boost | 8 | 3 | 0 |
| restaurant-bounce | bounce | 14 | 2 | 0 |
| restaurant-brake | brake | 18 | 12 | 0 |
| restaurant-spin | spin | 20 | 5 | 0 |
| restaurant-death | death | 22 | 26 | 0 |
| restaurant-exit | blast | 24 | 3 | 0 |
| mall-entry | boost | 32 | 4 | 0 |
| mall-bounce-a | bounce | 43 | 2 | 0 |
| mall-spin-a | spin | 55 | 7 | 0 |
| mall-brake-a | brake | 64 | 9 | 0 |
| mall-boost | boost | 75 | 4 | 0 |
| mall-bounce-b | bounce | 86 | 2 | 0 |
| mall-spin-b | spin | 98 | 7 | 0 |
| mall-brake-b | brake | 108 | 9 | 0 |
| mall-death | death | 114 | 26 | 0 |
| mall-exit | blast | 124 | 4 | 0 |

店入口はboost、商業施設入口もboost、中盤はbounce/spin/boostと高めのbrake、両出口側はblastとした。即死は任意の高所ルートに置く。

静的契約の「標準軌道帯」は x=4.5〜135、カニ中心y=0〜20m、半径方向余裕3.5mと定義した配置用の帯。death中心y=26m、Collider半高1.15mなので下端24.85m > 帯上限＋カニ余裕23.5m、差1.35m。前後移動してもyは変わらない。これは実投から推定した統計ではなく、今回の明示的な静的配置基準である。あらゆる角度や、bounce/jet後の軌道で当たらないという保証ではない。標準40度/勢い70の実PhysX軌道との重ね合わせはクロエ側で確認する。

壊れるだけの対象は `x = 11,17,27,38,49,61,80,92,104,130`、各xでy=2/5/8、z=0、幅0.5×高さ2×奥行2.4mの30個/+x（両側60個）。作用物の後ろを帯にし、その直前を空けた。各作用物の移動範囲と前方の帯の包囲箱について、少なくとも0.5mの隙間があることも契約で確認した。従来の4.5<|x|<135の破壊対象をredesign時だけ非表示・接触無効にする。x=±4.5の生簀ガラスは維持する。背景や静的な区間目印、他区間は変更しない。

帯はtriggerとし、壁solverによる急停止を発生させず、接触バッチごとに `I=−m×0.005×v`（速度残率0.995）の小さい損失を加える。既存の壁用速度回復/天井リダイレクト/破壊時自動加速を帯に適用しない。破壊そのものは既存ApplyBreaks経由なので、粉砕数、素材別集計、イベント、marks/SEへ通る。blastによる破壊も同じ集計へ入り、二重加算しない。

## 飛行操作・結果

噴射は1投2回。進行符号と上向き0.55のベクトルを正規化し、固定Δv=14m/sのインパルス。右下ボタン、J、↑。連続キーリピートを抑え、C#側の残数と保留フラグでも3回目/重複を拒否する。HUDの●○はUnityの残数を表示する。

急降下は押している間のみ追加下向き38m/s²、回数制限なし。左下ボタン、K、↓。pointer capture、pointerup/cancel/lostcapture、keyup、blur、画面非表示、ポーズ、結果/再投の解除経路を実装した。J/Kと矢印の新経路はflight＋redesignだけで受け付ける。ready/chargeのスワイプと既存矢印照準には送らない。redesignでは旧着地プッシュを隠し、旧Space入力も無効にする。legacyの操作経路は維持。

390px縦で118×84px、320px縦で約115×84px、844×390横で110×64px。左右下端に配置し、既存のpaper/ink/gold・書体を使用。タッチ/マウス/キーの操作部品をChromeで実際に確認した。実iPhoneでの片手の体感は未確認。

最大連鎖は既存の実破壊成功箇所で記録する。物理時間1.5秒以内の連続破壊数の最大値。時間窓が切れても最大値を維持し、再投で0へ戻す。両モードで動作し、結果では粉砕数を主役のまま補助数字として表示する。集計は演出offでも動く。death時もそれまでの実破壊/飛距離を結果に残す。

## 検証結果

| 検査 | 結果 | 証拠 |
|---|---|---|
| 実Effekseer 1.80.8参照＋ゲーム133ソース、Editor分岐 | errors 0 / warnings 18（Effekseer側0） | compile.json / Assembly-CSharp-editor.log |
| 同WebGLプリプロセッサ分岐 | errors 0 / warnings 17（Effekseer側0） | compile.json / Assembly-CSharp-webgl.log |
| 実ObjectRulesを実行 | 85 passed / 0 failed | rules-contracts.log |
| 配置・既定legacy・物理接続・保存byte契約 | 232 passed / 0 failed | contracts.json |
| 既存Effekseer実コード契約＋今回境界 | 45 passed / 0 failed | bridge-contracts.log |
| 既存JuiceRules/clock/burst実コード契約 | 33 passed / 0 failed | legacy-juice-contracts.log |
| 新操作/結果/契約のPlaywright | 10 passed / 0 failed / 0 skipped | playwright.json |
| 既存ジュースの音程/音声予約/HUD契約 | 8 passed / 0 failed / 0 skipped | juice-playwright.json |
| 保護26ファイルSHA256 | 26一致 / 0変更 | protected-before.json / protected-after.json |

型検査はインストール済みUnityのRoslynと参照DLLを使用。Unityを起動していない。WebGL検査も条件分岐の型検査であり、IL2CPP/link/import/Shader/WebGLビルドの合格ではない。Effekseer参照cloneは `/tmp/kani-effekseer-1808-reference`、commit `f02f739f5910a4a14aac12a9a4a57022930e163f`。新しいパッケージ導入やmanifest変更は行っていない。

①6定義/色/効果実数、②+x16個、③death帯外、④既存配置JSONの全byte一致＋legacyの全件保持述語、⑤噴射予算/急降下の実入力経路をカバーした。なお④は静的同一性であり、PhysXの旧版と同じ飛行結果を実測した意味ではない。

ベース68c007fはEffekseer初期化をPlayまで遅らせる修正を持っていたが、既存BridgeContractsにはApplicationスタブと遅延初期化の期待が未反映だった。製品挙動を戻さず、テストを現行の「edit時作らない/再生時一度だけ作る」へ合わせた。従来の上限・再利用・優先・解除・モード契約を保持し、edit境界2件と即死結果境界2件を加えて旧41→45件。古い別段階の「コード一切変更なし」スナップショット検査は、今回許可された機能変更に対する判定として使わず、本段階の配置/基礎物理/26保護契約へ置き換えて検査した。

初回UI試験では暗黙headのHTMLへCSSリンクを挿入できず、3サイズでボタン高さ48pxとなる不具合を検出。続く操作試験を中断してCSS参照を修正し、全10件を再実行して合格。初回記録はplaywright-initial.jsonに保持。最終保存時の遅いファイル読取りも完了し、最終10件の結果を記録した。音声試験はグラフスタブであり実聴ではない。PNGは明示ラベル付きの合成状態HTML部品であり、Unityゲーム画面の証拠ではない。

実行コマンド（worktreeルート）:

```sh
python3 docs/object-redesign/typecheck.py
python3 docs/object-redesign/contracts.py
python3 docs/effekseer-proto/contracts.py
NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/playwright.config.cjs
NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c qa/juice-layer/playwright.config.cjs --grep 'existing WAV|fallback receives|audio generation|HUD'
```

## クロエ / Stage 2への引き継ぎ

1. 同コミットの完全なcheckoutを用意する。ここは大きなモデル等を省いたsparse編集worktreeで、そのまま完全Unityプロジェクトと扱わない。既存手順のEffekseer 1.80.8注入とToonHit importが必要。既定feel.jsonはlegacy。試作側の配布/実行用feel.jsonだけ `"objectMode":"redesign"` にし、ToonHitを確認する時は既存 `"burstMode":"effekseer"` も指定する。
2. Unity/PhysXで6種の実接触、重なったdeathの優先、強入力での通過漏れ、3秒spinの物理反発、2回jet、dive解除、blast範囲、同投1回消費、両方向、結果→再投を実操作で確認する。標準帯との照合、床侵入/Collider残差、旧条件による弱20/強20と反例確認、既存ループへの影響を同版で扱う。今回これらはNOT_RUN。
3. 発光とToonHit色/サイズ、各形の読みやすさ、実Colliderと見た目の箱近似、既存背景との関係、反復/密度、FPS/GC、iPhone、実聴、点滅M14を確認する。形はStage 1のプリミティブ造形で、個別の専用VFX作り分けはStage 2。抑制した旧対象は生成自体を省いておらず、初期化コストの削減は今回の成果として主張しない。

既存r22の床一時侵入最大5.26cm、Collider近似残差、モール反復FAILを解消したとは扱わない。新作用の快適さ/採用、ゲーム完成、公開は未判定。push、公開、外部送信、Linear/KB更新はしていない。案件固有のKB禁止に従ってTEAMは未確認。Reason一覧では該当別担当の作業を確認せず、他セッションの再起動なし。既存HANDOFFとtasksへ保存する。今回のClaude構造検証/実例リサーチ成果物はMacで所在未確認であり、本人の具体的な本指示を実装範囲とした。Studio入口snapshotの本文を参照したが現行native catalog/sourceとの照合は未接続。通常の実装・オフライン確認を実施した自己検証のソース提出である。
