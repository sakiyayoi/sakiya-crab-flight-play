# テスト用フック（ゲームに入れるもの）— 名前はこのとおりに。変えない

ゲート（`harness/tests/feel.spec.js`・`flow.spec.js`）はこの名前で読みます。名前が違うと ✕ になります。
今のゲーム（r1 の host.js で確かめたもの）にすでにあるもの：`body[data-view]`（loading／title／aim／flight／result）、`#start` `#titleOptions` `#launchZone` `#retry` `#prepare`、トップレベルの `lastState`・`config`・`configure()`。
**r23 以降で名前を変えていたら、ゲームを戻すのではなく `harness/tests/kani-helpers.js` だけを直し、【提出】の「変えた値」に書く。**

## 1. `window.__kaniTest`（URL に `?test=1` があるときだけ作る。本番の URL では作らない）

| 名前 | 形 | 中身 |
|---|---|---|
| `ready` | true／false | フックが使えるようになったら true（Unity の読み込み後） |
| `version` | 文字列 | 版名。リポジトリ直下の `VERSION` と同じ文字（例 `kani-r30`） |
| `events` | 配列 | 出た演出の記録（下の「記録 1 行の形」）。古いものから捨てて最大 5000 行 |
| `clear()` | 関数 | `events` を空にし、`cameraMaxOffset()` も 0 に戻す |
| `settings()` | 関数 → `{preset, shake, reducedMotion}` | `preset` は `'mashimashi'`／`'futsuu'`／`'off'`。`shake` は true／false（画面揺れ）。`reducedMotion` は OS の「視差効果を減らす」を読んだ結果 true／false |
| `setSettings({preset?, shake?})` | 関数 | 設定画面と**同じ道**で切り替え、同じように保存する（localStorage）。再読み込み後も残る |
| `resolvedFeel(preset)` | 関数 → 表 | その preset で実際に使う値（feel.json を読んで倍率をかけた後）。下の「resolvedFeel の形」 |
| `fire(event, opts)` | 関数 | 物理を動かさずに、その出来事の演出だけを**本番と同じ FeelDirector で**出す。下の「出来事の名前」 |
| `cameraMaxOffset()` | 関数 → 数（m） | `clear()` から今までで、カメラが元の位置から一番ずれた距離（揺れ＋蹴り）。揺れオフなら 0 |
| `fps()` | 関数 → `{now, min, avg, samples}` | 今の fps、`clear()` からの最低・平均、数えたフレーム数 |

Unity から JS へは `.jslib` に `KaniTestPush(json)` を作り、`window.__kaniTestPush(json)` を呼んで `events` に足す。
JS から Unity へは今の `send()`（`SendMessage('CrabFlightGame', …)`）を使う：`fire` は `send('TestFire', JSON)`、設定は `send('SetFxPreset', …)`・`send('SetShake', …)`。

### 出来事の名前（`fire` の event と、記録の `event`）
`charge_stroke` `charge_ready` `launch` `flight_fast` `glass_break` `break`（opts `{material}`）`chain`（opts `{chain: 2|4|7…}`）`ground_hit`（opts `{part: 'shell'|'claw'|'leg'}`）`push_perfect` `push_good` `push_miss` `new_record` `result`

### 記録 1 行の形（`events` の 1 つ）
```
{ t: performance.now() の ms, event: "glass_break", occ: 同じ 1 回の出来事で同じ整数, chain?: 連鎖数, fx: "hitstop", effect?: 1〜7（①〜⑦のとき）, …fx ごとの値 }
```
1 回の出来事（例 ガラス突破 1 回）で出た演出は全部、同じ `occ` にする。`t` は実時間（スロー中も実時間）。

| fx | 値 | 書くとき |
|---|---|---|
| `hitstop` | `frames`（60fps 基準）、`timeScale`（0 にしない） | ヒットストップを始めたとき |
| `slowmo` | `seconds`、`timeScale` | スローを始めたとき（⑦。ヒットストップが終わってから） |
| `shake` | `amp`（m）、`decayMs`、`kick`（m） | **安全の決まりをかけた後の値**。揺れオフ・視差効果を減らすなら `amp:0, kick:0` |
| `flash` | `colors`（例 `["#FFFFFF","#FFE14A"]`）、`frames`、`target`（`'object'` 物が光る／`'screen'` 画面） | 実際に光らせたとき。**白→黄の 2 段は 1 行（colors に 2 色）＝点滅 1 回** |
| `flash_skipped` | `reason`（`'rate'` 1 秒 3 回を超えた／`'reduced'`／`'off'`） | 点滅を出さなかったとき（点滅には数えない） |
| `particles` | `kind`（`water` `bubble` `glass` `wood` `dust` `star` `confetti` など）、`count` | 粒子を出したとき（実際に出した数。スマホで減らしたら減らした後の数） |
| `squash` | `pct`（%） | 伸び縮み（C4） |
| `fov` | `from`、`to`、`ms` | FOV を動かしたとき |
| `shockwave` | `effect: 1` | ①衝撃波リング |
| `speedlines` | `on`（true／false）、`effect: 2` | ②スピードライン |
| `popup_number` | `value`（+1 なら 1、連鎖 n なら n）、`effect: 3` | ③壊した数のポップ |
| `afterimage` | `on`、`effect: 5` | ⑤残像 |
| `chain_text` | `chain`、`peak`、`settle`、`text`、`effect: 6` | ⑥連鎖の吹き出し |
| `voice` | `id`（ファイル名から拡張子を取ったもの。例 `glassbreak_kani_tsuntsun`） | 声を鳴らしたとき |
| `knockback` | `dist`（m） | 壊れた物が後ろへ（C6） |
| `debris_keep` | `seconds` | 残骸を残す（C5） |
| `countup` | `ms` | 結果の飛距離カウントアップ（C8） |

④色つきフラッシュは `flash` の `colors` に白以外が入っていれば ④（`effect: 4` も付ける）。⑦は `slowmo`＋`voice`（`effect: 7`）。
**演出：オフ**では、`voice` と `flash_skipped` 以外の行を出さない（実際に何も出さない）。

### `resolvedFeel(preset)` の形
出来事の名前ごと（連鎖は `chain_2` `chain_4` `chain_7`）に：
```
{ hitstopFrames, shakeAmp, shakeDecayMs, kick, flash: true|false, particles: 数 か [最小, 最大], squashPct, fovTo, slowmoSec, slowmoTimeScale }
```
無い値は書かない。ゲートは「記録に出た値 ＝ この値」を見る（値の直書きが無いことの確かめ。E1）。揺れオフのときも、ここはオフにする前の値でよい（記録の `shake` は 0 にする）。

## 2. 画面の id（設定・クレジット）
| id | 中身 |
|---|---|
| `#fxPreset` | 「演出」の 3 択。中に `[data-value="mashimashi"]`「マシマシ」、`[data-value="futsuu"]`「ふつう」、`[data-value="off"]`「オフ」。選んでいるものに `aria-checked="true"` |
| `#shakeToggle` | 「画面揺れ」オン／オフ。`role="switch"`、`aria-checked="true|false"`、文字に「画面揺れ」 |
| `#creditsButton` | タイトル画面からクレジット画面を開くボタン |
| `#credits` | クレジット画面。「VOICEVOX:ずんだもん」を含む |
| `#debugFpsCopy` | `?debug=1` のときだけ。押すと fps の記録 JSON（`evidence/fps_mobile.example.json` の形）をコピー |
設定画面は今の `#titleOptions` から開く。旧「演出の動き」（`#motion`）は消す。

## 3. feel.json（`StreamingAssets/feel.json`、ZIP に入れる）
```
{ "version": "feel-2026-10-28a",        ← 変えるたびに上げる。spec.md に「feel.json 版：<version>」と書く
  "defaultPreset": "mashimashi",          ← 固定
  "targetFpsMobile": 30,                  ← リリーが決める（スマホの fps の目標）
  "mobileParticleMinRatio": 0.6,          ← スマホで粒子を減らしてよい下限（リリーが決める）
  "presets": { "mashimashi": {…}, "futsuu": {…}, "off": {…} } }
```
presets の中の形はリリーが決めてよい（`resolvedFeel` が上の形で返せればよい）。
