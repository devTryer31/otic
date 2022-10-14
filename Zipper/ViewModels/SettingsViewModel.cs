using Zipper.Common.Enums;
using Zipper.ViewModels.Algos;

namespace Zipper.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Edits

        private static readonly ViewModelBase _nonSettedAlgoViewModel = new NonAlgoViewModel();
        private static readonly ViewModelBase _rleAlgoViewModel = new RLEAlgoSettingsViewModel();

        private readonly CompresserViewModel _compresserViewModel;
        public SettingsViewModel(CompresserViewModel compresserViewModel)
        {
            _compresserViewModel = compresserViewModel;
        }

        private ViewModelBase _algoWitoutContextViewModel = _nonSettedAlgoViewModel;

        public ViewModelBase AlgoWitoutContextViewModel
        {
            get => _algoWitoutContextViewModel;
            set => Set(ref _algoWitoutContextViewModel, value);
        }

        private bool _isEditAlgoWitoutContext = false;

        public bool IsEditAlgoWitoutContext
        {
            get => _isEditAlgoWitoutContext;
            set => Set(ref _isEditAlgoWitoutContext, value);
        }

        private ViewModelBase _algoContextedViewModel = _nonSettedAlgoViewModel;

        public ViewModelBase AlgoContextedViewModel
        {
            get => _algoContextedViewModel;
            set => Set(ref _algoContextedViewModel, value);
        }

        private bool _isEditAlgoContexted = false;

        public bool IsEditAlgoContexted
        {
            get => _isEditAlgoContexted;
            set => Set(ref _isEditAlgoContexted, value);
        }

        private ViewModelBase _algoInterferenceProtViewModel = _nonSettedAlgoViewModel;

        public ViewModelBase AlgoInterferenceProtViewModel
        {
            get => _algoInterferenceProtViewModel;
            set => Set(ref _algoInterferenceProtViewModel, value);
        }

        private bool _isEditAlgoInterferenceProt = false;

        public bool IsEditAlgoInterferenceProt
        {
            get => _isEditAlgoInterferenceProt;
            set => Set(ref _isEditAlgoInterferenceProt, value);
        }
        #endregion Edits


        #region AlgosLists
        private static readonly string _nonAlgorithmString = "Не выбран";

        private readonly string[] _encodingTypes = new[]
        {
            _nonAlgorithmString,
            "Шеннона-Фано"
        };

        public string[] EncodingTypes => _encodingTypes;

        private string _selectedEncodingType = _nonAlgorithmString;

        private static readonly ShenonFanoAlgoViewModel _shenonFanoAlgoViewModel = new();

        public string SelectedEncodingType
        {
            get => _selectedEncodingType;
            set
            {
                Set(ref _selectedEncodingType, value);
                if (_selectedEncodingType == _nonAlgorithmString)
                {
                    AlgoWitoutContextViewModel = _nonSettedAlgoViewModel;
                    _compresserViewModel.CurrentNoneContextedEncoding = EncodingAlgosTypes.None;
                }
                if(_selectedEncodingType == _encodingTypes[1])
                {
                    _compresserViewModel.CurrentNoneContextedEncoding = EncodingAlgosTypes.ShenonFano;
                    AlgoWitoutContextViewModel = _shenonFanoAlgoViewModel;
                }
            }
        }

        private readonly string[] _encodingTypesWIthContext = new[]
        {
            _nonAlgorithmString,
            "Алгоритм RLE",
        };

        public string[] EncodingTypesWithContext => _encodingTypesWIthContext;

        private string _selectedEncodingTypeWithContext = _nonAlgorithmString;

        public string SelectedEncodingTypeWithContext
        {
            get => _selectedEncodingTypeWithContext;
            set
            {
                Set(ref _selectedEncodingTypeWithContext, value);
                if (_selectedEncodingTypeWithContext == _nonAlgorithmString)
                {
                    _compresserViewModel.CurrentContextedEncoding = EncodingAlgosTypes.None;
                    AlgoContextedViewModel = _nonSettedAlgoViewModel;
                }
                if (_selectedEncodingTypeWithContext == _encodingTypesWIthContext[1])
                {
                    _compresserViewModel.CurrentContextedEncoding = EncodingAlgosTypes.RLE;
                    AlgoContextedViewModel = _rleAlgoViewModel;
                }
            }
        }

        private readonly string[] _interferenceProtectionTypes = new[]
        {
            _nonAlgorithmString,
        };

        public string[] InterferenceProtectionTypres => _interferenceProtectionTypes;

        private string _selectedinterferenceProtection = _nonAlgorithmString;

        public string SelectedinterferenceProtection
        {
            get => _selectedinterferenceProtection;
            set
            {
                Set(ref _selectedinterferenceProtection, value);
                if (_selectedinterferenceProtection == _nonAlgorithmString)
                    AlgoInterferenceProtViewModel = _nonSettedAlgoViewModel;
            }
        }
        #endregion AlgosLists
    }
}
