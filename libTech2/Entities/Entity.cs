using FishGfx;

using libTech.Map;
using libTech.Materials;
using libTech.Physics;
using libTech.Reflection;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	sealed class EntityClassNameAttribute : Attribute {
		public string ClassName;

		public EntityClassNameAttribute(string ClassName) {
			this.ClassName = ClassName;
		}
	}

	[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
	sealed class EntityKeyAttribute : Attribute {
		public string KeyName;

		public EntityKeyAttribute(string KeyName) {
			this.KeyName = KeyName;
		}
	}

	struct EntityDefinition {
		public Type EntityType;
		public string ClassName;

		public EntityDefinition(Type T, EntityClassNameAttribute EntityClassName) {
			if (!T.Inherits<Entity>())
				throw new Exception(T.ToString() + " is not Entity");

			EntityType = T;
			ClassName = EntityClassName.ClassName;
		}

		public override string ToString() {
			return string.Format("{0}({1})", EntityType.Name, ClassName);
		}
	}

	public struct EntityKeyValue {
		public string Key;
		public string ValueRaw;
		public object Value;

		public EntityKeyValue(string Key, string ValueRaw) {
			this.Key = Key;
			this.Value = ValueRaw;
			this.ValueRaw = ValueRaw;
		}

		public EntityKeyValue(string Key, object Value) {
			this.Key = Key;
			this.Value = Value;
			this.ValueRaw = Value.ToString();
		}

		public override string ToString() {
			if (Value is string SValue)
				return Key + " = \"" + SValue + "\"";

			return string.Format("{0} = {1}", Key, Value);
		}
	}

	public class EntityKeyValues {
		List<EntityKeyValue> KVs;

		public libTechMap Map {
			get; private set;
		}

		public EntityKeyValues(libTechMap Map) {
			KVs = new List<EntityKeyValue>();
			this.Map = Map;
		}

		public void Add(EntityKeyValue KV) {
			KVs.Add(KV);
		}

		public bool TryGet(string Key, out EntityKeyValue Val) {
			foreach (var KV in KVs)
				if (KV.Key == Key) {
					Val = KV;
					return true;
				}

			Val = new EntityKeyValue();
			return false;
		}

		public EntityKeyValue Get(string Key) {
			foreach (var KV in KVs)
				if (KV.Key == Key)
					return KV;

			return new EntityKeyValue();
		}

		public T Get<T>(string Key, T Default = default(T)) {
			EntityKeyValue KV;

			if (!TryGet(Key, out KV))
				return Default;

			if (KV.Value is T Val)
				return Val;

			if (typeof(T) == typeof(Color)) {
				byte[] RGBA = KV.ValueRaw.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(V => byte.Parse(V)).ToArray();
				return (T)(object)new Color(RGBA[0], RGBA[1], RGBA[2]);
			} else if (typeof(T) == typeof(float))
				return (T)(object)float.Parse(KV.ValueRaw, CultureInfo.InvariantCulture);
			else if (typeof(T) == typeof(string))
				return (T)(object)KV.ValueRaw;

			throw new NotImplementedException();
		}

		public bool Contains(string Key) {
			foreach (var KV in KVs)
				if (KV.Key == Key)
					return true;

			return false;
		}

		public int Count {
			get {
				return KVs.Count;
			}
		}
	}

	public abstract class Entity {
		internal bool HasSpawned = false;

		public libTechMap Map;

		public string ClassName {
			get; private set;
		}

		public virtual void Spawned() {
		}

		public virtual BoundSphere GetBoundingSphere() {
			return BoundSphere.Empty;
		}

		public virtual void Update(float Dt) {
		}

		public virtual void DrawOpaque() {
		}

		public virtual void DrawTransparent() {
		}

		public virtual void DrawShadowVolume(BoundSphere Light, ShaderMaterial ShadowVolume) {
		}

		// Static stuffs

		static EntityDefinition[] EntityDefs;

		public static void LoadAllTypes() {
			List<EntityDefinition> EntityDefsList = new List<EntityDefinition>();

			Type[] EntityTypes = Reflect.GetTypesWithAttributes<EntityClassNameAttribute>().ToArray();
			for (int i = 0; i < EntityTypes.Length; i++)
				foreach (var EntClassName in EntityTypes[i].GetCustomAttributes<EntityClassNameAttribute>())
					EntityDefsList.Add(new EntityDefinition(EntityTypes[i], EntClassName));

			EntityDefs = EntityDefsList.ToArray();
		}

		public static Entity CreateInstance(string ClassName, EntityKeyValues KVs) {
			for (int DefIdx = 0; DefIdx < EntityDefs.Length; DefIdx++)
				if (EntityDefs[DefIdx].ClassName == ClassName) {
					Type EntType = EntityDefs[DefIdx].EntityType;

					if (KVs.Count == 0)
						return (Entity)Activator.CreateInstance(EntType);

					foreach (var C in EntType.GetConstructors()) {
						EntityKeyAttribute[] KeyAttribs = C.GetParameters().Select(P => P.GetCustomAttribute<EntityKeyAttribute>()).ToArray();

						if (KeyAttribs.Length == 0)
							continue;

						bool ValidConstructor = true;

						for (int i = 0; i < KeyAttribs.Length; i++)
							if (KeyAttribs[i] == null || !KVs.Contains(KeyAttribs[i].KeyName)) {
								ValidConstructor = false;
								break;
							}

						if (!ValidConstructor)
							continue;

						return (Entity)C.Invoke(KeyAttribs.Select(KA => KVs.Get(KA.KeyName).Value).ToArray());
					}

					ConstructorInfo CInf = null;
					if ((CInf = EntType.GetConstructor(new[] { typeof(EntityKeyValues) })) != null)
						return (Entity)CInf.Invoke(new object[] { KVs });

					throw new NotImplementedException();
				}

			return null;
		}
	}
}
