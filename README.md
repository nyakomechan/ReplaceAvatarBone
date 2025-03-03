Replace Avatar Bone
====

アバターに義手、義足等のボーン構造を変更する必要があるアセットを非破壊的に導入できるようにするツール群です。

## 前提アセット

事前に以下のパッケージの導入が必要です。

* Modular Avatar (https://github.com/bdunderscore/modular-avatar)
* Avatar Optimizer (https://github.com/anatawa12/AvatarOptimizer)

## コンポーネント一覧

* ReplaceAvatarBone

指定したHumanoidBone(UpperLeg_Lなど)をこのコンポーネントをアタッチしたオブジェクトに疑似的に置き換えます。

指定したHumanoidBoneの位置、向きがアタッチしたオブジェクトの位置、向きになります。

また、置き換えによってボーンの長さが変動した場合に生じるIKのずれ（脚が長くなった場合に地面に足が埋まる、手が長くなった場合にコントローラとアバターの手の位置がずれる等）を補正します。

* RemoveMeshHelper

AvatarOptimizerのRemoveMeshByBoxの設定補助用のコンポーネントになります。

衣装や義手義足側のオブジェクトにアタッチし、指定したSkinnedMeshRendererのメッシュについて、設定したボックス状の範囲のポリゴンを削除します。

アバターにより体のSkinnedMeshRendererのオブジェクト名が異なるため、衣装、義手義足アセットを導入するユーザー（アセットの購入者）側で導入時にポリゴンの削除を行うオブジェクトを選択する形を想定しています。
* …説明追加予定

## サンプルプレハブ

ReplaceAvatarBone/Sample/sampleLeg.prefabがコンポーネント設定済みのサンプルのプレハブになります。
導入する場合はアバター内に配置し、位置を調整した後、sampleLegのRemoveMeshHelperコンポーネントのインスペクタにて、体の干渉するポリゴンを削除するSkinnedMeshRendererを選択してください。

## 更新履歴
2025/3/4 β版 
