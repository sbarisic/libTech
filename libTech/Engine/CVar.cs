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

	public class CVar {
		public string Name;
		public string Info;

		public CVarType CVarType;

		object Val;
		object DefaultValue;

		public object Value {
			get {
				if (TriggerOnGet && OnGet != null) {
					TriggerOnGet = false;
					object Ret = OnGet(this);
					TriggerOnGet = true;

					return Ret;
				}

				return Val;
			}

			set {
				if (CVarType.HasFlag(CVarType.ReadOnly)) {
					// TODO: Cannot set read only variable
					return;
				}

				if (CVarType.HasFlag(CVarType.Init) && !CVar.InitMode) {
					// TODO: Trying to set variable after it has been initialized 
					return;
				}

				object Old = Val;
				Val = value;

				if (TriggerOnSet && OnSet != null) {
					TriggerOnSet = false;
					OnSet(this, Old, Val);
					TriggerOnSet = true;
				}
			}
		}

		bool TriggerOnSet;
		public CVarSetFunc OnSet;

		bool TriggerOnGet;
		public CVarGetFunc OnGet;

		public CVar(string Name, object DefaultValue, CVarType Type, CVarSetFunc OnSet = null, CVarGetFunc OnGet = null) {
			Info = Name;
			TriggerOnSet = true;
			TriggerOnGet = true;

			this.OnSet = OnSet;
			this.OnGet = OnGet;
			this.Name = Name;
			CVarType = Type;

			Val = DefaultValue;
			Value = Val;
			this.DefaultValue = Value;
		}

		public override string ToString() {
			return string.Format("{0} = '{1}', default '{2}'", Name, Value ?? "null", DefaultValue ?? "null");
		}

		static List<CVar> CVars;
		public static bool InitMode;

		static CVar() {
			CVars = new List<CVar>();
			InitMode = false;
		}

		public static CVar[] GetAll() {
			return CVars.ToArray();
		}

		public static CVar Find(string Name) {
			foreach (var V in CVars)
				if (V.Name == Name)
					return V;

			return null;
		}

		public static CVar Register(string Name, object DefaultVal = null, CVarType Type = CVarType.Default, CVarSetFunc OnSet = null, CVarGetFunc OnGet = null) {
			if (Find(Name) != null)
				throw new Exception("CVar " + Name + " already registered");

			CVar Var = new CVar(Name, DefaultVal, Type, OnSet, OnGet);
			CVars.Add(Var);
			return Var;
		}

		public static string GetString(string Name, string Default = "") {
			CVar CVar = Find(Name);

			if (CVar != null) {
				object Val = CVar.Value;

				if (Val != null)
					return Val.ToString();
			}

			return Default;
		}

		public static int GetInt(string Name, int Default = 0) {
			string Str = GetString(Name);

			if (int.TryParse(Str, out int I))
				return I;

			return Default;
		}

		public static bool GetBool(string Name, bool Default = false) {
			string Str = GetString(Name).ToLower();

			if (Str == "1")
				return true;
			else if (Str == "0")
				return false;

			if (bool.TryParse(Str, out bool B))
				return B;

			return Default;
		}
	}
}
