using SaveUp.Views;

namespace SaveUp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(EditEntryPage), typeof(EditEntryPage));
        }
    }
}
