Replace Avatar Bone
====

アバターに義手、義足等のボーン構造を変更する必要があるアセットを非破壊的に導入できるようにするツール群です。

## 前提アセット

プロジェクトに以下のパッケージが必要です。

* Modular Avatar (https://github.com/bdunderscore/modular-avatar)
* Avatar Optimizer (https://github.com/anatawa12/AvatarOptimizer)

## コンポーネント一覧

* ReplaceAvatarBone

指定したHumanoidBone(UpperLeg_Lなど)をこのコンポーネントをアタッチしたオブジェクトに疑似的に置き換えます。
指定したHumanoidBoneの位置、向きがアタッチしたオブジェクトの位置、向きになります。
　
また、置き換えによってボーンの長さが変動した場合に生じるIKのずれ（脚が長くなった場合に地面に足が埋まる、手が長くなった場合にコントローラとアバターの手の位置がずれる）を補正します。

* RemoveMeshHelper

AvatarOptimizerのRemoveMeshByBoxの設定補助用のコンポーネントになります。
衣装や義手義足側のオブジェクトにアタッチし、指定したSkinnedMeshRendererのメッシュについて、設定したボックス状の範囲のポリゴンを削除します。
* …説明追加予定