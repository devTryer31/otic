namespace Zipper.ViewModels.Algos
{
    public class ShenonFanoAlgoViewModel : ViewModelBase
    {
        private bool _applyAlgoToEachFile;

        public bool ApplyAlgoToEachFile
        {
            get => _applyAlgoToEachFile;
            set => Set(ref _applyAlgoToEachFile, value);
        }
    }
}
