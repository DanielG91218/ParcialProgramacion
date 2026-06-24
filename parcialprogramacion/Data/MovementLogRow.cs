namespace DronParcial.Data;

public sealed record MovementLogRow(int Id, int SavedStep, int RealStep, int X, int Y);
