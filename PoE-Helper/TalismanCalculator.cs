using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE_Helper {
	class TalismanCalculator {
		/*
		Highest x 3
		Medium x2
		Lowest x 1
		*/
		private readonly List<int> levels;

		public TalismanCalculator() : this(1, 1, 1, 1, 1) { }

		public TalismanCalculator( int t1, int t2, int t3, int t4, int t5 ) {
			this.levels = new List<int>(5) { t1, t2, t3, t4, t5 };
		}

		public void SetTalismanLevel( int value, int talismanNumber ) {
			if (talismanNumber < 1 || talismanNumber > 5) { throw new IndexOutOfRangeException(); }
			levels[talismanNumber - 1] = value;
		}

		private int[] Sorted {
			get {
				int[] sorted = this.levels.OrderBy(x => x).ToArray();
				return sorted;
			}
			set { throw new InvalidCastException(); }
		}

		public int HighestTalisman {
			get { return Sorted[4]; }
			private set { throw new InvalidCastException(); }
		}

		public int LowestTalisman {
			get { return Sorted[0]; }
			private set { throw new InvalidCastException(); }
		}

		public int[] MediumTalismans {
			get {
				return new int[] { Sorted[3], Sorted[2], Sorted[1] };
			}
			private set { throw new InvalidCastException(); }
		}

		public int CombinedTalisman {
			get {
				double result =
					(3 * HighestTalisman
					+ 2 * (MediumTalismans[0] + MediumTalismans[1] + MediumTalismans[2])
					+ LowestTalisman) / 10d;
				return (int) Math.Round(result);
			}
			private set { throw new InvalidCastException(); }
		}
	}
}
