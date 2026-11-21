# resultspace1-r1 受入テスト結果と修正依頼（2026-10-28 JST）

対象: CrabFlight-resultspace1-r1-candidate.zip（SHA256 13872891…633bc 一致）
環境: headless Chromium（Playwright, SwiftShader WebGL2, 約4〜9fps）。投てきは launchZone の Enter（チャージ→発射）経路、config.power=100（強）/55（弱）、角度20/40/55/70°・左右4投ごと交互。
判定: **FAIL**（床の安全停止は 0/40 で再現せず。ただし 40投中 2投が発射されず 300 秒停止、1投が設定より大幅に弱い力で発射されて 3.87 m で停止）

| 項目 | 結果 | 証拠 |
|---|---|---|
| 起動 1280×800 / 390×844 | PASS（タイトル→あそぶ→照準画面） | desktop_0*.png, mobile390_0*.png, *_smoke.json |
| console error / pageerror / requestfailed | PASS（0件） | *_smoke.json, throws_console_errors.json |
| メインループ | PASS（lastState.time が進み result へ到達） | probe_result.png, throws.jsonl |
| 床停止（physicsFault） 強20 / 弱20 | 0/20・0/20 | throws.jsonl |
| 全40投が結果まで到達 | **FAIL** 38/40 | strong#17, weak#9 |
| 極端に短い停止（5 m未満） | **FAIL** 1/40 | weak#10 3.87 m |

## 修正・確認依頼
1. **発射されないまま止まる（2/40）**: strong#17・weak#9 は照準画面（AIM）のまま、ゲーム時間 0 で 300 秒止まった。どちらも左向き4投のあとの最初の「右・20°」投。左→右の切替だけを狙った追加テスト（dirswitch_probe.json）では再現しなかったため、条件は未特定。
   - host.js 87行（launchZone の keydown、Enter の分岐）: `ReleaseLaunch` を送ったあと、Unity 側が受けたかどうかを host が確かめていない。提案: `ReleaseLaunch` の送信後 0.5 秒たっても `lastState.phase==='ready'` のままなら、`aimHint('発射できませんでした。もう一度')` を出してチャージ状態をリセットする（`send('CancelAim')`）。あわせて Unity 側から `launchRejectReason` を state に出してほしい。
2. **前の投げのチャージが残る疑い**: 止まった直後の weak#10 は、設定 power55 に対して state の power が 11.79 で発射され、ガラスに当たって 3.87 m で停止した。Unity 側で `Prepare` / `Configure` / `CancelAim` を受けたときに、チャージ量（chargeEnergy・chargeElapsed）と spin が確実にリセットされるか確認してほしい（C# ソースは ZIP に入っていないため、行番号は特定できない）。
3. **床貫通の深さが外から測れない**: `lastState.height` は 0 で下限が切られている。床停止の受入判定を自動化するために、`minHeight`（貫通量）と `groundStuckSeconds` を state に追加してほしい（host.js 111行の physicsFault 表示はそのままでよい）。
4. **版名の混在**: ラッパーは resultspace1-r1 だが、ビルドは `Build/street3-gap1-r1.*`、state.revision は `crab-flight-street3-gap1-r1`（host.js 22行の qaWrite も同じ）、productVersion は `crab-flight-city2`（host.js 115行）。納品物がどの版かを一目でわかるよう、名前をそろえてほしい。
5. launch_steps.md では URL が `localhost:8000` と `127.0.0.1:8000` の2通り書かれている。`127.0.0.1` に統一してほしい。
6. 参考: この ZIP は「旧比較試作・最新版ではない」と明記されている。ここで PASS した項目も、最新版の床停止が直った証拠にはならない。最新版にも同じ40投テストをかけてほしい（harness-for-lily/）。
