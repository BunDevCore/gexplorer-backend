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
    public bool SwizzleOutput = false;
    public bool SwizzleInput = false;


    public DotSpatialReprojector(ProjectionInfo src, ProjectionInfo dest, bool swizzleInput = false, bool swizzleOutput = false)
    {
        _src = src;
        _dest = dest;
        SwizzleInput = swizzleInput;
        SwizzleOutput = swizzleOutput;
    }
    
    public DotSpatialReprojector Reversed()
    {
        return new DotSpatialReprojector(_dest, _src);
    }

    public DotSpatialReprojector OutputSwizzled()
    {
        return new DotSpatialReprojector(_src, _dest, SwizzleInput, !SwizzleOutput);
    }
    
    public DotSpatialReprojector InputSwizzled()
    {
        return new DotSpatialReprojector(_src, _dest, !SwizzleInput, SwizzleOutput);
    }

    public bool Done => false;
    public bool GeometryChanged => true;
    public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i)
    {
        
        XY[0 + (SwizzleInput ? 1 : 0)] = seq.GetX(i);
        XY[1 - (SwizzleInput ? 1 : 0)] = seq.GetY(i);
        Z[0] = seq.GetZ(i);
        ReprojectPoints(XY, Z, _src, _dest, 0, 1);
        seq.SetX(i, XY[0 + (SwizzleOutput ? 1 : 0)]);
        seq.SetY(i, XY[1 - (SwizzleOutput ? 1 : 0)]);
        seq.SetZ(i, Z[0]);
    }
}