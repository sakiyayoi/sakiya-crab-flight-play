r27 only: formal harness original byte copy kept outside. Existing Chrome executable supplied in config, no browser install. kani-helpers.fireAll explicitly clicks the real sound button then waits for the four decoded voice files. This retains initial mute and tests voice through its user-gesture audio path. No acceptance threshold, count, flash limit or error exclusion changed. T01 formal branch unchanged. G-LINK pending owner-approved URL.

## 同一最終版の全件実行
r26を保護し、r27側受入testのVERSION定数だけをkani-v0.2-feel-r27cへ更新。試行20/20・角度20/40/55/70・向き・5m・120s充填/300s決着・error監視・expectedCorePower式は不変。新しいqa/final-r27.playwright.config.jsは既存5受入+27正式flow/feelを1系統で全件実行する接続設定。G-LINKや物理スマホFPSはこのコマンドのPASSに含めない。
注意：正式gate.jsのG-POWERはraw設定powerとの差<=5、既存受入は実充填energyを使う既存CoreLaunchPower式との一致を確認する。保存済み55入力の実power61.9230766はraw差6.923で、同じ意味の測定ではない。どちらの定義/閾値も書き換えず、最終40で各々結果を返す。
