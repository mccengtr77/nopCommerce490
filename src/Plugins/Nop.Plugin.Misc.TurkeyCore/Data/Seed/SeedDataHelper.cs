using System.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Seed;

/// <summary>
/// Migration'ların ortak ihtiyaç duyduğu helper'lar — parent lookup ve parametre ekleme.
/// </summary>
public static class SeedDataHelper
{
    /// <summary>
    /// IDbCommand'a tipli parametre ekler. ParameterName <c>@adı</c> formatındadır.
    /// </summary>
    public static void AddParam(IDbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }

    /// <summary>
    /// Province PlateCode → Id eşleme tablosunu yükler.
    /// </summary>
    public static Dictionary<int, int> LoadProvincePlateToIdMap(IDbConnection conn, IDbTransaction tx)
    {
        var map = new Dictionary<int, int>();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"SELECT Id, PlateCode FROM {nameof(TurkishProvince)}";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var plate = reader.GetInt32(1);
            map[plate] = id;
        }
        return map;
    }

    /// <summary>
    /// (ProvincePlate, DistrictName) → DistrictId eşlemesi. Mahalle seed'i için.
    /// </summary>
    public static Dictionary<(int Plate, string DistrictName), int> LoadDistrictLookup(IDbConnection conn, IDbTransaction tx)
    {
        var map = new Dictionary<(int, string), int>();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText =
            $"SELECT d.Id, p.PlateCode, d.Name " +
            $"FROM {nameof(TurkishDistrict)} d " +
            $"INNER JOIN {nameof(TurkishProvince)} p ON p.Id = d.ProvinceId";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var plate = reader.GetInt32(1);
            var name = reader.GetString(2);
            map[(plate, name)] = id;
        }
        return map;
    }
}
