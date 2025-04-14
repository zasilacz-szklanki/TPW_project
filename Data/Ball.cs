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
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this,Position);
        }

        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x,Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

        internal void MoveInBox()
        {
            const int Width = 400;
            const int Height = 420;
            const int Diameter = 28;

            if(Position.x <= 4 || Position.x >= Width - Diameter)
            {
                Velocity = new Vector(-Velocity.x,Velocity.y);
            }

            if(Position.y <= 4 || Position.y >= Height - Diameter)
            {
                Velocity = new Vector(Velocity.x,-Velocity.y);
            }

            Position = new Vector(Position.x + Velocity.x,Position.y + Velocity.y);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}