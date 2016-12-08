using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Collections.ObjectModel {
	public class CollectionBasedDictionary<TKey,TValue>
		: KeyedCollection<TKey, KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> {

	}
}
