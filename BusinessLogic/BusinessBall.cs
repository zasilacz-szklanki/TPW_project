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

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball, List<IBall> otherBallsList, object sharedLock)
        {
            dataBall = ball;
            otherBalls = otherBallsList;
            locker = sharedLock;
            dataBall.NewPositionNotification += RaisePositionChangeEvent;
            collisionCts = new CancellationTokenSource();
            collisionTask = Task.Run(() => CollisionDetection(collisionCts.Token), collisionCts.Token);
        }

        #region IBall
        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => dataBall.Radius;
        public double Mass => dataBall.Mass;
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
            Data.IVector x1 = dataBall.Position;
            Data.IVector x2 = other.DataBall.Position;

            Data.Vector dx = x1.Sub(x2);
            Data.Vector dv = v1.Sub(v2);

            double dot = dx.DotProd(dv);

            double m1 = Mass;
            double m2 = other.Mass;
            double factor = 2 / (m1 + m2) * dot / dx.EuclideanNormSquared();

            dataBall.Velocity = v1.Sub(dx.Mul(factor*m2));
            other.DataBall.Velocity = v2.Add(dx.Mul(factor*m1));
            
            double distance = dx.EuclideanNorm();
            double overlap = (Radius + other.Radius) - distance;
            if (overlap > 0)
            {
                Data.Vector n=dx.Div(distance);
                double correctionFactor = overlap / (m1 + m2);
                dataBall.Position = x1.Add(n.Mul(correctionFactor*m2));
                other.DataBall.Position = x2.Sub(n.Mul(correctionFactor*m1));
            }
        }

        internal void CheckWallCollision()
        {
            double tableWidth = dataBall.TableWidth;
            double tableHeight = dataBall.TableHeight;

            double newX = dataBall.Position.x + dataBall.Velocity.x;
            double newY = dataBall.Position.y + dataBall.Velocity.y;
            double newVx = dataBall.Velocity.x;
            double newVy = dataBall.Velocity.y;

            double min = 0;
            double maxX = tableWidth - Radius * 2 - 4;
            double maxY = tableHeight - Radius * 2 - 4;

            if (newX <= min || newX >= maxX)
            {
                newVx = -newVx;
                newX = Math.Clamp(newX, min, maxX);
            }

            if (newY <= min || newY >= maxY)
            {
                newVy = -newVy;
                newY = Math.Clamp(newY, min, maxY);
            }

            dataBall.Position = new Data.Vector(newX, newY);
            dataBall.Velocity = new Data.Vector(newVx, newVy);
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
                            double dx = dataBall.Position.x - otherBall.DataBall.Position.x;
                            double dy = dataBall.Position.y - otherBall.DataBall.Position.y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            if (distance < Radius + otherBall.Radius)
                            {
                                CheckBallCollision(otherBall);
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