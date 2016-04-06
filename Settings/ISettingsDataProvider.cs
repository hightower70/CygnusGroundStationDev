using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusGroundStation
{
	public interface ISettingsDataProvider
	{
		void LoadSettings(SettingsFileBase in_settings);
		void SaveSettings(SettingsFileBase in_settings);
	}
}
