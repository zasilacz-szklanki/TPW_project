//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void MoveTestMethod()
    {
      DataBallFixture dataBallFixture = new DataBallFixture();
      List<IBall> other = new List<IBall>();
      object locker = new object();
      Ball newInstance = new(dataBallFixture, other, locker);
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
      dataBallFixture.Move();
      Assert.AreEqual<int>(1, numberOfCallBackCalled);
    }

    #region testing instrumentation

    private class DataBallFixture : Data.IBall
    {
      private Data.IVector _velocity = new VectorFixture(1.0, 1.0);
      
      public Data.IVector Velocity 
      { 
        get => _velocity;
        set => _velocity = value;
      }
        public Data.IVector Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double Mass => throw new NotImplementedException();

        public double Radius => throw new NotImplementedException();

        public double TableWidth => throw new NotImplementedException();

        public double TableHeight => throw new NotImplementedException();

        public event EventHandler<Data.IVector>? NewPositionNotification;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void Move()
            {
            NewPositionNotification?.Invoke(this, new VectorFixture(10.0, 10.0));
            }
    }

    private class VectorFixture : Data.IVector
    {
      public VectorFixture(double X, double Y)
      {
        x = X; 
        y = Y;
      }

      public double x { get; init; }
      public double y { get; init; }
    }

    #endregion testing instrumentation
  }
}