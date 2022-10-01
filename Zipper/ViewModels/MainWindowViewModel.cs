using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zipper.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
		private ViewModelBase _currentViewModel = new CompresserViewModel();

		public ViewModelBase CurrentViewModel
		{
			get => _currentViewModel;
			set => Set(ref _currentViewModel, value);
		}
	}
}
