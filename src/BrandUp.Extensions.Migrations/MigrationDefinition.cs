namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Default <see cref="IMigrationDefinition"/> built from a handler type and its migration attribute.
    /// </summary>
    public class MigrationDefinition : IMigrationDefinition, IEquatable<MigrationDefinition>
    {
        readonly static Type MigrationHandlerInterface = typeof(IMigrationHandler);
        readonly MigrationAttribute attribute;

        /// <inheritdoc />
        public string Name { get; }
        /// <inheritdoc />
        public string? Description => attribute.Description;
        /// <inheritdoc />
        public Type HandlerType { get; }
        /// <summary>
        /// The handler type this migration must be applied after, or <c>null</c> for a root migration.
        /// </summary>
        public Type? ParentHandlerType { get; }
        /// <summary>
        /// <c>true</c> when this migration has no predecessor (declared with <see cref="SetupAttribute"/>).
        /// </summary>
        public bool IsRoot => ParentHandlerType == null;

        /// <summary>
        /// Initializes a new migration definition.
        /// </summary>
        /// <param name="handlerType">The migration handler type. Must implement <see cref="IMigrationHandler"/>.</param>
        /// <param name="attribute">The migration attribute applied to <paramref name="handlerType"/>.</param>
        public MigrationDefinition(Type handlerType, MigrationAttribute attribute)
        {
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            this.attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

            if (attribute is UpgradeAttribute upgradeAttribute)
                ParentHandlerType = upgradeAttribute.AfterType;

            if (!MigrationHandlerInterface.IsAssignableFrom(handlerType))
                throw new ArgumentException($"Type {handlerType} does not implement interface {MigrationHandlerInterface}.", nameof(handlerType));

            Name = handlerType.FullName ?? handlerType.Name;
        }

        #region Object members

        /// <inheritdoc />
        public bool Equals(MigrationDefinition? other)
        {
            return other is not null && HandlerType == other.HandlerType;
        }
        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as MigrationDefinition);
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HandlerType.GetHashCode();
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return HandlerType.ToString();
        }

        #endregion
    }
}
