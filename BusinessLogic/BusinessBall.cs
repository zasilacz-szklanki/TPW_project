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
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private Data.IVector currentPosition;

        public Ball(Data.IBall ball, List<IBall> otherBallsList, object sharedLock)
        {
            dataBall = ball;
            otherBalls = otherBallsList;
            locker = sharedLock;
            currentPosition = new Data.Vector(0, 0);
            dataBall.NewPositionNotification += (s, pos) => { currentPosition = pos; RaisePositionChangeEvent(s, pos); };
            collisionCts = new CancellationTokenSource();
            collisionTask = Task.Run(() => CollisionDetection(collisionCts.Token), collisionCts.Token);
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => dataBall.Radius;
        public Data.IBall DataBall => dataBall;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                collisionCts.Cancel();
                collisionTask.Wait(100);
            }
            catch (AggregateException) { }
            finally
            {
                collisionCts.Dispose();
                dataBall.NewPositionNotification -= RaisePositionChangeEvent;
                dataBall.Dispose();
            }
        }
        #endregion

        #region internal
        internal void CheckBallCollision(IBall other)
        {
            Data.IVector v1 = dataBall.Velocity;
            Data.IVector v2 = other.DataBall.Velocity;
            Data.IVector x1 = currentPosition;
            Data.IVector x2 = ((Ball)other).currentPosition;

            Data.Vector dx = x1.Sub(x2);
            Data.Vector dv = v1.Sub(v2);

            double dot = dx.DotProd(dv);

            double factor = 2 / (1 + 1) * dot / dx.EuclideanNormSquared();

            dataBall.setVelocity(v1.Sub(dx.Mul(factor * 1)));
            other.DataBall.setVelocity(v2.Add(dx.Mul(factor * 1)));
        }

        internal void CheckWallCollision()
        {
            var dimensions = BusinessLogicAbstractAPI.GetDimensions;
            double tableWidth = dimensions.TableWidth;
            double tableHeight = dimensions.TableHeight;

            double newX = currentPosition.x + dataBall.Velocity.x;
            double newY = currentPosition.y + dataBall.Velocity.y;
            IVector newV = dataBall.Velocity;

            double min = 0;
            double maxX = tableWidth - Radius * 2 - 4;
            double maxY = tableHeight - Radius * 2 - 4;

            if (newX <= min || newX >= maxX)
            {
                newV = new Vector(-newV.x,newV.y);
            }

            if (newY <= min || newY >= maxY)
            {
                newV = new Vector(newV.x,-newV.y);
            }

            if (newV != dataBall.Velocity)
            {
                dataBall.setVelocity(newV);
            }
        }
        #endregion

        #region private
        private readonly Data.IBall dataBall;
        private readonly List<IBall> otherBalls;
        private readonly object locker;
        private readonly Task collisionTask;
        private readonly CancellationTokenSource collisionCts;
        private bool _disposed = false;

        private async Task CollisionDetection(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    CheckWallCollision();
                    lock (locker)
                    {
                        foreach (var otherBall in otherBalls)
                        {
                            if (otherBall == this) continue;
                            
                            double dx = currentPosition.x - ((Ball)otherBall).currentPosition.x;
                            double dy = currentPosition.y - ((Ball)otherBall).currentPosition.y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            
                            double minDistance = Radius + otherBall.Radius;
                            if (distance < minDistance)
                            {
                                double relativeVelocityX = dataBall.Velocity.x - otherBall.DataBall.Velocity.x;
                                double relativeVelocityY = dataBall.Velocity.y - otherBall.DataBall.Velocity.y;
                                double approachSpeed = (dx * relativeVelocityX + dy * relativeVelocityY) / distance;
                                
                                if (approachSpeed < 0)
                                {
                                    CheckBallCollision(otherBall);
                                }
                            }
                        }
                    }
                    await Task.Delay(10, cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }
        #endregion
    }
}