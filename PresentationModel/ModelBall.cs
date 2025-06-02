//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class ModelBall : IBall
    {
        public ModelBall(double top, double left, LogicIBall underneathBall)
        {
            this.underneathBall = underneathBall;
            TopBackingField = top;
            LeftBackingField = left;
            Diameter = underneathBall.Radius * 2;
            underneathBall.NewPositionNotification += NewPositionNotification;
        }

        #region IBall Implementation

        public double Top
        {
            get => TopBackingField;
            private set
            {
                if (TopBackingField == value)
                {
                    return;
                }

                TopBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Left
        {
            get => LeftBackingField;
            private set
            {
                if (LeftBackingField == value)
                {
                    return;
                }

                LeftBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Diameter { get; init; }

        #endregion IBall Implementation

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion INotifyPropertyChanged Implementation

        #region Private Methods

        private void NewPositionNotification(object? sender, IPosition e)
        {
            Top = e.y;
            Left = e.x;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private Methods

        #region Private Fields

        private double TopBackingField;
        private double LeftBackingField;
        private readonly LogicIBall underneathBall;

        #endregion Private Fields

        #region public
        public void Dispose()
        {
            underneathBall.NewPositionNotification -= NewPositionNotification;
            underneathBall.Dispose();
        }
        #endregion public

        #region Testing Instrumentation

        [Conditional("DEBUG")]
        internal void SetLeft(double x)
        {
            Left = x;
        }

        [Conditional("DEBUG")]
        internal void SetTop(double x)
        {
            Top = x;
        }

        #endregion Testing Instrumentation
    }
}