using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.XamlTypeInfo;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityToolkit.WinUI.UI.Controls;

namespace WinUI3ClassLibrary
{
    public partial class SampleWindow : Window
    {
        public SampleWindow()
        {
            InitializeComponent();
        }

        public int DialogResult { get; protected set; }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e) => Close();
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = 1;
            Close();
        }

        private static DummyApp? _app;

        // this is called by the Win32 app (see hosting.cpp)
        #pragma warning disable IDE0060 // Remove unused parameter
        public static int ShowWindow(nint args, int sizeBytes)
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            // Initialize WinAppSDK 1.6 or 1.7
            if (!Bootstrap.TryInitialize(0x00010007, string.Empty, new PackageVersion(), Bootstrap.InitializeOptions.None, out var hr) &&
                !Bootstrap.TryInitialize(0x00010006, string.Empty, new PackageVersion(), Bootstrap.InitializeOptions.OnNoMatch_ShowUI, out hr))
                return hr;

            if (_app == null)
            {
                _app = new DummyApp(); // Optional: enables WinUI 3 styles
                DispatcherQueueController.CreateOnCurrentThread();
            }

            var _source = new DesktopWindowXamlSource();
            _source.Initialize(Win32Interop.GetWindowIdFromWindow(args));

            var window = new Microsoft.UI.Xaml.Window();

            // Sample data class
            var sampleData = new List<Person>
            {
                new Person { Name = "Alice", Age = 30 },
                new Person { Name = "Bob", Age = 25 },
                new Person { Name = "Charlie", Age = 35 }
            };

            var dataGrid = new CommunityToolkit.WinUI.UI.Controls.DataGrid
            {
                AutoGenerateColumns = true,
                ItemsSource = sampleData,
                Margin = new Thickness(20)
            };

            // Create a layout container
            var grid = new Microsoft.UI.Xaml.Controls.Grid
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
            };
            grid.Children.Add(dataGrid);

            // Set the window content
            window.Content = grid;
            window.Activate(); // Show the window

            return 0;
        }

        // Sample data model
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        // this is needed for proper XAML support
        private sealed partial class DummyApp : Application, IXamlMetadataProvider
        {
            private readonly XamlControlsXamlMetaDataProvider provider = new();
            private readonly IXamlMetadataProvider _myLibProvider;

            public DummyApp()
            {
                // find the generated IXamlMetadataProvider for this lib
                var type = GetType().Assembly.GetTypes().First(t => typeof(IXamlMetadataProvider).IsAssignableFrom(t) && t.GetCustomAttribute<GeneratedCodeAttribute>() != null);
                _myLibProvider = (IXamlMetadataProvider)Activator.CreateInstance(type)!;
            }

            public IXamlType GetXamlType(Type type)
            {
                var ret = provider.GetXamlType(type);
                ret ??= _myLibProvider.GetXamlType(type);
                return ret;
            }

            public IXamlType GetXamlType(string fullName)
            {
                var ret = provider.GetXamlType(fullName);
                ret ??= _myLibProvider.GetXamlType(fullName);
                return ret;
            }

            public XmlnsDefinition[] GetXmlnsDefinitions()
            {
                var ret = provider.GetXmlnsDefinitions();
                ret ??= _myLibProvider.GetXmlnsDefinitions();
                return ret;
            }

            protected override void OnLaunched(LaunchActivatedEventArgs args)
            {
                Resources.MergedDictionaries.Add(new XamlControlsResources());
                base.OnLaunched(args);
            }
        }
    }
}
