using System.Windows;
using System.Windows.Input;

namespace Zipper.Common
{
    public static class DropBehavior
    {
        private static readonly DependencyProperty PreviewDropCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "PreviewDropCommand",
                        typeof(ICommand),
                        typeof(DropBehavior),
                        new PropertyMetadata(PreviewDropCommandPropertyChangedCallBack)
                    );

        public static void SetPreviewDropCommand(this UIElement inUIElement, ICommand inCommand)
            => inUIElement.SetValue(PreviewDropCommandProperty, inCommand);

        private static ICommand GetPreviewDropCommand(UIElement inUIElement)
            => (ICommand)inUIElement.GetValue(PreviewDropCommandProperty);

        private static void PreviewDropCommandPropertyChangedCallBack(
            DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            var uiElement = inDependencyObject as UIElement;

            if (uiElement is null)
                return;

            uiElement.Drop += (sender, args) =>
            {
                GetPreviewDropCommand(uiElement).Execute(args.Data);
                args.Handled = true;
            };
        }
    }
}
