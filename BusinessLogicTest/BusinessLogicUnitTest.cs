﻿//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BusinessLogicImplementationUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
      {
        bool newInstanceDisposed = true;
        newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
        Assert.IsFalse(newInstanceDisposed);
      }
    }

    [TestMethod]
    public void DisposeTestMethod()
    {
        DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
        BusinessLogicImplementation newInstance = new(dataLayerFixcure);
        Assert.IsFalse(dataLayerFixcure.Disposed);
        bool newInstanceDisposed = true;
        newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
        Assert.IsFalse(newInstanceDisposed);
        newInstance.Dispose();
        newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
        Assert.IsTrue(newInstanceDisposed);
        Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
        Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
        Assert.IsTrue(dataLayerFixcure.Disposed);
    }

    [TestMethod]
    public void StartTestMethod()
    {
        DataLayerStartFixcure dataLayerFixcure = new();
        using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
        {
            int called = 0;
            int numberOfBalls2Create = 10;
            newInstance.Start(
                numberOfBalls2Create,
                (startingPosition, ball) => { called++; Assert.IsNotNull(startingPosition); Assert.IsNotNull(ball); });
            Assert.AreEqual<int>(numberOfBalls2Create, called);
            Assert.IsTrue(dataLayerFixcure.StartCalled);
            Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
        }
    }

    #region testing instrumentation

    private class DataLayerConstructorFixcure : Data.DataAbstractAPI
    {
        public override void Dispose()
        { }

        public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
        {
            throw new NotImplementedException();
        }
    }

    private class DataLayerDisposeFixcure : Data.DataAbstractAPI
    {
        internal bool Disposed = false;

        public override void Dispose()
        {
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
        {
            throw new NotImplementedException();
        }
    }

    private class DataLayerStartFixcure : Data.DataAbstractAPI
    {
        internal bool StartCalled = false;
        internal int NumberOfBallseCreated = -1;

        public override void Dispose()
        { }

        public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
        {
            StartCalled = true;
            NumberOfBallseCreated = numberOfBalls;
            for (int i = 0; i < numberOfBalls; i++)
            {
                upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
            }
        }

        private record DataVectorFixture : Data.IVector
        {
            public double x { get; init; } = 0.0;
            public double y { get; init; } = 0.0;

                public Vector Add(IVector other)
                {
                    throw new NotImplementedException();
                }

                public Vector Div(double lambda)
                {
                    throw new NotImplementedException();
                }

                public double DotProd(IVector other)
                {
                    throw new NotImplementedException();
                }

                public double EuclideanNorm()
                {
                    throw new NotImplementedException();
                }

                public Vector Mul(double lambda)
                {
                    throw new NotImplementedException();
                }

                public Vector Sub(IVector other)
                {
                    throw new NotImplementedException();
                }
            }

        private class DataBallFixture : Data.IBall
        {
            public IVector Velocity { get => new DataVectorFixture(); set { } }
            public IVector Position { get => new DataVectorFixture(); set { } }
            public double Mass => 1.0;
            public double Radius => 14.0;
            public double TableWidth => 400.0;
            public double TableHeight => 420.0;

                public int Id => throw new NotImplementedException();

                public event EventHandler<IVector>? NewPositionNotification = null;

            public void Dispose()
            { }

                public void setVelocity(IVector velocity)
                {
                    throw new NotImplementedException();
                }

                public void StartMoving()
                {
                    throw new NotImplementedException();
                }
            }
    }
    #endregion testing instrumentation
  }
}