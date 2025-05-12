//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

// Data/Ball.cs
namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        public event EventHandler<IVector> NewPositionNotification;
        public IVector Velocity { get; set; }
        public IVector Position { get; set; }
        public double Mass { get; }
        public double Radius { get; } = 10.0;
        public double TableWidth { get; } = 400.0;
        public double TableHeight { get; } = 420.0;

        private Thread? moveThread;
        private volatile bool isRunning;
        private bool disposed;

        internal Ball(IVector initialPosition, IVector initialVelocity, double mass)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Mass = mass;
            isRunning = true;
        }

        internal void Move()
        {
            Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
            NewPositionNotification?.Invoke(this, Position);
        }

        public void StartMoving()
        {
            if (moveThread == null)
            {
                moveThread = new Thread(new ThreadStart(MoveContinuously))
                {
                    IsBackground = true
                };
                moveThread.Start();
            }
        }

        private void MoveContinuously()
        {
            while (isRunning)
            {
                Move();
                Thread.Sleep(30);
            }          
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            isRunning = false;

            if (moveThread != null && moveThread.IsAlive)
            {
                moveThread?.Join();
            }
            moveThread = null;
        }
    }
}