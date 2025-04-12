//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
    /// <summary>
    /// View implementation
    /// </summary>
    public partial class MainWindow:Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if(DataContext is MainWindowViewModel viewModel)
                viewModel.Dispose();
            base.OnClosed(e);
        }
        private void StartButtonAction(object sender,RoutedEventArgs e)
        {
            if(int.TryParse(NumberOfBallsTextBox.Text,out int ballCount) && ballCount >= 1 && ballCount <= 10)
            {
                ErrorMessageTextBlock.Text = "";
                MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
                viewModel.Start(ballCount);
                StartButton.IsEnabled=false;
            }
            else
            {
                ErrorMessageTextBlock.Text = "Please enter number between 1 and 10";
            }
        }
    }
}