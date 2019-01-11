using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public class RunningAverage {
		float[] Values;
		int Index;
		float AverageValue;
		bool Dirty;

		public RunningAverage(int ElementCount) {
			Index = 0;
			Values = new float[0];
			Resize(ElementCount);
		}

		public void Resize(int ElementCount) {
			int OldLen = Values.Length;

			if (ElementCount == OldLen)
				return;

			if (Index >= ElementCount)
				Index = ElementCount - 1;

			Array.Resize(ref Values, ElementCount);
			Dirty = true;
		}

		public void Push(float Value) {
			Values[Index++] = Value;

			if (Index >= Values.Length)
				Index = 0;

			Dirty = true;
		}

		public float Average() {
			if (Dirty) {
				Dirty = false;
				float Sum = 0;

				for (int i = 0; i < Values.Length; i++)
					Sum += Values[i];

				AverageValue = Sum / Values.Length;
			}

			return AverageValue;
		}
	}
}
