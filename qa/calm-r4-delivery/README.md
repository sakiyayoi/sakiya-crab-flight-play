# CalmR4 配布の軽量化

ゲームC#・Wasm・dataの展開後バイトは変更しない。source snapshot a558b099からの既存playerを、Node内蔵Brotliで可逆圧縮して通常のZIPへ入れる。互換性の弱いZIP-LZMAはMac標準解凍で失敗し、不採用。音質・モデル・破片数・勢いは落とさない。

`node pack-compact.cjs /absolute/original-player /absolute/new-player`

出力playerは `python3 serve.py` でlocalhostのみへ配信する。HTTP Content-Encoding/MIMEが必要なため単純なhttp.serverを使わない。正常展開後の3payload SHAをmanifestへ保存する。Unityでの再buildや公開は不要。

現行の検証：前面維持、同じ発射receiptの6投をtrace付き/traceなしで別々に観察。83.4msは再現せず、元の単発は原因未確定の低頻度残差として保持。40投/M14を今回の追加停止条件にしない。旧7/10の不合格をPASSへ書き換えない。
