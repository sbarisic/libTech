using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;
using CARP;
using System.IO;

using NuklearDotNet;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace libTech.UI {
	public class Screen {
		public Screen() {
			ScreenService.RegisterInstance(this);
		}
	}
}