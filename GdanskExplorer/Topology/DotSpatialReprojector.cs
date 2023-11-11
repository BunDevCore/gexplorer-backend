using DotSpatial.Projections;
using static DotSpatial.Projections.Reproject;

namespace GdanskExplorer.Topology;

public sealed class DotSpatialReprojector : NetTopologySuite.Geometries.ICoordinateSequenceFilter
{
    private readonly ProjectionInfo _src;
    private readonly ProjectionInfo _dest;
    public int SrcSrid => _src.AuthorityCode;
    public int DestSrid => _dest.AuthorityCode;
    
    public double[] XY = new double[2];
    public double[] Z = new double[1];


    public DotSpatialReprojector(ProjectionInfo src, ProjectionInfo dest)
    {
        _src = src;
        _dest = dest;
    }
    
    public DotSpatialReprojector Reversed()
    {
        return new DotSpatialReprojector(_dest, _src);
    }

    public bool Done => false;
    public bool GeometryChanged => true;
    public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i)
    {
        
        XY[0] = seq.GetX(i);
        XY[1] = seq.GetY(i);
        Z[0] = seq.GetZ(i);
        ReprojectPoints(XY, Z, _src, _dest, 0, 1);
        seq.SetX(i, XY[1]);
        seq.SetY(i, XY[0]);
        seq.SetZ(i, Z[0]);
    }
}