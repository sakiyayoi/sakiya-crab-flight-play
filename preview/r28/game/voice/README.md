# ずんだもんの声（カニとばし用・正式版：ツンツン）

**音声：VOICEVOX:ずんだもん**

作成：2026-10-28 18:30 JST 頃、box 上で VOICEVOX ENGINE 0.26.0（Linux CPU x64、公式 GitHub releases の `voicevox_engine-linux-cpu-x64-0.26.0.vvpp`）を使って作成。どこにも公開・送信していない。

## ライセンス・注意書き
- この声の音声ファイルは、ゲームのリポジトリの MIT（など）ライセンスの**対象外**。ゲームに入れるときは `voice/` フォルダにまとめ、このファイル（または `voice/NOTICE.md`）を一緒に置く。
- 利用条件：ずんだもん音源利用ガイドライン https://zunko.jp/con_ongen_kiyaku.html ／ VOICEVOX ソフトウェア利用規約 https://voicevox.hiroshiba.jp/term/ （エンジン同梱の規約 `resources/engine_manifest_assets/terms_of_service.md` も同じ趣旨：「作成された音声を利用する際は、各音声ライブラリの規約に従ってください」「ご利用の際は VOICEVOX を利用したことがわかるクレジット表記が必要です。」）
- エンジン同梱のずんだもんの規約（`policy.md`）：「ずんだもんの音声ライブラリを用いて生成した音声は、「VOICEVOX:ずんだもん」とそれぞれクレジットを記載すれば、商用・非商用で利用可能です。」
- クレジット「VOICEVOX:ずんだもん」を、ゲーム内のクレジット画面・公開ページ（GitHub Pages／README）・`CREDITS.md` に書く。
- 配信・動画では『VOICEVOX:ずんだもん』の表記をお願いします。
- 声のファイルを素材として配る・再配布を認めることはしない。

## 作り方
- `/audio_query` → `/synthesis`。設定：speedScale 1.0、prePhonemeLength 0、postPhonemeLength 0.08、outputSamplingRate 48000、モノラル。
- ffmpeg 7.1.5 で前後の無音を削除（-50 dB 以下）→ `ebur128` で測って音量をそろえる（−24 LUFS）。`newrecord_shinkiroku_normal`（不採用、unused/）だけは、そのままだと真のピークが −7.9 dBFS で −9 を超えたため、先に `alimiter=limit=0.12` で山を軽く削ってから音量をそろえた。
- 48 kHz・モノラルの WAV（16 bit）と OGG（Vorbis q5）。生成直後の元の音は `raw/`。
- 生成スクリプト：`/workspace/tools/voicevox/gen.py`

## 測定結果（ffmpeg `ebur128=peak=true`。「真のピーク」は dBTP 相当）
| ファイル | きっかけ | セリフ | スタイル | speedScale | 長さ(秒) | 平均ラウドネス(LUFS) | 真のピーク(dBFS) | SHA256 |
|---|---|---|---|---|---|---|---|---|
| `glassbreak_kani_tsuntsun.wav` | 生簀のガラス突破 | カニー！ | ツンツン（id 7） | 1.0 | 0.448 | -24.0 | -9.8 | `85a5d70c87f4c7f97237e47a659c62a4e1200457c655ae6d559bacd4b76f0d88` |
| `glassbreak_kani_tsuntsun.ogg` | 生簀のガラス突破 | カニー！ | ツンツン（id 7） | 1.0 | 0.448 | -24.1 | -9.8 | `0363e33eb4a2d4dc2d25fd8b146f1632a368d8ca205e0561b012332b3cbe6591` |
| `chain7_matomete_tsuntsun.wav` | 連鎖7個以上 | まとめてー！ | ツンツン（id 7） | 1.0 | 0.762 | -24.0 | -10.0 | `ecb208812011e282d4d2004afe80a807d44e222f9e1fcf14124bd2592eac6455` |
| `chain7_matomete_tsuntsun.ogg` | 連鎖7個以上 | まとめてー！ | ツンツン（id 7） | 1.0 | 0.762 | -24.0 | -10.0 | `9ee8e14be75b22ba0f8fcf19b9bd67922ba0b4f91691f2eda7a21f9620717538` |
| `newrecord_shinkiroku_tsuntsun.wav` | 新記録 | 新記録！ | ツンツン（id 7） | 1.0 | 0.785 | -24.0 | -9.1 | `d2664ceff2f5f62853a1a0b1b2d80ded4c56f4121dfab4f4c469b2d3c8f6e888` |
| `newrecord_shinkiroku_tsuntsun.ogg` | 新記録 | 新記録！ | ツンツン（id 7） | 1.0 | 0.785 | -24.1 | -9.2 | `e8d483a7497add5bc3975e33cb2f23b08c6a3b6d74741fad4f350d1ad69cd1c3` |
| `perfect_nice_tsuntsun.wav` | 着地プッシュ PERFECT | ナイス！ | ツンツン（id 7） | 1.0 | 0.478 | -24.0 | -9.5 | `2911fcac05f19ce31c9fbf1fe421d95f6d58e640dc9eb1953bb8efc323c47af7` |
| `perfect_nice_tsuntsun.ogg` | 着地プッシュ PERFECT | ナイス！ | ツンツン（id 7） | 1.0 | 0.478 | -24.1 | -9.4 | `6fafb08803be902de841c84ea7075c90c6a3de6cfd1f1e448121f6f954ed77f2` |

- 全ファイル 1.2 秒以下（最長 0.785 秒）。speedScale を上げる必要は無かった。
- 真のピークは全部 −9 dBFS 以下。ただしツンツンはぎりぎり（−9.1〜−10.0）。
- 注意：ebur128 の平均ラウドネスは 400 ms 単位で測るので、0.45〜0.83 秒の短い音では値があまり当てにならない。loudnorm で測り直しても WAV は −23.95〜−24.07 LUFS で、ほぼ同じだった。

## 正式版（10/28 18:43 JST 決定）
- 通治さんの決定により、ずんだもんの**ツンツン（style id 7）**の4本（各 WAV/OGG）を正式版とする。
- ノーマル（id 3）の4本は `unused/` に移した（採用しない、記録として保管）。
