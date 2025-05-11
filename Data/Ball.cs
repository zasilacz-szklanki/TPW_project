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
    internal class Ball:IBall
    {
        #region ctor

        internal Ball(Vector initialPosition,Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            isRunning = true;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;
        private Thread? moveThread;
        private volatile bool isRunning;
        private bool disposed;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this,Position);
        }

        internal void Move()
        {
            Position = new Vector(Position.x + Velocity.x,Position.y + Velocity.y);
            RaiseNewPositionChangeNotification();
        }

        public void StartMoving(){ 
            moveThread=new Thread(new ThreadStart(MoveContinuously));
            moveThread.Start();
        }

        private void MoveContinuously(){
            while(isRunning)
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
            moveThread?.Join();
        }
        #endregion private
    }
}