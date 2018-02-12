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

namespace libTech.UI {
	public static class ScreenService {
		static Dictionary<Type, object> ScreenInstances;
		//static UIScreenStack ScreenStack;

		/*internal static void Init(IUltravioletUI UI) {
			ScreenStack = UI.GetScreens();
			ScreenInstances = new Dictionary<Type, object>();
		}*/

		internal static void RegisterInstance(Type InstanceType, object Value) {
			if (ScreenInstances.ContainsKey(InstanceType))
				throw new Exception("Can only have one instance of " + InstanceType);

			ScreenInstances.Add(InstanceType, Value);
		}

		internal static void RegisterInstance<T>(T Scr) where T : Screen {
			RegisterInstance(Scr.GetType(), Scr);
		}

		internal static object GetInstance(Type InstanceType) {
			if (!ScreenInstances.ContainsKey(InstanceType))
				throw new Exception("Instance of " + InstanceType + " not found, create one first");

			return ScreenInstances[InstanceType];
		}

		internal static T GetInstance<T>() where T : Screen {
			return (T)GetInstance(typeof(T));
		}

		internal static void Open<T>(T Scr, TimeSpan? Duration = null) where T : Screen {
			//ScreenStack.Open(Scr, Duration);
		}

		public static void Register<T>(T Inst = null) where T : Screen {
			if (Inst == null)
				Activator.CreateInstance<T>();
		}

		public static void Open(Type ScreenType, TimeSpan? Duration = null) {
			Open((Screen)GetInstance(ScreenType), Duration);
		}

		public static void Open<T>(TimeSpan? Duration = null) where T : Screen {
			Open(GetInstance<T>(), Duration);
		}
	}
}
