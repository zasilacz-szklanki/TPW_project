//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        internal Ball(Vector initialPosition, Vector initialVelocity, int id)
        {
            position = initialPosition;
            velocity = initialVelocity;
            cancellationTokenSource = new CancellationTokenSource();
            Id = id;
        }

        public int Id { get; }

        public event EventHandler<IVector> NewPositionNotification;

        #region IBall

        public IVector Velocity
        { 
            get
            {
                lock (velocityLock)
                {
                    return velocity;
                }
            }

            set
            {
                lock(velocityLock)
                {
                    velocity = value;
                }
            }
        }

        public IVector Position
        {
            get
            {
                lock(positionLock)
                {
                    return position;
                }
            }
        }

        #endregion IBall

        private IVector position;
        private IVector velocity;
        private Task? moveTask;
        private CancellationTokenSource? cancellationTokenSource;
        private bool disposed;
        private readonly object velocityLock = new object();
        private readonly object positionLock = new object();
        private readonly object locker = new object();
        private DateTime lastUpdateTime = DateTime.Now;


        internal void Move()
        {
            IVector currentVelocity;
            IVector newPosition;
            
            var currentTime = DateTime.Now;
            var deltaTime = (currentTime - lastUpdateTime).TotalSeconds;
            lastUpdateTime = currentTime;

            const double TIME_SCALE_FACTOR = 50.0;
            const double CRITICAL_TIME_THRESHOLD_MS = 100.0;
            var scaledDeltaTime = deltaTime * TIME_SCALE_FACTOR;

            if (deltaTime * 1000 > CRITICAL_TIME_THRESHOLD_MS)
            {
                DiagnosticLogger.Instance.LogBallState(this, "Time exceeded");
                return;
            }

            lock(locker)
            {
                currentVelocity = velocity;
                var scaledVelocity = currentVelocity.Mul(scaledDeltaTime);
                newPosition = position.Add(scaledVelocity);
                position = newPosition;
                NewPositionNotification?.Invoke(this, position);
                
                DiagnosticLogger.Instance.LogBallState(this);
            }
        }

        public void StartMoving()
        {
            if (moveTask == null)
            {
                moveTask = MoveContinuouslyAsync(cancellationTokenSource!.Token);
            }
        }

        private async Task MoveContinuouslyAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Move();
                IVector currentVelocity;
                lock (velocityLock)
                {
                    currentVelocity = velocity;
                }
                await Task.Delay((int)(30.0/(currentVelocity.EuclideanNorm()+0.01)), cancellationToken);
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                try
                {
                    if (moveTask != null)
                    {
                        moveTask.Wait(100);
                    }
                }
                catch (AggregateException) { }
                finally
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                    moveTask = null;
                }
            }
        }
    }
}