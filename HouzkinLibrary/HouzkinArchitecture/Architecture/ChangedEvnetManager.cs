using Houzkin.Architecture;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace System.ComponentModel {

	/// <summary>
	/// プロパティ値またはコレクション変更通知の発行を管理する。
	/// </summary>
	[Serializable]
	public sealed class NotifyChangedEventManager  : INotifyPropertyChanged, INotifyCollectionChanged {
		
		/// <summary>新規インスタンスを初期化する。</summary>
		public NotifyChangedEventManager () { } 
		/// <summary>変更通知の制御を指定したスレッドで処理する新規インスタンスを初期化する。</summary>
		/// <param name="owner">このオブジェクトを利用するインスタンス。</param>
		/// <param name="dispatcher">動作するスレッドが関連付けられているディスパッチャ</param>
		public NotifyChangedEventManager (object owner){
			this._owner = owner;
		}
		
		object _owner;
		object owner {
			get {
				if (_owner == null) return this;
				else return _owner;
			}
		}
		
		string _indexPropertyName;
		/// <summary>インデックスを使用して取得するプロパティ名</summary>
		public string IndexPropertyName {
			get { return string.IsNullOrEmpty(_indexPropertyName) ? "Item[]" : _indexPropertyName; }
			set { _indexPropertyName = value; }
		}
		/// <summary>プロパティ値が変更されたときに発生する。</summary>
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>コレクションが変更されたときに発生する。</summary>
		[field:NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		[NonSerialized]
		ISet<string> _bn;
		/// <summary>不感プロパティ名。Disposeによって解除される。</summary>
		ISet<string> blockingName {
			get {
				if (_bn == null) _bn = new HashSet<string>();
				return _bn;
			}
		}
		[NonSerialized]
		ISet<string> _rn;
		/// <summary>処理中のプロパティ名</summary>
		ISet<string> raisingName {
			get {
				if (_rn == null) _rn = new HashSet<string>();
				return _rn;
			}
		}

		/// <summary>再帰的な発行を行わない。</summary>
		private void Raise(object sender, string name,NotifyCollectionChangedEventArgs e) {
			if (raisingName.Add(name)) {
				if (name == this.IndexPropertyName && e != null) {
					this.CollectionChanged?.Invoke(sender, e);
				} else {
					this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
				}
				raisingName.Remove(name);
			}
		}
		/// <summary>ブロックできた場合、同名の発行をブロックして発行する。</summary>
		private void RaiseWithBlock(object sender, string name,NotifyCollectionChangedEventArgs e) {
			if (blockingName.Add(name)) {
				Raise(sender, name,e);
				blockingName.Remove(name);
			}
		}
		/// <summary>ブロックできたら解除Keyを返す</summary>
		private bool Block(string name, out IDisposable dispose) {
			if (blockingName.Add(name)) {
				dispose = new Dsp(() => blockingName.Remove(name));
				return true;
			}
			dispose = new Dsp(null);
			return false;
		}
		/// <summary>コレクション変更通知を発行する。</summary>
		public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			this.onChanged(owner, this.IndexPropertyName, e);
		}
		/// <summary>コレクション変更通知を発行する。</summary>
		public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			this.onChanged(sender, this.IndexPropertyName, e);
		}
		/// <summary>プロパティ変更通知を発行する。</summary>
		public void OnPropertyChanged(string name) {
			this.onChanged(owner, name, null);
		}
		/// <summary>プロパティ変更通知を発行する。</summary>
		public void OnPropertyChanged(object sender, string name) {
			this.onChanged(sender, name, null);
		}
		void onChanged(object sender, string name, NotifyCollectionChangedEventArgs e) {
			RaiseWithBlock(sender, name, e);
		}
		/// <summary>プロパティ変更通知の発行とブロックの解除キーを取得する。</summary>
		/// <param name="sender">イベントソース</param>
		/// <param name="name">プロパティ名</param>
		/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
		public IDisposable OnPropertyChanged(object sender, string name, bool delay) {
			return onChanged(sender, name, null, delay);
		}
		/// <summary>プロパティ変更通知の発行とブロックの解除キーを取得する。</summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
		public IDisposable OnPropertyChanged(string name, bool delay) {
			return this.OnPropertyChanged(owner, name, delay);
		}
		/// <summary>コレクション変更通知の発行とブロックの解除キーを取得する。</summary>
		/// <param name="sender">イベントソース</param>
		/// <param name="e">イベント引数</param>
		/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
		public IDisposable OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, bool delay) {
			return onChanged(sender, IndexPropertyName, e, delay);
		}
		/// <summary>コレクション変更通知の発行とブロックの解除キーを取得する。</summary>
		/// <param name="e">イベント引数</param>
		/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
		public IDisposable OnCollectionChanged(NotifyCollectionChangedEventArgs e, bool delay) {
			return this.OnCollectionChanged(owner, e, delay);
		}
		IDisposable onChanged(object sender, string name, NotifyCollectionChangedEventArgs e, bool delay) {
			IDisposable blocker;
			Dsp dsp;
			if (Block(name, out blocker)) { //ブロックできた場合
				if (delay) {
					dsp = new Dsp(() => {
						Raise(sender, name, e);
						blocker.Dispose();
					});
				} else {
					Raise(sender, name, e);
					dsp = new Dsp(() => {
						blocker.Dispose();
					});
				}
			} else {
				dsp = new Dsp(null);
			}
			return dsp;
		}
		/// <summary>指定されたプロパティ名での発行が現在ブロックされているかどうかを示す値を取得する。</summary>
		public bool IsBlocking(string name) {
			return blockingName.Contains(name);
		}
		/// <summary>コレクション変更通知の発行が現在ブロックされているかどうかを示す値を取得する。</summary>
		public bool IsBlocking() {
			return this.IsBlocking(this.IndexPropertyName);
		}
		private class Dsp : IDisposable {
			Action _dispose;
			public Dsp(Action dispose) { this._dispose = dispose; }
			public void Dispose() {
				if (_dispose != null) {
					_dispose();
					_dispose = null;
				}
			}
		}// end inner class
	}
	///// <summary>スレッドセーフな、プロパティ値またはコレクション変更通知の発行を管理する。</summary>
	//[Serializable]
	//public sealed class NotifyChangedEventDispatcher : INotifyPropertyChanged, INotifyCollectionChanged {
		
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	public NotifyChangedEventDispatcher() : this(Livet.DispatcherHelper.UIDispatcher) { } 
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="owner">このオブジェクトを利用するインスタンス。</param>
	//	public NotifyChangedEventDispatcher(object owner)
	//	: this(owner, Dispatcher.CurrentDispatcher){ }
	//	/// <summary>変更通知の制御を指定したスレッドで処理する新規インスタンスを初期化する。</summary>
	//	/// <param name="owner">このオブジェクトを利用するインスタンス。</param>
	//	/// <param name="dispatcher">動作するスレッドが関連付けられているディスパッチャ</param>
	//	public NotifyChangedEventDispatcher(object owner, Dispatcher dispatcher)
	//	: this(dispatcher){
	//		this._owner = owner;
	//	}
	//	/// <summary>変更通知の制御を指定したスレッドで処理する新規インスタンスを初期化する。</summary>
	//	/// <param name="dispatcher">動作するスレッドが関連付けられているディスパッチャ</param>
	//	public NotifyChangedEventDispatcher(Dispatcher dispatcher) {
	//		if (dispatcher == null) throw new ArgumentNullException();
	//		this._rundp = dispatcher;
	//	}
	//	object _owner;
	//	object owner {
	//		get {
	//			if (_owner == null) return this;
	//			else return _owner;
	//		}
	//	}
	//	Dispatcher _rundp;
	//	Dispatcher runDpt {
	//		get {
	//			//if (_rundp == null) _rundp = Dispatcher.CurrentDispatcher;
	//			return _rundp;
	//		}
	//	}
	//	string _indexPropertyName;
	//	/// <summary>インデックスを使用して取得するプロパティ名</summary>
	//	public string IndexPropertyName {
	//		get { return string.IsNullOrEmpty(_indexPropertyName) ? "Item[]" : _indexPropertyName; }
	//		set { _indexPropertyName = value; }
	//	}
	//	/// <summary>プロパティ値が変更されたときに発生する。</summary>
	//	[field: NonSerialized]
	//	public event PropertyChangedEventHandler PropertyChanged;

	//	/// <summary>コレクションが変更されたときに発生する。</summary>
	//	[field:NonSerialized]
	//	public event NotifyCollectionChangedEventHandler CollectionChanged;

	//	[NonSerialized]
	//	ISet<string> _bn;
	//	/// <summary>不感プロパティ名。Disposeによって解除される。</summary>
	//	ISet<string> blockingName {
	//		get {
	//			if (_bn == null) _bn = new HashSet<string>();
	//			return _bn;
	//		}
	//	}
	//	[NonSerialized]
	//	ISet<string> _rn;
	//	/// <summary>処理中のプロパティ名</summary>
	//	ISet<string> raisingName {
	//		get {
	//			if (_rn == null) _rn = new HashSet<string>();
	//			return _rn;
	//		}
	//	}

	//	/// <summary>再帰的な発行を行わない。</summary>
	//	private void Raise(object sender, string name,NotifyCollectionChangedEventArgs e) {
	//		if (raisingName.Add(name)) {
	//			if (name == this.IndexPropertyName && e != null) {
	//				this.CollectionChanged?.Invoke(sender, e);
	//			} else {
	//				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
	//			}
	//			raisingName.Remove(name);
	//		}
	//	}
	//	/// <summary>ブロックできた場合、同名の発行をブロックして発行する。</summary>
	//	private void RaiseWithBlock(object sender, string name,NotifyCollectionChangedEventArgs e) {
	//		if (blockingName.Add(name)) {
	//			Raise(sender, name,e);
	//			blockingName.Remove(name);
	//		}
	//	}
	//	/// <summary>ブロックできたら解除Keyを返す</summary>
	//	private bool Block(string name, out IDisposable dispose) {
	//		if (blockingName.Add(name)) {
	//			dispose = new Dsp(runDpt,() => blockingName.Remove(name));
	//			return true;
	//		}
	//		dispose = new Dsp(runDpt, null);
	//		return false;
	//	}
	//	/// <summary>コレクション変更通知を発行する。</summary>
	//	public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
	//		this.onChanged(owner, this.IndexPropertyName, e);
	//	}
	//	/// <summary>コレクション変更通知を発行する。</summary>
	//	public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
	//		this.onChanged(sender, this.IndexPropertyName, e);
	//	}
	//	/// <summary>プロパティ変更通知を発行する。</summary>
	//	public void OnPropertyChanged(string name) {
	//		this.onChanged(owner, name, null);
	//	}
	//	/// <summary>プロパティ変更通知を発行する。</summary>
	//	public void OnPropertyChanged(object sender, string name) {
	//		this.onChanged(sender, name, null);
	//	}
	//	void onChanged(object sender, string name, NotifyCollectionChangedEventArgs e) {
	//		if (Thread.CurrentThread != runDpt.Thread) {
	//			runDpt.Invoke(new Action(() => RaiseWithBlock(sender, name, e)));
	//		} else {
	//			RaiseWithBlock(sender, name, e);
	//		}
	//	}
	//	/// <summary>プロパティ変更通知の発行とブロックの解除キーを取得する。</summary>
	//	/// <param name="sender">イベントソース</param>
	//	/// <param name="name">プロパティ名</param>
	//	/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
	//	public IDisposable OnPropertyChanged(object sender, string name, bool delay) {
	//		if (Thread.CurrentThread != runDpt.Thread) {
	//			return runDpt.Invoke(new Func<IDisposable>(() => onChanged(sender, name, null, delay)));
	//		} else {
	//			return onChanged(sender, name, null, delay);
	//		}
	//	}
	//	/// <summary>プロパティ変更通知の発行とブロックの解除キーを取得する。</summary>
	//	/// <param name="name">プロパティ名</param>
	//	/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
	//	public IDisposable OnPropertyChanged(string name, bool delay) {
	//		return this.OnPropertyChanged(owner, name, delay);
	//	}
	//	/// <summary>コレクション変更通知の発行とブロックの解除キーを取得する。</summary>
	//	/// <param name="sender">イベントソース</param>
	//	/// <param name="e">イベント引数</param>
	//	/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
	//	public IDisposable OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, bool delay) {
	//		if (Thread.CurrentThread != runDpt.Thread) {
	//			return runDpt.Invoke(new Func<IDisposable>(() => onChanged(sender, IndexPropertyName, e, delay)));
	//		} else {
	//			return onChanged(sender, IndexPropertyName, e, delay);
	//		}
	//	}
	//	/// <summary>コレクション変更通知の発行とブロックの解除キーを取得する。</summary>
	//	/// <param name="e">イベント引数</param>
	//	/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
	//	public IDisposable OnCollectionChanged(NotifyCollectionChangedEventArgs e, bool delay) {
	//		return this.OnCollectionChanged(owner, e, delay);
	//	}
	//	IDisposable onChanged(object sender, string name, NotifyCollectionChangedEventArgs e, bool delay) {
	//		IDisposable blocker;
	//		Dsp dsp;
	//		if (Block(name, out blocker)) { //ブロックできた場合
	//			if (delay) {
	//				dsp = new Dsp(runDpt, () => {
	//					Raise(sender, name, e);
	//					blocker.Dispose();
	//				});
	//			} else {
	//				Raise(sender, name, e);
	//				dsp = new Dsp(runDpt, () => {
	//					blocker.Dispose();
	//				});
	//			}
	//		} else {
	//			dsp = new Dsp(runDpt, null);
	//		}
	//		return dsp;
	//	}
	//	/// <summary>指定されたプロパティ名での発行が現在ブロックされているかどうかを示す値を取得する。</summary>
	//	public bool IsBlocking(string name) {
	//		if (Thread.CurrentThread != runDpt.Thread) {
	//			return runDpt.Invoke(new Func<bool>(() => blockingName.Contains(name)));
	//		} else {
	//			return blockingName.Contains(name);
	//		}
	//	}
	//	/// <summary>コレクション変更通知の発行が現在ブロックされているかどうかを示す値を取得する。</summary>
	//	public bool IsBlocking() {
	//		return this.IsBlocking(this.IndexPropertyName);
	//	}
	//	private class Dsp : IDisposable {
	//		Dispatcher _dpt;
	//		Action _dispose;
	//		public Dsp(Dispatcher dpt, Action dispose) { this._dispose = dispose; this._dpt = dpt; }
	//		public void Dispose() {
	//			if (_dispose != null) {
	//				if (Thread.CurrentThread != _dpt.Thread) {
	//					_dpt.Invoke(new Action(() => _dispose()));
	//				} else {
	//					_dispose();
	//				}
	//				_dispose = null;
	//			}
	//		}
	//	}// end inner class
	//}
}
