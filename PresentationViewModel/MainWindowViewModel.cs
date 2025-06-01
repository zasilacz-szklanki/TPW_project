//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel:ViewModelBase, IDisposable
    {
        #region ctor

        public MainWindowViewModel() : this(null)
        { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));

            StartCommand = new RelayCommand(StartBalls,() => !IsSimulationRunning);
        }
        #endregion

        #region Commands
        public RelayCommand StartCommand { get; }

        private void StartBalls()
        {
            if(!int.TryParse(NumberOfBallsText,out int ballCount) || ballCount < 1 || ballCount > 100)
            {
                ErrorMessage = "Please enter number between 1 and 100";
                return;
            }

            ErrorMessage = "";
            IsSimulationRunning = true;
            StartCommand.RaiseCanExecuteChanged();
            Start(ballCount);
        }
        #endregion

        #region Properties
        private string _numberOfBallsText = "";
        public string NumberOfBallsText
        {
            get => _numberOfBallsText;
            set
            {
                _numberOfBallsText = value;
                RaisePropertyChanged();
            }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSimulationRunning = false;
        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            private set
            {
                _isSimulationRunning = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region public API

        public void Start(int numberOfBalls)
        {
            if(Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls);
            Observer.Dispose();
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if(!Disposed)
            {
                if(disposing)
                {
                    Balls.Clear();
                    Observer.Dispose();
                    ModelLayer.Dispose();
                }
                Disposed = true;
            }
        }

        public void Dispose()
        {
            if(Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region Scaling
        private static double _tableSize = 0.7;
        private double _scale = _tableSize;
        private double _windowWidth = 800;
        private double _windowHeight = 840;

        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                RaisePropertyChanged();
            }
        }

        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                RaisePropertyChanged();
                UpdateScale();
            }
        }

        public double WindowHeight
        {
            get => _windowHeight;
            set
            {
                _windowHeight = value;
                RaisePropertyChanged();
                UpdateScale();
            }
        }

        private void UpdateScale()
        {
            double baseWidth = 420;
            double baseHeight = 400;
            double scaleX = WindowWidth / baseWidth;
            double scaleY = WindowHeight / baseHeight;
            Scale = _tableSize * Math.Min(scaleX, scaleY);
        }
        #endregion Scaling

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        #endregion private
    }
}