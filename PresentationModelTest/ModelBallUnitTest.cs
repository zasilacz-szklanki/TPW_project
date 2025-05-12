//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
  [TestClass]
  public class ModelBallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      ModelBall ball = new ModelBall(0.0, 0.0, new BusinessLogicIBallFixture());
      Assert.AreEqual<double>(0.0, ball.Top);
      Assert.AreEqual<double>(0.0, ball.Left);
      Assert.AreEqual<double>(28.0, ball.Diameter);
    }

    [TestMethod]
    public void PositionChangeNotificationTestMethod()
    {
        int notificationCounter = 0;
        ModelBall ball = new ModelBall(0.0, 0.0, new BusinessLogicIBallFixture());
        ball.PropertyChanged += (sender, args) => notificationCounter++;

        Assert.AreEqual(0, notificationCounter);

        ball.SetLeft(1.0);
        Assert.AreEqual(1, notificationCounter);
        Assert.AreEqual(1.0, ball.Left);
        Assert.AreEqual(0.0, ball.Top);

        ball.SetTop(1.0);
        Assert.AreEqual(2, notificationCounter);
        Assert.AreEqual(1.0, ball.Left);
        Assert.AreEqual(1.0, ball.Top);
    }

    #region testing instrumentation

    private class BusinessLogicIBallFixture : BusinessLogic.IBall
    {
        public double Radius => 14.0;

        public double Mass => 1.0;

        public Data.IBall DataBall => throw new NotImplementedException();

        public event EventHandler<IPosition>? NewPositionNotification;

        public void Dispose()
        {
        }
    }

    #endregion testing instrumentation
  }
}