#!/usr/bin/env bash
# カニとばし ゲート（1 つのコマンド）。リポジトリのどこから呼んでもよい。
#   bash harness/scripts/gate.sh T01      … 項目 T01 のゲート
#   bash harness/scripts/gate.sh --all    … 全部（公開候補 R01）
# 出力はそのまま【提出】に全部貼る。✕ が 1 つでもあれば終了コード 1。
set -u
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
command -v node >/dev/null || { echo "✕ node が無い"; exit 2; }
command -v python3 >/dev/null || { echo "✕ python3 が無い（ZIP を解くのとテスト用サーバーに使う）"; exit 2; }
exec node "$HERE/gate.js" "$@"
