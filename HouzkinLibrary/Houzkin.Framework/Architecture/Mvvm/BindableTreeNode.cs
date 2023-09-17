using Houzkin.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections;

namespace Houzkin.Architecture {
	/// <summary>外部への公開用に共通のメンバーを定義可能な、ビューによってバインドされるツリー構造として参照元のノードをラップする。</summary>
	/// <typeparam name="TViewModel">各ノードの共通実装部分として公開する型</typeparam>
	/// <typeparam name="TModel">各ノードが内包するモデルの型</typeparam>
	public abstract class ReadOnlyBindableTreeNode<TModel,TViewModel> : ViewModelBase<TModel> , IReadOnlyTreeNode<TViewModel>, IDisposable
	where TViewModel : ReadOnlyBindableTreeNode<TModel,TViewModel>
	where TModel : IReadOnlyObservableTreeNode<TModel> {
		/// <summary>新規インスタンスを初期化する。
		/// <para>このオブジェクトは親ノードから生成された場合のみ親ノードの参照を保持する。</para></summary>
		/// <param name="model">参照するノード</param>
		protected ReadOnlyBindableTreeNode(TModel model) : base(model) { }// { _model = model; }

		/// <summary>現在のビューモデルが参照するソースの子ノードから現在のビューモデルの子ノードを生成する。</summary>
		/// <param name="modelChildNode">ソースの子ノード</param>
		/// <returns>現在のビューモデルに追加する、子ノードのビューモデル</returns>
		protected abstract TViewModel GenerateChild(TModel modelChildNode);

		/// <summary>
		/// 子ノードのコレクションを生成する。デフォルトではGenerateChildメソッドを使用します。
		/// </summary>
		/// <param name="model">モデル</param>
		/// <returns>ビューモデルコレクション</returns>
		protected virtual ReadOnlyBindableCollection<TViewModel> GenerateChildCollection(TModel model) {
			Func<TModel, TViewModel> conv = m => {
				var c = GenerateChild(m);
				c._parent = this as TViewModel;
				return c;
			};
			return ReadOnlyBindableCollection.Create(model.Children, conv);
		}
		TViewModel _parent;
		/// <summary>親ノードを取得する。</summary>
		public TViewModel Parent {
			get { return _parent; }
		}
		ReadOnlyBindableCollection<TViewModel> 〆childNodes;
		
		ReadOnlyBindableCollection<TViewModel> ChildNodes {
			get {
				if (〆childNodes == null) 〆childNodes = GenerateChildCollection(this.Model);
				return 〆childNodes;
			}
		}
		IReadOnlyList<TViewModel> IReadOnlyTreeNode<TViewModel>.Children {
			get { return ChildNodes; }
		}
		/// <summary>子ノードのコレクションを取得する。</summary>
		public ReadOnlyBindableCollection<TViewModel> Children {
			get { return ChildNodes; }
		}
		
	}
	/// <summary>ビューによってバインドされる簡易的なツリー構造として参照元のノードをラップする。</summary>
	/// <typeparam name="TModel">各ノードが内包するモデルの型</typeparam>
	public abstract class ReadOnlyBindableTreeNode<TModel> : ReadOnlyBindableTreeNode<TModel, ReadOnlyBindableTreeNode<TModel>>
	where TModel : IReadOnlyObservableTreeNode<TModel> {
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="model">参照するノード</param>
		public ReadOnlyBindableTreeNode(TModel model) : base(model) { }
	}
	//public static class ReadOnlyBindableTreeNode {
	//	public static TViewModel Create<TModel,TViewModel>(TModel root, Func<TModel,TViewModel> generate)
	//	where TModel : IReadOnlyObservableTreeNode<TModel>
	//	where TViewModel : ReadOnlyBindableTreeNode<TModel,TViewModel> {
	//		return generate(root);
	//	}
	//}
}
