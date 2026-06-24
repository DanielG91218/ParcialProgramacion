using DronParcial.Domain;
using Npgsql;
using NpgsqlTypes;

namespace DronParcial.Data;

public sealed class DroneRepository
{
    private readonly string _connectionString;

    public DroneRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int SaveSuccessfulExecution(int size, Coordinate start, IReadOnlyList<Coordinate> route)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            int masterId = InsertMaster(connection, transaction, size, start);
            InsertDetails(connection, transaction, masterId, route);

            transaction.Commit();
            return masterId;
        }
        catch
        {
            TryRollback(transaction);
            throw;
        }
    }

    public IReadOnlyList<MovementLogRow> GetLastFiveMovements(int masterId)
    {
        var rows = new List<MovementLogRow>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            SELECT id, paso_ofuscado, posicion_x, posicion_y
            FROM tb_det_log
            WHERE master_control_id = @master_control_id
            ORDER BY id DESC
            LIMIT 5;
            """;

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("@master_control_id", NpgsqlDbType.Integer).Value = masterId;

        using NpgsqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            int savedStep = reader.GetInt32(1);
            int realStep = DecodeStep(savedStep);
            int x = reader.GetInt32(2);
            int y = reader.GetInt32(3);

            rows.Add(new MovementLogRow(id, savedStep, realStep, x, y));
        }

        return rows;
    }

    private static int InsertMaster(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int size,
        Coordinate start)
    {
        const string sql = """
            INSERT INTO tb_master_control (fecha_sistema, terreno_n, despegue_x, despegue_y)
            VALUES (CURRENT_TIMESTAMP, @terreno_n, @despegue_x, @despegue_y)
            RETURNING id;
            """;

        using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.Add("@terreno_n", NpgsqlDbType.Integer).Value = size;
        command.Parameters.Add("@despegue_x", NpgsqlDbType.Integer).Value = start.X;
        command.Parameters.Add("@despegue_y", NpgsqlDbType.Integer).Value = start.Y;

        object? result = command.ExecuteScalar();
        if (result is null)
        {
            throw new InvalidOperationException("PostgreSQL no devolvio el id generado para tb_master_control.");
        }

        return Convert.ToInt32(result);
    }

    private static void InsertDetails(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int masterId,
        IReadOnlyList<Coordinate> route)
    {
        const string sql = """
            INSERT INTO tb_det_log (master_control_id, paso_ofuscado, posicion_x, posicion_y)
            VALUES (@master_control_id, @paso_ofuscado, @posicion_x, @posicion_y);
            """;

        using var command = new NpgsqlCommand(sql, connection, transaction);
        NpgsqlParameter masterIdParameter = command.Parameters.Add("@master_control_id", NpgsqlDbType.Integer);
        NpgsqlParameter stepParameter = command.Parameters.Add("@paso_ofuscado", NpgsqlDbType.Integer);
        NpgsqlParameter xParameter = command.Parameters.Add("@posicion_x", NpgsqlDbType.Integer);
        NpgsqlParameter yParameter = command.Parameters.Add("@posicion_y", NpgsqlDbType.Integer);

        masterIdParameter.Value = masterId;

        int index = 0;
        while (index < route.Count)
        {
            Coordinate coordinate = route[index];
            stepParameter.Value = EncodeStep(index);
            xParameter.Value = coordinate.X;
            yParameter.Value = coordinate.Y;

            command.ExecuteNonQuery();
            index++;
        }
    }

    private static int EncodeStep(int realStep)
    {
        if (realStep % 2 == 0)
        {
            return realStep * 2;
        }

        return realStep * -1;
    }

    private static int DecodeStep(int savedStep)
    {
        if (savedStep < 0)
        {
            return savedStep * -1;
        }

        return savedStep / 2;
    }

    private static void TryRollback(NpgsqlTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
            // If the connection is already broken, the original exception is more useful.
        }
    }
}
