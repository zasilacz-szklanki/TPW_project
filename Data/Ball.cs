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
        public event EventHandler<IVector> NewPositionNotification;
        public IVector Velocity { get; set; }
        public double Radius { get; } = 10.0;

        private Vector Position;
        private Task? moveTask;
        private CancellationTokenSource? cancellationTokenSource;
        private bool disposed;

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            cancellationTokenSource = new CancellationTokenSource();
        }

        internal void Move()
        {
            Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
            NewPositionNotification?.Invoke(this, Position);
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
                await Task.Delay((int)(30.0/(Velocity.EuclideanNorm()+0.01)), cancellationToken);
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

        public void setVelocity(IVector velocity)
        { 
            Velocity=velocity;
        }
    }
}