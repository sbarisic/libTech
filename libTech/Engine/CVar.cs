using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Reflection;

namespace libTech {
	[Flags]
	public enum ConVarType : int {
		Default = (1 << 0),
		Archive = (1 << 1), // Saved to config
		Userinfo = (1 << 2), // Sent to server
		Replicated = (1 << 3), // Replicated from server

		Init = (1 << 4), // Can be set on init only, not from console
		ReadOnly = (1 << 5), // Cannot be set at all
		Unsafe = (1 << 6), // Might cause a crash when changing
		Cheat = (1 << 7), // Is a cheat
	}

	//public delegate void ConVarSetFunc(ConVar This, object OldValue, object NewValue);
	//public delegate object ConVarGetFunc(ConVar This);

	internal static class ConItems {
		static List<ConItem> Items = new List<ConItem>();

		public static void Register(string Name, ConItem Item) {
			if (Find(Name) != null)
				throw new Exception("Console item already registered '" + Name + "'");

			Items.Add(Item);
		}

		public static ConItem Find(string Name) {
			foreach (var Item in Items)
				if (Item.Name == Name)
					return Item;

			return null;
		}

		public static bool TryFind(string Name, out ConItem Item) {
			Item = Find(Name);
			return Item != null;
		}

		public static T Find<T>(string Name) where T : ConItem {
			foreach (var Item in Items)
				if (Item is T && Item.Name == Name)
					return (T)Item;

			return null;
		}

		public static IEnumerable<ConItem> GetAll() {
			return Items.ToArray();
		}

		public static IEnumerable<T> GetAll<T>() where T : ConItem {
			foreach (var ConItem in Items)
				if (ConItem is T)
					yield return (T)ConItem;
		}
	}

	public abstract class ConItem {
		public readonly string Name;

		public ConItem(string Name) {
			this.Name = Name;
		}

		public override string ToString() {
			return Name;
		}
	}

	public abstract class ConVar : ConItem {
		object _ObjectValue;

		public virtual object ObjectValue {
			get {
				return _ObjectValue;
			}

			set {
				_ObjectValue = value;
			}
		}
		public virtual string StringValue {
			get {
				return ObjectValue.ToString();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public readonly ConVarType Type;

		protected ConVar(string Name, ConVarType Type) : base(Name) {
			this.Type = Type;
		}

		public override string ToString() {
			return string.Format(CultureInfo.InvariantCulture, "{0} = \"{1}\"", Name, ObjectValue);
		}

		public static ConVar<T> Register<T>(string Name, T Value, ConVarType Type = ConVarType.Default) {
			ConVar<T> CVar = new ConVar<T>(Name, Value, Type);
			ConItems.Register(Name, CVar);
			return CVar;
		}

		public static bool TryFind(string Name, out ConVar Var) {
			Var = ConItems.Find(Name) as ConVar;
			return Var != null;
		}

		public static IEnumerable<ConVar> GetAll() {
			return ConItems.GetAll<ConVar>();
		}
	}

	public class ConVar<T> : ConVar {
		public virtual T Value {
			get => (T)ObjectValue;
			set => ObjectValue = value;
		}

		public override string StringValue {
			get => base.StringValue;

			set {
				string Val = value;

				if (typeof(T).IsClass() && Val.ToLower() == "null") {
					Value = (T)(object)null;
					return;
				}

				if (typeof(T) == typeof(string)) {
					Value = (T)(object)Val;
				} else if (typeof(T) == typeof(int)) {
					Value = (T)(object)int.Parse(Val, CultureInfo.InvariantCulture);
				} else if (typeof(T) == typeof(float)) {
					Value = (T)(object)float.Parse(Val, CultureInfo.InvariantCulture);
				} else if (typeof(T) == typeof(bool)) {
					Value = (T)(object)bool.Parse(Val);
				} else
					throw new NotImplementedException(string.Format("String parsing for type '{0}' not implemented", typeof(T).Name));
			}
		}

		internal ConVar(string Name, T Value, ConVarType Type) : base(Name, Type) {
			this.Value = Value;
		}

		public static implicit operator T(ConVar<T> CVar) {
			return CVar.Value;
		}
	}

	public delegate void ConCmdAction(string[] Argv);
	public delegate string[] ConCmdAutocompleteAction(ConCmd Cmd, string[] Argv);

	public class ConCmd : ConItem {
		public virtual ConCmdAction Command { get; private set; }
		ConCmdAutocompleteAction Autocomplete;

		public ConCmd(string Name, ConCmdAction Command, ConCmdAutocompleteAction Autocomplete) : base(Name) {
			this.Command = Command;
			this.Autocomplete = Autocomplete;
		}

		public string[] GetAutocomplete(string[] Argv) {
			return Autocomplete(this, Argv);
		}

		static string[] DefaultAutocomplete(ConCmd Cmd, string[] Argv) {
			if (Argv.Length == 0)
				return new string[] { };

			return new string[] { string.Format("{0} {1}", Cmd.Name, string.Join(" ", Argv.Skip(1))).Trim() };
		}

		public static ConCmd Register(string Name, ConCmdAction Command, ConCmdAutocompleteAction Autocomplete = null) {
			ConCmd Cmd = new ConCmd(Name, Command, Autocomplete ?? DefaultAutocomplete);
			ConItems.Register(Name, Cmd);
			return Cmd;
		}

		public static bool TryFind(string Name, out ConCmd Var) {
			Var = ConItems.Find(Name) as ConCmd;
			return Var != null;
		}

		public static IEnumerable<ConCmd> GetAll() {
			return ConItems.GetAll<ConCmd>();
		}
	}
}
