using Dapper;
using Npgsql;
using System.Data;

public class FloatArrayToVectorHandler : SqlMapper.TypeHandler<float[]>
{
    public override void SetValue(IDbDataParameter parameter, float[] value)
    {
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.Value = value;
        }
        else
        {
            parameter.Value = value;
        }
    }

    public override float[] Parse(object value)
    {
        return value switch
        {
            float[] f => f,
            double[] d => d.Select(x => (float)x).ToArray(),
            Pgvector.Vector v => v.ToArray(),
            _ => throw new Exception($"Unsupported type: {value.GetType()}")
        };
    }
}
