# カニとばし ZIP 受入チェックリスト

## 実行方法
1. ZIP を完全に展開し、index.html のあるフォルダを `build/` に置く（または `CRAB_BUILD_DIR=展開先` を指定）。
2. `npm i -D @playwright/test && npx playwright install chromium`
3. `npx playwright test`（webServer が `python3 -m http.server 8000 --bind 127.0.0.1` を自動起動）
   - 回数変更: `CRAB_THROWS=20`（強・弱それぞれ）、床停止とみなす距離: `CRAB_MIN_DIST=5`
4. 結果: `qa-out/`（スクリーンショット、`throws.json`、`*_console.json`、`results.json`）

## 合格条件（すべて満たして PASS）
- [ ] SHA256 が納品報告と一致（`sha256sum` で手動確認）
- [ ] 起動: 1280×800 と 390×844 の両方で読込→タイトル→「あそぶ」→照準画面に到達
- [ ] 390×844 で launchZone が画面内、横はみ出しなし
- [ ] メインループ: 1投で `lastState.time` が進み、`phase=result` に到達
- [ ] 床停止: 強(power100)20回 + 弱(power55)20回、角度 20/40/55/70°・左右交互で、以下が **0件**
  - `physicsFault` が空でない（floor-tunnel / settle-timeout / world-bound / non-finite＝「安全停止・記録なし」）
  - 発射後 300 秒以内に result へ行かない（止まったまま未決着）
  - 距離 5 m 未満で停止
- [ ] console error / pageerror / requestfailed / HTTP 4xx・5xx が 0 件（全テスト通算）

## 注意
- 投てきは正規のキーボード経路（launchZone で Enter＝チャージ、チャージ完了後 Enter＝発射）と `config.power/angle/direction` の設定で行う。スワイプの手触りは対象外なので、実機タッチ確認は別に行う。
- ヘッドレスは SwiftShader（ソフト描画）で数 fps しか出ない。物理は Unity 内のゲーム時間で進むため判定には使えるが、フレームレート依存の不具合は実 GPU でも確認すること。
- 床貫通の深さ（cm）や距離の短縮は、この ZIP の公開状態からは測れない。必要ならビルド側で `lastState` に最小高さ等を出す。
