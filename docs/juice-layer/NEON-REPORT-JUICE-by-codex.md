# カニ飛ばし・ジュース層 実装レポート

2026年11月25日 JST。対象ブランチは `juice-layer`、基点は `grammar-opening` の `342cd9f5ec6a97817f4a50b21810266d30f909f2`。本レポートを含むコミットが提出対象である。原方式の破壊イベントに、爆散、連鎖音程、粉砕数のポップ、山場の減速と規模別シェイクを接続した。ソース実装とオフライン検証を終えた段階であり、Unity上の見た目・実操作・実聴・実FPSの合格を示すものではない。

既存の `source` は `new-loop-fix` と26件の作業差分を保持していたため、同じリポジトリの別worktree `../juice-layer` に新ブランチを作って実装した。元のブランチや作業差分は切り替えも上書きもしていない。指定された `source/docs/juice-layer/NEON-REPORT-JUICE-by-codex.md` には、本レポートの同一内容の写しを置く。Gitで管理する正本は `juice-layer` ブランチ内の `docs/juice-layer/NEON-REPORT-JUICE-by-codex.md` である。

## 参照と実装範囲

今回の本人指示にある四つの具体要件、現行 `spec.md`、既存の案件HANDOFF、第一手の成果とコードを照合した。視覚トークンv2は `../briefs/kani-visual-re-2026-11-23.md` の本文を読み、破壊を「物の固有色の破片＋白煙＋提灯橙の火花」とした。移植元は `new-loop-fix` の `911bc64f2df223219b74b40c7077ad8e59766e14` 周辺の `PortParticles.cs` で、爆散の三層と速度・サイズの組み立てだけを取り出した。

指定された会議・実例リサーチの二本文は、このMacの `~/workspace/goals/goal-2/hidden_files/` に存在せず、所在を照会したが提出時点で未確認である。本文を読んだとは扱わず、本人が今回の指示で確定した四要件と会議要約の範囲に限定して実装した。Studio指定入口は現在のcatalogで未接続。案件のKB読書き禁止に従い、sakiya-kbのTEAM.mdも未確認である。新しい設計案や新ループは追加していない。

## 破壊位置の爆散

入口は既存の `FlightEventStream` が発行する `destruction` である。物理側で粉砕カウントを増やして `WorldBreakEvent` を積む既存経路から、`ObserveFeelEventR27` に届いた同じ通知を `PresentJuiceBreak` へ渡す。`FlightEventStream.cs`、カウントの更新、接触・破壊判定には変更を入れていない。通知IDを記録し、同一IDの重複では再発生させない。

一破壊は40マークで、固有色の破片24、白煙8、提灯橙 `#FFB347` の火花8とした。破片の寿命は0.55秒、白煙0.65秒、火花0.32秒。初期サイズはそれぞれ0.22m、0.65m、0.09m。カメラの右・上方向へ広げ、破片は12〜21.6m/s、煙は2.2m/s、火花は14〜18m/sを基準に、上方向へ3m/sを足している。移植元の大きく飛び散る反応を保ち、煙だけは短く残る設定である。すべて描画用マークであり、RigidbodyやColliderは生成しない。物理の乱数も消費しない。

色と規模は、準備済みの世界の無傷モデルから読取り専用で収集する。港の素材に `_Family` があればその色を使い、それ以外は頂点色を三角形の面積で重み付けして主色を選ぶ。細かく分割された装飾色が頂点数だけで勝たないようにした。別系統の40パネルはFBXのCPU読取り可否に依存させないため、基点コミットの19個の無傷FBXから主色をオフライン抽出し、40IDの色表を新設した。色表には入力FBXのSHA256を収録し、再生成一致も確認した。元FBX・Importer設定・保護対象metaは変更していない。読めるメッシュの結果は世界準備時にキャッシュし、毎フレーム色を探索しない。

描画は既存の `MashimashiPresentationR27` の一つのメッシュを再利用する。既存破壊キューが出していた汎用粒子は抑え、40マークと二重に発生しないようにした。既存の輪、残留片、対象の見た目の押し戻し、フラッシュの安全制御は残している。

## 連鎖で上がる破壊音

既存のWebオーディオ経路 `sound-pack.js` の `destruction → materialEvent → shot → makeVoice` にピッチ倍率を渡す引数を加えた。素材ごとのWAV、音量、パン、クールダウン、ボイス優先順位は維持している。半音段数は `min(12, 連鎖数−1)`、倍率は `2^(半音段数/12)` で、初回1倍、13破壊目以降2倍を上限とする。連続破壊の間隔が1.5秒を超えたら初回へ戻る。時計は既存AudioContextの時間で、停止・ミュート・再投時には連鎖状態と遅延再生を消す。

元の破壊音には物体の重さによる約0.94〜1.06倍の変化があった。半音上昇がその揺れで逆転しないよう、破壊音の基準を既存範囲の中央値に固定し、そこへ連鎖倍率を掛けている。音色の素材とゲインは変更していない。音声ロード前の既存フォールバックにも第三引数で倍率を渡し、遅延再生には発生時点のピッチを保持する。再生開始前に倍率を設定するため、発音直後だけ元の高さになる処理は作っていない。

## 粉砕数の表示

既存HUDの `destructionValue` だけを `juice-hud.js` に接続した。状態値 `destructionEvent` や結果集計は変更せず、飛行中の表示値が240msで最新値に追いつく。大量同時破壊も複数フレームに分けて表示し、連続更新では現在の表示値から新しい値へ追いかける。結果・ready・世代変更・数値減少では直ちに正しい数へ戻し、表示遅延を結果画面へ持ち越さない。

表示が増えたときは150msで1倍→1.16倍→1倍にする。最大値は時間の35%地点。拡大の支点を左中央とし、要素の横幅を数字に合わせたため、スマホ幅でも左へはみ出さない。Animationは一つを再利用し、requestAnimationFrameも同時に一つまでとした。ポーズ中は補間とポップを止め、再開時に残り時間で続ける。動きを抑える設定、OSのprefers-reduced-motion、演出offでは補間・拡大を止めて正しい数を即時表示する。

## 山場の減速と規模別シェイク

減速の対象は、第一手でheroを持つ店・商業施設の4ID、港の `jackpot` 1ID・`landmark` 2ID、および1.5秒以内の連続破壊が10・20・30…に達したときである。同時破壊で繰り返し延長しないよう、開始間隔を1秒以上空けた。通常破壊には減速を掛けない。旧 `break / glass_break / chain` の毎回の停止要求も無効にし、山場の判定へ一本化した。

値は時間倍率0.25、実時間0.28秒。既存の `PresentationClock` の短い減速区間を使い、グローバルの `Time.timeScale`、物理定数、固定刻み、力、カニの制御には書き込まない。見せる時間が変わるため、壁時計基準での軌跡同一性を検証したとはしていない。停止後の通常速度への復帰、ポーズ・演出offの解除は実コードのオフライン契約で確認した。

シェイクは既存のカメラ計算後にオフセットを足し、次のフレーム冒頭でその分を戻す。`PortCamera.cs` 自体は基点と同一である。破壊の大きさは無傷描画物のbounds最大辺で読む。1.25m未満は0、通常は0.025〜0.07m、目印は0.12m、目玉は0.18m、連鎖節目は0.10m。通常は0.12秒、山場は0.20秒で収める。同程度以下の入力で動作中の揺れを延長せず、大きい山場への昇格だけを許す。終了後にも通常・山場とも少なくとも約0.06秒の間を作り、連続破壊で常時揺れる状態を避けた。演出off・シェイクoff・低モーション・飛行終了では新しい揺れを消す。

第一手で置かれた境界目印6基は、元から静的な描画物で破壊通知を持たない。今回その6基を壊せるようにする変更はしていない。現在壊せる目印は港の役割データに従って反応する。この区別は配置と破壊判定を凍結する条件によるものである。

## 上限と性能設計

同時表示マークは既存の輪・残留片などを含めて512まで。一破壊の40個を加える前に、必要数だけ挿入順の古いマークを消すため、新しい破壊のバーストを残せる。10,000破壊を実際の追加処理へ通し、上限と古い順の排出を確認した。増加が無限に続くリストや、破片ごとのGameObjectは追加していない。

マーク配列は512、頂点・色・インデックスの各配列は132,096要素を初期確保した。既存設定の輪を含む一マークあたりの頂点数を収める余裕があり、配列領域は合計約4.03MiBにマーク等を加えた規模となる。新しい爆散更新のための配列・Listの毎フレーム生成はしない。イベントログや既存オーディオ経路を含めたプロジェクト全体のGCゼロを保証するものではない。

音の上限は既存の総ボイス12、破壊素材ボイス4、遅延予約12を維持した。混雑で音を落とす既存仕様も維持し、すべての破壊が必ず独立して聞こえるとはしていない。HUDは一つのアニメーションと一つの更新予約を再利用する。既存初期化の描画目標は45fpsだったため、演出設定の読込時に `Application.targetFrameRate=60` を指定する。これは目標値であって、WebGL実測60fpsの証明ではない。

## 変更ファイル

既存の制作コードで変更したのは次の7ファイルである。

- `games/flight/crab-flight/Assets/Scripts/FeelDirectorR27.cs`：破壊通知、演出初期化・解除、減速とカメラオフセットの接続。
- `games/flight/crab-flight/Assets/Scripts/MashimashiPresentationR27.cs`：描画用バッファの初期容量。
- `games/flight/web-template/sound-pack.js`：連鎖音程と再生・遅延経路の引数。
- `games/flight/web-template/sound.js`：既存フォールバックのピッチ引数。
- `games/flight/web-template/host.js`：粉砕表示更新の接続。
- `games/flight/web-template/feel.js`：演出設定変更時のHUD解除。
- `games/flight/web-template/index.html`：HUDスクリプトの読込一行。

追加した制作ファイルは `Assets/JuiceLayer/` 内の `JuiceRules.cs`、`JuicePresentation.cs`、`JuiceParticles.cs`、`JuiceVisualCatalog.cs`、`Resources/JuiceLayer/PanelColors.json` とそれぞれの新規meta・フォルダmeta、および `games/flight/web-template/juice-hud.js` である。上記Assetsはすべて `games/flight/crab-flight/` 配下に置いた。

検証用には `tools/juice-layer/typecheck.py`、`contracts.py`、`bake_colors.py` と、`qa/juice-layer/RulesContract.cs`、`juice.spec.cjs`、`playwright.config.cjs` を追加した。記録は `docs/juice-layer/` に型検査JSON・ログ、契約ログ、最終Playwright結果、初回失敗と時計調整時の結果、保護対象の開始時SHA、保護差分確認、HUD部品画像3枚、本レポート、ファイルのSHA256一覧を保存した。既存の `tasks.md` の今回行も更新した。既存案件HANDOFFは従来の一箇所だけへ追記する。

## 検証結果と根拠

Unityは起動・再生・ビルドしていない。インストール済みRoslynと参照アセンブリによる型チェックは127ソース、エラー0、警告18。型チェック後のソースSHAが検査対象と一致することをPlaywright側でも確認した。これはImporter・Shader・Playerの成功を示さない。

`JuiceRules`、実際の爆散追加処理、既存 `PresentationClock` を、小さな数値用Unityスタブと.NET上で実行した契約は33 passed / 0 failed。重複通知、連鎖窓とリセット、10連鎖・クールダウン、0.25倍と0.28秒後の復帰、通常破壊の無停止、揺れの大きさと休止間隔、40個の内訳・色・寿命・512上限・古い順の排出を確認した。FBX色表は19モデル・40IDの再生成一致を確認した。

最終Playwrightは12 passed / 0 failed / 0 skipped。音声グラフのスタブで半音0〜12、上限、間隔切れ、重複、遅延再生のピッチ保持、再投・ミュート・ポーズ時の破棄、既存ゲインを検査した。既存Chromeでは実際のHUD要素を390×844、320×568、844×390で操作し、カウント補間、拡大、画面内の文字領域、ポーズ・再開、再投・結果、低モーションと演出offを確認した。これらはUnityのゲーム画面ではなく、合成状態を渡すHUD部品試験である。

初回はPlaywright専用ブラウザ未導入で失敗したため、インストール済みChromeへ切り替えた。次に試験の仮想時計と実時計が混ざる競合を、時計を固定してからスクリプトを読み込む方法へ修正した。画像を見てポップ時の左端はみ出しを発見したため、製品側の支点と幅を直し、拡大途中の文字領域を検査に加えた。最終の3枚はその修正後の出力を実際に開いて確認した。初回の失敗記録は履歴として残し、最終合格と混同しない。

同じMac環境での実行コマンドは以下。作業ディレクトリは `../juice-layer` である。

```sh
python3 tools/juice-layer/typecheck.py
python3 tools/juice-layer/contracts.py
python3 tools/juice-layer/bake_colors.py --check
env NODE_PATH=/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules npx --offline --prefix ../source/.git/placement-fix-qa/runtime --no-install playwright test --config qa/juice-layer/playwright.config.cjs
```

主な読戻し先は [compile.json](compile.json)、[typecheck.log](typecheck.log)、[rules-contracts.log](rules-contracts.log)、[playwright.json](playwright.json)、[verification.json](verification.json)、[artifact-sha256.json](artifact-sha256.json)。HUD画像は [390×844](hud-fixture-portrait.png)、[320×568](hud-fixture-small.png)、[844×390](hud-fixture-landscape.png) に保存した。画像の空白キャンバスは部品試験の領域であり、ゲームの描画証拠ではない。

## 触っていないものと残る検証

配置JSON・生成器、FlightField.unity、既存26保護ファイル、物理定数、飛行・衝突・破壊の本体、入力、PortCamera、結果画面の項目構成を変えていない。`FlightEventStream.cs`、`CrabFlightGame.cs`、`RestaurantPhysics.cs`、`RigidCrabFlight.cs` も基点と同一である。追加partialの `ReadJuiceVisuals` は既存世界から色・寸法・役割を読むだけで、パネル状態には書き込まない。HTMLは新スクリプト一行を取り除くと基点と同一になる。元のsourceにあった26件の内容は開始時のSHA256とすべて一致し、stage対象外である。区間・エネルギー・ビームなど新ループの仕組みは持ち込んでいない。

Nativeの爆散・色・遮蔽、同時破壊の見やすさ、通常破壊と山場の手触り、実聴、物理iPhone、WebGL実FPS・GC、正式入力による開始→発射→破壊→結果→再投、Game Studio game-playtestはNOT_RUN。実装上の既知の未修正バグを今回の契約では検出していないが、これら未実行範囲を合格とはしない。過去からの床侵入、Collider近似残差、モール反復FAILも今回直した扱いにはしない。

クロエのネイティブ検証では、このコミットの完全な隔離checkoutを用意し、既存の `LoadFeelR27` に `Assets/StreamingAssets/feel.json` を読み込ませる演出初期化を通す必要がある。今回の編集worktreeはsparseであり、そのまま完全Unityプロジェクトとして扱わない。通常破壊が止まらず、小物が揺れず、目玉・壊せる目印・10連鎖だけが山場になること、低モーション・off・ポーズ・再投で残留しないことを確認する。続けて正規スワイプ／タップ入力の一巡、三層の色、連鎖音程、密集時のWebGL実FPSを同じ版で確認すれば、今回のソース提出と実ゲーム検証を結び付けられる。

外部送信、Linear同期、Git push、デプロイ、公開、課金、認証変更、他セッションの再起動は行っていない。今回の提出はローカルのソース実装・コミットと検証記録までである。
