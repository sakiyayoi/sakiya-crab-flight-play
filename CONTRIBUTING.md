# 統合の接続条件

統合担当が物理・入力・ゲームロジック・メインScene・共有設定を所有します。3D、街、音、VFXは隔離した素材ディレクトリで制作し、共有Sceneを直接変更しません。.metaとGUIDは対応する資産と一緒に保持します。Unity GUIの同時操作は避けます。

mainは確認済みの比較基点。未確認の機能はbranch / draft PRへ保存し、入力版、変更、検証、既知欠陥を記述します。PR作成・merge・試遊ページ配備・本人採用は別です。公開範囲を変更しません。

音とVFXの共通接続は charge / launch / impact / destruction / landing / reset。producerがgeneration、単調なeventId、objectId、world位置・法線・速度、強度、材質を発行します。consumerは衝突を再判定せず、同じIDを二重処理しません。reset / pause / muteではloop・pending・粒子の寿命を同期します。このproducerは統合branchで実装・検証中で、mainに実装済みとの主張ではありません。

Git LFSは現環境の容量・利用可否が確認できるまで使いません。新料金・認証・招待・公開設定変更は追加しません。原本と比較証拠は制作workspaceに保護し、生ログや個人情報をこのrepoへ送信しません。
