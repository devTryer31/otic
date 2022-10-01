﻿namespace Zipper.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly CompresserViewModel _compresserViewModel;
		private readonly SettingsViewModel _settingsViewModel;

		public MainWindowViewModel()
		{
            _compresserViewModel = new CompresserViewModel();
			_settingsViewModel = new SettingsViewModel();
			_currentViewModel = _compresserViewModel;
        }

		private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
		{
			get => _currentViewModel;
			set => Set(ref _currentViewModel, value);
		}

		private bool _isSettingsOn = false;

		public bool IsSettingsOn
		{
			get => _isSettingsOn;
			set
			{
				Set(ref _isSettingsOn, value);
				if(_isSettingsOn)
					CurrentViewModel = _settingsViewModel;
				else
					CurrentViewModel = _compresserViewModel;
			}
		}

	}
}
