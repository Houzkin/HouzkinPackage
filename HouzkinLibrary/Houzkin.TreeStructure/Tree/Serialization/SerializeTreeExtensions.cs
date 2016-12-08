using Houzkin.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree.Serialization {

	/// <summary>シリアライズに関する拡張メソッドを提供する。</summary>
	public static class SerializeTreeExtensions {
		/// <summary>各ノードから生成したシリアライズ用のインスタンスとそのインデックスを、XmlSerializerによってシリアライズ可能なコレクションとして取得する。</summary>
		/// <typeparam name="U">シリアライズするオブジェクトの型</typeparam>
		/// <typeparam name="T">各ノードの型</typeparam>
		/// <param name="self">始点となるノード</param>
		/// <param name="conv">各ノードからシリアライズ用のオブジェクトを取得する関数</param>
		public static SerializableNodeMap<U> ToSerializableNodeMap<U, T>(this T self, Func<T, U> conv)
		where T : IReadOnlyTreeNode<T> {
			return new SerializableNodeMap<U>(self.ToNodeMap(conv));
		}
		/// <summary>各ノードとそのインデックスを、XmlSerializerによってシリアライズ可能なコレクションとして取得する。</summary>
		/// <typeparam name="T">シリアライズするノードの型</typeparam>
		public static SerializableNodeMap<T> ToSerializableNodeMap<T>(this T self)
		where T : IReadOnlyTreeNode<T> {
			return self.ToSerializableNodeMap(x => x);
		}
	}
}

