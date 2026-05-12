# 敵AIライブラリセットアップ手順 (Setup Guide)

このガイドでは、新しく実装された `Alpha_EnemyAI` と ScriptableObject ベースの挙動（Behavior）ライブラリを使って、敵をセットアップする方法を説明します。

## 1. 新しい挙動（Behavior Data）の作成
まず、敵に割り当てる「挙動のデータ」を作成します。

1. Unityエディタの **Projectウィンドウ** で右クリックします。
2. `Create > EnemyAI > Behaviors > ...` を選択します。
   - `Chase` (直進追尾)
   - `Keep Distance` (一定距離の維持)
   - `Dash` (突進)
   - `Scareclaw Legacy` (以前の左右・軌道移動)
3. 作成されたデータ（ScriptableObject）を選択し、Inspectorから各パラメータ（速度、距離、チャージ時間など）を調整します。

## 2. エネミーのセットアップ
敵のプレハブ（例：Scareclaw）にAIを組み込みます。

1. 敵のGameObjectに `Alpha_EnemyAI` コンポーネントをアタッチします。（Scareclawの場合は既に継承済みなのでそのまま使えます）
2. `Rigidbody2D` が自動で追加されるので、`Body Type` を必要に応じて `Dynamic` または `Kinematic` に設定し、重力（Gravity Scale）を `0` にします。
3. `Alpha_EnemyAI` コンポーネントの `Initial Behavior` フィールドに、先ほど作成した「挙動のデータ（ScriptableObject）」をドラッグ＆ドロップで割り当てます。

## 3. スクリプトから挙動を切り替える（応用）
別のスクリプト（例えばボスのHP管理スクリプトなど）から、途中で挙動を変えたい場合は以下のように呼び出します。

```csharp
// 敵のAIコンポーネントを取得
Alpha_EnemyAI enemyAI = GetComponent<Alpha_EnemyAI>();

// インスペクターで割り当てておいた別の挙動データ（例: 発狂時の突進データ）
public EnemyBehaviorData_Base phase2Behavior;

// 挙動をチェンジ
enemyAI.ChangeBehavior(phase2Behavior);
```

## 4. 新しい挙動プログラミング（拡張）を追加する手順
新しいオリジナルの挙動を作りたい場合は、以下の手順でスクリプトを作成します。

1. `EnemyBehaviorData_Base` を継承した新しいC#スクリプトを作成します。
2. クラスの頭に `[CreateAssetMenu(fileName = "New Custom Behavior", menuName = "EnemyAI/Behaviors/Custom")]` を付けます。
3. `RunBehavior(Alpha_EnemyAI ai)` メソッドを `override` して、コルーチンの処理（`yield return new WaitForFixedUpdate();` のループなど）を記述します。
