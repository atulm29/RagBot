using Dapper;
using Pgvector;
using Npgsql;
using System.Data;

public class PgVectorHandler : SqlMapper.TypeHandler<Vector>
{
    public override void SetValue(IDbDataParameter parameter, Vector value)
    {
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            // Simply set the value - let Npgsql's registered type mapping handle it
            npgsqlParameter.Value = value;
        }
        else
        {
            parameter.Value = value;
        }
    }

    public override Vector Parse(object value)
    {
        return value switch
        {
            Vector v => v,
            float[] f => new Vector(f),
            double[] d => new Vector(d.Select(x => (float)x).ToArray()),
            string s => new Vector(s),
            _ => throw new Exception($"Unsupported vector read type: {value.GetType()}")
        };
    }
}
