using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.GUI {
	public class GUIControl {
		List<GUIControl> Children;

		public GUIControl() {
			Children = new List<GUIControl>();
		}

		public virtual void Render() {
			foreach (var Ctrl in Children)
				Ctrl.Render();
		}
	}

	public class libGUI : GUIControl {
		public libGUI() {
		}
	}
}
