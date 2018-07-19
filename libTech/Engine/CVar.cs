using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	[Flags]
	public enum CVarType : int {
		Default = (1 << 0),
		Archive = (1 << 1), // Saved to config
		Userinfo = (1 << 2), // Sent to server
		Replicated = (1 << 3), // Replicated from server

		Init = (1 << 4), // Can be set on init only, not from console
		ReadOnly = (1 << 5), // Cannot be set at all
		Unsafe = (1 << 6), // Might cause a crash when changing
		Cheat = (1 << 7), // Is a cheat
	}

	public delegate void CVarSetFunc(CVar This, object OldValue, object NewValue);
	public delegate object CVarGetFunc(CVar This);

	public abstract class CVar {
		object _ObjectValue;

		public virtual object ObjectValue {
			get {
				return _ObjectValue;
			}

			set {
				_ObjectValue = value;
			}
		}

		public readonly string Name;
		public readonly CVarType Type;

		protected CVar(string Name, CVarType Type) {
			this.Name = Name;
			this.Type = Type;
		}

		static List<CVar> CVars = new List<CVar>();

		public static CVar<T> Register<T>(string Name, T Value, CVarType Type = CVarType.Default) {
			if (Find(Name) != null)
				throw new Exception("Variable already registered '" + Name + "'");

			CVar<T> CVar = new CVar<T>(Name, Value, Type);
			CVars.Add(CVar);
			return CVar;
		}

		public static CVar Find(string Name) {
			foreach (var CVar in CVars)
				if (CVar.Name == Name)
					return CVar;

			return null;
		}

		public static CVar<T> Find<T>(string Name) {
			return (CVar<T>)Find(Name);
		}

		public static IEnumerable<CVar> GetAll() {
			return CVars.ToArray();
		}

		public static IEnumerable<CVar<T>> GetAll<T>() {
			foreach (var CVar in CVars)
				if (CVar is CVar<T>)
					yield return (CVar<T>)CVar;
		}

		public override string ToString() {
			return string.Format("{0} = '{1}'", Name, ObjectValue);
		}
	}

	public class CVar<T> : CVar {
		public virtual T Value {
			get => (T)ObjectValue;
			set => ObjectValue = value;
		}

		internal CVar(string Name, T Value, CVarType Type) : base(Name, Type) {
			this.Value = Value;
		}

		public static implicit operator T(CVar<T> CVar) {
			return CVar.Value;
		}
	}
}
