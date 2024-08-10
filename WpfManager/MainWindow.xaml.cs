using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfManager
{
	partial class MainWindowModel : ObservableObject
	{
		public ObservableCollection<Mod> InstalledMods { get; set; } = new();
		public ObservableCollection<Mod> AvailableMods { get; set; } = new();
		[ObservableProperty]
		public Mod? selectedMod;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ModIOManager modIOManager;

		private MainWindowModel model;

		public MainWindow()
		{
			InitializeComponent();
			WriteLogMessage("Loading mod list...");
			modIOManager = new ModIOManager();
			model = new MainWindowModel();
			try
			{
				var mods = modIOManager.ReadMods();
				foreach ( var mod in mods )
				{
					if (mod.InstalledVersion == null) model.AvailableMods.Add(mod);
					else model.InstalledMods.Add(mod);
				}
				WriteLogMessage("Mods loaded");
				this.DataContext = model;
				model.SelectedMod = model.InstalledMods.FirstOrDefault();

				installedGrid.SelectionChanged += Grid_SelectionChanged;
				availableGrid.SelectionChanged += Grid_SelectionChanged;
			}
			catch (Exception ex)
			{
				WriteLogMessage(ex.Message);
			}
		}

		private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			model.SelectedMod = ((DataGrid)sender).SelectedValue as Mod;
		}

		private void WriteLogMessage(string text)
		{
			log.AppendText(text + Environment.NewLine);
			log.ScrollToEnd();
		}

		private void Uninstall_Click(object sender, RoutedEventArgs e)
		{
			if (installedGrid.SelectedValue is Mod mod)
			{
				WriteLogMessage($"Uninstalling {mod.Name}...");

				Task.Run(() =>
				{
					try
					{
						modIOManager.UninstallMod(mod);
						Dispatcher.Invoke(() =>
						{
							WriteLogMessage($"{mod.Name} succesfully uninstalled.");
							model.InstalledMods.Remove(mod);
							if (mod.AvailableVersion != null) model.AvailableMods.Add(mod);
						});
					}
					catch (Exception ex)
					{
						Dispatcher.Invoke(() => WriteLogMessage(ex.Message));
					}
				});
			}
		}

		private void Install_Click(object sender, RoutedEventArgs e)
		{
			if (availableGrid.SelectedValue is Mod mod)
			{
				WriteLogMessage($"Installing {mod.Name}...");

				Task.Run(() =>
				{
					try
					{
						modIOManager.InstallMod(mod);
						Dispatcher.Invoke(() =>
						{
							Dispatcher.Invoke(() => WriteLogMessage($"{mod.Name} succesfully installed."));
							model.InstalledMods.Add(mod);
							model.AvailableMods.Remove(mod);
						});
					}
					catch (Exception ex)
					{
						Dispatcher.Invoke(() => WriteLogMessage(ex.Message));
					}
				});
			}
		}

	}
}