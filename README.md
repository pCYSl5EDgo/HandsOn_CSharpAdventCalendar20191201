この記事は[Unity Advent Calendar 2019](https://qiita.com/advent-calendar/2019/unity)の第1日目の記事です。

# ハンズオンの目標

 - Mono.Cecilの使い方に慣れる
  - Mono.Cecil使い増えろ
 - LINQの内部実装についての初歩の理解を得る
 - マネージドプラグイン開発に慣れる
 - GitHub Actionsに慣れる
 - UniNativeLinqのファンになる

LINQやエディタ拡張については結構資料がありますので、主にMono.Cecilについてこの記事で学んでいただければと思います。

# 筆者の開発環境

 - Windows10 Home 
 - Intel Core i7-8750H
 - RAM 16GB
 - Unity
  - 2018.4.9f1
 - .NET Core 3.1
  - 3.1.100-preview1-014459
 - Visual Studio 2019
  - 16.4.0 Preview2.0
 - Git
  - 2.24.0.windows.2
 - Rider 2019.3 EAP

# 前提知識

 - C#
  - 値型、参照型の違いとパフォーマンス特性への理解　[参考文献：C# によるプログラミング入門  [メモリとリソース管理] 値型と参照型](https://ufcpp.net/study/csharp/oo_reference.html)
  - foreachとそのコンパイラによる展開、特にパターンベースであることの理解　[参考文献：C# によるプログラミング入門  [データ列処理] foreach](https://ufcpp.net/study/csharp/sp_foreach.html)
  - LINQのAPIに対する理解　[参考文献：C# によるプログラミング入門  [データ列処理] LINQ](https://ufcpp.net/study/csharp/sp3_linq.html)
 - IL
  - C#のコードがIL(Intermediate Language)の集合にコンパイルされるということへの理解　[参考文献：ILに関するWikipedia記事](https://ja.wikipedia.org/wiki/%E5%85%B1%E9%80%9A%E4%B8%AD%E9%96%93%E8%A8%80%E8%AA%9E)
  - 各命令についてわからないことがあれば[MSDocsのOpCodesクラスの説明を読むのが良いでしょう](https://docs.microsoft.com/ja-jp/dotnet/api/system.reflection.emit.opcodes?view=netframework-4.8)
 - Unity
  - Unity2018でC#7.3の機能が使えるということの理解　[参考文献：Unity公式ブログより「Unity 2018.3 リリース」](https://blogs.unity3d.com/jp/2018/12/13/introducing-unity-2018-3/)
  - Unity2018からUnity.Collections.NativeArray&lt;T&gt;というアンマネージドなヒープやスタック上の連続したメモリ領域を表す配列的構造体についての理解
  - [参考文献1：UnityのScriptingリファレンスのNativeArrayに関するページ](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html)
  - [参考文献2：【Unity】アセット読書会に行ってきたよ。NativeArrayってなんだろう？](https://www.urablog.xyz/entry/2018/03/24/140423)
  - Unity.Collections.UnsafeUtilityというstatic classがNativeArrayの基礎であることへの理解　[参考文献：【Unity】UnsafeUtilityについて纏めてみる](https://qiita.com/mao_/items/fc9b4340b05e7e83c3ff)
  - UnsafeUtilityの詳細なAPIとその機能についての理解
  - [参考文献1：【Unity】UnsafeUtility基礎論【入門者向け】](https://qiita.com/pCYSl5EDgo/items/4b5a5e089eabc8f4387d)
  - [参考文献2：UnityのScriptingリファレンスのUnsafeUtilityに関するページ](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.html)
  - エディタ拡張についての簡単な理解
  - ScriptableObjectに関する理解
 - GitHub Actions
  - GitHubに統合されたCI/CDサービスであることの理解　[参考文献：GitHub Actionsについて](https://help.github.com/ja/actions/automating-your-workflow-with-github-actions/about-github-actions)
 - UniNativeLinq
  - NativeArray&lt;T&gt;向けのLINQライブラリであることの理解　[参考文献：UniNativeLinqに関して](https://speakerdeck.com/pcysl5edgo/uninativelinq-unity2018shi-dai-falsenativearrayyong-linq)

# 事前にインストールしておくべきもの

 - Unity2018.4
  - Unityのバージョンについては2018.4系列である限りなんでもよいです。適宜読み替えを行ってください。
 - .NET Core 3.1以上
 - Visual Studio2019またはRider2019.2以上
 - Git

# 第0章　準備

## パス通し
以後ターミナル操作はPowerShell上で行います。

まずUnity2018.4の実体のあるパスを追加します。
私は&quot;コントロール パネル\システムとセキュリティ\システム\システムの詳細設定\環境変数\Path&quot;に&quot;C:\Users\conve\Documents\Unity\Editor&quot;と追加していますが、環境変数を汚したくない方は都度下のように書いてパスを通すのが良いのではないでしょうか。

```powershell
$Env:Path += ";C:\Program Files\Unity\Hub\Editor\2018.4.13f1\Editor"
```

## 作業ディレクトリ

適当なディレクトリの下に新規に作業ディレクトリを作成します。
今回はUniNativeLinqHandsOnという名前にしましょう。

```powershell
mkdir UniNativeLinqHandsOn
```

前節で正常にパスが通っているならば次のシェルコマンドを実行してUnityエディタが起動するはずです。

```powershell
unity -createProject ./UniNativeLinqHandsOn/
```

起動して図のようにエディタが正常に起動しましたか？
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/256047/3722d9cf-20c5-7f0a-4b4b-552e425f5667.png)

では、一旦エディタを閉じましょう。

## Git初期化

GitHub Actionsを使う兼ね合いもあり、Gitのリポジトリを用意しましょう。

```powershell
cd UniNativeLinqHandsOn
git init
echo [Ll]ibrary/ [Ll]ogs/ [Oo]bj/ .idea/ .vs/ .vscode/ /*.csproj /*.sln /*.sln.user /TestResults-*.xml > .gitignore
```

## マネージドプラグインの下拵え

これからUniNativeLinqの基礎となるNativeEnumerable&lt;T&gt; where T : unmanagedを実装します。
マネージドプラグインとしてUnity外でDLLをビルドしますので、フォルダを作りましょう。フォルダ名は&quot;core~&quot;とします。

```powershell
mkdir core~
cd core~
```

DLLを作るためにdotnet newコマンドでclasslib（ライブラリ作成）オプションを指定して初期化します。
Class1.csは特に要らないので削除します。
追加で.gitignoreをこのフォルダにも定義します。

```powershell
dotnet new classlib
del Class1.cs
echo bin/ obj/ > .gitignore
```

次にcore~.csprojを編集します。
初期状態では以下のように記述されているはずです。

```core~.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>_core</RootNamespace>
  </PropertyGroup>

</Project>
```

<details><summary>csprojの各要素の軽い解説</summary>
<div>Unity使いの方はcsprojファイルの中身をこれまで読んだ時に物凄く長くて冗長な記述を見てきたことでしょう。 そもそもcsprojの中身を読まない？　アッ、ハイ
Visual Studio 2017の頃からcsprojの新しい形式としてSdk形式というものが登場しました。 [参考文献：ufcppのブログ記事](https://ufcpp.net/blog/2017/5/newcsproj/)
全てを設定していた従来のものよりも、デフォルト値と異なる点のみ設定するSdk形式の方が非常に記述量が少なく可読性が高いですね。
`&lt;Project Sdk=&quot;Microsoft.NET.Sdk&quot;&gt;`というトップレベルのProject要素にSdk属性が定義されている場合Sdk形式となります。

PropertyGroup要素以下に基本設定を記述します。
TargetFarmework要素にビルド対象のプラットフォーム/フレームワークを指定します。
Unity2018以上で使うことを考え、.NET Standard 2.0を意味するnetstandard2.0を指定しておきます。

RootNamespace要素はVisual Studioで新規にcsファイルを作成する時に使用される名前空間を指定します。
</div>
</details>

上記csprojを編集して以下の通りにします。

```core~.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>UniNativeLinq</RootNamespace>
    <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
    <AssemblyName>UniNativeLinq</AssemblyName>
    <LangVersion>8</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>○Unityのインストールしてあるフォルダ○\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

AllowUnsafeBlocks要素をTrueにしてポインタを使用可能にします。
AssemblyName要素によりアセンブリ名と出力されたDllファイルの名前を指定します。
そして、LangVersion要素を8に指定してunmanaged型制約の判定を緩めます。　[参考文献：アンマネージな総称型に関するunmanaged型制約](https://ufcpp.net/study/csharp/sp_unsafe.html#unmanaged-generic-struct)

最後に、ItemGroup/Reference要素でUnityEngine.CoreModule.dllを参照に追加しましょう。
Unityのランタイムで使用される基本的な機能はUnityEngine.CoreModule.dllを通じて提供されています。

以上で最初の下拵えを終わります。

#第1章　UniNativeLinq-Coreの最低限の実装(1)

現在の作業ディレクトリが&quot;UniNativeLinqHandsOn/core~&quot;であることを確認してください。

これから私達は以下のファイル群を作成し、UniNativeLinqのコア機能を最低限の形で実装していきます。

 - NativeEnumerable.cs
  - NativeEnumerable&lt;T&gt;構造体を定義します。
  - NativeArray&lt;T&gt;に対してSpan&lt;T&gt;的な役割を果たす基本的な構造体です。
  - IRefEnumerable&lt;T&gt;を実装します。
 - IRefEnumerable.cs
  - System.Collections.IEnumerable&lt;T&gt;を継承したIRefEnumerable&lt;TEnumerator, T&gt;インターフェイスを定義します。
  - 通常のIEnumerable&lt;T&gt;の型引数が1つであるのに対して、IRefEnumerable&lt;TEnumerator, T&gt;の型引数が2つであるのは、構造体イテレータのボクシングを避ける目的があります。
 - IRefEnumerator.cs
  - System.Collections.IEnumerator&lt;T&gt;を継承したIRefEnumerator&lt;T&gt;インターフェイスを定義します。
  - foreach(ref var item in collection)のような参照をイテレーションするための種々の操作を定義します。
 - AsRefEnumerable.cs
  - NativeEnumerable静的クラスを定義します。
  - NativeEnumerable&lt;T&gt;構造体と名前がほぼ同じですが別物です。
  - NativeArray&lt;T&gt;とT[]に対して拡張メソッドを定義します。
 - UnsafeUtilityEx.cs
  - UnsafeUtilityEx静的クラスを定義します。
  - Unityの提供するメモリ操作用APIであるUnity.Collections.LowLevel.Unsafe.UnsafeUtilityは使い勝手が悪いので薄くラップします。

```powershell
mkdir Collection
mkdir Interface
mkdir Utility
mkdir API
New-Item Collection/NativeEnumerable.cs
New-Item Interface/IRefEnumerator.cs
New-Item Interface/IRefEnumerable.cs
New-Item API/AsRefEnumerable.cs
New-Item Utility/UnsafeUtilityEx.cs
```

## NativeEnumerable&lt;T&gt;の最初の定義

<details><summary>最初のNativeEnumerable.cs</summary><div>

```csharp:NativeEnumerable.cs
namespace UniNativeLinq
{
  public readonly unsafe struct NativeEnumerable<T>
    where T : unmanaged
  {
    public readonly T* Ptr;
    public readonly long Length;

    public NativeEnumerable(T* ptr, long length)
    {
      if (ptr == default || length <= 0)
      {
        Ptr = default;
        Length = default;
        return;
      }
      Ptr = ptr;
      Length = length;
    }

    public ref T this[long index] => ref Ptr[index];
  }
}
```
</div></details>

unsafeでreadonlyな構造体UniNativeLinq.NativeEnumerable&lt;T&gt;を定義します。
これはジェネリックなTのポインタであるPtrと要素数であるLengthフィールドを露出させています。
nullポインタやダングリングポインタに対する安全性保証は一切ないので、その辺りはエンドユーザーに一切合切投げっぱなしになるC++スタイルです。

これをビルドし、テストコードをUnityの方で実行してみましょう。

## 最低限のテスト

現在のワーキングディレクトリは&quot;UniNativeLinqHandsOn/core~&quot;のはずです。
以下のようにAssets以下Plugins/UNLフォルダを作成し、core~のビルド成果物であるUniNativeLinq.dllをコピーして配置します。

```powershell
mkdir -p ../Assets/Plugins/UNL
dotnet build -c Release
cp -Force ./bin/Release/netstandard2.0/UniNativeLinq.dll ../Assets/Plugins/UNL/UniNativeLinq.dll
```

ビルドした後毎回&quot;cp -Force ほげほげ&quot;と入力するのも面倒ですので、core~.csprojにビルド後イベントを定義して自動化します。

<details><summary>ビルド後イベントでコピーを自動化したcsproj</summary><div>

```core.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>UniNativeLinq</RootNamespace>
    <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
    <LangVersion>8</LangVersion>
    <AssemblyName>UniNativeLinq</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>○Unityのインストールしてあるフォルダ○\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll</HintPath>
    </Reference>
  </ItemGroup>

  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="copy $(TargetPath) $(ProjectDir)..\Assets\Plugins\UNL\UniNativeLinq.dll"/>
  </Target>
</Project>
```
</div></details>

ビルド後イベントでローカルデプロイの自動化は結構重宝しますのでオススメです。

さて、Assets/Plugins/UNL以下にdllを配置しましたので、それを対象としたテストコードを書きましょう。

```powershell
cd ..
unity -projectPath .
```

エディタが起動しましたね？
ProjectタブのAssetsを選択してコンテキストメニューから&quot;Create/Testing/Tests Assembly Folder&quot;を選択してTestsフォルダーを作成してください。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/256047/eee73538-0d3d-2771-3585-be1785a87d9f.png)

無事にTestsフォルダが作成されたならばそのフォルダ以下にTests.asmdefファイルがあるはずです。
それを選択し、Inspectorタブから設定を変更します。
&quot;Allow 'unsafe' Code&quot;と&quot;Override References&quot;にチェックを入れ、&quot;Assembly References&quot;に&quot;UniNativeLinq.dll&quot;を加えてください。
そしてPlatformsをEditorだけにしてください。
次の画像のようなInspectorになるはずです。正しく設定できたならば一番下のApplyボタンを押して設定を保存してください。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/256047/dfffafe3-bafd-eff6-ad38-3a3619a80c73.png)

次にProjectタブでAssets/Testsフォルダを右クリックしてコンテキストメニューを呼び出し、&quot;Create/Testing/C# Test Script&quot;を押して新規にテスト用スクリプトを作成します。
ファイル名は&quot;NativeEnumerableTestScript&quot;としましょう。

NativeEnumerableTestScriptをダブルクリックして編集を行います。

<details><summary>NativeEnumerableTestScript.csの中身</summary><div>

```csharp:NativeEnumerableTestScript.cs
using NUnit.Framework;
using UniNativeLinq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Tests
{
  public sealed unsafe class NativeEnumerableTestScript
  {
    [Test]
    public void DefaultValuePass()
    {
      NativeEnumerable<int> nativeEnumerable = default;
      Assert.AreEqual(0L, nativeEnumerable.Length);
      Assert.IsTrue(nativeEnumerable.Ptr == null);
    }

    [TestCase(0L)]
    [TestCase(-10L)]
    [TestCase(-12241L)]
    [TestCase(long.MinValue)]
    public void ZeroOrNegativeCountTest(long count)
    {
      using (var array = new NativeArray<int>(1, Allocator.Persistent))
      {
        Assert.IsFalse(array.GetUnsafePtr() == null);
        var nativeEnumerable = new NativeEnumerable<int>((int*) array.GetUnsafePtr(), count);
        Assert.AreEqual(0L, nativeEnumerable.Length);
        Assert.IsTrue(nativeEnumerable.Ptr == null);  
      }
    }

    [TestCase(0, Allocator.Temp)]
    [TestCase(1, Allocator.Temp)]
    [TestCase(10, Allocator.Temp)]
    [TestCase(114, Allocator.Temp)]
    [TestCase(0, Allocator.TempJob)]
    [TestCase(1, Allocator.TempJob)]
    [TestCase(10, Allocator.TempJob)]
    [TestCase(114, Allocator.TempJob)]
    [TestCase(0, Allocator.Persistent)]
    [TestCase(1, Allocator.Persistent)]
    [TestCase(10, Allocator.Persistent)]
    [TestCase(114, Allocator.Persistent)]
    public void FromNativeArrayPass(int count, Allocator allocator)
    {
      using (var array = new NativeArray<int>(count, allocator))
      {
        var nativeEnumerable = new NativeEnumerable<int>((int*) array.GetUnsafePtr(), array.Length);
        Assert.AreEqual((long)count, nativeEnumerable.Length);
        for (var i = 0; i < nativeEnumerable.Length; i++)
        {
          Assert.AreEqual(0, nativeEnumerable[i]);
          nativeEnumerable[i] = i;
        }
        for (var i = 0; i < count; i++)
          Assert.AreEqual(i, array[i]);
      }
    }
  }
}
```
</div></details>
上記コードに従ってUnity Test Runnerの為のEditor Mode Testを複数個用意します。
NUnit.Framework.TestCase属性はバリエーションを作り出すのにかなり便利な属性です。

テストコードの記述後はエディタに戻り、Unity Test Runnerのウィンドウを呼び出しましょう。メニューの&quot;Window/General/Test Runner&quot;をクリックすると開きます。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/256047/e1bf7ce1-cc2a-1b93-ed06-8781f5f633d5.png)

出てきたウィンドウのRun Allを押すと全ての項目が緑になり、テスト全てをPassしたことがわかります。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/256047/0538d616-fa8b-2fb5-10be-7fd971a9b4fd.png)

## GitHubにリポジトリを作って成果物を公開する

GitHubに適当なリポジトリ名で新規リポジトリを作成してください。そこにこのプロジェクトを公開します。
私は&quot;HandsOn_CSharpAdventCalendar20191201&quot;と命名しました。
現在のワーキングディレクトリはUniNativeLinqHandsOnのはずです。

```powershell
git switch -c develop
git add .
git commit -m "[init]"
git remote add origin https://github.com/pCYSl5EDgo/HandsOn_CSharpAdventCalendar20191201.git
git push -u origin develop
```
`git remote add origin https://github.com/pCYSl5EDgo/HandsOn_CSharpAdventCalendar20191201.git`については適切な読み替えを行ってください。
適切な.gitignore設定を行っているならば上記の操作で最初のコミットを過不足なくできます。

基本的にローカルのワーキングブランチはdevelopとし、リモートリポジトリのdevelopブランチにpushすることとします。
この措置はリモートのmasterブランチをUPM用にする為のものです。Assetsを含む通常のUnityプロジェクトはUPMの構成と相性が悪いのです。

## GitHub Actions対応 CI/CDを行う
これからGitHub ReleasesでUniNativeLinq.dllをpush時に自動的に公開する仕組みを作ります。その際にテストも走らせ、テスト失敗時はリリースしないようにします。

<details><summary>GitHub ActionsでUnityを使うための下拵え　[参考文献：GitHub ActionsでUnity開発](https://qiita.com/pCYSl5EDgo/items/690dd56ffb0fcf64e70b)</summary><div>GitHub ActionsではLinux, Windows, MacOSXの３種類の環境でCI/CDを行うことができます。
CI/CDサービスからUnityを利用する場合にはLinux環境を利用する形になります。
<details><summary>なぜWindowsやMacではなくLinuxなのかについての補足</summary><div>WindowsやMacOS環境でも[pCYSl5EDgo/setup-unity](https://github.com/pCYSl5EDgo/setup-unity)というGitHub Actionを利用してUnityをインストール可能です。
WinやMacはVMインスタンスとして立ち上がるので、ジョブ毎にMachine IDが変化します。
困ったことに後述するalfファイルの項目にMachine IDがありまして、ここがulfにも受け継がれてしまい、不一致だと認証にコケるのです。
故にulfファイルを使用してオフライン認証を行う手法をWindowsとMacOS環境では取り得ません。</div></details>

Unityを利用するためには必ずメールアドレスとパスワードで認証する必要があります。
CUIで認証する場合には[オフライン/ 手動アクティベーション](https://docs.unity3d.com/ja/2017.4/Manual/ManualActivationGuide.html)を行う方がパスワード漏洩対策として安全です。
これは事前にUnityを動かすPCの情報と、Unityのバージョン、ユーザーのパスワードとメールアドレス等全ての情報を含んだulfファイルを生成しておき、GitHub Actionsでの実行時にulfファイルを使用して認証を行うという手法です。

詳細な手順は[公式の参考文献](https://docs.unity3d.com/ja/2017.4/Manual/ManualActivationGuide.html)を読んで理解していただくとして、次のような手順でulfファイルを作成してください。

 - https://github.com/pCYSl5EDgo/CreateALF/releases から v1028.4.12f1.alfをダウンロード
 - [alfファイルを元にulfファイルにするためのUnityの提供するウェブページ](https://license.unity3d.com/)に行き、Unityにログインする（ログイン済の方はメールアドレスとパスワード入力不要）
 - alfファイルをアップロード [^1] ![manualactivationwindow.png](https://docs.unity3d.com/ja/2017.4/uploads/Main/manualactivationwindow.png)
 - 使用しているUnityがProまたはPersonalであるかを選択 [^1] ![activateyourlicense.png](https://docs.unity3d.com/ja/2017.4/uploads/Main/activateyourlicense.png)
 - &quot;Download License File&quot;をクリックしてulfファイルを入手 [^1] ![youAreAlmostDone.png](https://docs.unity3d.com/ja/2017.4/uploads/Main/youAreAlmostDone.png)

<details><summary>もしあなたがUnity2018.4.12f1以外でこのハンズオンを行う場合</summary><div>そのバージョンのalfファイルを作成しなくてはいけません。
[CreateALFというGitHubのリポジトリ](https://github.com/pCYSl5EDgo/CreateALF)をForkし、&quot;.github/workflows/CreateLicenseALF.yml&quot;を編集してください。

```yaml:CreateLicenseALF.yml
name: Create ALF File

on: [push]

jobs:
  build:

    runs-on: ubuntu-latest
    strategy:
      matrix:
        unity-version:
        - 2018.4.9f1
        - 2018.4.10f1
        - 2018.4.11f1
        - 2018.4.12f1
        - 2018.4.13f1
        - 2019.3.0f1
        - 2020.1.0a14
    
    steps:
    - uses: pCYSl5EDgo/setup-unity@master
      with:
        unity-version: ${{ matrix.unity-version }}
    - name: Create Manual Activation File
      run: /opt/Unity/Editor/Unity -quit -batchmode -nographics -logfile -createManualActivationFile || exit 0
    - name: Create Release
      id: create_release
      uses: actions/create-release@v1.0.0
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: setup-${{ matrix.unity-version }}
        release_name: Release setup-Unity ${{ matrix.unity-version }}
        draft: false
        prerelease: false
    - name: Upload Release Asset
      id: upload-release-asset 
      uses: actions/upload-release-asset@v1.0.1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ steps.create_release.outputs.upload_url }} # This pulls from the CREATE RELEASE step above, referencing it's ID to get its outputs object, which include a `upload_url`. See this blog post for more info: https://jasonet.co/posts/new-features-of-github-actions/#passing-data-to-future-steps 
        asset_path: Unity_v${{ matrix.unity-version }}.alf
        asset_name: Unity_v${{ matrix.unity-version }}.alf
        asset_content_type: application/xml
```
matrix.unity-versionにあなたの使うUnityのバージョンを指定してください。
masterブランチにpushするとGitHub Releaseにそのバージョンのalfファイルが登録されます。
</div></details>

入手したulfファイルをリポジトリ&quot;HandsOn_CSharpAdventCalendar20191201&quot;で利用しますが、秘密にすべき情報であるため、GitHub Secretsという機能を使って暗号化しましょう。
GitHub SecretsはSettings/Secretsを選択し、そこにキーと値のペアを登録します。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/256047/5e8e32b6-9a66-243c-b634-2c7ba89d884d.png)
今回はulfというキーでulfファイルの中身を登録しましょう。
以上でGitHub ActionsでUnityを扱う下拵えは完了です。</div></details>

現在のワーキングディレクトリはUniNativeLinqHandsOnのはずです。

```powershell
mkdir -p .github/workflows
New-Item .github/workflows/CI.yaml
```
&quot;.github/workflows&quot;フォルダ以下にyamlファイルを作成し、そこに自動化する仕事を記述します。
更にcore~.csproje.txtを新規に作成します。core~.csprojはWindows向けの記述をしていて、そのままではLinuxのDockerコンテナ上では動作しません。
<details><summary>core~.csprojはこのように記述しなおしてください。</summary><div>ほぼ元のcore~.csprojと変わりは無いですが、HintPathが変更され、かつビルド後イベントが削除されています。

```yaml:core~.csproj.txt
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>UniNativeLinq</RootNamespace>
    <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
    <LangVersion>8</LangVersion>
    <AssemblyName>UniNativeLinq</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath Condition="Exists('C:\Users\conve')">C:\Users\conve\Documents\Unity\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll</HintPath>
      <HintPath Condition="Exists('/opt/Unity/Editor/Unity')">/opt/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```
</div></details>

<details><summary>CI.yamlの内容</summary><div>

```yaml:CI.yaml
name: CreateRelease

on:
  push:
    branches:
    - develop

jobs:
  buildReleaseJob:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        unity-version: [2018.4.9f1]
        user-name: [pCYSl5EDgo]
        repository-name: [HandsOn_CSharpAdventCalendar20191201]
        exe: ['/opt/Unity/Editor/Unity']

    steps:
    - uses: pCYSl5EDgo/setup-unity@master
      with:
        unity-version: ${{ matrix.unity-version }}

    - name: License Activation
      run: |
        echo -n "$ULF" > unity.ulf
        ${{ matrix.exe }} -nographics -batchmode -quit -logFile -manualLicenseFile ./unity.ulf || exit 0
      env:
        ULF: ${{ secrets.ulf }}
    
    - run: git clone https://github.com/${{ github.repository }}

    - uses: actions/setup-dotnet@v1.0.2
      with:
        dotnet-version: '3.0.101'

    - name: Builds DLL
      run: |
        cd ${{ matrix.repository-name }}/core~
        dotnet build -c Release
        
    - name: Post Process DLL
      run: |
        cd ${{ matrix.repository-name }}
        mv -f ./core~/bin/Release/netstandard2.0/UniNativeLinq.dll ./Assets/Plugins/UNL/UniNativeLinq.dll

    - name: Run Test
      run: ${{ matrix.exe }} -batchmode -nographics -projectPath ${{ matrix.repository-name }} -logFile ./log.log -runEditorTests -editorTestsResultFile ../result.xml || exit 0

    - run: ls -l
    - run: cat log.log
    - run: cat result.xml
        
    - uses: pCYSl5EDgo/Unity-Test-Runner-Result-XML-interpreter@master
      id: interpret
      with:
        path: result.xml

    - if: steps.interpret.outputs.success != 'true'
      run: exit 1

    - name: Get Version
      run: |
        cd ${{ matrix.repository-name }}
        git describe --tags 1> ../version 2> ../error || exit 0

    - name: Cat Error
      uses: pCYSl5EDgo/cat@master
      id: error
      with:
        path: error
    
    - if: startsWith(steps.error.outputs.text, 'fatal') != 'true'
      run: |
        cat version
        cat version | awk '{ split($0, versions, "-"); split(versions[1], numbers, "."); numbers[3]=numbers[3]+1; variable=numbers[1]"."numbers[2]"."numbers[3]; print variable; }' > version_increment

    - if: startsWith(steps.error.outputs.text, 'fatal')
      run: echo -n "0.0.1" > version_increment

    - name: Cat
      uses: pCYSl5EDgo/cat@master
      id: version
      with:
        path: version_increment

    - name: Create Release
      id: create_release
      uses: actions/create-release@v1.0.0
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: ${{ steps.version.outputs.text }}
        release_name: Release Unity${{ matrix.unity-version }} - v${{ steps.version.outputs.text }}
        draft: false
        prerelease: false
    
    - name: Upload DLL
      uses: actions/upload-release-asset@v1.0.1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ steps.create_release.outputs.upload_url }}
        asset_path: ${{ matrix.repository-name }}/Assets/Plugins/UNL/UniNativeLinq.dll
        asset_name: UniNativeLinq.dll
        asset_content_type: application/vnd.microsoft.portable-executable

```
</div></details>

jobs.buildReleaseJob.startegy.matrix以下の3項目は適切に書き換えてください。

```3項目抜粋
unity-tag: [2018.4.12f1]
user-name: [pCYSl5EDgo]
repository-name: [HandsOn_CSharpAdventCalendar20191201]
```

 - unity-tag
  - Unityのバージョン
 - user-name
  - あなたのGitHubアカウント名
 - repository-name
  - あなたのリポジトリ名

全体の流れとしては以下の通りになります。

 - 作業リポジトリをクローン
  - actions/checkoutだとcore~などの隠しフォルダを無視してしまうのでgit cloneするのが安牌
 - GitHub Secretsに登録したulfファイルをトップレベルに `echo -n "${ULF}" > Unity_v2018.x.ulf`で出力
  - 環境変数に仕込んでいるので外部にバレずに利用可能
 - ビルドに必要なdotnet core 3.1環境のセットアップ
 - DLLをビルドしてそれをAssets/Plugins/UNL以下に配置
 - Unityのライセンス認証
 - Unity Test Runnerをコマンドラインから走らせる
 - テストに失敗したなら全体を失敗させて終了
 - 前回のビルド時のバージョンを取得
 - 取得に失敗したならば今回のバージョンを0.0.1とする
 - 取得成功時はawkでゴニョゴニョしてマイナーバージョンをインクリメントする
 - GitHub Releasesに新規リリースを作成する
 - リリースにファイルを追加する

```powershell
git add .
git commit -m "[update]Publish Release"
git push
```
現在のワーキングディレクトリはUniNativeLinqHandsOnのはずです。
全ての作業が終わったらGitHubにpushして最初のGitHub Releasesを公開しましょう。

## IEnumerable&lt;T&gt;の実装
NativeEnumerable&lt;T&gt;の中身として全てのフィールドとインデクサを定義しました。
これからIEnumerable&lt;T&gt;を実装します。記述が増えるのでpartial structにします。
<details><summary>IEnumerable&lt;T&gt;を実装したNativeEnumerable&lt;T&gt;</summary><div>

```csharp:NativeEnumerable.cs
using System.Collections;
using System.Collections.Generic;

namespace UniNativeLinq
{
  public readonly unsafe partial struct NativeEnumerable<T>
    : IEnumerable<T>
    where T : unmanaged
  {
    public readonly T* Ptr;
    public readonly long Length;

    public NativeEnumerable(T* ptr, long length)
    {
      if (ptr == default || length <= 0)
      {
        Ptr = default;
        Length = default;
        return;
      }
      Ptr = ptr;
      Length = length;
    }

    public ref T this[long index] => ref Ptr[index];

    public Enumerator GetEnumerator() => new Enumerator(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
```
</div></details>
<details><summary>IEnumerator&lt;T&gt;を実装したNativeEnumerable&lt;T&gt;.Enumerator</summary><div>

```csharp:NativeEnumerable.Enumerator.cs
using System.Collections;
using System.Collections.Generic;

namespace UniNativeLinq
{
  public readonly partial struct NativeEnumerable<T>
  {
    public unsafe struct Enumerator : IEnumerator<T>
    {
      private readonly T* ptr;
      private readonly long length;
      private long index;

      public Enumerator(NativeEnumerable<T> parent)
      {
        ptr = parent.Ptr;
        length = parent.Length;
        index = -1;
      }

      public bool MoveNext() => ++index < length;
      public void Reset() => index = -1;
      public ref T Current => ref ptr[index];
      T IEnumerator<T>.Current => Current;
      object IEnumerator.Current => Current;
      public void Dispose() => this = default;
    }
  }
}
```
</div></details>

イテレータ構造体を内部型として定義するのはforeachの性能向上の常套手段です。

```powershell
cd core~
dotnet build -c Release
cd ..
unity -projectPath .
```

ビルドをした後エディタを起動し、テストコードを書きましょう。
NativeEnumerableTestScriptクラスに追記する形で単一のクラスを肥大させましょう。
Unityのよくわからない仕様なのですが、１つのプロジェクトに２ファイル以上のテストスクリプトが存在するとコマンドラインからrunEditorTestsするとエラー吐きます。
このような事情もあり、簡易的な処置ですが神テストクラスを肥えさせます。本格的な処置についてはいずれまた別の記事で書くこともあるかも知れません。
<details><summary>NativeEnumerableTestScript.cs</summary><div>

```csharp:NativeEnumerableTestScript.cs
[TestCase(0, Allocator.Temp)]
[TestCase(114, Allocator.Temp)]
[TestCase(114514, Allocator.Temp)]
[TestCase(0, Allocator.TempJob)]
[TestCase(114, Allocator.TempJob)]
[TestCase(114514, Allocator.TempJob)]
[TestCase(0, Allocator.Persistent)]
[TestCase(114, Allocator.Persistent)]
[TestCase(114514, Allocator.Persistent)]
public void IEnumerableTest(int count, Allocator allocator)
{
    using (var array = new NativeArray<long>(count, allocator))
    {
    var nativeEnumerable = new NativeEnumerable<long>((long*) array.GetUnsafePtr(), array.Length);
    Assert.AreEqual(count, nativeEnumerable.Length);
    for (var i = 0L; i < count; i++)
        nativeEnumerable[i] = i;
    var index = 0L;
    foreach (ref var i in nativeEnumerable)
    {
        Assert.AreEqual(index++, i);
        i = index;
    }
    index = 1L;
    foreach (var i in nativeEnumerable)
        Assert.AreEqual(index++, i);
    }
}
```
</div></details>

foreach文が正しく動いていることがこれで確認できます。
Unityエディターを閉じた後、GitHubにpushしてCI/CDを体感しましょう。

```powershell
git add .
git commit -m "[update]Implement IEnumerable<T> & IEnumerator<T>"
git push
```

## AsEnumerable()に相当するAsRefEnumerable()の実装

NativeArray&lt;T&gt;からNativeEnumerable&lt;T&gt;を生成するのに一々 `var nativeEnumerable = new NativeEnumerable<T>((T*) array.GetUnsafePtr(), array.Length);`と記述するのも手間です。
`var nativeEnumerable = array.AsRefEnumerable();`だったら非常に楽ですので、拡張メソッドを定義します。

```csharp:AsRefEnumerable.cs
namespace UniNativeLinq
{
  public static unsafe class NativeEnumerable
  {
    public static NativeEnumerable<T> AsRefEnumerable<T>(this Unity.Collections.NativeArray<T> array)
      where T : unmanaged
      => new NativeEnumerable<T>(ptr: (T*)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), length: array.Length);
  }
}
```

## IRefEnumerable/torの定義と実装

NativeEnumerable&lt;T&gt;とその内部型Enumeratorは `public Enumerator GetEnumetor();`と `public ref T Current{get;}`が特徴的な要素です。
これをインターフェイスに抽出します。

<details><summary>IRefEnumerable.csとIRefEnumerator.csの定義</summary><div>

```csharp:IRefEnumerable.cs
namespace UniNativeLinq
{
  public interface IRefEnumerable<TEnumerator, T> : System.Collections.Generic.IEnumerable<T>
    where TEnumerator : IRefEnumerator<T>
  {
    new TEnumerator GetEnumerator();
  }
}
```

```csharp:IRefEnumerator.cs
namespace UniNativeLinq
{
  public interface IRefEnumerator<T> : System.Collections.Generic.IEnumerator<T>
  {
    new ref T Current { get; }
  }
}
```
</div></details>

<details><summary>上記インターフェイスをNativeEnumerableに実装します。</summary><div>実際は各ファイルを一行書き換えるだけです。

```csharp:NativeEenumerable.cs
public readonly unsafe partial struct NativeEnumerable<T>
  : IRefEnumerable<NativeEnumerable<T>.Enumerator, T>
```

```csharp:NativeEenumerable.Enumerator.cs
public unsafe struct Enumerator : IRefEnumerator<T>
```
</div></details>

テストコードには何も差は生じません。（既存の実装を元にインターフェイスを抽出しただけですので）

#第2章　初めてのAPI - Select
LINQで一番使うAPIはSelectまたはWhereのはずです。
今回はUniNativeLinqの特異性を学ぶのに好適であるため、Selectを実装してみます。

## 通常LINQのSelectについて
通常のSystem.Linq.Enumerableの提供するSelectメソッドのシグネチャを見てみましょう。

```csharp:Select.cs
public static IEnumerable<TTo> Select<TFrom, TTo>(this IEunmerable<TFrom> collection, Func<TFrom, TTo> func);
```
引数にIEnumerable&lt;TFrom&gt;なコレクションと、Func&lt;TFrom, TTo&gt;な写像を取ってマッピングを行います。
LINQの優れている点は拡張メソッドの型引数を（C#の貧弱な型推論でも）型推論完了できるという点にあります。

標準にLINQに習ってAPIを定義してみましょう。

```csharp:UniNativeLinq.Select.cs
public static IRefEnumerable<TToEnumerator, TTo> Select<TFromEnumerator, TFrom, TToEnumerator, TTo>(this IRefEunmerable<TFromEnumerator, TFrom> collection, Func<TFrom, TTo> func);
```
このような感じでしょうか？
TToEnumeratorを引数から導出できず、センスが悪いですね。
<details><summary>実際のUniNativeLinqでは新たにSelectEnumerable&lt;TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction&gt;型を定義します。</summary><div>

```powershell
New-Item API/RefAction.cs
New-Item Interface/IRefAction.cs
New-Item Utility/DelegateRefActionToStructOperatorAction.cs
New-Item Utility/Unsafe.cs
New-Item Collection/SelectEnumerable.cs
New-Item Collection/SelectEnumerable.Enumerator.cs
```
細々と必要な型があるので他にもいくつか新規にファイルを作成します。

<details><summary>RefAction.csとIRefAction.cs</summary><div>似たような内容なので同一ファイル内に記述するのも良いでしょう。

```csharp:RefAction.cs
namespace UniNativeLinq
{
  public delegate void RefAction<T0, T1>(ref T0 arg0, ref T1 arg1);
  public interface IRefAction<T0, T1>
  {
    void Execute(ref T0 arg0, ref T1 arg1);
  }
}
```

```csharp
namespace UniNativeLinq
{
  public readonly struct DelegateRefActionToStructOperatorAction<T0, T1> : IRefAction<T0, T1>
  {
    private readonly RefAction<T0, T1> action;
    public DelegateRefActionToStructOperatorAction(RefAction<T0, T1> action) => this.action = action;
    public void Execute(ref T0 arg0, ref T1 arg1) => action(ref arg0, ref arg1);
  }
}
```
</div></details>

<details><summary>Unsafe.csは、[System.Runtime.CompilerServices.Unsafe](https://ufcpp.net/blog/2018/12/unsafe/)の一部抜粋です。</summary><div>

```csharp:Unsafe.cs
namespace UniNativeLinq
{
  public static class Unsafe
  {
    // ref T AsRef<T>(in T value) => ref value;
    public static ref T AsRef<T>(in T value) => throw new System.NotImplementedException();
  }
}
```
実際の所、NotImplementExceptionとして中身は空っぽなモックAPIです。
後にいい感じにこのモックAPIを処理します。

Unsafe.AsRefはin引数をref戻り値に変換します。
引数にreadonlyフィールドの参照を与えたら、その戻り値が変更可能な参照になります。
</div></details>

<details><summary>SelectEnumerable.cs</summary><div>

```csharp:SelectEnumerable.cs
namespace UniNativeLinq
{
  public readonly partial struct SelectEnumerable<TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction>
    : IRefEnumerable<SelectEnumerable<TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction>.Enumerator, T>
    where TPrevEnumerable : IRefEnumerable<TPrevEnumerator, TPrev>
    where TPrevEnumerator : IRefEnumerator<TPrev>
    where TAction : IRefAction<TPrev, T>
  {
    private readonly TPrevEnumerable enumerable;
    private readonly TAction action;

    public SelectEnumerable(in TPrevEnumerable enumerable)
    {
      this.enumerable = enumerable;
      action = default;
    }
    public SelectEnumerable(in TPrevEnumerable enumerable, in TAction action)
    {
      this.enumerable = enumerable;
      this.action = action;
    }

    public Enumerator GetEnumerator() => new Enumerator(ref Unsafe.AsRef(in enumerable), action);
    System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator() => GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
```
</div></details>
GetEnumerator()においてUnsafe.AsRef(in enumerable)と記述されています。
Unsafe.AsRefはreadonly制約を無視する危険なメソッドですが、この場合において問題はありません。
UniNativeLinqの提供する範囲において、全てのGetEnumeratorメソッドがreadonlyなメソッドであるからです。
この辺りをC#の型制約で保証できればよいのですが、出来ないため今回のようにUnsafe.AsRefを利用する必要があるのです。

<details><summary>SelectEnumerable.Enumerator.cs</summary><div>

```csharp:SelectEnumerable.Enumerator.cs
namespace UniNativeLinq
{
  public readonly partial struct SelectEnumerable<TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction>
  {
    public struct Enumerator : IRefEnumerator<T>
    {
      private TPrevEnumerator enumerator;
      private TAction action;
      private T element;

      public Enumerator(ref TPrevEnumerable enumerable, in TAction action)
      {
        enumerator = enumerable.GetEnumerator();
        this.action = action;
        element = default;
      }

      public bool MoveNext()
      {
        if (!enumerator.MoveNext()) return false;
        action.Execute(ref enumerator.Current, ref element);
        return true;
      }

      public void Reset() => throw new System.InvalidOperationException();
      public ref T Current => throw new System.NotImplementedException();
      T System.Collections.Generic.IEnumerator<T>.Current => Current;
      object System.Collections.IEnumerator.Current => Current;
      public void Dispose() { }
    }
  }
}
```
</div></details></div></details>
SelectEnumerableの実装はそこまで変なものではありません。
コンストラクタで必要な情報をフィールドに初期化し、GetEnumerator()でEnumeratorを返すだけのシンプルな作りです。
EnumeratorではIRefAction&lt;T0, T1&gt;を実装した型引数TActionのインスタンスactionを使用してMoveNext()する度にT型フィールドであるelementを更新しています。

このEnumeratorの最大の特徴は、 `public ref T Current => throw new System.NotImplementedException();`です。
そう、未実装のままなのです。これはバグではなく極めて意図的な仕様です。
これをこのままビルドしてテストコードを追加してもエラーを吐くだけです。

本当は `public ref T Current => ref element;`と記述したいのですが、[C#の文法の制限として無理です](https://ufcpp.net/study/csharp/sp_ref.html?p=2#struct-this)。

## UniNativeLinq.dllのポストプロセス用dotnet core 3.1プロジェクト
現在のワーキングディレクトリはUniNativeLinqHandsOnのはずです。
Mono.Cecilを利用してUniNativeLinq.dllを編集してSelectEnumerable.Enumerator.CurrentからNotImplementedExceptionを消し飛ばしましょう。

```powershell
mkdir post~
cd post~
dotnet new console
echo bin/ obj/ post~.sln > .gitignore
dotnet add package Mono.Cecil
New-Item DllProcessor.cs
New-Item InstructionUtility.cs
New-Item ToDefinitionUtility.cs
New-Item GenericInstanceUtility.cs
```
<details><summary>参考までにpost~.csprojの中身</summary><div>

```
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <RootNamespace>_post</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Mono.Cecil" Version="0.11.1" />
  </ItemGroup>
</Project>
```
PackageReferenceタグでMono.Cecilをインストール可能です。</div></details>

<details><summary>Program.cs</summary><div>

```csharp:Program.cs
using System;
using System.IO;
public sealed class Program
{
  static int Main(string[] args)
  {
    if (!ValidateArguments(args, out FileInfo inputUniNativeLinqDll, out FileInfo outputUniNativeLinqDllPath, out DirectoryInfo unityEngineFolder))
    {
      return 1;
    }
    using (DllProcessor processor = new DllProcessor(inputUniNativeLinqDll, outputUniNativeLinqDllPath, unityEngineFolder))
    {
      processor.Process();
    }
    return 0;
  }

  private static bool ValidateArguments(string[] args, out FileInfo inputUniNativeLinqDll, out FileInfo outputNativeLinqDllPath, out DirectoryInfo unityEngineFolder)
  {
    if (args.Length != 3)
    {
      Console.Error.WriteLine("Invalid argument count.");
      inputUniNativeLinqDll = default;
      outputNativeLinqDllPath = default;
      unityEngineFolder = default;
      return false;
    }
    inputUniNativeLinqDll = new FileInfo(args[0]);
    if (!inputUniNativeLinqDll.Exists)
    {
      Console.Error.WriteLine("Empty Input UniNativeLinq.dll path");
      outputNativeLinqDllPath = default;
      unityEngineFolder = default;
      return false;
    }
    string outputNativeLinqDllPathString = args[1];
    if (string.IsNullOrWhiteSpace(outputNativeLinqDllPathString))
    {
      Console.Error.WriteLine("Empty Output UniNativeLinq.dll path");
      unityEngineFolder = default;
      outputNativeLinqDllPath = default;
      return false;
    }
    outputNativeLinqDllPath = new FileInfo(outputNativeLinqDllPathString);
    unityEngineFolder = new DirectoryInfo(args[2]);
    if (!unityEngineFolder.Exists)
    {
      Console.Error.WriteLine("Unity Engine Dll Folder does not exist");
      return false;
    }
    return true;
  }
}
```
Main関数はコマンドライン引数を2つ要求します。

 - 第1引数：core~で生成したDllのパス &quot;core~/bin/Release/netstandard2.0/UniNativeLinq.dll&quot;
 - 第2引数：Mono.Cecilで編集した後、Dllを出力するパス &quot;Assets/Plugins/UNL/UniNativeLinq.dll&quot;

ValidateArgumentsで引数の妥当性を検証します。
IDisposableを実装したDllProcessorのインスタンスを生成し、Processメソッドを実行することで適切な処理を加えます。
</div></details>
<details><summary>DllProcessor.cs</summary><div>

```csharp:DllProcessor.cs
using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
internal struct DllProcessor : IDisposable
{
  private readonly ModuleDefinition mainModule;
  private readonly FileInfo outputDll;

  public DllProcessor(FileInfo input, FileInfo output)
  {
    mainModule = ModuleDefinition.ReadModule(input.FullName);
    outputDll = output;
  }

  public void Process()
  {
    ProcessEachMethod(RewriteUnsafeAsRef);
    mainModule.Types.Remove(mainModule.GetType("UniNativeLinq", "Unsafe"));
    ProcessEachMethod(RewriteThrowNotImplementedException, PredicateThrowNotImplementedException);
  }

  private void ProcessEachMethod(Action<MethodDefinition> action, Func<TypeDefinition, bool> predicate = default)
  {
    foreach (TypeDefinition typeDefinition in mainModule.Types)
      ProcessEachMethod(action, predicate, typeDefinition);
  }

  private void ProcessEachMethod(Action<MethodDefinition> action, Func<TypeDefinition, bool> predicate, TypeDefinition typeDefinition)
  {
    foreach (TypeDefinition nestedTypeDefinition in typeDefinition.NestedTypes)
      ProcessEachMethod(action, predicate, nestedTypeDefinition);
    if (predicate is null || predicate(typeDefinition))
      foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
        action(methodDefinition);
  }

  private void RewriteUnsafeAsRef(MethodDefinition methodDefinition)
  {
    Mono.Collections.Generic.Collection<Instruction> instructions;
    try
    {
      instructions = methodDefinition.Body.Instructions;
    }
    catch (NullReferenceException)
    {
      return;
    }
    catch
    {
      Console.WriteLine(methodDefinition.FullName);
      throw;
    }
    for (int i = instructions.Count - 1; i >= 0; i--)
    {
      Instruction instruction = instructions[i];
      if (instruction.OpCode.Code != Code.Call) continue;
      MethodDefinition callMethodDefinition;
      try
      {
        callMethodDefinition = ((MethodReference)instruction.Operand).ToDefinition();
      }
      catch
      {
        continue;
      }
      if (callMethodDefinition.Name != "AsRef" || callMethodDefinition.DeclaringType.Name != "Unsafe") continue;
      instructions.RemoveAt(i);
    }
  }

  private bool PredicateThrowNotImplementedException(TypeDefinition typeDefinition)
  {
    if (!typeDefinition.HasFields) return false;
    return typeDefinition.Fields.Any(field => !field.IsStatic && field.Name == "element");
  }

  private void RewriteThrowNotImplementedException(MethodDefinition methodDefinition)
  {
    if (methodDefinition.IsStatic) return;
    FieldReference elementFieldReference = methodDefinition.DeclaringType.FindField("element").MakeHostInstanceGeneric(methodDefinition.DeclaringType.GenericParameters);
    ILProcessor processor = methodDefinition.Body.GetILProcessor();
    Mono.Collections.Generic.Collection<Instruction> instructions = methodDefinition.Body.Instructions;
    for (int i = instructions.Count - 1; i >= 0; i--)
    {
      Instruction throwInstruction = instructions[i];
      if (throwInstruction.OpCode.Code != Code.Throw) continue;
      Instruction newObjInstruction = instructions[i - 1];
      if (newObjInstruction.OpCode.Code != Code.Newobj) continue;
      MethodDefinition newObjMethodDefinition;
      try
      {
        newObjMethodDefinition = ((MethodReference)newObjInstruction.Operand).ToDefinition();
      }
      catch
      {
        continue;
      }
      if (newObjMethodDefinition.Name != ".ctor" || newObjMethodDefinition.DeclaringType.FullName != "System.NotImplementedException") continue;
      newObjInstruction.Replace(Instruction.Create(OpCodes.Ldarg_0));
      throwInstruction.Replace(Instruction.Create(OpCodes.Ldflda, elementFieldReference));
      processor.InsertAfter(throwInstruction, Instruction.Create(OpCodes.Ret));
    }
  }

  public void Dispose()
  {
    using (Stream writer = new FileStream(outputDll.FullName, FileMode.Create, FileAccess.Write))
    {
      mainModule.Assembly.Write(writer);
    }
    mainModule.Dispose();
  }
}
```

 - Process
  - 元となる不完全なDllに対して処理を加えて完全なDllにするメソッドです。
 - ProcessEachMethod
  - Dllに含まれる全ての型の全てのメソッドを走査して全てのメソッドに対して引数のactionを適用します。
 - RewriteUnsafeAsRef
  - Unsafe.AsRefはreadonlyな要素を非readonlyな状態に変換する極めて危険なAPIです。
  - 使い所がGetEnumeratorに限定されているため特に問題はないです。
 - PredicateThrowNotImplementedException
  - インスタンスフィールドに&quot;element&quot;という名前のそれが存在する型のみを処理対象に選ぶメソッドです。
 - RewriteThrowNotImplementedException
  - `throw new NotImplementedException();`を `return ref this.element;`に置換します。
</div></details>
<details><summary>InstructionUtility.cs</summary><div>

```csharp:InstructionUtility.cs
using Mono.Cecil.Cil;
internal static class InstructionUtility
{
  public static void Replace(this Instruction instruction, Instruction replace) => (instruction.OpCode, instruction.Operand) = (replace.OpCode, replace.Operand);
}
```

RewriteThrowNotImplementedException内部で使用されます。
ILの命令を置換するための拡張メソッドです。
ILProcessorのReplaceメソッドはバグを誘発するわりと使い物にならないメソッドです。
gotoやif, switchなどのジャンプ系の命令の行き先にまつわる致命的なバグを生じます。
こうしてわざわざ拡張メソッドを用意する必要があるのです。
</div></details>
<details><summary>ToDefinitionUtility.cs</summary><div>

```csharp:ToDefinitionUtility.cs
using Mono.Cecil;
internal static class ToDefinitionUtility
{
  public static TypeDefinition ToDefinition(this TypeReference reference) => reference switch
  {
    TypeDefinition definition => definition,
    GenericInstanceType generic => generic.ElementType.ToDefinition(),
    _ => reference.Resolve(),
  };
  public static MethodDefinition ToDefinition(this MethodReference reference) => reference switch
  {
    MethodDefinition definition => definition,
    GenericInstanceMethod generic => generic.ElementMethod.ToDefinition(),
    _ => reference.Resolve(),
  };
}
```
特に気にする必要はない拡張メソッドです。
Resolve()が例外を投げる可能性が結構あります。
</div></details>

<details><summary>GenericInstanceUtility.cs</summary><div>

```csharp:GenericInstanceUtility.cs
using Mono.Cecil;
using System.Linq;
using System.Collections.Generic;
internal static class GenericInstanceUtility
{
  public static FieldReference FindField(this TypeReference type, string name)
  {
    if (type is TypeDefinition definition)
      return definition.FindField(name);
    if (type is GenericInstanceType genericInstanceType)
      return genericInstanceType.FindField(name);
    var typeDefinition = type.ToDefinition();
    var fieldDefinition = typeDefinition.Fields.Single(x => x.Name == name);
    if (fieldDefinition.Module == type.Module)
      return fieldDefinition;
    return type.Module.ImportReference(fieldDefinition);
  }
  public static FieldReference FindField(this TypeDefinition type, string name) => type.Fields.Single(x => x.Name == name);

  public static FieldReference FindField(this GenericInstanceType type, string name)
  {
    var typeDefinition = type.ToDefinition();
    var definition = typeDefinition.Fields.Single(x => x.Name == name);
    return definition.MakeHostInstanceGeneric(type.GenericArguments);
  }

  public static FieldReference MakeHostInstanceGeneric(this FieldReference self, IEnumerable<TypeReference> arguments) => new FieldReference(self.Name, self.FieldType, self.DeclaringType.MakeGenericInstanceType(arguments));

  public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, IEnumerable<TypeReference> arguments)
  {
    var instance = new GenericInstanceType(self);
    foreach (var argument in arguments)
      instance.GenericArguments.Add(argument);
    return instance;
  }
}
```
</div></details>

## CI.yamlをアップデート

<details><summary>post~によりUniNativeLinq.dllにポストプロセスをする必要があり、CI.yamlを書き換えます。</summary><div>

<details><summary>CI.yaml全文</summary><div>

```yaml:CI.yaml
name: CreateRelease

on:
  push:
    branches:
    - develop

jobs:
  buildReleaseJob:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        unity-version: [2018.4.9f1]
        user-name: [pCYSl5EDgo]
        repository-name: [HandsOn_CSharpAdventCalendar20191201]
        exe: ['/opt/Unity/Editor/Unity']

    steps:
    - uses: pCYSl5EDgo/setup-unity@master
      with:
        unity-version: ${{ matrix.unity-version }}

    - name: License Activation
      run: |
        echo -n "$ULF" > unity.ulf
        ${{ matrix.exe }} -nographics -batchmode -quit -logFile -manualLicenseFile ./unity.ulf || exit 0
      env:
        ULF: ${{ secrets.ulf }}
    
    - run: git clone https://github.com/${{ github.repository }}

    - uses: actions/setup-dotnet@v1.0.2
      with:
        dotnet-version: '3.0.101'

    - name: Builds DLL
      run: |
        cd ${{ matrix.repository-name }}/core~
        dotnet build -c Release
        
    - name: Post Process DLL
      run: |
        cd ${{ matrix.repository-name }}/post~
        ls -l ../Assets/Plugins/UNL/
        dotnet run ../core~/bin/Release/netstandard2.0/UniNativeLinq.dll ../Assets/Plugins/UNL/UniNativeLinq.dll
        ls -l ../Assets/Plugins/UNL/

    - name: Run Test
      run: ${{ matrix.exe }} -batchmode -nographics -projectPath ${{ matrix.repository-name }} -logFile ./log.log -runEditorTests -editorTestsResultFile ../result.xml || exit 0

    - run: ls -l
    - run: cat log.log
    - run: cat result.xml
        
    - uses: pCYSl5EDgo/Unity-Test-Runner-Result-XML-interpreter@master
      id: interpret
      with:
        path: result.xml

    - if: steps.interpret.outputs.success != 'true'
      run: exit 1

    - name: Get Version
      run: |
        cd ${{ matrix.repository-name }}
        git describe --tags 1> ../version 2> ../error || exit 0

    - name: Cat Error
      uses: pCYSl5EDgo/cat@master
      id: error
      with:
        path: error
    
    - if: startsWith(steps.error.outputs.text, 'fatal') != 'true'
      run: |
        cat version
        cat version | awk '{ split($0, versions, "-"); split(versions[1], numbers, "."); numbers[3]=numbers[3]+1; variable=numbers[1]"."numbers[2]"."numbers[3]; print variable; }' > version_increment

    - if: startsWith(steps.error.outputs.text, 'fatal')
      run: echo -n "0.0.1" > version_increment

    - name: Cat
      uses: pCYSl5EDgo/cat@master
      id: version
      with:
        path: version_increment

    - name: Create Release
      id: create_release
      uses: actions/create-release@v1.0.0
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: ${{ steps.version.outputs.text }}
        release_name: Release Unity${{ matrix.unity-tag }} - v${{ steps.version.outputs.text }}
        draft: false
        prerelease: false
    
    - name: Upload DLL
      uses: actions/upload-release-asset@v1.0.1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ steps.create_release.outputs.upload_url }}
        asset_path: ${{ matrix.repository-name }}/core~/bin/Release/netstandard2.0/UniNativeLinq.dll
        asset_name: UniNativeLinq.dll
        asset_content_type: application/vnd.microsoft.portable-executable
```
</div></details>
<details><summary>変更箇所のみ抜粋</summary><div>

```yaml:CI.yaml
- name: Builds DLL
  run: |
    mkdir artifact
    cd ${{ matrix.repository-name }}/core~
    dotnet build -c Release
    
- name: Post Process DLL
  run: |
    cd ${{ matrix.repository-name }}/post~
    dotnet run ../core~/bin/Release/netstandard2.0/UniNativeLinq.dll ../Assets/Plugins/UNL/UniNativeLinq.dll

- name: License Activation
```
</div></details>
</div></details>

```powershell:push_post.ps
git add .
git commit -m "[add]post~ prot-process project"
git push
```

## Selectの為のテスト追加

post~によってDLLをポストプロセッシングできるようになりました。
大手を振ってテストコードを書けますね！

<details><summary>NativeEnumerableTestScriptに追記するテストメソッド</summary><div>

```csharp:NativeEnumerableTestScript
private readonly struct TripleAction : IRefAction<long, long>
{
  public void Execute(ref long arg, ref long result) => result = arg * 3;
}

[TestCase(114, Allocator.Temp)]
[TestCase(114, Allocator.TempJob)]
[TestCase(114, Allocator.Persistent)]
public void SelectOperatorTest(int count, Allocator allocator)
{
  using (var array = new NativeArray<long>(count, allocator))
  {
    var nativeEnumerable = new NativeEnumerable<long>((long*) array.GetUnsafePtr(), array.Length);
    for (var i = 0L; i < count; i++)
      nativeEnumerable[i] = i;
    var selectEnumerable = new SelectEnumerable<NativeEnumerable<long>, NativeEnumerable<long>.Enumerator, long, long, TripleAction>(nativeEnumerable);
    var assertNumber = 0L;
    foreach (ref var item in selectEnumerable)
    {
      Assert.AreEqual(assertNumber, item);
      assertNumber += 3;
    }
  }
}
```

要素を3倍の値にして返すSelectのオペレーターを定義しています(TripleAction)。
そしてせこせこ5つ型引数を設定してSelectEnumerableをnewします。
foreach中できちんと元のソースの3倍になっていることを確認できていますね。
</div></details>

# 終わりに

[UniNativeLinq本家](https://github.com/pCYSl5EDgo/UniNativeLinq-EditorExtension)ではエディタ拡張に関連して更にえげつない最適化やMono.Cecilテクニックが使用されています。
既存のLINQに比べて非常に高速に動作しますので是非使ってください。

このハンズオンよりも更に深くMono.CecilやUniNativeLinqを学びたいという方は[私のTwitter](https://twitter.com/pCYSl5EDgo)のDMなどでご相談いただければ嬉しいです。

[^1]: 画像はUnityのサイトより引用