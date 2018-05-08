using OpticalSimulator.Service;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpticalSimulator.View
{
    public partial class SkiaView : UserControl
    {
        public static readonly DependencyProperty ServiceProperty = DependencyProperty.Register("Service", typeof(SkiaService), typeof(SkiaView), new PropertyMetadata(OnServiceChanged));

        private static void OnServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SkiaView view = d as SkiaView;
            if (view != null)
            {
                SkiaService service = e.NewValue as SkiaService;
                view.image.Source = service.Bitmap;
                CompositionTarget.Rendering += (obj, args) => service.Update();
            }
        }

        public SkiaService Service
        {
            get { return (SkiaService)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }

        public SkiaView()
        {
            InitializeComponent();
        }
    }
}
