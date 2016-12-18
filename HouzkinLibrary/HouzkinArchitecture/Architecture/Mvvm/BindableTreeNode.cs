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
	public abstract class ReadOnlyBindableTreeNode<TModel,TViewModel> : MarshalViewModel<TModel> , IReadOnlyTreeNode<TViewModel>, IDisposable
	where TViewModel : ReadOnlyBindableTreeNode<TModel,TViewModel>
	where TModel : IReadOnlyObservableTreeNode<TModel> {
		/// <summary>新規インスタンスを初期化する。
		/// <para>このオブジェクトは親ノードから生成された場合のみ親ノードの参照を保持する。</para></summary>
		/// <param name="model">参照するノード</param>
		protected ReadOnlyBindableTreeNode(TModel model) : base(model) { }// { _model = model; }

		//TModel _model;
		///// <summary>対象インスタンスが参照しているオブジェクトを取得する。</summary>
		//protected TModel Model {
		//	get { return _model; }
		//}

		/// <summary>現在のビューモデルが参照するソースの子ノードから現在のビューモデルの子ノードを生成する。</summary>
		/// <param name="modelChildNode">ソースの子ノード</param>
		/// <returns>現在のビューモデルに追加する、子ノードのビューモデル</returns>
		protected abstract TViewModel GenerateChild(TModel modelChildNode);

		private ReadOnlyBindableCollection<TViewModel> generateChildCollection(TModel model) {
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
		ReadOnlyBindableCollection<TViewModel> childNodes {
			get {
				if (〆childNodes == null) 〆childNodes = generateChildCollection(this.Model);
				return 〆childNodes;
			}
		}
		IReadOnlyList<TViewModel> IReadOnlyTreeNode<TViewModel>.Children {
			get { return childNodes; }
		}
		/// <summary>子ノードのコレクションを取得する。</summary>
		public ReadOnlyBindableCollection<TViewModel> Children {
			get { return childNodes; }
		}
		
		//bool _isDisposed;
		///// <summary>既に破棄されているかどうかを示す値を取得する。</summary>
		//protected bool IsDisposed { get { return _isDisposed; } }
		///// <summary>ビューモデルを破棄する。</summary>
		//public void Dispose() {
		//	this.Dispose(true);
		//	GC.SuppressFinalize(this);
		//}
		///// <summary>ビューモデルを破棄する。</summary>
		//protected virtual void Dispose(bool disposing) {
		//	if (IsDisposed) return;
		//	if (disposing) {
		//		_parent = null;
		//		childNodes.Dispose();
		//	}
		//	_isDisposed = true;
		//}
		///// <summary>既に破棄されているインスタンスの操作を禁止する。</summary>
		//protected void ThrowExceptionIfDisposed() {
		//	if (IsDisposed)
		//		throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
		//}
	}
	/// <summary>ビューによってバインドされる簡易的なツリー構造として参照元のノードをラップする。</summary>
	/// <typeparam name="TModel">各ノードが内包するモデルの型</typeparam>
	public abstract class ReadOnlyBindableTreeNode<TModel> : ReadOnlyBindableTreeNode<TModel, ReadOnlyBindableTreeNode<TModel>>
	where TModel : IReadOnlyObservableTreeNode<TModel> {
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="model">参照するノード</param>
		public ReadOnlyBindableTreeNode(TModel model) : base(model) { }
	}
	public static class ReadOnlyBindableTreeNode {
		public static TViewModel Create<TModel,TViewModel>(TModel root, Func<TModel,TViewModel> generate)
		where TModel : IReadOnlyObservableTreeNode<TModel>
		where TViewModel : ReadOnlyBindableTreeNode<TModel,TViewModel> {
			return generate(root);
		}
	}
	/*
	/// <summary>
	/// 使用時に子ノードの生成関数を指定可能なラッパーインスタンス
	/// </summary>
	/// <typeparam name="TModel">各ノードが内包するモデルの型</typeparam>
	/// <typeparam name="TViewModel">各ノードの共通実装として公開する型</typeparam>
	public class ReadOnlyBindableTreeNode<TModel, TViewModel> : ReadOnlyBindableTreeNodeBase<TModel, TViewModel>
	where TModel : IReadOnlyObservableTreeNode<TModel>
	where TViewModel : ReadOnlyBindableTreeNode<TModel,TViewModel>{
		Func<TModel, TViewModel> _generate;
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="model">モデル</param>
		/// <param name="generate">モデルからビューモデルを生成する関数</param>
		public ReadOnlyBindableTreeNode(TModel model,Func<TModel,TViewModel> generate) : base(model) {
			if (generate == null) throw new ArgumentNullException("generate");
			_generate = generate;
		}
		/// <summary>モデルからノードを生成する。このメンバーはオーバーライドできません。</summary>
		/// <param name="modelChildNode">現在のモデルの子ノード</param>
		protected sealed override TViewModel GenerateChild(TModel modelChildNode) {
			return _generate(modelChildNode);
		}
	}*/
}
