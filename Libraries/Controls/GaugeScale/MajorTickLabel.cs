using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CygnusControls
{
	public class MajorTickLabel
	{
		public MajorTickLabel()
		{
			FontColor = null;
			FontHeight = 0;
		}

		public string Value { set; get; }
		public string Label { set; get; }
		public Brush FontColor { get; set; }
		public double FontHeight { get; set; }
	}
}
