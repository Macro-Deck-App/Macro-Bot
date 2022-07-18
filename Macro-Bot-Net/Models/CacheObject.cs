using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develeon64.MacroBot.Models {
	public class CacheObject<T> {
		private T data;
		private DateTime time;

		public CacheObject (T data) {
			this.data = data;
			this.time = DateTime.Now;
		}

		public T? GetValue () {
			return DateTime.Now.Subtract(this.time).TotalMinutes <= 5 ? data : default;
		}

		public Type Type {
			get {
				return typeof(T);
			}
		}
	}
}
