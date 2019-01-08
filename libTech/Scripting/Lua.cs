using LuaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LL = LuaNET.Lua;

namespace libTech.Scripting {
	public static class Lua {
		static lua_StatePtr L;

		public static LuaReference DefaultEnvironment;
		public static LuaReference GUIEnvironment;

		internal static void Init() {
			L = LL.luaL_newstate();
			LL.luaL_openlibs(L);

			DefaultEnvironment = GetEnvironment();
			GUIEnvironment = CreateNewEnvironment(DefaultEnvironment);
		}

		public static void SetTable<T>(LuaReference Table, string Name, T Obj) where T : struct {
			Table.GetRef();

			using (LuaReference Tbl = ConvertToTable(Obj)) {
				Tbl.GetRef();
				LL.lua_setfield(L, -2, Name);
			}
		}

		public static LuaReference ConvertToTable<T>(T Obj) where T : struct {
			Type ObjType = Obj.GetType();
			FieldInfo[] Fields = ObjType.GetFields();

			LL.lua_newtable(L);

			for (int i = 0; i < Fields.Length; i++) {
				Advanced.Push(L, Fields[i].GetValue(Obj));
				LL.lua_setfield(L, -2, Fields[i].Name);
			}

			return new LuaReference(L);
		}

		public static void Set(LuaReference Table, string Name, object Obj) {
			Table.GetRef();

			if (Obj is LuaReference Ref)
				Ref.GetRef();
			else
				Advanced.Push(L, Obj);

			LL.lua_setfield(L, -2, Name);
			LL.lua_pop(L, 1);
		}

		public static T Get<T>(LuaReference Table, string Name) {
			int Top = GetTop();

			Table.GetRef();
			LL.lua_getfield(L, -1, Name);
			T Val = (T)Advanced.Get(L, 1, typeof(T));

			SetTop(Top);
			return Val;
		}

		public static void Copy(LuaReference TableFrom, LuaReference TableTo, string FromName, string ToName = null) {
			if (ToName == null)
				ToName = FromName;

			TableTo.GetRef();

			TableFrom.GetRef();
			LL.lua_getfield(L, -1, FromName);

			LL.lua_setfield(L, -3, ToName);
			LL.lua_pop(L, 2);
		}

		public static LuaFuncRef Compile(string Str) {
			if (LL.luaL_loadstring(L, Str) != 0)
				throw new Exception(LL.lua_tostring(L, -1));

			return new LuaFuncRef(L);
		}

		static void Run(LuaFuncRef FuncRef, int ReturnValues, object[] Args) {
			FuncRef.GetRef();

			for (int i = 0; i < Args.Length; i++)
				Advanced.Push(L, Args[i]);

			if (LL.lua_pcall(L, Args.Length, ReturnValues, 0) != 0)
				throw new Exception(LL.lua_tostring(L, -1));
		}

		public static T Run<T>(LuaFuncRef FuncRef, params object[] Args) {
			int Top = GetTop();

			Run(FuncRef, 1, Args);
			T Ret = (T)Advanced.Get(L, 1, typeof(T));

			SetTop(Top);
			return Ret;
		}

		public static void Run(LuaFuncRef FuncRef, params object[] Args) {
			Run(FuncRef, 0, Args);
		}

		public static void Run(string LuaStr, params object[] Args) {
			using (LuaFuncRef F = Compile(LuaStr))
				Run(F, Args);
		}

		public static T Run<T>(string LuaStr, params object[] Args) {
			using (LuaFuncRef F = Compile(LuaStr))
				return Run<T>(F, Args);
		}

		public static LuaReference GetEnvironment() {
			LL.lua_pushvalue(L, LL.LUA_GLOBALSINDEX);
			return new LuaReference(L);
		}

		public static void SetEnvironment(LuaFuncRef Func, LuaReference Env) {
			Func.GetRef();
			Env.GetRef();
			LL.lua_setfenv(L, -2);
			LL.lua_pop(L, 1);
		}

		public static LuaReference GetEnvironment(LuaFuncRef Func) {
			Func.GetRef();
			LL.lua_getfenv(L, -1);
			return new LuaReference(L);
		}

		public static LuaReference CreateNewEnvironment(LuaReference OldEnvironment = null) {
			if (OldEnvironment != null) {
				OldEnvironment.GetRef();
				CopyTable();
			} else
				LL.lua_newtable(L);

			LuaReference Ref = new LuaReference(L);

			if (OldEnvironment != null)
				LL.lua_pop(L, 1);

			return Ref;
		}

		static void CopyTable() {
			int Idx = LL.lua_gettop(L);
			LL.lua_newtable(L);

			LL.lua_pushnil(L);
			while (LL.lua_next(L, Idx) != 0) {
				LL.lua_pushvalue(L, -2);
				LL.lua_insert(L, -2);
				LL.lua_settable(L, -4);
			}
		}

		public static int GetTop() {
			return LL.lua_gettop(L);
		}

		public static void SetTop(int Top) {
			LL.lua_settop(L, Top);
		}

		public static string[] DumpStack() {
			return Advanced.DumpStack(L);
		}
	}
}
