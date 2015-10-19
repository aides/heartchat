using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.Devices.Notification;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Heartchat.Client;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Heartchat.Mobile
{
    using System.Threading;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Foundation.Point anchorPoint;
        private List<Windows.Foundation.Point> points = new List<Windows.Foundation.Point>();
        private HeartchatClient client;

        private DateTime vibeTouchTime;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            client = new HeartchatClient("10.0.9.153", 5555);

            client.OnDraw += HeartchatClient_Draw;
            client.OnClear += HeartchatClient_Clear;
            client.OnVibrate += HeartchatClient_Vibrate;

            this.VibeButton.AddHandler(
                PointerPressedEvent,
                new PointerEventHandler(this.VibeButton_OnPointerPressed),
                true);

            this.VibeButton.AddHandler(
                PointerReleasedEvent,
                new PointerEventHandler(this.VibeButton_OnPointerReleased),
                true);
        }

        private void HeartchatClient_Vibrate(object sender, VibrateEventHandlerArgs e)
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                VibrationDevice testVibrationDevice = VibrationDevice.GetDefault();
                testVibrationDevice.Vibrate(TimeSpan.FromSeconds(e.Duration));
            });           
        }

        private void HeartchatClient_Clear(object sender, EventArgs e)
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.DrawCanvas.Children.Clear();
            });
        }

        private void HeartchatClient_Draw(object sender, DrawEventHandlerArgs e)
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DoDraw(e.Points);
                });
        }

        private void DoDraw(Windows.Foundation.Point[] points)
        {
            for (int i = 1; i < points.Length; i++)
            {
                Windows.UI.Xaml.Shapes.Line line = new Windows.UI.Xaml.Shapes.Line();
                line.Stroke = new SolidColorBrush(Colors.Red);
                line.StrokeThickness = 5;
                line.X1 = points[i - 1].X * DrawCanvas.ActualWidth;
                line.Y1 = points[i - 1].Y * DrawCanvas.ActualHeight;
                line.X2 = points[i].X * DrawCanvas.ActualWidth;
                line.Y2 = points[i].Y * DrawCanvas.ActualHeight;
                DrawCanvas.Children.Add(line);   
            }   
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await client.Connect();

            ConnectButton.Background = new SolidColorBrush(Colors.DarkGreen);

            StatusTextBlock.Text += "\nConnected with guid " + client.GetId();
        }

        private void DrawCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeThickness = 5;
            line.X1 = this.anchorPoint.X;
            line.Y1 = this.anchorPoint.Y;
            line.X2 = e.Position.X;
            line.Y2 = e.Position.Y;

            if (!e.IsInertial)
            {
                DrawCanvas.Children.Add(line);

                this.anchorPoint = e.Position;
                this.points.Add(
                    new Windows.Foundation.Point(
                        e.Position.X / DrawCanvas.ActualWidth,
                        e.Position.Y / DrawCanvas.ActualHeight));
                
                if (points.Count > 25)
                {
                    var newPoints = this.points.ToList();
                    client.SendDraw(newPoints);
                    this.points = new List<Windows.Foundation.Point>();

                    // We need to add current point back to avoid gaps
                    this.points.Add(
                        new Windows.Foundation.Point(
                            e.Position.X / DrawCanvas.ActualWidth,
                            e.Position.Y / DrawCanvas.ActualHeight));
                }
            }
        }

        private void DrawCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this.points = new List<Windows.Foundation.Point>();
            this.anchorPoint = e.Position;
            this.points.Add(
                new Windows.Foundation.Point(
                    e.Position.X / DrawCanvas.ActualWidth,
                    e.Position.Y / DrawCanvas.ActualHeight));
        }

        private void DrawCanvas_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
        }

        private async void DrawCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var newPoints = this.points.ToList();
            client.SendDraw(newPoints);
            this.points = new List<Windows.Foundation.Point>();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Children.Clear();
            this.client.Clear();
        }

        private void VibeButton_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.vibeTouchTime = DateTime.Now;
        }

        private void VibeButton_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            double duration = (DateTime.Now - this.vibeTouchTime).TotalSeconds;
            this.client.Vibrate(duration);
        }
    }
}
