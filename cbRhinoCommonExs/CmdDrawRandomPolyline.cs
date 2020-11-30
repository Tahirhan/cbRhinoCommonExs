using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input;

namespace cbRhinoCommonExs
{
    public class CmdDrawRandomPolyline : Command
    {
        static CmdDrawRandomPolyline _instance;
        public CmdDrawRandomPolyline()
        {
            _instance = this;
        }

        ///<summary>The only instance of the CmdDrawRandomPolyline command.</summary>
        public static CmdDrawRandomPolyline Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "cbDrawRandomPolyline"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            int stepSize = 100;
            RhinoGet.GetInteger("Step Size", true, ref stepSize);

            DrawRandomPolyline(stepSize, doc);
            return Result.Success;
        }

        private void DrawRandomPolyline(int stepSize, RhinoDoc doc)
        {
            Polyline pline = new Polyline();
            List<Point3d> linePts = new List<Point3d>();
            linePts.Add(Point3d.Origin); // starting point
            int tempDirPtIndex = -1;
            Guid guid = Guid.Empty;
            for (int step = 0; step < stepSize; step++)
            {
                Point3d movePt;
                bool isStuck = false;
                (movePt, tempDirPtIndex, isStuck) = GetNewMovePt(pline, linePts, doc, tempDirPtIndex);
                if(isStuck) { RhinoApp.WriteLine("Stuck at step : "+step.ToString()); break; }
                linePts.Add(movePt);
                pline = new Polyline(linePts);
                if (guid != Guid.Empty) doc.Objects.Delete(new ObjRef(guid), true, true);
                guid = doc.Objects.AddPolyline(pline);
                doc.Views.Redraw();
            }

        }

        private (Point3d, int, bool) GetNewMovePt(Polyline pline, List<Point3d> linePts, RhinoDoc doc, int tempDirPtIndex)
        {
            Point3d randomPt = new Point3d();
            int tryCounter = 0;
            bool isIntersecting = false, stuckPt = false;
            do
            {
                tryCounter++;
                if (tryCounter == 10000) { stuckPt = true; break; }
                isIntersecting = false;
                int recentDirIndex;
                (randomPt, recentDirIndex) = GetRandomPt(linePts.Last());
                Line newLinePart = new Line(linePts.Last(), randomPt);
                if (pline.Count == 0) continue; // first entrance
                CurveIntersections intersections = Intersection.CurveCurve(newLinePart.ToNurbsCurve(), pline.ToNurbsCurve(), doc.ModelAbsoluteTolerance, doc.ModelAbsoluteTolerance);
                if (intersections.Count != 1) isIntersecting = true;
                if (tempDirPtIndex == recentDirIndex) isIntersecting = true;
                if(linePts.Contains(randomPt)) isIntersecting = true;
                tempDirPtIndex = recentDirIndex;
            } while (isIntersecting);
            return (randomPt, tempDirPtIndex, stuckPt);
        }

        private (Point3d,int) GetRandomPt(Point3d point3d)
        {
            Point3d r1 = new Point3d(point3d.X + 50, point3d.Y, point3d.Z);
            Point3d r2 = new Point3d(point3d.X + 50, point3d.Y+50, point3d.Z);
            Point3d r3 = new Point3d(point3d.X, point3d.Y+50, point3d.Z);
            Point3d r4 = new Point3d(point3d.X - 50, point3d.Y+50, point3d.Z);
            Point3d r5 = new Point3d(point3d.X - 50, point3d.Y, point3d.Z);
            Point3d r6 = new Point3d(point3d.X - 50, point3d.Y-50, point3d.Z);
            Point3d r7 = new Point3d(point3d.X, point3d.Y-50, point3d.Z);
            Point3d r8 = new Point3d(point3d.X + 50, point3d.Y-50, point3d.Z);
            List<Point3d> ptList = new List<Point3d>() { r1, r2, r3, r4, r5, r6, r7, r8 };
            Random random = new Random();
            int randomIndex = random.Next(ptList.Count);
            return (ptList[randomIndex], randomIndex);
        }
    }
}