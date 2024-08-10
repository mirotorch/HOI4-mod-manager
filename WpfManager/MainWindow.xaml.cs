using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ModIOManager modIOManager;

		private List<Mod> installedMods = new();
		private List<Mod> availableMods = new();

		public MainWindow()
		{
			InitializeComponent();
			modIOManager = new ModIOManager();
			try
			{
				var mods = modIOManager.ReadMods();
				foreach ( var mod in mods )
				{
					if (mod.InstalledVersion == null) availableMods.Add(mod);
					else installedMods.Add(mod);
				}
			}
			catch (Exception ex)
			{
				WriteLogMessage(ex.Message);
			}
		}

		private void WriteLogMessage(string text)
		{
			log.AppendText(text);
		}
	}
}