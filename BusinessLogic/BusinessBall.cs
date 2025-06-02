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
        public Ball(Data.IBall ball, List<IBall> otherBallsList, object sharedLock)
        {
            dataBall = ball;
            otherBalls = otherBallsList;
            locker = sharedLock;
            currentPosition = new Data.Vector(0, 0);
            dataBall.NewPositionNotification += (s, pos) =>
            {
                lock (locker)
                {
                    currentPosition = pos;
                    RaisePositionChangeEvent(s, pos);
                }
            };
            collisionCts = new CancellationTokenSource();
            collisionTask = Task.Run(() => CollisionDetection(collisionCts.Token), collisionCts.Token);
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius { get; } = BusinessLogicAbstractAPI.GetDimensions.BallDimension / 2.0;
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
            Data.IVector v1, v2;
            Data.IVector x1, x2;
            lock (locker)
            {
                v1 = dataBall.Velocity;
                v2 = other.DataBall.Velocity;
                x1 = currentPosition;
                x2 = other.DataBall.Position;

                Data.Vector dx = x1.Sub(x2);
                Data.Vector dv = v1.Sub(v2);

                double dot = dx.DotProd(dv);
                double factor = dot / dx.EuclideanNormSquared();

                dataBall.Velocity = v1.Sub(dx.Mul(factor * 1));
                other.DataBall.Velocity = v2.Add(dx.Mul(factor * 1));
            }
        }

        internal void CheckWallCollision()
        {
            var dimensions = BusinessLogicAbstractAPI.GetDimensions;

            Vector position;
            Data.IVector velocity;
            lock (locker)
            {
                position = currentPosition.Add(dataBall.Velocity);
                velocity = dataBall.Velocity;

                IVector newV = velocity;
                double min = 0;
                double maxX = dimensions.TableWidth - Radius * 2 - 4;
                double maxY = dimensions.TableHeight - Radius * 2 - 4;

                if (position.x <= min || position.x >= maxX)
                {
                    newV = new Vector(-newV.x,newV.y);
                }

                if (position.y <= min || position.y >= maxY)
                {
                    newV = new Vector(newV.x,-newV.y);
                }

                if (newV != velocity)
                {
                    dataBall.Velocity = newV;
                }
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
        private Data.IVector currentPosition;

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

                            Vector delta;
                            delta = currentPosition.Sub(otherBall.DataBall.Position);
                            double distance = delta.EuclideanNorm();

                            double minDistance = Radius * 2;
                            if (distance < minDistance)
                            {
                                Vector relativeVelocity;
                                relativeVelocity = dataBall.Velocity.Sub(otherBall.DataBall.Velocity);
                                double approachSpeed = delta.DotProd(relativeVelocity) / distance;

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