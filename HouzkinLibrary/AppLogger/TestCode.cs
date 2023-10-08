using Houzkin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLogger {
	public static class TestCode {

		public static void Main(string[] args) {
			var dic = new Dictionary<int, string>();
			dic.Add(1, "AAA");
			dic.Add(2, "BBB");
			var rst = ResultWithValue.Of<int, string>(dic.TryGetValue,5);
			Console.WriteLine(rst.Result);

			string num = "1000";
			//int.TryParse(num,result)
			var result = ResultWithValue.Of<int>(int.TryParse, num);
			Console.WriteLine(result.Value);
			Console.ReadKey();
		}
	}
}
