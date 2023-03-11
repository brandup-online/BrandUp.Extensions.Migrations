namespace BrandUp.Extensions.Migrations
{
    public class MigrationDefinition : IMigrationDefinition
    {
        readonly static Type MigrationHandlerInterface = typeof(IMigrationHandler);
        readonly MigrationAttribute attribute;

        public string Name { get; }
        public string Description => attribute.Description;
        public Type HandlerType { get; }
        public Type ParentHandlerType { get; }
        public bool IsRoot => ParentHandlerType == null;

        public MigrationDefinition(Type handlerType, MigrationAttribute attribute)
        {
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            this.attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

            if (attribute is UpgradeAttribute upgradeAttribute)
                ParentHandlerType = upgradeAttribute.AfterType;

            if (!MigrationHandlerInterface.IsAssignableFrom(handlerType))
                throw new ArgumentException($"Type {handlerType} is not inherit interface {MigrationHandlerInterface}.");

            Name = handlerType.FullName;
        }

        #region Object members

        public override int GetHashCode()
        {
            return HandlerType.GetHashCode();
        }
        public override string ToString()
        {
            return HandlerType.ToString();
        }

        #endregion
    }
}