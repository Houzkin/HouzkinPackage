using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Livet;

namespace Houzkin.Architecture {
	public class PropertyChainChangedWeakEventListener : WeakEventListener<PropertyChangedEventHandler,PropertyChangedEventArgs> {
		public PropertyChainChangedWeakEventListener(Action<PropertyChangedEventHandler> add, Action<PropertyChangedEventHandler> remove, EventHandler<PropertyChangedEventArgs> handler)
			: base(h => new PropertyChangedEventHandler(h), add, remove, handler) {

		}
	}
}
