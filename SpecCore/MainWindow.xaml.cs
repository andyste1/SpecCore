namespace SpecCore
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            new HelloWorld(this);
            //new TestHarness2(this);

            InitializeComponent();
        }

        /// <summary>
        /// Keeps the window centred on its current location as it is resized using F6 & F7
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.HeightChanged)
            {
                Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;
            }
            if (sizeInfo.WidthChanged)
            {
                Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
            }
        }
    }
}
