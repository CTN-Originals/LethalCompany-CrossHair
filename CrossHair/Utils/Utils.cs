using System.Reflection;

namespace CrossHair.Utilities
{
	public class PrivateFieldAccessor<InstanceType> {
		private FieldInfo _field;
		private object _instance;

		public PrivateFieldAccessor(object instance, string fieldname) {
			_field = typeof(InstanceType).GetField(
				fieldname,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
			);
			_instance = instance;
		}

		public T Get<T>() {
			return Utils.GetInstanceField<T>(_instance, _field.Name);
		}

		public void Set<T>(T value) {
			_field.SetValue(_instance, value);
		}
	}

	internal static class Utils
	{
		/// <summary>
		/// Uses reflection to get the field value from an object.
		/// </summary>
		///
		/// <param name="instance">The instance object.</param>
		/// <param name="fieldName">The field's name which is to be fetched.</param>
		///
		/// <returns>The field value from the object.</returns>
		public static T GetInstanceField<T>(object instance, string fieldName) {
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			FieldInfo field = instance.GetType().GetField(fieldName, bindFlags);
			return (T)field.GetValue(instance);
		}
	}
}