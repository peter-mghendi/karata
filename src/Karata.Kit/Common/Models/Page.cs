namespace Karata.Kit.Common.Models;

public record Page<T>(int CurrentPage, int Length, int TotalItems, List<T> Items);