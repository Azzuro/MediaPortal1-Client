#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;

namespace MediaPortal.UI.SkinEngine.DirectX.Triangulate
{
  /// <summary>
  /// Summary description for CPoint2D.
  /// </summary>

  //A point in Coordinate System
  public class CPoint2D
  {
    private float m_dCoordinate_X;
    private float m_dCoordinate_Y;

    public CPoint2D()
    {

    }

    public CPoint2D(float xCoordinate, float yCoordinate)
    {
      m_dCoordinate_X = xCoordinate;
      m_dCoordinate_Y = yCoordinate;
    }

    public float X
    {
      get { return m_dCoordinate_X; }
      set { m_dCoordinate_X = value; }
    }

    public float Y
    {
      get { return m_dCoordinate_Y; }
      set { m_dCoordinate_Y = value; }
    }

    public static bool SamePoints(CPoint2D Point1, CPoint2D Point2)
    {
      float dDiff_X =
        Math.Abs(Point1.X - Point2.X);
      float dDiff_Y =
        Math.Abs(Point1.Y - Point2.Y);

      return dDiff_X < ConstantValue.SmallValue && dDiff_Y < ConstantValue.SmallValue;
    }

    public bool SamePoint(CPoint2D other)
    {

      float dDeff_X = Math.Abs(m_dCoordinate_X - other.X);
      float dDeff_Y = Math.Abs(m_dCoordinate_Y - other.Y);

      return dDeff_X < ConstantValue.SmallValue && dDeff_Y < ConstantValue.SmallValue;
    }

    public bool Equals(CPoint2D other)
    {
      return m_dCoordinate_X == other.m_dCoordinate_X && m_dCoordinate_Y == other.m_dCoordinate_Y;
    }

    /***To check whether the point is in a line segment***/
    public bool InLine(CLineSegment lineSegment)
    {
      bool bInline = false;

      float Ax, Ay, Bx, By, Cx, Cy;
      Bx = lineSegment.EndPoint.X;
      By = lineSegment.EndPoint.Y;
      Ax = lineSegment.StartPoint.X;
      Ay = lineSegment.StartPoint.Y;
      Cx = this.m_dCoordinate_X;
      Cy = this.m_dCoordinate_Y;

      float L = lineSegment.GetLineSegmentLength();
      float s = Math.Abs(((Ay - Cy) * (Bx - Ax) - (Ax - Cx) * (By - Ay)) / (L * L));

      if (Math.Abs(s - 0) < ConstantValue.SmallValue)
      {
        if ((SamePoints(this, lineSegment.StartPoint)) || (SamePoints(this, lineSegment.EndPoint)))
          bInline = true;
        else if ((Cx < lineSegment.GetXmax()) && (Cx > lineSegment.GetXmin()) && (Cy < lineSegment.GetYmax()) && (Cy > lineSegment.GetYmin()))
          bInline = true;
      }
      return bInline;
    }

    /*** Distance between two points***/
    public float DistanceTo(CPoint2D point)
    {
      return (float)Math.Sqrt((point.X - this.X) * (point.X - this.X) + (point.Y - this.Y) * (point.Y - this.Y));

    }

    public bool PointInsidePolygon(CPoint2D[] polygonVertices)
    {
      if (polygonVertices.Length < 3) //not a valid polygon
        return false;

      int nCounter = 0;
      int nPoints = polygonVertices.Length;

      CPoint2D s1, p1, p2;
      s1 = this;
      p1 = polygonVertices[0];

      for (int i = 1; i < nPoints; i++)
      {
        p2 = polygonVertices[i % nPoints];
        if (s1.Y > Math.Min(p1.Y, p2.Y))
        {
          if (s1.Y <= Math.Max(p1.Y, p2.Y))
          {
            if (s1.X <= Math.Max(p1.X, p2.X))
            {
              if (p1.Y != p2.Y)
              {
                float xInters = (s1.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                if ((p1.X == p2.X) || (s1.X <= xInters))
                {
                  nCounter++;
                }
              }  //p1.y != p2.y
            }
          }
        }
        p1 = p2;
      } //for loop

      if ((nCounter % 2) == 0)
        return false;
      else
        return true;
    }

    /*********** Sort points from Xmin->Xmax ******/
    public static void SortPointsByX(CPoint2D[] points)
    {
      if (points.Length > 1)
      {
        for (int i = 0; i < points.Length - 2; i++)
        {
          for (int j = i + 1; j < points.Length - 1; j++)
          {
            if (points[i].X > points[j].X)
            {
              CPoint2D tempPt = points[j];
              points[j] = points[i];
              points[i] = tempPt;
            }
          }
        }
      }
    }

    /*********** Sort points from Ymin->Ymax ******/
    public static void SortPointsByY(CPoint2D[] points)
    {
      if (points.Length > 1)
      {
        for (int i = 0; i < points.Length - 2; i++)
        {
          for (int j = i + 1; j < points.Length - 1; j++)
          {
            if (points[i].Y > points[j].Y)
            {
              CPoint2D tempPt = points[j];
              points[j] = points[i];
              points[i] = tempPt;
            }
          }
        }
      }
    }

    public static bool operator ==(CPoint2D p1, CPoint2D p2)
    {
      bool p2null = ReferenceEquals(p2, null);
      if (ReferenceEquals(p1, null))
        return p2null;
      if (p2null)
        return false;
      return p1.Equals(p2);
    }

    public static bool operator !=(CPoint2D p1, CPoint2D p2)
    {
      return !(p1 == p2);
    }

    public override int GetHashCode()
    {
      return (int) (m_dCoordinate_X + m_dCoordinate_Y);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is CPoint2D))
        return false;
      CPoint2D other = (CPoint2D) obj;
      return Equals(other);
    }

    public override string ToString()
    {
      return string.Format("{0};{1}", m_dCoordinate_X, m_dCoordinate_Y);
    }
  }
}
