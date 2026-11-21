# 鳥・人間モデル/反応パック life-actors-r1

入力: r19 `accef9b02977c7d88b59dac8e240ba311ff28e64`。カニ・共有Scene・共有制御・物理・配置は変更しない。全体統合の単独書き手は既存主担当 `01a15e93-f1d5-7240-ace5-8dfcd16ce02f`。このパックは専用追加folder `Assets/LifeActorsR20/` だけを渡す。

r19には `FarWorldLifeR10` が存在し、48鳥/22人を非接触表示する。鳥は空中の共通形、人は同じ帽子・箱胴体・腕上げ反応で、近接判定を同ファイル内で行う。今回その共有コードは編集せず、置換/接続の判断を主担当へ返す。

## 形と動き

| Kind | 区間の用途例（配置の決定ではない） | 色以外の造形差 | Contact / 近接驚き |
|---|---|---|---|
| HarborGull | 港、屋外 | 大きい翼、長い黄色嘴、細長い白い体 | 大きく羽ばたき、緩い上昇で離脱 |
| CityPigeon | 街、モール外周 | 丸い体・短い嘴・短い翼・扇状尾 | 小刻みに足を動かし、短い間から素早く飛び立つ |
| Chef | 店内 | 高いコック帽、幅ある胴、長いエプロン、フライパン | かばう腕とパン、後方へよろめき、姿勢を戻す |
| Shopper | 商業施設 | 広いコート、髪の塊、片手の大きな袋 | 袋を振り、横へ傾きながら踏み直す |
| OfficeWalker | 街 | 細い胴、コート裾、ネクタイ、小さな鞄 | 上体を捻り、腕を回してバランスを取る |
| Courier | 街/配送口 | つば付きヘルメット、背負い箱、肩ひも | 膝を畳む小さな吹き飛び弧。軽減時は弧と横回転を抑える |

すべて血・傷・破損・ラグドールなし。カニより視覚的に低い優先度の、丸い形と限定palette。モデルの人数は増やさない。衣装色だけ違う同形ではないが、モール内部等の実配置と地域ごとの採用役は未決。

## 専用API

```cs
using CrabLifeActors;
var actor = LifeActorRig.Create(ActorKind.Chef, visualParent, localPosition,
                                yawDegrees: 0, scale: 1, seed: 17);
actor.ResetVisuals(generation);
actor.SetBirdMode(BirdMode.Grounded); // 鳥だけ。Flyingの場合は高度をhostが置く
// 毎frame、hostが所有するgameplay clockの絶対時刻を渡す。
actor.Sample(visualAge, reducedMotion);
// hostが確定した近接/接触/衝撃波の時だけ送る。consumerは衝突を再判定しない。
actor.React(ReactionCue.Contact, visualAge, localDirection, strength, eventId, generation);
```

`Create`のroot localPosition/yaw/scaleは主担当が設定。反応で動くのは子 `Motion offset - render only` とboneだけ。Root/Collider/Rigidbody/得点/回収/破壊は変更・生成しない。位置は左右X、上Y、顔/嘴はlocal −Z。1unit=既存world1unit。人の足は約Y0、Chef帽子上端約2.15unit、他の人は約1.85unit。鳥の足は約Y0、体高は約0.7unit、カモメ翼幅約1.6unit。scaleは表示用0.1..4にclampする。

`localDirection` はactor root座標系。例: `actor.transform.InverseTransformDirection(worldAway)`。Contactの実力積を物理に二重投入しない。strengthは0..1の描画強度で、ゼロでも小さな読める反応を残す。NearbyStartleは同じ役に応じた控えめな身振り、Contact/Shockwaveは強いコミカル反応。鳥は反応後に飛翔を保持するため、hostが消去/プール復帰/Resetを決める。

`eventId`はactorごとに単調増加するhost確定ID。現在generationと違うIDや、処理済み以下のIDは無視。`ResetVisuals(newGeneration)`でID・反応・離脱状態・鳥モードを戻す。その後必要ならSetBirdModeを再指定。pause中はageを進めない。同じageでSampleしても同じposeになる。ループ/再投時はhostのreset eventから呼ぶ。個体ごとのseedはidle位相用で、役ごとの反応型は表の通り別。

`ReactionStarted` は同じ確定cueを受けた時の通知（generation/eventId/kind/cue/time/strength）。必要なら主担当がSE等へ接続する。音源・得点・追加破壊イベントをconsumerが勝手に発行しない。

## 主担当が決める接続点

- `FarWorldLifeR10` を残すか近景分を置換するか。旧表示へ新70体を丸ごと加算しない。
- 近景のactor anchor、地域/Kind選択、床高さ、向き、表示/プール範囲、camera内人数上限。
- 既存hostの確定contact/proximity/destruction eventから、対象actorとlocalDirectionを選択する。
- pause/reset/retry/reducedMotion/世代と時計を既存producerから同期する。
- ColliderやRigidbodyが必要なら主担当の物理側で作る。このfolderには追加しない。
- shader `CrabLifeActors/VertexChitin` は保存Prefabの共有材質から参照する。生成だけで使うbuildではshader strippingを避ける参照をscene/Resources/Always Includedで保持する。

同時近景数は今回の画面見本では鳥1+人1の2体まで。これは配置案の採用ではなく、カニ中央を空けた比較条件。70体の全面置換やGPU FPSの保証はしていない。主担当はactorで前景を埋めず、カニ中心と次の壊す対象への視線を優先する。

## 収録条件と未確認

専用Editor Sceneだけで、6役のidle/side/contact/reducedと連続150frameをNative描画。Editorのskinned pose反映のため、各SampleのboneをUnity BakeMeshでCPU skinningしてNativeで描画した確認用画像。保存Prefabは本来のSkinnedMeshRenderer/bone rigを保持し、baked確認メッシュは入れていない。既存runtimeの接触から自然に反応した映像ではない。

比較旧モデルはr19 `FarWorldLifeR10`の実meshから単体を抽出し、同じ6anchor・同じcamera/lightで表示。確認用の静的正規化あり、実配置数や遠景位置のbefore/afterではない。context画像はr19背景とカニを保った固定確認camera、同じ2anchor/2体。既存far-life batchは両方で隠し、密度条件を一致させる。主担当の共有Sceneは保存していない。

Nativeの非物理・anchor不変、重複ID/旧世代無視、同時計pause、reset、Prefab/meshを確認。WebGL統合、自然接触、主担当の最終配置、スマホ実機、GPU FPS、本人採用、実聴は未確認。担当の納品判断は確認用モデル/反応素材としての範囲。
