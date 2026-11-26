# カニ飛ばし Effekseer 試作の実装報告

2026年11月27日 JST。`juice-layer` の指定tip `e5aa6d1d6e240d7ad5c53ecfc0d2eeefbf3ee6a9` を基点に、別worktree `../effekseer-proto` とブランチ `effekseer-proto` を作り、破壊時の視覚バーストだけをToonHitへ切り替えるコードを実装した。このレポートを含むコミットがソース提出版である。Unityの起動・インポート・再生・ビルドは本人指示により実施していない。今回完了した範囲はコード、指定素材の保存、オフライン検証とローカルコミットであり、実ゲームの描画品質合格や公開ではない。

現行spec.md、案件AGENTS.md、既存task-4/HANDOFF.md、juice-layerの提出報告と実コードを読んだ。元sourceは `new-loop-fix` のまま残し、既存の25個のFBX metaとFlightField.unityの計26差分を開始時と終了時のSHA256で照合した。Reason Networkの一覧ではこの作業セッションだけを確認し、別担当の再起動や重複実行は行っていない。Studio指定入口は現行catalogで未検出、TEAM.mdは案件固有のsakiya-kb読書き禁止に従って未確認。VM側の調査正本はこのMacでは未読で、今回の本人指示に記載された調査結果と、以下の公式ソース実物を根拠にした。視覚v2の既存トークンは維持し、今回は本人指定のToonHit原素材による比較試作として保存した。ToonHitの配色・質感が最終採用されたとは扱わない。

公式 [EffekseerForUnity_Release tag 1808](https://github.com/effekseer/EffekseerForUnity_Release/tree/1808) を `/tmp/kani-effekseer-1808-reference` に取得した。実際の参照commitは `f02f739f5910a4a14aac12a9a4a57022930e163f`、package.jsonは1.80.8である。参照用cloneはリポジトリ外で、パッケージ本体、DLL、Shader、URP/HDRP部品はコミットしていない。`Packages/manifest.json` も基点から変更していない。型参照があるため、marksモードでもビルドには指定のパッケージ注入が必要になる。

切替は既存の `StreamingAssets/feel.json → Web側feel読込 → LoadFeelR27 → JsonUtility.FromJson<FeelConfig>` に接続した。`FeelConfig.burstMode` のコード既定値は `"marks"` で、正確に `"effekseer"` が指定された場合だけ新しい描画経路を使う。フィールド省略、null、空文字、不明値は従来マークの経路になる。追跡中のfeel.jsonは一切変更していない。試作配信コピーの既存JSONのルートに `"burstMode": "effekseer"` を追加すれば切り替わり、`"marks"` に戻すかフィールドを削除すれば従来の40マーク爆散へ戻る。これは演出presetの選択とは別で、マシマシ／ふつう／オフの設定は保持する。JSON読込後のUnity上の実挙動はクロエの確認対象である。

接続点は既存 `ObserveFeelEventR27` がdestruction通知を受ける `PresentJuiceBreak` である。同一通知の重複排除と役割情報の読取りをそのまま通し、従来の `JuiceParticles` 呼出部分だけを `PresentJuiceBurst` に移した。effekseerでは `EffekseerSystem.PlayEffect(effectAsset, e.position)` を呼び、イベントが持つワールド座標をそのまま渡す。従来マークとの二重再生はしない。既存の衝撃波リング、残留片、フラッシュ制御は残り、音、音程、粉砕数ポップ、連鎖の数え方、山場の局所ヒットストップ、規模別シェイクも既存juice層の処理を通る。バーストが混雑で間引かれても、これらの後続処理は中断しない。

初期化では `LoadFeelR27` 内の `ConfigureJuiceBurst` がゲーム配下に `EffekseerBurstBridge` を一つ作る。公式 [EffekseerRuntime.cs](https://github.com/effekseer/EffekseerForUnity_Release/blob/1808/Assets/Effekseer/Scripts/EffekseerRuntime.cs) はBeforeSceneLoadで共有Runtimeを作り、そのAwakeとOnEnableでSystem／Rendererを初期化する実装だった。通常はこの共有Runtimeを再利用する。Systemが無効なら既存Runtimeを探して有効にし、RuntimeとSystemがともに存在しない場合だけ、Native設定のRuntimeを実行時に生成する。SystemはMonoBehaviourではないのでAddComponentしない。Built-inの描画はパッケージ側NativeRendererが登録する `Camera.onPreCull` を使うため、シーン編集もカメラ本体の変更も追加Rendererコンポーネントも不要である。公開APIでNative設定とSystem／Rendererの存在を確認し、不足は警告にする。パッケージ自身の自動Runtime生成はmarksモードでも行われるため、marksでパッケージの初期化コストがゼロになるという意味ではない。

素材は `/tmp/efk-sample/ToonHit.efkefc` とParts一式を、`Assets/Resources/Effekseer/ToonHit/` 配下に相対構成と内容を変えずコピーした。32ファイル、合計1,753,481 bytes。ToonHit本体は10,131 bytes。公式ツール1.80.8同梱の02_Tktk03由来という提供情報を記録し、自作素材には数えない。全ファイルのSHA256は [sample-manifest.json](sample-manifest.json) に収録した。efkefcのBIN_部分を公式ソースと同じ形式で読み、直接参照するテクスチャ6件、モデル1件、材質2件がすべて存在すること、音声参照が0件であることを確認した。Partsの未使用ファイルも指定どおり残した。

インポータの前提には補足が必要だった。[Editor/EfkFileImporter.cs](https://github.com/effekseer/EffekseerForUnity_Release/blob/1808/Assets/Effekseer/Editor/EfkFileImporter.cs) はメニューからefkと依存資源をコピーする補助で、efkefcの自動処理本体は [EffekseerAssetPostProcessor.cs](https://github.com/effekseer/EffekseerForUnity_Release/blob/1808/Assets/Effekseer/Editor/EffekseerAssetPostProcessor.cs) にある。これは資源を先に処理し、EFKEファイルからBIN_を取り出して `EffekseerEffectAsset.CreateAsset` を呼ぶ。生成先は拡張子を変えた同じ場所の `ToonHit.asset`、依存パスはそのディレクトリを基準に解決される。このため指定どおり `Resources.Load<EffekseerEffectAsset>("Effekseer/ToonHit/ToonHit")` で読む。生成されるasset／Shader／Importerのmetaは今回手作業で偽造していない。クロエの隔離ビルドコピーでパッケージを注入してインポートし、ToonHit.assetとPartsの生成資源・参照が実在してからビルドする必要がある。読み込めない場合は一度だけ警告し、従来マークへ黙って代替せず再生失敗を記録する。

再生枠の上限は `EffekseerBurstBridge.ActiveLimit = 24`。粒子の個数ではなく、このブリッジが所有する `EffekseerHandle` の `exists` を毎Updateと再生前に確認し、終了したものを回収する。満杯時は小物を含む通常破壊をすべて間引く。目玉hero、破壊可能な目印landmark、既存juiceの1.5秒窓で10・20・30…連鎖となった節目は優先し、最古の通常ハンドルをStopして新しい山場を再生する。全24本が山場なら最古の山場を置き換える。このため山場を上限によって拒否せず、追跡する同時再生枠も24以内に保つ。ただし演出off／低モーション、資源未読込、Native側の再生失敗を越えて再生する保証ではない。Stopの反映フレームやNative粒子プールの挙動、実負荷は未実測である。静的境界目印6基は元から破壊通知を持たず、今回壊せるようにはしていない。

サイズの調整場所は `Assets/JuiceLayer/EffekseerBurstBridge.cs` の `BurstScale = 1.0f` である。再生直後に `handle.SetScale(Vector3.one * BurstScale)` を適用する。ToonHitの生成assetのScale初期値も公式インポータでは1.0で、1〜10m級の物に対する見え方の最終調整はクロエのキャプチャ確認後とした。時間グループはUnscaleで、既存juiceの局所スローに合わせてバースト寿命を引き延ばさない。

ポーズ、演出off、低モーション、結果への移行、再投・リセット、ブリッジの無効化／破棄では、このブリッジ所有のハンドルだけをStopする。共有System全体のStopAllEffectsや破棄は行わない。ポーズ解除時に途中のバーストを復元せず、従来マークと同じく次の破壊から再生する。低モーションではToonHitを新規再生せず、実行中のものも消す。既存フラッシュの安全ゲートは変更していないが、ToonHit内部の明滅はそのゲートの計数対象に自動では入らない。M14の1秒3回制限を新素材込みで満たすかは実映像での確認が必要であり、今回合格扱いにはしない。

制作コードの変更は `Assets/Scripts/FeelConfig.cs`、`Assets/Scripts/FeelDirectorR27.cs`、`Assets/JuiceLayer/JuicePresentation.cs` の3既存ファイルと、新規 `Assets/JuiceLayer/EffekseerBurstBridge.cs`、`Assets/JuiceLayer/JuiceBurst.cs` および各cs.metaである。以上のAssetsはすべて `games/flight/crab-flight/` 配下を指す。ほかに指定のToonHit配下32素材、本 `docs/effekseer-proto/` のレポート・型検査・契約試験・保存根拠をコミットする。既存tasks.mdの今回行と案件HANDOFFはローカル記録として更新したが、最新指示のコミット対象限定に従いtasks.mdはステージしない。新しい管理台帳は作っていない。

オフライン型検査では、公式パッケージの実ソースをまず独立したEffekseerアセンブリへコンパイルし、それを参照してゲーム側129ソースを検査した。Editor分岐はパッケージerrors 0／warnings 0、ゲームerrors 0／warnings 18。WebGL条件分岐もパッケージerrors 0／warnings 0、ゲームerrors 0／warnings 17。WebGL分岐の型検査は、上流ソースのUnityEditor名前空間importのためインストール済みEditor参照DLLを残しており、Unity Playerビルド・IL2CPP・ストリッピングの代替ではない。初回は旧参照リストにVRModuleがなくエラーになり、インストール済み参照を追加した。次に非公開RendererType参照を公開設定APIへ修正した。初回ログは同ディレクトリの `initial-missing-vr-reference/` と `initial-api-access/` に保持した。最終結果と入力ソースSHAは [compile.json](compile.json) にある。

契約試験はブリッジ、呼出元のPresentJuiceBreak、切替、連鎖ルールを実コードのまま.NET上で実行した。UnityとNative呼出先だけをメモリ上の代役にし、24枠・10,000破壊後の上限、優先入替、終了回収、座標とスケール、重複通知、hero／landmark／連鎖節目の接続、ポーズ／off／低モーション／再投／結果／破棄を確認した。新規41 passed / 0 failed、既存juiceルール・マーク・時計も33 passed / 0 failed。Playwrightからこれらの契約、素材参照、保護差分、型検査対象SHA、feelの初期化／解除接続を検査し、3 passed / 0 failed / 0 skipped。これはブラウザを開かないオフライン試験で、実ゲームの入力・表示合格ではない。根拠は [bridge-contracts.log](bridge-contracts.log)、[legacy-juice-contracts.log](legacy-juice-contracts.log)、[playwright.json](playwright.json)、[verification.json](verification.json) に保存した。

再現コマンドの作業ディレクトリは `../effekseer-proto`。型検査には上記の公式1808 cloneとインストール済みUnity 6000.3.23f1のRoslyn／参照DLLを使用し、Unityプロセスは起動しない。

```sh
python3 docs/effekseer-proto/typecheck.py /tmp/kani-effekseer-1808-reference
python3 docs/effekseer-proto/contracts.py
python3 docs/effekseer-proto/verify.py
env NODE_PATH=/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/.git/placement-fix-qa/runtime/node_modules npx --offline --prefix ../source/.git/placement-fix-qa/runtime --no-install playwright test --config docs/effekseer-proto/playwright.config.cjs
```

配置、物理、衝突／破壊判定、FlightEventStream、CrabFlightGame本体、PortCamera、結果画面、音声／HUDのWebコード、StreamingAssets/feel.json、Packages/manifest.json、FlightField.unity、保護26ファイルは変更していない。`protected-before.json` とverificationで元sourceの26件の内容一致を読戻した。配布WebGL、旧比較版、公開版も未変更である。元sourceへ置くのは指定された `docs/effekseer-proto/` の報告・検証記録の同一内容の写しだけで、そこを別の実装版とは扱わない。ブランチ内の正本は `effekseer-proto/docs/effekseer-proto/NEON-REPORT-EFKESEER-by-codex.md`、本人指定の読戻し用写しは `source/docs/effekseer-proto/NEON-REPORT-EFKESEER-by-codex.md` である。

この編集worktreeは疎な作業用コピーで、そのまま完全なUnityプロジェクトとしてビルドしない。最初のGit展開が遅延したため、この作業自身の展開プロセスだけを中断し、指定基点と同一の既存juice-layerファイルから必要コードを用意した。保護対象や他セッションを停止・再起動していない。クロエは提出commitの完全な隔離checkoutへRelease 1.80.8のAssets/Effekseerを `Packages/EffekseerForUnity/` として注入し、資源インポートを確認する。そのうえで配信コピーだけburstModeを切り替え、marksとeffekseerを同条件で比較する。

クロエへ残す確認は、UnityでのResources読込みと全依存資源・Shaderの取り込み、Built-in NativeRenderer／WebGLリンクと描画、普通の正式入力で開始→発射→破壊→結果→再投、密集時と山場の24枠入替、1〜10mの物との大きさ・奥行き・遮蔽、ポーズ／off／低モーション、ToonHit内部も含む点滅、実FPS／GC、実聴、物理iPhone、Game Studio game-playtestである。これらはすべてNOT_RUN。実画面のスクリーンショットや試遊URLは今回生成していない。過去の床一時侵入、Collider近似残差、モール反復FAILも解消扱いにはしない。外部送信、Linear同期、Git push、デプロイ、公開、新規認証、課金は行っていない。
