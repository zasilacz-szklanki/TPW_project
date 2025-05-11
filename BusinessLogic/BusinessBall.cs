//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public event EventHandler<IPosition>? NewPositionNotification;
        
        private readonly Data.IBall dataBall;
        private const double TableWidth = 400;
        private const double TableHeight = 420;
        private const double BallDiameter = 28;

        public Ball(Data.IBall ball)
        {
            dataBall = ball;
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector position)
        {
            var result = CalculateMovement(
                position: position,
                velocity: dataBall.Velocity
            );
            
            dataBall.Velocity = result.newVelocity;
            NewPositionNotification?.Invoke(this, new Position(result.newPosition.x, result.newPosition.y));
        }

        private (Data.IVector newPosition, Data.IVector newVelocity) CalculateMovement(Data.IVector position, Data.IVector velocity)
        {
            double newX = position.x + velocity.x;
            double newY = position.y + velocity.y;
            double newVx = velocity.x;
            double newVy = velocity.y;

            if (newX <= 4 || newX >= TableWidth - BallDiameter)
            {
                newVx = -newVx;
                newX = Math.Clamp(newX, 4, TableWidth - BallDiameter);
            }

            if (newY <= 4 || newY >= TableHeight - BallDiameter)
            {
                newVy = -newVy;
                newY = Math.Clamp(newY, 4, TableHeight - BallDiameter);
            }

            return (
                newPosition: new Data.Vector(newX, newY),
                newVelocity: new Data.Vector(newVx, newVy)
            );
        }
    }
}