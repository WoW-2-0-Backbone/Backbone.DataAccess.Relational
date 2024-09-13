using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Backbone.DataAccess.Relational.EfCore.Abstractions.Extensions;

/// <summary>
/// Contains extensions for db context.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Extension method for applying entity configurations to the provided ModelBuilder instance. Assumes TDataContext is a DbContext.
    /// </summary>
    /// <typeparam name="TDataContext"></typeparam>
    /// <param name="modelBuilder"></param>
    /// <param name="assemblies">A collection of assemblies to search for entity configurations.</param>
    public static void ApplyEntityConfigurations<TDataContext>(this ModelBuilder modelBuilder, ICollection<Assembly>? assemblies = null)
        where TDataContext : DbContext
    {
        if (assemblies is null)
            assemblies = new List<Assembly> { typeof(TDataContext).Assembly };

        var entityConfigurationTypes = GetEntityConfigurationTypes(typeof(TDataContext), assemblies).ToList();
        entityConfigurationTypes.ForEach(type => modelBuilder.ApplyConfiguration((dynamic)Activator.CreateInstance(type)!));
    }

    /// <summary>
    /// Gets all entity types registered in a database context.
    /// </summary>
    /// <param name="dbContextType"></param>
    /// <returns>A collection of entity types.</returns>
    public static IEnumerable<Type> GetEntityTypes(Type dbContextType)
    {
        return dbContextType
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0]);
    }

    /// <summary>
    /// Gets entity configuration types related to the entity.
    /// </summary>
    /// <param name="dbContextType">The type of DbContext to analyze.</param>
    /// <param name="assemblies">A collection of assemblies to search for entity configurations.</param>
    /// <returns>
    /// A list of types representing entity configurations associated with the entities in the DbContext.
    /// </returns>
    public static IEnumerable<Type> GetEntityConfigurationTypes(Type dbContextType, ICollection<Assembly> assemblies)
    {
        var possibleEntityConfigurationTypes = GetEntityTypes(dbContextType)
            .Select(dbSetType => typeof(IEntityTypeConfiguration<>)
                .MakeGenericType(dbSetType))
            .ToList();

        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                           possibleEntityConfigurationTypes.Exists(configType => configType.IsAssignableFrom(type)));
    }

    /// <summary>
    /// Gets the entity configuration type for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="assemblies">A collection of assemblies to search for the entity configuration.</param>
    /// <returns>
    /// The type representing the entity configuration associated with the entity.
    /// </returns>
    public static Type? GetEntityConfigurationType(Type entityType, ICollection<Assembly> assemblies)
    {
        var entityConfigurationType = typeof(IEntityTypeConfiguration<>).MakeGenericType(entityType);

        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(type => type is { IsClass: true, IsAbstract: false } &&
                                    entityConfigurationType.IsAssignableFrom(type));
    }
}