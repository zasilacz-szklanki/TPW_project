//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            position = initialPosition;
            velocity = initialVelocity;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public event EventHandler<IVector> NewPositionNotification;

        #region IBall

        public IVector Velocity
        { 
            get
            {
                // to consider
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
                // to consider
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

        internal void Move()
        {
            IVector currentVelocity;
            lock (velocityLock)
            {
                currentVelocity = velocity;
            }
            IVector newPosition;
            lock(positionLock)
            {
                newPosition = position.Add(currentVelocity);
                position = newPosition;
                NewPositionNotification?.Invoke(this, position);
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