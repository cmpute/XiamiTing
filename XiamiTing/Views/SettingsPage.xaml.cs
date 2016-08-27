using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

namespace JacobC.Xiami.Views
{
    public sealed partial class SettingsPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        public SettingsPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());//TODO: 把参数的获取封装起来
            MyPivot.SelectedIndex = index;
        }
    }
}
