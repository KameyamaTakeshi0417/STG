# エリートエネミー セットアップ手順 (Elite Setup Guide)

このガイドでは、`testEliteEnemy` プレハブを作成し、フェーズ制（ブレイク）や移動＋弾幕を組み合わせる手順を説明します。

## 1. エリートAI用コンポーネントのアタッチ
1. 敵のGameObject（例: `testEliteEnemy`）を作成し、SpriteやRigidbody2D（重力0）を設定します。
2. これまでの `Health` と `Alpha_EnemyAI` を削除し、代わりに **`Alpha_EliteHealth`** と **`Alpha_EliteEnemyAI`** をアタッチします。
   （※ Alpha_EliteEnemyAI をアタッチすると自動で Alpha_EliteHealth もアタッチされます）

## 2. フェーズごとの体力（ブレイクゲージ）設定
1. `Alpha_EliteHealth` コンポーネントの `Phase HPs` を展開します。
2. 必要なフェーズ数だけ要素を追加し、それぞれの最大HPを入力します。（例: Element 0: 1000, Element 1: 1500）

## 3. 挙動データの作成（弾幕 / 召喚）
1. Projectウィンドウで右クリックし、`Create > EnemyAI > Behaviors > Barrage`（または `Summon`）を選択してデータを作成します。
2. 弾幕データ（`Behavior_Barrage`）の Inspector で、以下を設定します。
   - `bulletPrefab` : 撃ち出したい弾のプレハブ（`NormalBullet`など）
   - `bulletCount` / `spreadAngle` : 例として 3発、60度 にすると 3-Way 扇状弾幕になります。
   - `fireInterval` : 0.5 にすると0.5秒ごとに撃ちます。

## 4. フェーズと行動（Behavior）の紐付け
1. `Alpha_EliteEnemyAI` コンポーネントの `Phases` リストを展開し、要素（フェーズ）を追加します。
2. 各フェーズに以下のようにデータをセットします。
   - **Phase 0 (1本目のHPの時)**
     - `Movement Behavior` : 作成済みの追尾（Chase）や距離維持（Keep Distance）データをセット
     - `Attack Behavior` : 先ほど作った `Barrage`（弾幕）データをセット
   - **Phase 1 (ブレイク後、2本目のHPの時)**
     - `Movement Behavior` : 突進（Dash）データをセット
     - `Attack Behavior` : 別の激しい弾幕パターンをセット
     - `Summon Behavior` : 召喚（Summon）データをセット

## 5. カットイン演出用フックの利用（プログラマー向け）
フェーズの切り替わりや弾幕の開始時に演出を出したい場合は、スクリプトから以下のようにイベントに登録します。

```csharp
Alpha_EliteEnemyAI eliteAI = GetComponent<Alpha_EliteEnemyAI>();

eliteAI.OnPhaseStartEvent += (phaseIndex, phaseName) => {
    Debug.Log($"フェーズ {phaseIndex} ({phaseName}) が開始！ここでカットインUIを表示！");
};

eliteAI.OnAttackStartEvent += (attackName) => {
    Debug.Log($"攻撃 {attackName} が開始！キャラのセリフを表示！");
};
```
