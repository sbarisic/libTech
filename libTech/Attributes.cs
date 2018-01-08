using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
	public sealed class ClientAttribute : Attribute {
		public ClientAttribute() {
		}
	}

	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
	public sealed class ServerAttribute : Attribute {
		public ServerAttribute() {
		}
	}
}
