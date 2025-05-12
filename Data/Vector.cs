//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  /// <summary>
  ///  Two dimensions immutable vector
  /// </summary>
  public record Vector : IVector
  {
    #region IVector

    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public double x { get; init; }
    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public double y { get; init; }

    #endregion IVector

    /// <summary>
    /// Creates new instance of <seealso cref="Vector"/> and initialize all properties
    /// </summary>
    public Vector(double XComponent, double YComponent)
    {
      x = XComponent;
      y = YComponent;
    }

    public Vector Add(IVector other){ 
      return new Vector(x+other.x,y+other.y);          
    }
    public Vector Sub(IVector other){ 
      return new Vector(x-other.x,y-other.y);          
    }
    public Vector Mul(double lambda){ 
      return new Vector(x*lambda,y*lambda);
    }
    public Vector Div(double lambda){ 
      return new Vector(x/lambda,y/lambda);
    }
    public double DotProd(IVector other){ 
      return x*other.x+y*other.y;
    }
    public double EuclideanNorm(){ 
      return Math.Sqrt(x*x+y*y);
    }
    public double EuclideanNormSquared(){ 
      return x*x+y*y;
    }
  }
}