using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin {

	/// <summary>文字列に対する、TryParseパターンによって処理されるメソッドを表す。</summary>
	/// <typeparam name="TValue">出力値の型</typeparam>
	/// <param name="input">入力する文字列</param>
	/// <param name="value">出力値</param>
	/// <returns>実行の成否</returns>
	public delegate bool TryParseCallback<TValue>(string input,[MaybeNullWhen(false)] out TValue value);

	/// <summary>TryParseパターンによって処理されるメソッドを表す。</summary>
	/// <typeparam name="TInput">入力値の型</typeparam>
	/// <typeparam name="TValue">出力値の型</typeparam>
	/// <param name="input">入力値</param>
	/// <param name="value">出力値</param>
	/// <returns>実行の成否</returns>
	public delegate bool TryCallback<in TInput,TValue>(TInput input, [MaybeNullWhen(false)] out TValue value);

	/// <summary>TryParseパターンによって処理されるメソッドを表す。</summary>
	/// <typeparam name="TValue">出力値の型</typeparam>
	/// <param name="value">出力値</param>
	/// <returns>実行の成否</returns>
	public delegate bool TryCallback<TValue>([MaybeNullWhen(false)] out TValue value);

	/// <summary>ResultWithValueに関するstaticメソッドを提供する。</summary>
	public static class ResultWithValue {

		/// <summary>文字列に対する、TryParseメソッドをサポートする。</summary>
		/// <typeparam name="TValue">出力値の型</typeparam>
		/// <param name="tryParse">TryParseメソッド</param>
		/// <param name="input">入力文字列</param>
		/// <returns>結果とその値</returns>
		public static ResultWithValue<TValue> Of<TValue>(TryParseCallback<TValue> tryParse, string input) {
			//TValue value;
			if (tryParse(input, out var value))
				return new ResultWithValue<TValue>(value);
			else
				return new ResultWithValue<TValue>();
		}
		/// <summary>TryParseパターンによる処理をサポートする。</summary>
		/// <typeparam name="TInput">入力値の型</typeparam>
		/// <typeparam name="TValue">出力値の型</typeparam>
		/// <param name="try">Tryメソッド</param>
		/// <param name="input">入力値</param>
		/// <returns>結果とその値</returns>
		public static ResultWithValue<TValue> Of<TInput, TValue>(TryCallback<TInput, TValue> @try, TInput input) {
			//TValue value;
			if (@try(input, out var value))
				return new ResultWithValue<TValue>(value);
			else
				return new ResultWithValue<TValue>();
		}

		/// <summary>TryParseパターンによる処理をサポートする。</summary>
		/// <typeparam name="TValue">出力値の型</typeparam>
		/// <param name="try">Tryメソッド</param>
		/// <returns>結果とその値</returns>
		public static ResultWithValue<TValue> Of<TValue>(TryCallback<TValue> @try) {
			//TValue value;
			if (@try(out var value))
				return new ResultWithValue<TValue>(value);
			else
				return new ResultWithValue<TValue>();
		}
	}
}
