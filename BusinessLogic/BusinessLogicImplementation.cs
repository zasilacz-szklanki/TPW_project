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
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<IBall> businessBalls = new();
        private readonly object sharedLock = new();
        private bool Disposed = false;

        public BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer = null)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentException(nameof(upperLayerHandler));

            layerBellow.Start(numberOfBalls, (position, dataBall) =>
            {
                var businessBall = new Ball(dataBall, businessBalls, sharedLock);
                lock (sharedLock)
                {
                    businessBalls.Add(businessBall);
                }
                upperLayerHandler(new Position(position.x, position.y), businessBall);
            });
        }

        public override void Dispose()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            }

            lock (sharedLock)
            {
                foreach (var ball in businessBalls.ToList())
                {
                    ball.Dispose();
                }
                businessBalls.Clear();
            }
            layerBellow.Dispose();
            Disposed = true;
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
    }
}